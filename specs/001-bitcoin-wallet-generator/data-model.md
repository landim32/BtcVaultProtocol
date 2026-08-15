# Phase 1 — Data Model: Gerador de Carteiras Bitcoin (BIP39/BIP85) em Console

**Feature**: `001-bitcoin-wallet-generator` | **Date**: 2026-08-06

Todas as entidades existem **apenas em memória** durante a execução (Princípio III). Não há
esquema de persistência, migração ou serialização para disco. Todos os tipos são imutáveis
(`record` ou propriedades somente-leitura), conforme Princípio IV.

---

## Visão geral das relações

```text
EntropyCollection ──┐
                    ├──> MasterWallet ──┬──> DerivedWallet (0..10) ──> PassphraseWallet (0..3)
CsprngEntropy ──────┘                   │         │        │                   │
                                        │         │        └──> WalletAccount ─┘
                                        │         │                 │
                                        │         │                 └──> AddressEntry (10)
                                        │         │
                                        │         │   (as MESMAS carteiras, com a última passphrase)
                                        │         └──────> MultisigParticipant ──> WalletAccount
                                        │                          │
                                        └──> MultisigWallet (0..1) ─┘
                                                     │
                                                     └──> AddressEntry (10)

WalletSession = MasterWallet + DerivedWallet[] + MultisigWallet?
```

---

## Entidades

### EntropyCollection

> **Estrutura efêmera, não entidade com arquivo próprio.** `EntropyCollection` e `MasterEntropy`
> vivem apenas entre a coleta e a mistura e não possuem arquivo em `Models/` — residem em
> `Core/Entropy/EntropyStrength.cs` e `Core/Entropy/EntropyMixer.cs`, e são zeradas em seguida.

Entropia manual acumulada durante a coleta guiada.

| Campo | Tipo | Regras |
|---|---|---|
| `Rolls` | `IReadOnlyList<byte>` | Cada elemento ∈ {1..6}; mínimo 100 elementos |
| `BitsCollected` | `int` (derivado) | `floor(Rolls.Count × log2(6))`; deve ser ≥ 256 |
| `IsSufficient` | `bool` (derivado) | `BitsCollected >= 256` |

**Validações**: dígitos fora de 1–6 são rejeitados na entrada (FR-005). A coleta não pode ser
encerrada enquanto `IsSufficient` for falso.
**Ciclo de vida**: existe apenas durante a coleta; os bytes são zerados imediatamente após a
mistura em `EntropyMixer`.

### MasterEntropy

Resultado da combinação obrigatória das duas fontes (FR-004, R-001).

| Campo | Tipo | Regras |
|---|---|---|
| `Bytes` | `byte[32]` | `SHA256(EntropyCollection.Rolls) XOR RandomNumberGenerator(32)` |

**Invariante**: exatamente 32 bytes (256 bits). Se o CSPRNG estiver indisponível, a construção
falha e nada é gerado (FR-007).
**Ciclo de vida**: zerado logo após a criação do `MasterWallet`.

### MasterWallet

Raiz determinística da sessão inteira.

| Campo | Tipo | Regras |
|---|---|---|
| `Mnemonic` | `Mnemonic` (NBitcoin) | 24 palavras, wordlist inglesa, checksum válido |
| `RootKey` | `ExtKey` | Derivada da seed do mnemônico, **sem** passphrase |
| `Account` | `WalletAccount` | Conta `m/84'/0'/0'` da própria mestre |

**Relações**: origina todas as `DerivedWallet` e todos os `MultisigParticipant`.
**Invariante**: a mestre nunca recebe passphrase — passphrases pertencem às carteiras derivadas.

### DerivedWallet

Carteira filha produzida via BIP85 (FR-008 … FR-011).

| Campo | Tipo | Regras |
|---|---|---|
| `Index` | `int` | 0 … 9 (BIP85 index; ver R-004) |
| `DisplayNumber` | `int` (derivado) | `Index + 1` — numeração exibida ao usuário |
| `Mnemonic` | `Mnemonic` | 24 palavras derivadas de `m/83696968'/39'/0'/24'/{Index}'` |
| `Account` | `WalletAccount` | Conta `m/84'/0'/0'` sem passphrase |
| `PassphraseWallets` | `IReadOnlyList<PassphraseWallet>` | 0 a 3 elementos |

