---

description: "Task list for 001-bitcoin-wallet-generator"
---

# Tasks: Gerador de Carteiras Bitcoin (BIP39/BIP85) em Console

**Input**: Design documents from `/specs/001-bitcoin-wallet-generator/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/cli-contract.md, quickstart.md

**Tests**: testes contra os vetores oficiais dos BIPs são **obrigatórios** (Constitution
Principle V, não-waivable). Cada derivação tem seu teste escrito **antes** da implementação e ele
deve falhar antes de ela existir.

**Organization**: tarefas agrupadas por user story, permitindo implementar e testar cada uma de
forma independente.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: pode rodar em paralelo (arquivos distintos, sem dependência pendente)
- **[Story]**: a qual user story a tarefa pertence (US1, US2, US3, US4)
- Todo caminho de arquivo é relativo à raiz do repositório

## Path Conventions

Layout de três projetos fixado pela Constituição ("Restrições Adicionais → Estrutura da Solução"):

- **Domínio**: `src/BtcVaultProtocol.Wallet.Core/`
- **Console**: `src/BtcVaultProtocol.Wallet.Console/`
- **Testes**: `tests/BtcVaultProtocol.Wallet.Tests/`
- Fluxo de dependência: `Console → Core`. `Core` não referencia `Console` nem usa `System.Console`.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: inicialização da solução e das constantes de domínio

- [X] T001 Criar `BtcVaultProtocol.sln` e os três projetos — `src/BtcVaultProtocol.Wallet.Core/BtcVaultProtocol.Wallet.Core.csproj` (classlib), `src/BtcVaultProtocol.Wallet.Console/BtcVaultProtocol.Wallet.Console.csproj` (console), `tests/BtcVaultProtocol.Wallet.Tests/BtcVaultProtocol.Wallet.Tests.csproj` (xunit) — com as referências `Console → Core` e `Tests → Core, Console`
- [X] T002 [P] Adicionar o pacote NBitcoin a `src/BtcVaultProtocol.Wallet.Core/BtcVaultProtocol.Wallet.Core.csproj` (único pacote de produção permitido — Princípio II)
- [X] T003 [P] Adicionar xUnit, `Microsoft.NET.Test.Sdk` e `xunit.runner.visualstudio` a `tests/BtcVaultProtocol.Wallet.Tests/BtcVaultProtocol.Wallet.Tests.csproj` — infraestrutura de execução de testes permitida pelo Princípio II (emenda 2.0.1); nenhum desses pacotes é referenciado por `Core` ou `Console`
- [X] T004 [P] Criar `Directory.Build.props` na raiz com `TargetFramework=net8.0`, `Nullable=enable`, `TreatWarningsAsErrors=true`, `ImplicitUsings=enable` aplicados aos três projetos (Princípio IV)
- [X] T005 [P] Criar `src/BtcVaultProtocol.Wallet.Core/WalletConstants.cs` com todas as constantes da tabela de `data-model.md` (contagens, limites, índices BIP85, caminhos de derivação, version bytes SLIP-132)

> **Constitution gate**: nenhum pacote NuGet além de NBitcoin + xUnit; nenhum ORM, HTTP client ou
> telemetria; nenhuma tarefa depende de Docker local.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: derivação de contas/endereços, serialização SLIP-132 e o esqueleto de interação —
tudo que **todas** as user stories consomem

**⚠️ CRITICAL**: nenhuma user story pode começar antes desta fase terminar

### Vetores de teste (embutidos, sem leitura de arquivo — Princípio III)

- [X] T006 [P] Criar `tests/BtcVaultProtocol.Wallet.Tests/Vectors/Bip39Vectors.cs` com os vetores oficiais Trezor (entropia → mnemônico → seed), incluindo os casos com passphrase `TREZOR`
- [X] T007 [P] Criar `tests/BtcVaultProtocol.Wallet.Tests/Vectors/Bip32Vectors.cs` com os vetores oficiais do BIP32 (derivação endurecida e normal)
- [X] T008 [P] Criar `tests/BtcVaultProtocol.Wallet.Tests/Vectors/Bip84Vectors.cs` com o vetor oficial do BIP84 (mnemônico `abandon … about`, `zpub` da conta `m/84'/0'/0'` e os endereços `.../0/0` e `.../0/1`)

