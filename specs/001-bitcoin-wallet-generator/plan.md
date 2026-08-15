# Implementation Plan: Gerador de Carteiras Bitcoin (BIP39/BIP85) em Console

**Branch**: `001-bitcoin-wallet-generator` | **Date**: 2026-08-06 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-bitcoin-wallet-generator/spec.md`

## Summary

Aplicativo de console offline que coleta entropia de duas fontes obrigatórias (lançamentos de
dado digitados pelo usuário + CSPRNG do sistema operacional, combinados por XOR), gera uma
carteira mestre BIP39 de 24 palavras, deriva dela até 10 carteiras filhas via BIP85, associa até
3 passphrases a cada carteira e, opcionalmente, monta um esquema multisig M-de-N cujos
participantes também derivam da mestre. Ao final exibe um relatório único em tela com sementes,
chaves estendidas (`zpub`/`Zpub`) e os 10 primeiros endereços Native SegWit de cada conta. Nada
é gravado, nada trafega pela rede.

A abordagem técnica delega toda a criptografia à NBitcoin, compõe BIP85 a partir de derivação
BIP32 endurecida + HMAC-SHA512 da BCL (exceção prevista no Princípio I), e valida cada derivação
contra os vetores oficiais dos respectivos BIPs.

**Duas decisões que estendem minimamente a spec**, ambas a serviço da verificabilidade exigida
por SC-003 e pelo Princípio V, e ambas registradas em `research.md`:

- **R-007**: a seção multisig exibe também o output descriptor `wsh(sortedmulti(...))`. Sem ele,
  a chave estendida sozinha não define o esquema e o usuário não consegue reproduzir os
  endereços em nenhuma carteira de referência.
- **R-010**: resolve a pendência que o `/speckit.clarify` havia adiado — passphrase vazia é
  rejeitada, por equivaler à carteira sem passphrase já exibida.

## Technical Context

**Language/Version**: .NET 8.0 LTS (fixed by Constitution Principle II)
**Primary Dependencies**: NBitcoin + BCL only; xUnit for tests (fixed by Principle II)
**Storage**: None — nothing is persisted (Principle III)
**Testing**: xUnit against official BIP test vectors — BIP39 (Trezor), BIP32, BIP84, BIP85, BIP67, BIP381 (Principle V)
**Target Platform**: Cross-platform console (Windows/Linux/macOS), offline execution
**Project Type**: Interactive console application
**Performance Goals**: relatório completo no cenário máximo (10 carteiras × 3 passphrases + multisig 9 participantes = 41 contas × 10 endereços) renderizado em menos de 5 s após a última resposta
**Constraints**: No network calls, no file/log/clipboard writes (Principle III); no NuGet package outside the fixed stack (Principle II); Docker NOT available locally; interface integralmente em inglês (FR-024a)
**Scale/Scope**: até 10 carteiras derivadas, 3 passphrases por carteira, 9 participantes multisig; 10 endereços de recebimento por conta; usuário único, sessão única

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Resultado inicial (pré-Phase 0)**: PASS em todos os gates.
**Resultado pós-design (pós-Phase 1)**: PASS em todos os gates — nenhuma decisão de design
introduziu violação.

| Gate | Principle | Status | Evidência no design |
|---|---|---|---|
| Crypto primitives | I 🔒 | **PASS** | BIP39/BIP32/endereços/multisig via NBitcoin; BIP85 isolado em `Bip85/Bip85Deriver.cs`, composto apenas de `ExtKey.Derive` (NBitcoin) + `HMACSHA512` (BCL), com o caminho `m/83696968'/…` declarado e comentado; entropia por `RandomNumberGenerator`; nenhum uso de `System.Random` na solução. A serialização SLIP-132 (R-006) é troca de version bytes + `Encoders.Base58Check` da NBitcoin — codificação, não primitiva. |
| Stack | II | **PASS** | `Core` referencia apenas NBitcoin; `Tests` apenas xUnit; UI sobre `System.Console`; nenhum ORM, HTTP client ou telemetria; nenhuma tarefa depende de Docker. |
| No persistence / no network | III 🔒 | **PASS** | Nenhum tipo de I/O além de `System.Console`; nenhum `using` de `System.Net`/`System.IO` em produção; `Console.IsOutputRedirected` alerta sobre redirecionamento (R-011). Vetores de teste embutidos como constantes, não lidos de arquivo. |
| Code conventions | IV | **PASS** | Records imutáveis para todo resultado (ver `data-model.md`); `WalletConstants.cs` centraliza contagens, índices e limites; `Core` sem `System.Console`; nullable e warnings-as-errors habilitados na Phase 1 de Setup. |
| Test vectors | V 🔒 | **PASS** | Estratégia completa em R-008: vetor oficial por componente; a única lacuna (endereço multisig BIP48 fim-a-fim, sem vetor oficial publicado) é coberta por composição de vetores BIP67 + BIP381 + verificação cruzada manual obrigatória no quickstart. Nenhum golden file gerado pela própria implementação. |
| Memory & output hygiene | VI | **PASS** | `byte[]`/`char[]` + `CryptographicOperations.ZeroMemory` em `try/finally`; `Console.ReadKey(intercept: true)` para passphrases; limpeza também em `CancelKeyPress` e no `catch` de topo; avisos pré e pós-geração. Resíduo conhecido das `string` internas da NBitcoin documentado em R-009. |

