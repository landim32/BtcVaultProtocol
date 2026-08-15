# Phase 0 — Research: Gerador de Carteiras Bitcoin (BIP39/BIP85) em Console

**Feature**: `001-bitcoin-wallet-generator` | **Date**: 2026-08-06

Todas as incógnitas de Technical Context foram resolvidas. Nenhum `NEEDS CLARIFICATION` permanece.

---

## R-001 — Mistura das duas fontes de entropia

**Decision**: `entropia_final = SHA256(bytes_da_entrada_manual) XOR bytes_do_CSPRNG(32)`.

**Rationale**: XOR de duas fontes independentes tem a propriedade exata exigida pela spec (FR-004):
o resultado é uniforme se **qualquer uma** das parcelas for uniforme. Se o CSPRNG do sistema
estiver comprometido, um atacante que não conheça os lançamentos de dado ainda não consegue
prever a semente; se o usuário digitar uma sequência degenerada (`1111…`), o CSPRNG sozinho já
garante a imprevisibilidade — o que satisfaz o edge case "entropia manual de baixa qualidade" e
o SC-005. O SHA-256 sobre a entrada manual normaliza qualquer formato de digitação para 32
bytes sem introduzir viés de mapeamento.

**Alternatives considered**:

- `HMAC-SHA512(key = csprng, msg = manual)` truncado: equivalente em força, mas a propriedade
  "seguro se qualquer uma das fontes for boa" é menos evidente para quem audita o código.
- Conversão sem viés dos lançamentos de dado para bits (rejection sampling): necessário apenas
  se a entrada manual fosse usada crua. Como ela passa por SHA-256, o viés de mapeamento é
  irrelevante — complexidade sem ganho.
- Usar a entrada manual como passphrase da semente: não atende FR-004, pois a semente
  continuaria integralmente determinada pelo CSPRNG.

## R-002 — Formato da coleta de entropia manual

**Decision**: lançamentos de dado de 6 faces, digitados como dígitos `1`–`6`, mínimo de **100
lançamentos**. Progresso exibido como `floor(n × log2(6))` bits acumulados de 256.

**Rationale**: `log2(6) ≈ 2,585` bits por lançamento → 100 lançamentos ≈ 258 bits, cruzando o
mínimo de 256 exigido por FR-005 com margem mínima e um número redondo fácil de comunicar. Dado
de 6 faces é o instrumento físico mais disponível e é o padrão adotado por carteiras de
referência (Coldcard, Ian Coleman) para entropia manual.

**Alternatives considered**:

- Moeda (1 bit por lançamento): exigiria 256 lançamentos — inviável na prática.
- Dado de 20 faces: menos disponível, ganho marginal.
- Texto livre: impossível estimar bits com honestidade; convidaria o usuário a digitar uma frase
  memorável e superestimar a própria segurança.

## R-003 — Derivação BIP85 (aplicação BIP39)

**Decision**: caminho `m/83696968'/39'/0'/24'/{index}'`; entropia extraída como
`HMAC-SHA512(key = "bip-entropy-from-k", msg = chave_privada_derivada)` truncada aos **32
primeiros bytes**; esses 32 bytes viram o mnemônico de 24 palavras em inglês (`language = 0'`,
`words = 24'`).

**Rationale**: é literalmente o que a especificação BIP85 define para a aplicação BIP39. Existe
vetor de teste oficial que fecha o ciclo completo, permitindo validar a implementação
inteira de ponta a ponta (Princípio V):

| Campo | Valor |
|---|---|
| Path | `m/83696968'/39'/0'/24'/0'` |
| Entropia derivada | `ae131e2312cdc61331542efe0d1077bac5ea803adf24b313a4f0e48e9c51f37f` |
| Mnemônico | `puppy ocean match cereal symbol another shed magic wrap hammer bulb intact gadget divorce twin tonight reason outdoor destroy simple truth cigar social volcano` |

A master seed do vetor é a do BIP32 test vector padrão (`xprv9s21ZrQH143K2LBWUUQRFXhucrQqBpKdRRxNVq2zBqsx8HVqFxxuSFy…`, conforme o BIP85).

**Alternatives considered**: nenhuma — o padrão não admite variação. A única decisão de projeto
é o **espaço de índices**, tratado em R-004.

## R-004 — Composição dos participantes do multisig

> **Revisado em 2026-08-07.** A decisão anterior (derivar participantes novos via BIP85 em
> índices deslocados a partir de 1000) foi **descartada** pelo usuário. O texto abaixo reflete o
> comportamento vigente.

