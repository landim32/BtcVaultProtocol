<!--
SYNC IMPACT REPORT — 2.0.0 → 2.0.1 (mais recente)
==================================================
Version change: 2.0.0 → 2.0.1
Bump rationale: PATCH — esclarecimento sem mudança semântica. O Princípio II listava "xUnit" como
única dependência de teste, mas executar xUnit exige `Microsoft.NET.Test.Sdk` e
`xunit.runner.visualstudio`. A leitura literal tornava a tarefa de setup dos testes uma violação.
A emenda nomeia esses pacotes como infraestrutura de execução e registra que eles não são
dependência de produção, por não serem referenciados por `Core` nem por `Console`.

Princípios modificados: II. Stack Tecnológica Fixa e Mínima (tabela + primeira regra vinculante).
Nenhum outro princípio alterado. Nenhuma seção adicionada ou removida.

Templates atualizados:
- ✅ .specify/templates/plan-template.md (gate "Stack | II")
- ✅ .specify/templates/tasks-template.md (tarefa de setup T002)
- ✅ CLAUDE.md (lista de pacotes permitidos)
- ✅ .specify/templates/spec-template.md, checklist-template.md, agent-file-template.md
     (validados — sem referência à stack de pacotes)

Follow-up TODOs: nenhum.


SYNC IMPACT REPORT — 1.0.0 → 2.0.0 (histórico)
===============================================
Version change: 1.0.0 → 2.0.0
Bump rationale: MAJOR — redefinição incompatível de governança. O projeto foi redirecionado de
uma API web (ASP.NET + EF Core + PostgreSQL + NAuth) para um aplicativo de console offline de
geração de carteiras Bitcoin. Quatro dos cinco princípios anteriores deixaram de ter objeto e
foram removidos ou substituídos; a stack fixa foi integralmente trocada.

Princípios (1.0.0 → 2.0.0):
- I. Skills Obrigatórias → REMOVIDO (a skill `dotnet-architecture` cobre Clean Architecture
     backend com EF Core e repositórios — sem objeto num console sem persistência)
- II. Stack Tecnológica Fixa → II. Stack Tecnológica Fixa e Mínima (reescrito: NBitcoin + BCL;
     EF Core, PostgreSQL, NAuth, zTools e Swashbuckle removidos)
- III. Convenções de Código .NET → IV. Convenções de Código .NET (mantido; regra de
     `[JsonPropertyName]` removida por ausência de contrato JSON público)
- IV. Convenções de Banco de Dados (PostgreSQL) → REMOVIDO (não há banco de dados)
- V. Autenticação e Segurança → III. Zero Persistência, Zero Rede (NÃO-NEGOCIÁVEL) (substituído:
     o modelo de ameaça deixou de ser "endpoint desprotegido" e passou a ser "vazamento de chave")

Princípios adicionados:
- I. Primitivas Criptográficas Auditadas (NÃO-NEGOCIÁVEL)
- V. Verificabilidade por Vetores de Teste Oficiais (NÃO-NEGOCIÁVEL)
- VI. Higiene de Memória e de Saída

Seções:
- Restrições Adicionais: reescrita (variáveis de ambiente e try/catch de controller → estrutura
  de solução, tratamento de erros de console, distribuição offline)
- Fluxo de Desenvolvimento: reescrito com o novo checklist
- Governance: mantido (procedimento de emenda, versionamento semântico, conformidade)

Templates atualizados:
- ✅ .specify/templates/plan-template.md (Technical Context com a nova stack; Constitution Check
     com os seis gates concretos; estrutura de projeto console/core/tests)
- ✅ .specify/templates/tasks-template.md (Path Conventions por projeto; Setup/Foundational
     alinhados a NBitcoin + vetores de teste + fluxo de console)
- ✅ .specify/templates/spec-template.md (validado — agnóstico de tecnologia por design, sem
     alteração necessária)
- ✅ .specify/templates/checklist-template.md (validado — sem referências desatualizadas)
- ✅ .specify/templates/agent-file-template.md (validado — sem referências desatualizadas)

Follow-up TODOs: nenhum. Todos os placeholders foram resolvidos.
-->

# BtcVaultProtocol Constitution

BtcVaultProtocol é um aplicativo de console offline que gera carteiras Bitcoin (BIP39, BIP32, BIP85 e
multisig) e as exibe na tela, sem armazenar nada. Todo o conteúdo desta constituição parte dessa
premissa: o produto manipula material que dá controle irrevogável sobre fundos, e um único
vazamento ou erro de derivação é irreparável.