### Modelos e caminhos

- [X] T009 [P] Criar o record `src/BtcVaultProtocol.Wallet.Core/Models/AddressEntry.cs` (`Index`, `FullPath`, `Address`) conforme `data-model.md`
- [X] T010 [P] Criar o record `src/BtcVaultProtocol.Wallet.Core/Models/WalletAccount.cs` (`DerivationPath`, `AccountExtPubKey`, `Serialized`, `Fingerprint`, `Addresses`) conforme `data-model.md`
- [X] T011 [P] Criar `src/BtcVaultProtocol.Wallet.Core/Derivation/DerivationPaths.cs` expondo `m/84'/0'/0'` e `m/48'/0'/0'/2'` como `KeyPath` a partir de `WalletConstants`

### Serialização SLIP-132 (R-006)

- [X] T012 [P] Escrever `tests/BtcVaultProtocol.Wallet.Tests/Derivation/Slip132SerializerTests.cs` afirmando que a conta do vetor BIP84 serializa exatamente no `zpub` esperado, e que o prefixo `Zpub` usa `0x02AA7ED3` — depende de T008; deve FALHAR antes de T013
- [X] T013 Implementar `src/BtcVaultProtocol.Wallet.Core/Derivation/Slip132Serializer.cs` trocando os 4 bytes de versão da serialização BIP32 de 78 bytes e recodificando com `Encoders.Base58Check` da NBitcoin (nenhuma primitiva criptográfica reimplementada — Princípio I)

### Derivação de contas e endereços

- [X] T014 [P] Escrever `tests/BtcVaultProtocol.Wallet.Tests/Derivation/AccountDeriverTests.cs` validando conta e `zpub` contra o vetor BIP84 (com e sem passphrase, usando `Bip39Vectors`) — depende de T010, T011 e T013; deve FALHAR antes de T016
- [X] T015 [P] Escrever `tests/BtcVaultProtocol.Wallet.Tests/Derivation/AddressDeriverTests.cs` validando os 10 endereços `.../0/i` contra o vetor BIP84 e afirmando que são exatamente 10, numerados 0–9 — depende de T009, T010 e T011; deve FALHAR antes de T017
- [X] T016 Implementar `src/BtcVaultProtocol.Wallet.Core/Derivation/AccountDeriver.cs` — mnemônico (+ passphrase opcional) → `ExtKey` → conta no caminho informado → `WalletAccount` com `Serialized` coerente com o tipo de script
- [X] T017 Implementar `src/BtcVaultProtocol.Wallet.Core/Derivation/AddressDeriver.cs` — conta → 10 `AddressEntry` P2WPKH/P2WSH na cadeia de recebimento, com o caminho completo em cada entrada

### Esqueleto de interação e segurança operacional