**Decision**: o esquema multisig não gera carteira alguma. Cada participante é uma das carteiras
derivadas já criadas na sessão, tomada com a sua **última** passphrase — ou sem passphrase,
quando a carteira não tiver nenhuma. O número de participantes é, por consequência, a quantidade
de carteiras derivadas; o usuário informa apenas M (assinaturas exigidas). O esquema só é
oferecido quando existem ao menos 2 carteiras.

**Rationale**: o backup que o usuário já vai guardar — as carteiras derivadas e suas passphrases
— passa a ser exatamente o que reconstrói o multisig. Não existe um segundo conjunto de 24
palavras por participante para transcrever, o que reduz pela metade o material sensível na tela
e elimina a chance de o usuário guardar as carteiras e esquecer os participantes (ou vice-versa).
Usar a última passphrase mantém uma regra única e previsível, sem introduzir uma pergunta de
seleção a cada carteira.

**Alternatives considered**:

- Derivar participantes próprios via BIP85 em índices deslocados (decisão original): dobrava o
  material a transcrever e criava chaves que não aparecem em nenhum outro lugar do relatório.
- Deixar o usuário escolher quais carteiras e qual passphrase de cada uma participam: multiplica
  as perguntas do fluxo por carteira, para um cenário já marcado como de minoria (US4/P4).
- Usar a conta sem passphrase de cada carteira: descartaria justamente a proteção que o usuário
  acabou de configurar.

## R-005 — Caminhos de derivação e tipo de endereço

**Decision**:

| Uso | Caminho da conta | Endereços | Tipo |
|---|---|---|---|
| Assinatura única | `m/84'/0'/0'` | `.../0/i`, i = 0…9 | P2WPKH (`bc1q…`) |
| Participante multisig | `m/48'/0'/0'/2'` | `.../0/i`, i = 0…9 | P2WSH (`bc1q…`, 32 bytes) |

**Rationale**: fixado pela clarificação (Native SegWit). `m/48'/…/2'` é o caminho que BIP48
reserva para P2WSH e é o que Sparrow, Electrum e Specter usam por padrão — condição necessária
para a verificação cruzada exigida por SC-003.

**Alternatives considered**: `m/45'` (multisig legacy, obsoleto) e Taproot `m/86'`, ambos
rejeitados na clarificação.

## R-006 — Serialização das chaves estendidas (zpub / Zpub)

**Decision**: implementar a serialização SLIP-132 trocando os 4 bytes de versão da serialização
BIP32 de 78 bytes e recodificando com `Encoders.Base58Check` da NBitcoin —
`zpub = 0x04B24746`, `Zpub = 0x02AA7ED3`.

**Rationale**: NBitcoin deliberadamente **não** suporta SLIP-132 (`zpub`/`Zpub`) — para a
biblioteca, um `zpub` é um `ExtPubKey` acrescido de informação sobre o tipo de script, que não
faz parte do BIP32. Como FR-022a exige essas serializações, a troca de version bytes é
inevitável. Ela não viola o Princípio I: trata-se de **codificação** (Base58Check da própria
NBitcoin sobre bytes reordenados), não de primitiva criptográfica — nenhum hash, HMAC ou
operação de curva é reimplementado.

**Alternatives considered**:

- Exibir apenas `xpub`: contraria FR-022a e obrigaria o usuário a converter manualmente antes de
  importar em uma carteira Native SegWit.
- Depender de `NBXplorer.Client` (que traz `DerivationSchemeParser`): adiciona uma dependência
  fora da stack fixa (Princípio II) para resolver um problema de 4 bytes.

## R-007 — Construção do endereço multisig

**Decision**: `PayToMultiSigTemplate.Instance.GenerateScriptPubKey(m, pubKeys)` com as chaves
**ordenadas lexicograficamente** (BIP67), e o endereço obtido por `redeemScript.WitHash.GetAddress(Network.Main)`.
O relatório exibe também o output descriptor `wsh(sortedmulti(m, [fingerprint/48h/0h/0h/2h]Zpub/0/*, …))`.

**Rationale**: `sortedmulti` (ordenação BIP67) é o padrão de Sparrow, Electrum e Specter — sem
ele os endereços não bateriam na verificação cruzada, ainda que o esquema fosse criptograficamente
válido. O descriptor é incluído porque, no multisig, a chave estendida sozinha **não** define o
esquema: sem M, sem o conjunto de cossignatários e sem a regra de ordenação, o usuário não
consegue reproduzir os endereços em outra carteira. É a menor adição que torna FR-019/FR-020
efetivamente verificáveis. Para assinatura única o `zpub` basta e nenhum descriptor é exibido.