🔒 = não-waivable. Nenhum gate falhou; nada foi movido para Complexity Tracking.

## Project Structure

### Documentation (this feature)

```text
specs/001-bitcoin-wallet-generator/
├── plan.md              # This file (/speckit.plan command output)
├── spec.md              # Feature specification
├── research.md          # Phase 0 output — R-001 … R-011
├── data-model.md        # Phase 1 output — entidades, invariantes, fluxo de estado
├── quickstart.md        # Phase 1 output — build, execução e verificação cruzada
├── contracts/
│   └── cli-contract.md  # Phase 1 output — contrato de prompts e formato do relatório
├── checklists/
│   └── requirements.md  # Spec quality checklist (16/16)
└── tasks.md             # Phase 2 output (/speckit.tasks — NOT created by /speckit.plan)
```

### Source Code (repository root)

O layout de três projetos é fixado pela Constituição ("Restrições Adicionais → Estrutura da
Solução"). Projetos adicionais exigiriam emenda formal.

```text
BtcVaultProtocol.sln

src/BtcVaultProtocol.Wallet.Core/            # Domínio — sem System.Console
├── WalletConstants.cs                 # Contagens, limites, índices, caminhos
├── Entropy/
│   ├── EntropyMixer.cs                # SHA256(manual) XOR CSPRNG(32)  [R-001]
│   └── EntropyStrength.cs             # floor(n × log2(6)) bits        [R-002]
├── Mnemonics/
│   └── MnemonicFactory.cs             # 32 bytes → Mnemonic 24 palavras (NBitcoin)
├── Bip85/
│   └── Bip85Deriver.cs                # m/83696968'/39'/0'/24'/i'      [R-003, R-004]
├── Derivation/
│   ├── DerivationPaths.cs             # m/84'/0'/0' e m/48'/0'/0'/2'   [R-005]
│   ├── AccountDeriver.cs              # semente (+passphrase) → conta
│   ├── AddressDeriver.cs              # conta → 10 endereços .../0/i
│   └── Slip132Serializer.cs           # zpub / Zpub                     [R-006]
├── Multisig/
│   ├── MultisigBuilder.cs             # sortedmulti + P2WSH             [R-007]
│   └── MultisigDescriptor.cs          # wsh(sortedmulti(...))
└── Models/
    ├── MasterWallet.cs · DerivedWallet.cs · PassphraseWallet.cs
    ├── WalletAccount.cs · AddressEntry.cs
    ├── MultisigWallet.cs · MultisigParticipant.cs
    └── WalletSession.cs

src/BtcVaultProtocol.Wallet.Console/         # Interação — único ponto de I/O
├── Program.cs                         # Entry point, CancelKeyPress, catch de topo
├── Prompts/
│   ├── YesNoPrompt.cs · IntegerPrompt.cs
│   ├── PassphrasePrompt.cs            # sem eco + confirmação
│   └── DiceEntropyPrompt.cs           # coleta guiada com progresso em bits
├── Flow/
│   └── WalletFlow.cs                  # orquestra a sequência fixa de perguntas
├── Rendering/
│   ├── ReportRenderer.cs              # relatório final seccionado
│   └── Warnings.cs                    # avisos pré/pós-geração e de redirecionamento
└── SecureExit.cs                      # zeroing centralizado

tests/BtcVaultProtocol.Wallet.Tests/
├── Vectors/                           # Constantes embutidas — sem leitura de arquivo
│   ├── Bip39Vectors.cs · Bip32Vectors.cs · Bip84Vectors.cs
│   ├── Bip85Vectors.cs · Bip67Vectors.cs · DescriptorVectors.cs
├── Entropy/EntropyMixerTests.cs
├── Mnemonics/MnemonicFactoryTests.cs
├── Bip85/Bip85DeriverTests.cs
├── Derivation/AccountDeriverTests.cs · AddressDeriverTests.cs · Slip132SerializerTests.cs
├── Multisig/MultisigBuilderTests.cs
└── Flow/InputValidationTests.cs
```

Fluxo de dependência: `Console → Core`. `Core` NÃO referencia `Console`. `Tests` referencia ambos.

**Structure Decision**: mantido exatamente o layout de três projetos da Constituição. A
subdivisão interna do `Core` segue as fronteiras do domínio (entropia → mnemônico → BIP85 →
derivação → multisig), o que permite que cada uma seja testada isoladamente contra o vetor
oficial correspondente. `Models/` concentra os records imutáveis consumidos pelo renderizador,
mantendo o `Console` sem lógica de derivação.

## Complexity Tracking

> Preenchido apenas quando o Constitution Check tem violações que precisam ser justificadas.

**Nenhuma violação a justificar.** Todos os seis gates passaram.

Dois pontos foram avaliados e **não** constituem violação — registrados aqui apenas para que a
revisão não precise reabrir a análise:

| Ponto avaliado | Por que não é violação |
|---|---|
| BIP85 composto no projeto | Exceção explicitamente prevista no Princípio I, com as três condições atendidas: só primitivas NBitcoin/BCL, isolado em um componente único, validado contra o vetor oficial (R-003). |
| Serialização SLIP-132 (`zpub`/`Zpub`) escrita à mão | É codificação, não primitiva: troca de 4 bytes de versão + `Encoders.Base58Check` da própria NBitcoin. Nenhum hash, HMAC ou operação de curva reimplementado. NBitcoin não oferece alternativa (R-006), e a única opção pronta exigiria dependência fora da stack fixa. |