- [X] T018 [P] Implementar `src/BtcVaultProtocol.Wallet.Console/SecureExit.cs` com registro de buffers sensíveis e zeroing centralizado via `CryptographicOperations.ZeroMemory` (Princípio VI)
- [X] T019 [P] Implementar `src/BtcVaultProtocol.Wallet.Console/Prompts/YesNoPrompt.cs` — aceita `y/yes/n/no` case-insensitive, respeita o padrão em maiúscula, repete em entrada inválida (FR-003, contrato §4)
- [X] T020 [P] Implementar `src/BtcVaultProtocol.Wallet.Console/Prompts/IntegerPrompt.cs` — faixa mínima/máxima parametrizada, mensagem `Please enter a whole number between X and Y.` e repetição sem encerrar (FR-002, contrato §4)
- [X] T021 [P] Implementar `src/BtcVaultProtocol.Wallet.Console/Rendering/Warnings.cs` com o aviso de abertura, o alerta de `Console.IsOutputRedirected` e a orientação final de limpeza de tela (FR-028, FR-029, contrato §2 e §7)
- [X] T022 Implementar `src/BtcVaultProtocol.Wallet.Console/Program.cs` — ponto de entrada, `Console.CancelKeyPress` chamando `SecureExit`, `catch` de topo sem stack trace nem material sensível, e os códigos de saída 0/1/2 do contrato §1
- [X] T023 Implementar o esqueleto de `src/BtcVaultProtocol.Wallet.Console/Rendering/ReportRenderer.cs` — separadores, títulos de seção e o bloco reutilizável "conta + 10 endereços" conforme o layout do contrato §7
- [X] T024 [P] Escrever `tests/BtcVaultProtocol.Wallet.Tests/Flow/InputValidationTests.cs` cobrindo `YesNoPrompt` e `IntegerPrompt`: texto onde se espera número, zero, negativo, acima do limite e resposta vazia (SC-007)

**Checkpoint**: derivação e interação básicas prontas e verdes — as user stories podem começar

---

## Phase 3: User Story 1 - Gerar uma carteira mestre com entropia controlada (Priority: P1) 🎯 MVP

**Goal**: coletar entropia das duas fontes obrigatórias, gerar a semente mestre de 24 palavras e
exibi-la com seus 10 endereços — sem gravar nada.

**Independent Test**: executar o app, fornecer 100 lançamentos de dado, confirmar, e verificar que
24 palavras válidas e 10 endereços `bc1q…` aparecem; importar a semente no Sparrow (quickstart
V-1) e conferir que os endereços coincidem em conteúdo e ordem.

### Tests for User Story 1 ⚠️

> Escrever primeiro; garantir que falham antes da implementação.

- [X] T025 [P] [US1] Escrever `tests/BtcVaultProtocol.Wallet.Tests/Entropy/EntropyStrengthTests.cs` — `floor(n × log2(6))` para n = 0, 1, 99, 100, 200; `IsSufficient` verdadeiro só a partir de 100 lançamentos
- [X] T026 [P] [US1] Escrever `tests/BtcVaultProtocol.Wallet.Tests/Entropy/EntropyMixerTests.cs` — resultado sempre com 32 bytes; XOR verificado contra valores fixos; com entrada manual degenerada (`111…`) e CSPRNG variável as saídas de 1.000 execuções são todas distintas (SC-005); com CSPRNG fixo e entradas manuais diferentes as saídas diferem
- [X] T026a [P] [US1] Escrever `tests/BtcVaultProtocol.Wallet.Tests/Entropy/CsprngUnavailableTests.cs` — com um provedor de aleatoriedade que falha, `EntropyMixer` lança a exceção dedicada, nenhuma semente é produzida e a entropia manual sozinha nunca é usada (FR-007)
- [X] T027 [P] [US1] Escrever `tests/BtcVaultProtocol.Wallet.Tests/Mnemonics/MnemonicFactoryTests.cs` — 32 bytes de entropia → mnemônico de 24 palavras com checksum válido, conferido contra `Bip39Vectors`

### Implementation for User Story 1