**Alternatives considered**:

- `multi` sem ordenação: endereços dependeriam da ordem de digitação — irreproduzível na prática.
- Omitir o descriptor: tornaria a seção multisig impossível de importar sem trabalho manual do
  usuário, esvaziando o valor do relatório.

## R-008 — Estratégia de vetores de teste

**Decision**:

| Componente | Fonte do vetor | Cobertura |
|---|---|---|
| BIP39 (entropia → mnemônico → seed) | Vetores oficiais Trezor (`english.json`) | Completa |
| BIP32 (derivação) | Vetores oficiais do BIP32 | Completa |
| BIP84 (conta, zpub, endereços) | Vetores oficiais do BIP84 (mnemônico `abandon … about`) | Completa |
| BIP85 (aplicação BIP39, 24 palavras) | Vetor oficial do BIP85 (ver R-003) | Completa |
| Ordenação de chaves multisig | Vetores oficiais do BIP67 | Ordenação |
| Script P2WSH multisig | Vetores de descriptor do BIP381 (`wsh(multi(…))`) | scriptPubKey |
| Endereço multisig BIP48 fim-a-fim | **Não existe vetor oficial** | Verificação cruzada manual documentada no quickstart |

**Rationale**: BIP48 padroniza o caminho de derivação, mas não publica vetores de endereço. A
cobertura é obtida compondo vetores oficiais de cada peça (ordenação BIP67 + script BIP381 +
derivação BIP32/BIP84) e fechando a lacuna com uma verificação cruzada manual contra Sparrow,
registrada como passo obrigatório do quickstart. Isso mantém o espírito do Princípio V — nenhuma
peça é validada apenas contra a própria implementação.

**Alternatives considered**: gerar um "vetor" com a própria implementação e congelá-lo como
golden file — rejeitado: valida o código contra ele mesmo, exatamente o que o Princípio V proíbe.

## R-009 — Higiene de memória em .NET

**Decision**: material sensível trafega como `byte[]`/`char[]`, zerado com
`CryptographicOperations.ZeroMemory` em blocos `try/finally`; `Console.CancelKeyPress` e o
`catch` de topo disparam a mesma limpeza; passphrases lidas com `Console.ReadKey(intercept: true)`.

**Rationale**: é o máximo alcançável na plataforma. Limitação conhecida e aceita: `NBitcoin.Mnemonic`
mantém as palavras como `string` imutável internamente, fora do alcance do zeroing — o mesmo vale
para as strings que chegam à tela. A mitigação é minimizar cópias, não reter referências além do
necessário e documentar o resíduo. O Princípio VI exige zerar o que está sob nosso controle, não
prometer o impossível.

**Alternatives considered**:

- `SecureString`: desencorajado pela própria Microsoft, não é criptograficamente seguro em
  Linux/macOS e não resolveria as strings internas da NBitcoin.
- Reimplementar BIP39 sobre buffers zeráveis: violaria o Princípio I (não-negociável).

## R-010 — Passphrase vazia (pendência adiada pelo `/speckit.clarify`)

**Decision**: rejeitar. Ao confirmar uma passphrase vazia, o sistema informa que ela equivale à
carteira sem passphrase — já exibida — e solicita novamente.

**Rationale**: aceitar produziria duas seções idênticas no relatório, levando o usuário a crer
que possui uma carteira oculta adicional que na verdade não existe. Rejeitar é consistente com o
tratamento dado a passphrases duplicadas (FR-016).

**Alternatives considered**: aceitar com aviso — descartado por criar redundância silenciosa em
um relatório cujo único destino é a transcrição manual.

## R-011 — Detecção de saída redirecionada e avisos operacionais

**Decision**: `Console.IsOutputRedirected` verificado na inicialização; se verdadeiro, exibir o
aviso de FR-029 antes de qualquer geração. Aviso de boas práticas antes da coleta de entropia e
orientação de limpeza de tela ao final (FR-028).

**Rationale**: `Console.IsOutputRedirected` é a única verificação da BCL que cobre tanto `>` para
arquivo quanto `|` para outro processo, nos três sistemas operacionais alvo.

**Alternatives considered**: bloquear a execução quando redirecionada — rejeitado: impediria o
uso legítimo em ambientes de teste automatizado e a spec pede **alertar**, não impedir.