## Core Principles

### I. Primitivas Criptográficas Auditadas (NÃO-NEGOCIÁVEL)

Primitivas criptográficas NÃO DEVEM ser implementadas manualmente. Toda operação de hash, HMAC,
aritmética de curva elíptica, codificação de endereço e derivação BIP32/BIP39 DEVE usar a
biblioteca `NBitcoin` ou a API criptográfica da BCL (`System.Security.Cryptography`).

Exceção controlada — BIP85: por não existir implementação em `NBitcoin`, a derivação BIP85 PODE
ser composta no projeto, desde que atenda cumulativamente a:

- Usar exclusivamente primitivas de `NBitcoin`/BCL (derivação BIP32 endurecida + HMAC-SHA512);
  nenhuma primitiva nova pode ser escrita.
- Ser validada contra os vetores de teste oficiais do BIP85 antes de ser considerada pronta
  (Princípio V).
- Ficar isolada em um único componente, com o caminho de derivação (`m/83696968'/…`) declarado
  em código de forma explícita e comentada.

Aleatoriedade DEVE vir de `RandomNumberGenerator` (CSPRNG do sistema operacional).
`System.Random` NÃO DEVE ser usado em nenhum caminho que influencie material de chave, nem
mesmo para embaralhamento, preenchimento ou ordenação.

**Rationale**: criptografia escrita à mão falha silenciosamente — o programa produz uma chave
com aparência perfeitamente válida e o erro só se manifesta quando os fundos se tornam
irrecuperáveis. Delegar a primitiva a código auditado por milhares de usuários elimina a classe
inteira de defeitos mais cara do domínio.

### II. Stack Tecnológica Fixa e Mínima

A stack é fechada e NÃO DEVE ser estendida sem emenda formal a esta constituição:

| Tecnologia | Versão | Finalidade |
|---|---|---|
| .NET | 8.0 (LTS) | Runtime e framework |
| Aplicação | Console (`Microsoft.NET.Sdk`) | Único tipo de executável |
| NBitcoin | Latest | Primitivas Bitcoin: BIP39, BIP32, endereços, multisig |
| xUnit | Latest | Testes automatizados |
| `Microsoft.NET.Test.Sdk` + `xunit.runner.visualstudio` | Latest | Infraestrutura de execução do xUnit |

Regras vinculantes:

- Toda a interface com o usuário DEVE ser construída sobre `System.Console` da BCL. Bibliotecas
  de UI de terminal (Spectre.Console, Terminal.Gui e equivalentes) NÃO DEVEM ser adicionadas.
- Nenhum pacote NuGet além dos listados acima DEVE ser referenciado pelos projetos de produção.
  Uma dependência nova exige emenda, com justificativa do benefício frente ao risco de supply
  chain.
- A infraestrutura de execução de testes acompanha o xUnit e NÃO conta como dependência de
  produção: ela é referenciada exclusivamente pelo projeto de testes, nunca por `Core` ou
  `Console`. Qualquer outro pacote no projeto de testes — bibliotecas de asserção, mocking ou
  geração de dados — continua exigindo emenda.
- ORMs, drivers de banco, clientes HTTP, provedores de log com destino externo e SDKs de
  telemetria NÃO DEVEM ser referenciados — não há o que persistir nem para onde transmitir.
- Comandos `docker` ou `docker compose` NÃO DEVEM ser executados no ambiente local — Docker não
  está acessível. Planos e tarefas que dependam de Docker local são inválidos.

**Rationale**: cada dependência de um software que manipula chaves privadas é um vetor de
comprometimento que o usuário não tem como auditar. Uma superfície mínima é, aqui, um requisito
de segurança — não uma preferência de estilo.

### III. Zero Persistência, Zero Rede (NÃO-NEGOCIÁVEL)

O aplicativo NÃO DEVE persistir nem transmitir material sensível — sementes, entropia,
passphrases, chaves privadas, chaves estendidas e endereços gerados.

Regras vinculantes:

- Nenhuma escrita em arquivo, banco, log, variável de ambiente, registro do sistema ou área de
  transferência. A **única** saída permitida para material sensível é `stdout` do terminal.