- [X] T028 [P] [US1] Implementar `src/BtcVaultProtocol.Wallet.Core/Entropy/EntropyStrength.cs` — contagem de bits acumulados e verificação do mínimo de 256 (FR-005)
- [X] T029 [US1] Implementar `src/BtcVaultProtocol.Wallet.Core/Entropy/EntropyMixer.cs` — `SHA256(entrada manual) XOR RandomNumberGenerator(32)`, expondo a fonte de aleatoriedade como dependência injetável (para permitir T026a) e lançando exceção dedicada se o CSPRNG estiver indisponível (R-001, FR-004, FR-007)
- [X] T030 [P] [US1] Implementar `src/BtcVaultProtocol.Wallet.Core/Mnemonics/MnemonicFactory.cs` — 32 bytes → `Mnemonic` de 24 palavras, wordlist inglesa (FR-006)
- [X] T031 [P] [US1] Criar os records `src/BtcVaultProtocol.Wallet.Core/Models/MasterWallet.cs` e `src/BtcVaultProtocol.Wallet.Core/Models/WalletSession.cs` conforme `data-model.md`
- [X] T032 [US1] Implementar `src/BtcVaultProtocol.Wallet.Console/Prompts/DiceEntropyPrompt.cs` — aceita apenas dígitos 1–6, exibe progresso `X / 256 bits (n rolls)`, recusa encerrar antes do mínimo e alerta em sequência degenerada (contrato §3)
- [X] T033 [US1] Implementar `src/BtcVaultProtocol.Wallet.Console/Flow/WalletFlow.cs` com a etapa 1 do fluxo — avisos, coleta, mistura, construção da `MasterWallet` e montagem inicial da `WalletSession`
- [X] T034 [US1] Estender `src/BtcVaultProtocol.Wallet.Console/Rendering/ReportRenderer.cs` com a seção `MASTER WALLET` — 24 palavras em grade numerada, tipo de endereço, caminho, `zpub` e os 10 endereços (contrato §7)
- [X] T035 [US1] Ligar o encerramento do fluxo ao `SecureExit` — zerar entropia manual, `MasterEntropy` e buffers derivados em `try/finally`, inclusive no caminho de erro (FR-027, Princípio VI)
- [X] T036 [US1] Tratar a exceção de CSPRNG indisponível em `src/BtcVaultProtocol.Wallet.Console/Program.cs` com a mensagem `FATAL: the operating system CSPRNG is unavailable. No wallet was generated.` e código de saída 2 (contrato §1, §3)

**Checkpoint**: MVP funcional — o app gera e exibe uma carteira mestre verificável, sem persistir nada

---

## Phase 4: User Story 2 - Derivar múltiplas carteiras filhas a partir da mestre (BIP85) (Priority: P2)

**Goal**: derivar de 1 a 10 carteiras filhas determinísticas da mestre, cada uma com suas 24
palavras e seus 10 endereços.

**Independent Test**: com uma mestre gerada, responder `Y` e informar 3; verificar 3 carteiras
distintas com 24 palavras válidas; repetir com a mesma mestre e confirmar sementes idênticas.

### Tests for User Story 2 ⚠️

- [X] T037 [P] [US2] Criar `tests/BtcVaultProtocol.Wallet.Tests/Vectors/Bip85Vectors.cs` com o vetor oficial do BIP85 (path `m/83696968'/39'/0'/24'/0'`, entropia `ae131e…f37f`, mnemônico `puppy ocean match …`) e a master seed do vetor
- [X] T038 [P] [US2] Escrever `tests/BtcVaultProtocol.Wallet.Tests/Bip85/Bip85DeriverTests.cs` — a entropia derivada e o mnemônico de 24 palavras batem exatamente com `Bip85Vectors`; a chave HMAC usada é `bip-entropy-from-k`; o truncamento é de 32 bytes — deve FALHAR antes de T040
- [X] T039 [P] [US2] Escrever `tests/BtcVaultProtocol.Wallet.Tests/Bip85/Bip85DeterminismTests.cs` — a mesma mestre com os mesmos índices produz sempre as mesmas sementes, e os índices 0–9 nunca colidem com os índices 1000+ reservados ao multisig (FR-010, FR-019, R-004)

### Implementation for User Story 2