**Invariante de determinismo**: mesma `MasterWallet` + mesmo `Index` ⇒ mesmo `Mnemonic`, sempre.
**Regra de unicidade**: nenhum `Index` de `DerivedWallet` colide com índice de participante
multisig (que começa em 1000).

### PassphraseWallet

Carteira oculta definida por uma passphrase sobre a semente de uma `DerivedWallet` (FR-012 …
FR-016).

| Campo | Tipo | Regras |
|---|---|---|
| `Passphrase` | `string` | Não vazia (R-010); exibida em texto claro no relatório |
| `Account` | `WalletAccount` | Conta `m/84'/0'/0'` derivada da seed **com** a passphrase |

**Validações**:

- Digitação e confirmação devem coincidir exatamente, sem eco (FR-013, FR-014).
- Passphrase vazia é rejeitada com mensagem explicativa (R-010).
- Passphrase idêntica a outra da mesma carteira gera aviso de redundância (FR-016).

**Nota de normalização**: a passphrase é aplicada exatamente como digitada (NFKD, conforme
BIP39, tratado pela NBitcoin). Espaços à esquerda/direita são significativos e o usuário é
avisado disso.

### WalletAccount

Conta derivada — unidade comum de exibição para qualquer carteira.

| Campo | Tipo | Regras |
|---|---|---|
| `DerivationPath` | `KeyPath` | `m/84'/0'/0'` ou `m/48'/0'/0'/2'` |
| `AccountExtPubKey` | `ExtPubKey` | Chave pública estendida da conta |
| `Serialized` | `string` | `zpub…` (single-sig) ou `Zpub…` (participante multisig) — R-006 |
| `Fingerprint` | `HDFingerprint` | Fingerprint da chave mestre — usada no descriptor |
| `Addresses` | `IReadOnlyList<AddressEntry>` | Exatamente 10 elementos |

**Invariante**: `Serialized` sempre corresponde ao tipo de script do `DerivationPath` — `zpub`
para `m/84'`, `Zpub` para `m/48'/…/2'`.

### AddressEntry

Um endereço de recebimento exibido no relatório.

| Campo | Tipo | Regras |
|---|---|---|
| `Index` | `int` | 0 … 9 |
| `FullPath` | `string` | Caminho completo, ex.: `m/84'/0'/0'/0/3` |
| `Address` | `string` | Native SegWit mainnet — `bc1q…` |

**Regra**: apenas a cadeia de recebimento (`.../0/i`). Troco (`.../1/i`) não é exibido.

### MultisigWallet

Esquema M-de-N (FR-017 … FR-019c). **N não é escolhido pelo usuário**: é a quantidade de
carteiras derivadas criadas na sessão.

| Campo | Tipo | Regras |
|---|---|---|
| `RequiredSignatures` | `int` | 1 ≤ M ≤ N |
| `ParticipantCount` | `int` (derivado) | `Participants.Count` — igual ao número de carteiras derivadas |
| `Participants` | `IReadOnlyList<MultisigParticipant>` | Uma entrada por carteira derivada, na mesma ordem |
| `Descriptor` | `string` | `wsh(sortedmulti(M, …))` — R-007 |
| `Addresses` | `IReadOnlyList<AddressEntry>` | Exatamente 10 endereços P2WSH |

**Validações**: M fora de 1–N é rejeitado com nova solicitação (FR-018). O esquema só é oferecido
com pelo menos 2 carteiras derivadas (FR-019c).
**Invariante de ordenação**: em cada índice de endereço, as chaves públicas são ordenadas
lexicograficamente (BIP67) antes de compor o script — sem isso os endereços não reproduzem em
carteiras de referência.
**Aviso obrigatório**: a seção exibe que todos os participantes derivam da mesma mestre
(FR-019b).

### MultisigParticipant

Não é uma carteira nova: é uma `DerivedWallet` existente vista pela ótica do esquema (R-004).