- Nenhuma chamada de rede, de qualquer natureza, em qualquer momento da execução — incluindo
  verificação de atualização, telemetria e consulta a saldos, taxas ou estado da blockchain.
- Nenhum framework de logging com sink de arquivo ou rede DEVE ser configurado. Diagnóstico
  ocorre por mensagem em tela, e mensagens de diagnóstico NÃO DEVEM conter material sensível.
- O aplicativo DEVE detectar quando `stdout` não é um terminal interativo (redirecionamento
  para arquivo ou pipe) e alertar o usuário de que isso persiste material sensível em disco.

**Rationale**: a promessa central do produto é que, ao fechar o terminal, o segredo deixa de
existir na máquina. Qualquer gravação — inclusive uma linha de log de depuração esquecida —
quebra essa promessa de forma invisível para o usuário e permanente no disco.

### IV. Convenções de Código .NET

Todo código C# DEVE seguir as convenções abaixo:

| Elemento | Convenção | Exemplo |
|---|---|---|
| Namespaces | PascalCase, file-scoped | `namespace BtcVaultProtocol.Wallet.Core;` |
| Classes / Interfaces | PascalCase | `WalletGenerator`, `IEntropySource` |
| Métodos | PascalCase | `Derive()`, `GenerateMnemonic()` |
| Propriedades | PascalCase | `AccountIndex`, `DerivationPath` |
| Campos privados | _camelCase | `_entropySource`, `_network` |
| Constantes | UPPER_CASE | `MNEMONIC_WORD_COUNT` |

Regras adicionais:

- Nullable reference types DEVEM estar habilitados (`<Nullable>enable</Nullable>`) e warnings
  DEVEM ser tratados como erros (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`).
- Tipos que representam resultados de geração DEVEM ser imutáveis (`record` ou propriedades
  somente-leitura). Material de chave NÃO DEVE ser mutável após a criação.
- A camada de domínio (`*.Core`) NÃO DEVE conter chamadas a `System.Console`. Entrada e saída
  pertencem exclusivamente ao projeto de console.
- Toda constante numérica de domínio (contagem de palavras, bits de entropia, índices de
  derivação, quantidade de endereços exibidos) DEVE ser nomeada, nunca escrita literalmente no
  meio da lógica.

**Rationale**: nomeação previsível torna o código navegável por busca textual e elimina
discussões de estilo em revisão. A separação estrita entre domínio e console é o que permite
testar a derivação contra vetores oficiais sem simular um terminal.

### V. Verificabilidade por Vetores de Teste Oficiais (NÃO-NEGOCIÁVEL)

Toda funcionalidade de derivação DEVE ser coberta por testes automatizados executados contra os
vetores de teste oficiais dos padrões correspondentes (BIP39, BIP32, BIP85 e, quando aplicável,
BIP48/BIP84 para multisig e endereços).

Regras vinculantes:

- Uma função de derivação sem teste de vetor oficial NÃO DEVE ser considerada concluída.
- A derivação DEVE ser determinística: as mesmas entradas produzem sempre a mesma saída. Testes
  DEVEM afirmar essa reprodutibilidade explicitamente.
- Testes NÃO DEVEM depender de rede, de arquivos externos ou de relógio do sistema. Os vetores
  DEVEM estar embutidos no projeto de testes.
- Um valor esperado de teste NUNCA DEVE ser ajustado para acomodar a saída do código. Divergência
  em relação ao vetor oficial é defeito do código, sem exceção.

**Rationale**: um endereço errado é indistinguível de um endereço certo a olho nu, e o custo do
erro é a perda total dos fundos enviados a ele. O vetor oficial é a única evidência objetiva de
correção que existe neste domínio — e a única forma de garantir que o usuário conseguirá
recuperar a carteira em outro software.

### VI. Higiene de Memória e de Saída

O aplicativo DEVE minimizar a permanência de material sensível na memória e o risco de sua
captura acidental na tela.

Regras vinculantes:

- Passphrases DEVEM ser lidas sem eco de caracteres no terminal.
- Buffers contendo entropia, sementes e passphrases DEVEM ser sobrescritos
  (`CryptographicOperations.ZeroMemory` ou equivalente) assim que deixarem de ser necessários,
  inclusive nos caminhos de erro e de interrupção.
- Material sensível NÃO DEVE ser interpolado em mensagens de exceção nem em qualquer texto que
  possa escapar por um `catch` genérico ou pelo stack trace.
- O aplicativo DEVE exibir, antes da geração, um aviso de boas práticas (execução offline,
  ambiente confiável) e, ao final, orientação para limpar a tela e o buffer de rolagem do
  terminal.

**Rationale**: mesmo sem gravar em disco, o segredo sobrevive no heap, no buffer de rolagem do
terminal e — no pior caso — no arquivo de swap. Reduzir a janela de exposição é a única defesa
disponível contra um comprometimento que ocorra depois da geração.

## Restrições Adicionais

### Estrutura da Solução

A solução DEVE ter exatamente três projetos, com o fluxo de dependência abaixo:

```text
src/BtcVaultProtocol.Wallet.Core/       # Domínio: entropia, mnemônicos, derivação, multisig
src/BtcVaultProtocol.Wallet.Console/    # Interação: prompts, validação de entrada, relatório
tests/BtcVaultProtocol.Wallet.Tests/    # xUnit: vetores oficiais + regras de fluxo
```

- `Console` referencia `Core`. `Core` NÃO DEVE referenciar `Console`.
- `Tests` referencia ambos.
- Projetos adicionais exigem emenda a esta constituição.

### Tratamento de Erros

O aplicativo é interativo e conduzido por perguntas. Portanto:

- Entrada inválida do usuário DEVE produzir uma mensagem explicativa e a repetição da pergunta —
  nunca o encerramento do programa nem uma exceção não tratada.
- Exceções inesperadas DEVEM ser capturadas no ponto de entrada, resultando em uma mensagem
  genérica, limpeza do material sensível em memória e código de saída diferente de zero.
- Mensagens de erro NÃO DEVEM conter caminhos internos, stack traces ou qualquer fragmento de
  material sensível.

### Distribuição e Execução

- O aplicativo DEVE ser executável em uma máquina sem conexão de rede, sem nenhuma degradação de
  funcionalidade.
- A rede Bitcoin alvo é mainnet. Suporte a testnet/regtest exige emenda.
- Nenhuma configuração sensível existe — não há arquivos de configuração, variáveis de ambiente
  obrigatórias nem secrets a gerenciar.

## Fluxo de Desenvolvimento

Antes de submeter qualquer código, o contribuidor DEVE verificar:

- [ ] Nenhuma primitiva criptográfica foi escrita à mão (Princípio I).
- [ ] Nenhuma fonte de aleatoriedade além do CSPRNG influencia material de chave.
- [ ] Nenhum pacote NuGet fora da stack fixa foi adicionado (Princípio II).
- [ ] Nenhuma escrita em arquivo, log, clipboard ou rede foi introduzida (Princípio III).
- [ ] `BtcVaultProtocol.Wallet.Core` permanece livre de `System.Console`.
- [ ] Toda derivação nova possui teste contra vetor oficial e passa (Princípio V).
- [ ] Buffers sensíveis são zerados, inclusive nos caminhos de erro (Princípio VI).
- [ ] Passphrases não são ecoadas no terminal.
- [ ] Nenhuma tarefa depende de execução de Docker no ambiente local.

## Governance

Esta constituição supersede qualquer outra prática, convenção informal ou preferência
individual. Em caso de conflito entre esta constituição e outro documento do repositório, esta
constituição prevalece.

**Emendas**: alterações DEVEM ser propostas por escrito, documentando o princípio afetado, a
justificativa e o plano de migração do código existente. Uma emenda só entra em vigor após
atualização deste arquivo e propagação para os templates dependentes em `.specify/templates/`.

**Versionamento** (semântico):

- **MAJOR**: remoção ou redefinição incompatível de princípio ou regra de governança.
- **MINOR**: novo princípio ou seção adicionada, ou expansão material de orientação.
- **PATCH**: esclarecimentos, correções de redação, refinamentos não semânticos.

**Conformidade**: toda revisão de código DEVE verificar aderência ao checklist do Fluxo de
Desenvolvimento. Violações justificadas DEVEM ser registradas na seção Complexity Tracking do
`plan.md` da feature correspondente, com a alternativa mais simples que foi rejeitada e o
motivo. Complexidade não justificada é motivo suficiente para rejeitar a mudança. Violações dos
Princípios I, III e V NÃO DEVEM ser justificadas em Complexity Tracking — são bloqueantes
absolutos e exigem emenda formal.

**Version**: 2.0.1 | **Ratified**: 2026-04-02 | **Last Amended**: 2026-08-07