- [X] T040 [US2] Implementar `src/BtcVaultProtocol.Wallet.Core/Bip85/Bip85Deriver.cs` — derivação endurecida `m/83696968'/39'/0'/24'/{index}'` via `ExtKey.Derive` (NBitcoin) + `HMACSHA512` (BCL) com truncamento em 32 bytes, componente isolado e com o caminho comentado (R-003, exceção controlada do Princípio I)
- [X] T041 [P] [US2] Criar o record `src/BtcVaultProtocol.Wallet.Core/Models/DerivedWallet.cs` (`Index`, `DisplayNumber`, `Mnemonic`, `Account`, `PassphraseWallets`) conforme `data-model.md`
- [X] T042 [US2] Estender `src/BtcVaultProtocol.Wallet.Console/Flow/WalletFlow.cs` com a etapa 2 — pergunta `Derive child wallets…? [Y/n]` e `How many wallets? (1-10)` usando os prompts da Phase 2, e derivação das carteiras (FR-008, FR-009)
- [X] T043 [US2] Estender `src/BtcVaultProtocol.Wallet.Console/Rendering/ReportRenderer.cs` com a seção `DERIVED WALLET n of N`, exibindo o caminho BIP85 de origem, as 24 palavras, o `zpub` e os 10 endereços (contrato §7)
- [X] T044 [US2] Incluir as sementes das carteiras derivadas no registro de buffers sensíveis do `SecureExit` em `src/BtcVaultProtocol.Wallet.Console/SecureExit.cs`

**Checkpoint**: US1 e US2 funcionam de forma independente

---

## Phase 5: User Story 3 - Proteger cada carteira com uma ou mais passphrases (Priority: P3)

**Goal**: associar de 0 a 3 passphrases por carteira, com digitação sem eco e confirmação, e
exibir os endereços de cada carteira oculta separadamente.

**Independent Test**: informar 2 passphrases para uma carteira e verificar três conjuntos de
endereços distintos no relatório — um sem passphrase e um por passphrase.

### Tests for User Story 3 ⚠️

- [X] T045 [P] [US3] Escrever `tests/BtcVaultProtocol.Wallet.Tests/Derivation/PassphraseAccountTests.cs` — a mesma semente com passphrases distintas gera contas e endereços distintos; a seed com passphrase bate com os vetores `TREZOR` de `Bip39Vectors`; incluir caso com passphrase não-ASCII (acentos, emoji, espaços nas bordas) confirmando normalização NFKD e que espaços nas bordas são significativos (FR-015, edge case de não-ASCII)
- [X] T046 [P] [US3] Escrever `tests/BtcVaultProtocol.Wallet.Tests/Flow/PassphraseRulesTests.cs` — confirmação divergente repete o prompt; passphrase vazia é rejeitada (R-010); passphrase duplicada na mesma carteira gera aviso e é aceita (FR-016); contagem fora de 0–3 é rejeitada

### Implementation for User Story 3

- [X] T047 [P] [US3] Criar o record `src/BtcVaultProtocol.Wallet.Core/Models/PassphraseWallet.cs` (`Passphrase`, `Account`) conforme `data-model.md`
- [X] T048 [US3] Implementar `src/BtcVaultProtocol.Wallet.Console/Prompts/PassphrasePrompt.cs` — leitura com `Console.ReadKey(intercept: true)` sem eco algum, confirmação obrigatória, rejeição de vazia e aviso de duplicidade (FR-013, FR-014, contrato §5)
- [X] T049 [US3] Estender `src/BtcVaultProtocol.Wallet.Console/Flow/WalletFlow.cs` com a etapa 3 — `How many passphrases…? (0-3)` por carteira e coleta de cada passphrase (FR-012)
- [X] T050 [US3] Estender `src/BtcVaultProtocol.Wallet.Console/Rendering/ReportRenderer.cs` com os blocos `[no passphrase]` e `[passphrase n] "…"`, nesta ordem, cada um com `zpub` e 10 endereços (FR-023, contrato §7)
- [X] T051 [US3] Registrar os buffers de passphrase no `SecureExit` e zerá-los após a derivação da conta em `src/BtcVaultProtocol.Wallet.Console/SecureExit.cs` (Princípio VI)