| Campo | Tipo | Regras |
|---|---|---|
| `WalletIndex` | `int` | Índice da carteira derivada de origem (0 … 9) |
| `DisplayNumber` | `int` (derivado) | `WalletIndex + 1` — a mesma numeração da carteira |
| `Mnemonic` | `Mnemonic` | As 24 palavras **da própria carteira derivada** |
| `Passphrase` | `string?` | A última passphrase da carteira, ou `null` se não houver |
| `Account` | `WalletAccount` | Conta `m/48'/0'/0'/2'` derivada dessa semente **com essa passphrase**, serializada como `Zpub` |

**Invariante**: cada participante corresponde a exatamente uma `DerivedWallet` da sessão, na
mesma ordem. Nenhum material de chave novo é criado para o multisig.

### WalletSession

Agregação de tudo o que a execução produziu — entrada única do renderizador.

| Campo | Tipo | Regras |
|---|---|---|
| `Master` | `MasterWallet` | Sempre presente |
| `DerivedWallets` | `IReadOnlyList<DerivedWallet>` | 0 a 10 elementos |
| `Multisig` | `MultisigWallet?` | Nulo quando o usuário recusa |

**Ciclo de vida**: construída incrementalmente durante o fluxo, renderizada uma única vez e
descartada. Nunca serializada.

---

## Transições de estado do fluxo

```text
[Start]
  → WarnRedirectedOutput (se Console.IsOutputRedirected)        FR-029
  → ShowBestPracticesWarning                                    FR-028
  → CollectDiceEntropy        (loop até BitsCollected ≥ 256)    FR-004, FR-005
  → MixWithCsprng             (aborta se CSPRNG indisponível)   FR-007
  → BuildMasterWallet                                           FR-006
  → AskDeriveWallets? ── não ──────────────────┐                FR-008
        │ sim                                  │
        → AskWalletCount (1..10)               │                FR-009
        → para cada carteira:                  │
             AskPassphraseCount (0..3)         │                FR-012
             para cada passphrase:             │
                  ReadPassphrase + Confirm     │                FR-013, FR-014
                  (rejeita vazia / avisa igual)│                R-010, FR-016
  → (menos de 2 carteiras? informa e pula) ───┤                FR-019c
  → AskMultisig? ── não ──────────────────────┤                FR-017
        │ sim   (N = nº de carteiras criadas)  │
        → AskRequiredSignatures (1..N)         │                FR-018
        → BuildMultisig das carteiras          │                FR-019
          existentes, com a última passphrase  │
  → RenderReport ←─────────────────────────────┘                FR-023, FR-024
  → ShowClearTerminalGuidance                                   FR-028
  → ZeroSensitiveBuffers                                        FR-027
[End]
```

Qualquer entrada inválida em qualquer prompt retorna ao mesmo prompt com mensagem explicativa,
sem avançar e sem encerrar (FR-002). Interrupção por `Ctrl+C` salta direto para
`ZeroSensitiveBuffers` e encerra com código diferente de zero.

---

## Constantes de domínio

Centralizadas em `Core/WalletConstants.cs` — nenhum literal solto na lógica (Princípio IV).

| Constante | Valor |
|---|---|
| `MNEMONIC_WORD_COUNT` | 24 |
| `ENTROPY_BITS` | 256 |
| `ENTROPY_BYTES` | 32 |
| `MIN_DICE_ROLLS` | 100 |
| `DICE_FACES` | 6 |
| `MAX_DERIVED_WALLETS` | 10 |
| `MAX_PASSPHRASES_PER_WALLET` | 3 |
| `MIN_MULTISIG_PARTICIPANTS` | 2 (mínimo de carteiras para o esquema existir) |
| `ADDRESSES_PER_ACCOUNT` | 10 |
| `BIP85_PURPOSE` | 83696968 |
| `BIP85_APPLICATION_BIP39` | 39 |
| `BIP85_LANGUAGE_ENGLISH` | 0 |
| `BIP85_HMAC_KEY` | `"bip-entropy-from-k"` |
| `SINGLE_SIG_ACCOUNT_PATH` | `m/84'/0'/0'` |
| `MULTISIG_ACCOUNT_PATH` | `m/48'/0'/0'/2'` |
| `RECEIVE_CHAIN` | 0 |
| `ZPUB_VERSION` | `0x04B24746` |
| `ZPUB_MULTISIG_VERSION` | `0x02AA7ED3` |