**Checkpoint**: US1, US2 e US3 funcionam de forma independente

---

## Phase 6: User Story 4 - Criar uma carteira multisig (Priority: P4)

**Goal**: montar um esquema M-de-N cujos participantes derivam da mestre via BIP85 em índices
deslocados, exibindo sementes, `Zpub`, descriptor e 10 endereços P2WSH.

**Independent Test**: responder `y`, informar 3 participantes e 2 assinaturas, e verificar as 3
sementes, o descriptor `wsh(sortedmulti(2,…))` e 10 endereços — reproduzíveis no Sparrow
(quickstart V-3).

### Tests for User Story 4 ⚠️

- [X] T052 [P] [US4] Criar `tests/BtcVaultProtocol.Wallet.Tests/Vectors/Bip67Vectors.cs` com os vetores oficiais de ordenação lexicográfica de chaves públicas do BIP67
- [X] T053 [P] [US4] Criar `tests/BtcVaultProtocol.Wallet.Tests/Vectors/DescriptorVectors.cs` com os vetores de `wsh(multi(...))` do BIP381 (script e scriptPubKey esperados)
- [X] T054 [P] [US4] Escrever `tests/BtcVaultProtocol.Wallet.Tests/Multisig/MultisigBuilderTests.cs` — as chaves são ordenadas conforme `Bip67Vectors` antes de compor o script; o scriptPubKey P2WSH bate com `DescriptorVectors`; são gerados exatamente 10 endereços — deve FALHAR antes de T057
- [X] T055 [P] [US4] Escrever `tests/BtcVaultProtocol.Wallet.Tests/Multisig/MultisigParticipantTests.cs` — os participantes reusam as sementes das carteiras derivadas, usam a última passphrase de cada uma, contam exatamente o número de carteiras criadas, e a conta usa `m/48'/0'/0'/2'` serializada como `Zpub` (FR-019, R-004 revisado)
- [X] T056 [P] [US4] Escrever `tests/BtcVaultProtocol.Wallet.Tests/Flow/MultisigValidationTests.cs` — assinaturas fora de 1..N são rejeitadas com nova solicitação; o número de participantes nunca é perguntado (FR-018, SC-007)

### Implementation for User Story 4

- [X] T057a [US4] Implementar `src/BtcVaultProtocol.Wallet.Core/Multisig/MultisigParticipantSelector.cs` — monta os participantes a partir das carteiras derivadas JÁ criadas, cada uma com a sua última passphrase (ou sem passphrase, se não houver); exige ao menos 2 carteiras (FR-019, FR-019c, R-004 revisado)
- [X] T057 [US4] Implementar `src/BtcVaultProtocol.Wallet.Core/Multisig/MultisigBuilder.cs` — ordenação BIP67, `PayToMultiSigTemplate.Instance.GenerateScriptPubKey(m, pubKeys)` e endereço via `redeemScript.WitHash.GetAddress(Network.Main)` (R-007)
- [X] T058 [P] [US4] Implementar `src/BtcVaultProtocol.Wallet.Core/Multisig/MultisigDescriptor.cs` — string `wsh(sortedmulti(M,[fingerprint/48h/0h/0h/2h]Zpub/0/*,…))` (R-007)
- [X] T059 [P] [US4] Criar os records `src/BtcVaultProtocol.Wallet.Core/Models/MultisigParticipant.cs` e `src/BtcVaultProtocol.Wallet.Core/Models/MultisigWallet.cs` conforme `data-model.md`
- [X] T060 [US4] Estender `src/BtcVaultProtocol.Wallet.Console/Flow/WalletFlow.cs` com a etapa 4 — informa que o esquema combina as carteiras acima, pergunta apenas `How many signatures are required to spend?` (1..N), e pula a etapa com explicação se houver menos de 2 carteiras; nenhuma derivação no Console (FR-017, FR-018, FR-019c, Princípio IV)
- [X] T061 [US4] Estender `src/BtcVaultProtocol.Wallet.Console/Rendering/ReportRenderer.cs` com a seção `MULTISIG WALLET — M of N`: declaração de reuso das carteiras, aviso de raiz compartilhada, cada participante com a carteira de origem e a passphrase usada, descriptor e os 10 endereços (FR-019b, FR-023, contrato §7)
- [X] T062 [US4] Confirmar que as sementes dos participantes já estão cobertas pelo `SecureExit` — são as mesmas das carteiras derivadas, sem material novo a registrar

**Checkpoint**: todas as user stories funcionam de forma independente

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: auditorias de conformidade constitucional e validação fim-a-fim

- [X] T063 [P] Auditar toda a solução: nenhuma escrita em arquivo, log, clipboard ou variável de ambiente, e nenhuma chamada de rede — nenhum `using System.Net`/`System.IO` nos projetos de produção (Princípio III 🔒)
- [X] T064 [P] Auditar `src/BtcVaultProtocol.Wallet.Core/` e `src/BtcVaultProtocol.Wallet.Console/Prompts/` — nenhuma referência a `System.Console` no Core, nenhum uso de `System.Random`, nenhum literal mágico fora de `WalletConstants.cs`, e nenhum prompt que aceite chave estendida externa de cossignatário (Princípios I e IV, FR-019a)
- [X] T065 [P] Auditar `src/BtcVaultProtocol.Wallet.Console/SecureExit.cs` e os caminhos de erro — todo buffer sensível é zerado, inclusive em `CancelKeyPress` e no `catch` de topo, exercitando um cancelamento real durante a execução; se o handler não for exercitável sem sinal do SO, registrar como verificação manual (Princípio VI, FR-027)
- [X] T066 [P] Auditar mensagens de exceção e de erro — nenhum caminho interno, stack trace ou fragmento de semente/passphrase escapa ("Restrições Adicionais → Tratamento de Erros")
- [ ] T067 Executar a medição manual de desempenho do passo V-6 de `specs/001-bitcoin-wallet-generator/quickstart.md` — cenário máximo (10 carteiras × 3 passphrases + multisig 9-de-9 = 41 contas, 410 endereços) em menos de 5 s — e registrar o resultado. **Não** implementar como teste xUnit: o Princípio V proíbe testes que dependam do relógio do sistema
- [ ] T068 Executar a verificação cruzada manual do `quickstart.md` — V-1 em **duas** carteiras de referência independentes (SC-002), V-2 (passphrase), V-3 (multisig no Sparrow), V-5 (não persistência) — e registrar os resultados
- [ ] T068a Validar SC-001 e SC-008 com um usuário familiarizado com carteiras Bitcoin — fluxo padrão (3 carteiras, 1 passphrase cada) concluído em menos de 5 min sem consultar documentação, e localização de qualquer conta no relatório em menos de 15 s — registrar os tempos medidos
- [X] T069 [P] Conferir o relatório contra as 6 regras verificáveis do contrato §7 (ordem das seções, blocos por passphrase, 10 endereços numerados, `zpub`/`Zpub` corretos, aviso multisig, orientação de limpeza como última linha) e confirmar que toda string exibida — prompts, erros, avisos e títulos — está em inglês (FR-024a)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: sem dependências — pode começar imediatamente
- **Foundational (Phase 2)**: depende do Setup — **BLOQUEIA todas as user stories**
- **User Stories (Phase 3–6)**: todas dependem da Phase 2
  - US1 (P1) é independente das demais
  - US2 (P2) precisa da `MasterWallet` produzida por US1
  - US3 (P3) precisa das carteiras produzidas por US2 para ter a quem associar passphrases
  - US4 (P4) precisa da `MasterWallet` (US1) e do `Bip85Deriver` (US2); é independente de US3
- **Polish (Phase 7)**: depende de todas as histórias desejadas estarem completas

### User Story Dependencies

```text
Setup → Foundational → US1 (MVP) ─┬─→ US2 ──→ US3
                                  └─→ US4 (precisa do Bip85Deriver de US2)
```

US3 e US4 podem ser desenvolvidas em paralelo assim que US2 estiver pronta.

### Within Each User Story

- Testes de vetor escritos e falhando **antes** da derivação que cobrem
- Records de domínio antes dos componentes de derivação
- Derivação no `Core` antes dos prompts e do rendering no `Console`
- História completa antes de passar para a próxima prioridade

### Parallel Opportunities

- Setup: T002, T003, T004, T005 em paralelo após T001
- Foundational: T006–T011 todos em paralelo; T012/T014/T015 em paralelo entre si
- US1: T025, T026, T026a, T027 em paralelo; depois T028, T030, T031 em paralelo
- US2: T037, T038, T039 em paralelo
- US4: T052–T056 em paralelo; depois T058 e T059 em paralelo (T057a antes de T057)
- Polish: T063, T064, T065, T066, T069 em paralelo

---

## Parallel Example: User Story 1

```bash
# Todos os testes de US1 juntos:
Task: "Escrever tests/BtcVaultProtocol.Wallet.Tests/Entropy/EntropyStrengthTests.cs"
Task: "Escrever tests/BtcVaultProtocol.Wallet.Tests/Entropy/EntropyMixerTests.cs"
Task: "Escrever tests/BtcVaultProtocol.Wallet.Tests/Mnemonics/MnemonicFactoryTests.cs"

# Depois, os componentes independentes de US1:
Task: "Implementar src/BtcVaultProtocol.Wallet.Core/Entropy/EntropyStrength.cs"
Task: "Implementar src/BtcVaultProtocol.Wallet.Core/Mnemonics/MnemonicFactory.cs"
Task: "Criar src/BtcVaultProtocol.Wallet.Core/Models/MasterWallet.cs e WalletSession.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 apenas)

1. Phase 1: Setup (T001–T005)
2. Phase 2: Foundational (T006–T024) — crítico, bloqueia tudo
3. Phase 3: US1 (T025–T036)
4. **PARE e VALIDE**: gere uma carteira, importe no Sparrow (quickstart V-1) e confira os 10 endereços
5. Nesse ponto o produto já entrega valor: uma carteira BIP39 verificável, gerada com entropia de duas fontes, sem persistir nada

### Incremental Delivery

1. Setup + Foundational → base pronta
2. + US1 → carteira mestre verificável (**MVP**)
3. + US2 → múltiplas carteiras determinísticas via BIP85
4. + US3 → carteiras ocultas por passphrase
5. + US4 → esquema multisig com descriptor
6. + Polish → auditorias constitucionais e verificação cruzada completa

### Parallel Team Strategy

Com mais de uma pessoa, após a Phase 2:

1. Uma pessoa entrega US1 (bloqueia as demais)
2. Em seguida, US2
3. A partir daí, US3 e US4 em paralelo por pessoas diferentes

---

## Notes

- Tarefas `[P]` tocam arquivos distintos e não têm dependência pendente entre si
- O rótulo `[Story]` rastreia a tarefa até a user story correspondente
- **Nunca** ajuste um valor esperado de teste para acomodar a saída do código — divergência do
  vetor oficial é defeito do código (Princípio V 🔒)
- Nenhuma tarefa pode introduzir pacote NuGet fora de NBitcoin + xUnit, escrita em disco ou
  chamada de rede — três gates não-waivable
- Commit após cada tarefa ou grupo lógico; pare em qualquer checkpoint para validar a história
  isoladamente
