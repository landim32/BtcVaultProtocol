# Feature Specification: Gerador de Carteiras Bitcoin (BIP39/BIP85) em Console

**Feature Branch**: `001-bitcoin-wallet-generator`  
**Created**: 2026-08-06  
**Status**: Draft  
**Input**: User description: "Crie um App simples, uma carteira de bitcoin, usando .NET: programa de console; capacidade de criar carteira Bitcoin usando BIP39; não deve armazenar nada, apenas gerar as chaves e exibir na tela; fluxo: opção para criar a entropia da forma mais segura possível; perguntar se deseja criar carteira mestre usando BIP85; se sim, quantas carteiras criar; em cada carteira perguntar quantas passphrases usar e solicitar preenchimento com confirmação; perguntar se deseja criar carteira Multisig e quantas assinaturas serão necessárias; ao final exibir: 24 palavras da carteira mestre BIP85, lista das carteiras geradas (24 palavras, 10 primeiros endereços derivados da BIP32 Extended Key sem passphrase, cada passphrase criada + 10 primeiros endereços), e dados da carteira Multisig (24 palavras e 10 primeiros endereços)."

## Clarifications

### Session 2026-08-06

- Q: Quais métodos de geração de entropia devem ser oferecidos? → A: Combinação sempre obrigatória — entropia física fornecida manualmente pelo usuário misturada ao CSPRNG do sistema operacional
- Q: De onde vêm as chaves participantes da carteira multisig? → A: Todas derivadas da carteira mestre via BIP85 (um único backup reconstrói o esquema inteiro; trade-off de segurança aceito explicitamente)
- Q: Qual padrão de endereço e caminho de derivação usar? → A: Native SegWit (bech32) — single-sig `m/84'/0'/0'`, multisig P2WSH `m/48'/0'/0'/2'`
- Q: Quais os limites máximos de volume? → A: 10 carteiras derivadas, 3 passphrases por carteira, 9 participantes multisig
- Q: Qual o idioma da interface do console? → A: Inglês, alinhado ao vocabulário das carteiras de referência

### Session 2026-08-07

- Q: O multisig deve gerar carteiras próprias? → A: Não. O esquema combina as carteiras **já criadas**, cada uma tomada com a sua **última passphrase** (ou sem passphrase, quando a carteira não tiver nenhuma)
- Q: Quem define o número de participantes? → A: Ninguém escolhe — N é sempre a quantidade de carteiras derivadas criadas. O usuário informa apenas M (mínimo de assinaturas)
- Q: A seção multisig deve repetir as 24 palavras de cada participante? → A: Não. São as mesmas palavras já exibidas na carteira derivada correspondente; a seção apenas referencia a carteira de origem

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Gerar uma carteira mestre com entropia controlada (Priority: P1)

Um usuário que deseja criar uma carteira Bitcoin fria abre o aplicativo em um terminal, fornece manualmente entropia física (por exemplo, lançamentos de dado) até atingir a força exigida, e o sistema a combina com o gerador criptográfico do sistema operacional. Em seguida recebe na tela a semente mnemônica de 24 palavras correspondente e os 10 primeiros endereços de recebimento derivados dela. Nada é gravado em disco: ao fechar o terminal, o resultado desaparece.

**Why this priority**: É o núcleo indispensável do produto. Sem geração de semente e exibição de endereços não existe entrega de valor alguma — todas as demais histórias são refinamentos sobre este resultado.

**Independent Test**: Executar o aplicativo, fornecer a entropia manual até completar a força exigida, confirmar a geração e verificar que 24 palavras válidas e 10 endereços são exibidos. A semente pode ser importada em uma carteira de referência de terceiros e os endereços devem coincidir exatamente.

**Acceptance Scenarios**:

1. **Given** o aplicativo iniciado em um terminal, **When** o usuário fornece entropia manual suficiente e confirma, **Then** o sistema combina essa entropia com a do gerador criptográfico do sistema e exibe uma semente mnemônica de 24 palavras válida (checksum correto) e os 10 primeiros endereços de recebimento derivados dela.
2. **Given** a semente exibida, **When** o usuário a importa em uma carteira de referência independente, **Then** os 10 primeiros endereços apresentados por essa carteira são idênticos, na mesma ordem, aos exibidos pelo aplicativo.
3. **Given** uma execução concluída, **When** o usuário inspeciona o sistema de arquivos e a área de transferência, **Then** nenhum arquivo, registro de log ou entrada de histórico contendo palavras, chaves ou passphrases foi criado ou modificado.
4. **Given** o aplicativo aguardando a entrada de entropia manual, **When** o usuário fornece dados inválidos ou interrompe antes de atingir os bits exigidos, **Then** o sistema informa quantos bits faltam em linguagem clara e continua solicitando, sem gerar carteira.

---

### User Story 2 - Derivar múltiplas carteiras filhas a partir da mestre (BIP85) (Priority: P2)

Após gerar a carteira mestre, o usuário indica que deseja usá-la como raiz determinística e informa quantas carteiras filhas quer produzir. O sistema deriva essa quantidade de carteiras independentes a partir da mestre, cada uma com sua própria semente de 24 palavras, e as apresenta numeradas ao final.

**Why this priority**: É o principal diferencial do produto frente a um gerador de semente comum — permite que o usuário mantenha um único backup (a mestre) e reconstrua todas as carteiras derivadas a partir dele. Depende da História 1, mas é testável isoladamente sobre ela.

**Independent Test**: A partir de uma carteira mestre já gerada, responder "sim" à derivação e informar uma quantidade N. Verificar que N carteiras distintas, cada uma com 24 palavras válidas, são exibidas, e que repetir o processo com a mesma mestre e o mesmo N reproduz exatamente as mesmas N sementes.

**Acceptance Scenarios**:

1. **Given** uma carteira mestre gerada, **When** o usuário confirma a derivação e informa a quantidade N de carteiras, **Then** o sistema exibe N carteiras, cada uma com sua semente de 24 palavras e seus 10 primeiros endereços de recebimento.
2. **Given** a mesma carteira mestre e a mesma quantidade N, **When** o processo é repetido em uma nova execução, **Then** as N sementes derivadas são idênticas às da execução anterior (derivação determinística).
3. **Given** a pergunta sobre derivação, **When** o usuário responde "não", **Then** o sistema segue o fluxo sem gerar carteiras filhas e o relatório final apresenta apenas a carteira mestre.
4. **Given** a pergunta sobre a quantidade de carteiras, **When** o usuário informa um valor não numérico, zero, negativo ou acima do limite suportado, **Then** o sistema rejeita a entrada com uma mensagem explicativa e pergunta novamente.

---

### User Story 3 - Proteger cada carteira com uma ou mais passphrases (Priority: P3)

Para cada carteira gerada, o usuário informa quantas passphrases deseja associar e digita cada uma delas com confirmação (digitação em duplicata). O relatório final mostra, para cada carteira, os endereços sem passphrase e, separadamente, cada passphrase informada acompanhada dos endereços da carteira oculta correspondente.

**Why this priority**: Adiciona a camada de negação plausível (carteiras ocultas). É valioso, mas a carteira já é utilizável sem ele — por isso vem depois da derivação.

**Independent Test**: Para uma carteira qualquer, informar 2 passphrases e digitá-las com confirmação. Verificar que o relatório final apresenta três conjuntos de endereços para essa carteira: um sem passphrase e um para cada passphrase, todos distintos entre si.

**Acceptance Scenarios**:

1. **Given** uma carteira em processamento, **When** o usuário informa que deseja K passphrases, **Then** o sistema solicita K passphrases, cada uma exigindo digitação e confirmação idêntica.
2. **Given** a solicitação de confirmação de uma passphrase, **When** a confirmação não coincide com a primeira digitação, **Then** o sistema informa a divergência e solicita a mesma passphrase novamente, sem avançar.
3. **Given** uma carteira com passphrases informadas, **When** o relatório final é exibido, **Then** para essa carteira aparecem os 10 primeiros endereços sem passphrase e, para cada passphrase, a passphrase em texto claro seguida dos 10 primeiros endereços resultantes.
4. **Given** a pergunta sobre a quantidade de passphrases, **When** o usuário informa zero, **Then** a carteira é apresentada apenas com os endereços sem passphrase, sem qualquer solicitação adicional.
5. **Given** a digitação de uma passphrase, **When** o usuário a digita, **Then** os caracteres não são ecoados na tela durante a digitação.

---

### User Story 4 - Criar uma carteira multisig (Priority: P4)

O usuário indica que deseja também uma configuração multisig e informa apenas quantas assinaturas serão necessárias. O sistema combina as carteiras derivadas que ele acabou de criar — cada uma com a sua última passphrase — e apresenta os 10 primeiros endereços multisig resultantes, junto do aviso de que todas compartilham a mesma raiz.

**Why this priority**: É um cenário avançado, usado por uma minoria dos usuários, e o produto entrega valor completo sem ele. Deve ser a última fatia implementada.

**Independent Test**: Criar 3 carteiras com passphrase, responder "sim" à pergunta de multisig, informar 2 assinaturas e verificar que os 3 participantes exibidos são exatamente as 3 carteiras criadas, cada uma com a sua última passphrase, e que os 10 endereços multisig reproduzem em uma carteira de referência.

**Acceptance Scenarios**:

1. **Given** o fluxo de multisig aceito com N carteiras derivadas criadas, **When** o usuário informa quantas assinaturas são necessárias, **Then** o sistema exibe os N participantes — cada um identificando a carteira derivada de origem, a passphrase usada e sua chave pública estendida, sem repetir as 24 palavras —, o aviso de raiz compartilhada e os 10 primeiros endereços multisig.
2. **Given** a pergunta de multisig, **When** o usuário responde "não", **Then** o relatório final é apresentado sem qualquer seção de multisig.
3. **Given** a configuração do esquema, **When** o usuário informa um número de assinaturas maior que o número de participantes, ou menor que 1, **Then** o sistema rejeita a combinação com mensagem explicativa e solicita novamente.
4. **Given** a carteira multisig exibida, **When** o usuário reconstrói o esquema em uma carteira multisig de referência com as mesmas chaves e o mesmo esquema, **Then** os 10 primeiros endereços coincidem exatamente.
5. **Given** a mesma carteira mestre, o mesmo número de carteiras e as mesmas passphrases, **When** o fluxo é repetido em nova execução, **Then** os mesmos participantes e os mesmos endereços multisig são reproduzidos.
6. **Given** menos de 2 carteiras derivadas criadas, **When** o fluxo chega à etapa de multisig, **Then** o sistema informa que o esquema exige ao menos 2 carteiras e segue para o relatório sem perguntar nada.
7. **Given** uma carteira derivada sem nenhuma passphrase, **When** ela participa do esquema, **Then** ela entra com a conta sem passphrase, e o relatório indica explicitamente `(none)` como passphrase usada.

---

### Edge Cases

- **Quantidades extremas**: o usuário solicita mais de 10 carteiras ou mais de 3 passphrases. O sistema deve rejeitar informando o limite, em vez de travar ou consumir memória indefinidamente.
- **Multisig sem carteiras suficientes**: o usuário recusa a derivação de carteiras, ou cria apenas uma. Como o esquema é montado sobre as carteiras existentes, não há multisig possível — o sistema informa e segue adiante.
- **Interrupção no meio do fluxo**: o usuário encerra a execução (fechamento do terminal ou interrupção por teclado) antes do relatório final. Nenhum dado parcial deve ter sido persistido em lugar algum.
- **Saída redirecionada**: a execução tem sua saída redirecionada para um arquivo ou pipe. O sistema deve alertar que isso persiste material sensível em disco, contrariando a premissa de não armazenamento.
- **Passphrase vazia**: o usuário confirma uma passphrase em branco. Deve ser tratada explicitamente — ou rejeitada, ou reconhecida como equivalente à carteira sem passphrase, com aviso.
- **Passphrases duplicadas na mesma carteira**: duas passphrases idênticas produzem a mesma carteira oculta. O sistema deve avisar que o resultado é redundante.
- **Caracteres não-ASCII em passphrase**: acentos, emoji ou espaços à esquerda/direita alteram a carteira resultante. O comportamento de normalização deve ser consistente com o padrão adotado por carteiras de referência, para que a recuperação em outro software funcione.
- **Terminal com histórico de rolagem**: o material sensível permanece visível no buffer do terminal após o encerramento. O sistema deve avisar o usuário para limpar o terminal.
- **Ausência de fonte de aleatoriedade adequada**: se o gerador criptográfico do sistema operacional estiver indisponível, o sistema deve abortar a geração com erro claro, jamais prosseguir apenas com a entropia manual nem recorrer silenciosamente a uma fonte mais fraca.
- **Entropia manual de baixa qualidade**: o usuário fornece uma sequência visivelmente degenerada (todos os valores iguais ou um padrão repetitivo). O sistema deve alertar, e a combinação com o gerador criptográfico deve garantir que a semente resultante permaneça imprevisível mesmo assim.
- **Volume de saída**: com muitas carteiras e passphrases, o relatório final é extenso. Deve ser dividido em seções com títulos e separadores visuais que permitam localizá-las na rolagem do terminal. Não há paginação interativa — ela impediria a transcrição contínua, e o volume já é limitado pelos tetos de FR-009, FR-012 e FR-018.

## Requirements *(mandatory)*

### Functional Requirements

**Fluxo e interação**

- **FR-001**: O sistema DEVE operar como uma aplicação de terminal interativa, conduzindo o usuário por uma sequência de perguntas em ordem fixa: entropia → derivação de carteiras → passphrases por carteira → multisig → relatório final.
- **FR-002**: O sistema DEVE validar toda entrada do usuário e, diante de uma entrada inválida, exibir uma mensagem explicativa e repetir a pergunta, sem encerrar a execução nem avançar no fluxo.
- **FR-003**: O sistema DEVE aceitar respostas afirmativas e negativas nas perguntas de sim/não de forma tolerante (maiúsculas/minúsculas), e DEVE indicar qual é a resposta padrão quando o usuário apenas confirma sem digitar nada.

**Entropia e geração da carteira mestre**

- **FR-004**: O sistema DEVE coletar entropia obrigatoriamente de duas fontes independentes e combiná-las para originar a semente mestre: (a) entropia física informada manualmente pelo usuário e (b) o gerador criptográfico de números aleatórios do sistema operacional. A combinação DEVE ser feita de modo que o comprometimento ou a previsibilidade de qualquer uma das fontes isoladamente não permita determinar a semente resultante. NÃO DEVE existir opção de pular a coleta manual.
- **FR-005**: O sistema DEVE guiar a coleta da entropia manual informando ao usuário o que digitar, e DEVE exibir continuamente quantos bits já foram acumulados e quantos ainda faltam, recusando-se a prosseguir enquanto a entropia manual acumulada for inferior a 256 bits.
- **FR-006**: O sistema DEVE gerar a semente mestre com 256 bits de entropia, resultando em uma semente mnemônica de 24 palavras com checksum válido segundo o padrão BIP39.
- **FR-007**: O sistema DEVE recusar-se a gerar qualquer semente caso o gerador criptográfico do sistema operacional esteja indisponível, jamais substituindo-o por uma alternativa mais fraca nem prosseguindo apenas com a entropia manual.

**Derivação de carteiras filhas**

- **FR-008**: O sistema DEVE perguntar ao usuário se deseja usar a carteira mestre como raiz determinística para derivar carteiras filhas, conforme o padrão BIP85.
- **FR-009**: Em caso afirmativo, o sistema DEVE perguntar quantas carteiras filhas devem ser derivadas e DEVE aceitar apenas inteiros de 1 a 10, informando o limite ao usuário na própria pergunta.
- **FR-010**: O sistema DEVE derivar cada carteira filha de forma determinística e sequencialmente indexada, de modo que a mesma carteira mestre sempre produza as mesmas carteiras filhas, na mesma ordem.
- **FR-011**: Cada carteira filha DEVE possuir sua própria semente mnemônica de 24 palavras, independente e utilizável em qualquer carteira compatível com o padrão.

**Passphrases**

- **FR-012**: Para cada carteira **derivada**, o sistema DEVE perguntar quantas passphrases o usuário deseja associar, aceitando apenas inteiros de 0 a 3 e informando o limite na própria pergunta. A carteira mestre NÃO recebe passphrases. Quando o usuário recusa a derivação de carteiras filhas, a etapa de passphrases é integralmente pulada.
- **FR-013**: O sistema DEVE solicitar cada passphrase duas vezes (digitação e confirmação) e só aceitá-la quando ambas coincidirem exatamente.
- **FR-014**: O sistema NÃO DEVE ecoar os caracteres da passphrase na tela durante a digitação.
- **FR-015**: O sistema DEVE tratar cada passphrase como definidora de uma carteira distinta, derivada da mesma semente, e DEVE apresentar seus endereços separadamente dos endereços sem passphrase.
- **FR-016**: O sistema DEVE avisar o usuário quando duas passphrases informadas para a mesma carteira forem idênticas, indicando que produzirão o mesmo resultado.

**Multisig**

- **FR-017**: O sistema DEVE perguntar se o usuário deseja criar uma configuração multisig.
- **FR-018**: Em caso afirmativo, o sistema DEVE perguntar **apenas** quantas assinaturas são necessárias para movimentar os fundos, aceitando inteiros de 1 até o número de participantes. O número de participantes NÃO é perguntado: ele é sempre a quantidade de carteiras derivadas criadas na sessão.
- **FR-019**: O sistema NÃO DEVE gerar carteiras novas para o multisig. Cada participante do esquema DEVE ser uma das carteiras derivadas já apresentadas no relatório, tomada com a sua **última passphrase**; quando a carteira não tiver passphrase alguma, ela participa sem passphrase. A conta de cada participante DEVE ser derivada dessa mesma semente no caminho multisig.
- **FR-019a**: O sistema NÃO DEVE aceitar chaves públicas estendidas externas de cossignatários. Todas as chaves do esquema vêm das carteiras geradas na própria sessão.
- **FR-019c**: O sistema DEVE oferecer a criação do esquema multisig apenas quando existirem pelo menos 2 carteiras derivadas. Com menos que isso, DEVE informar o motivo e seguir para o relatório sem perguntar.
- **FR-019b**: O sistema DEVE exibir, junto da seção multisig, um aviso explícito de que todas as chaves participantes derivam da mesma carteira mestre e que, portanto, quem possuir a semente mestre pode satisfazer sozinho o esquema — a proteção obtida é contra perda de backups individuais, não contra o comprometimento da mestre.

**Exibição de endereços e relatório final**

- **FR-020**: O sistema DEVE derivar e exibir os 10 primeiros endereços de recebimento de cada carteira apresentada, numerados e acompanhados do respectivo caminho de derivação completo.
- **FR-021**: O sistema DEVE exibir, junto de cada carteira, a chave pública estendida da conta correspondente, para permitir a importação como carteira somente-leitura.
- **FR-022**: O sistema DEVE gerar exclusivamente endereços Native SegWit (bech32, prefixo `bc1q`), sem opção de escolha pelo usuário, usando os seguintes caminhos de derivação: `m/84'/0'/0'/0/i` para carteiras de assinatura única e `m/48'/0'/0'/2'` (P2WSH) para as chaves participantes do esquema multisig, com endereços em `.../0/i`. O padrão adotado DEVE ser exibido explicitamente no relatório.
- **FR-022a**: As chaves públicas estendidas DEVEM ser apresentadas na serialização correspondente ao tipo de script: `zpub` para as contas de assinatura única e `Zpub` para as contas participantes do multisig, garantindo importação direta em carteiras de referência.
- **FR-023**: Ao final do fluxo, o sistema DEVE exibir um relatório consolidado contendo, nesta ordem: (a) as 24 palavras da carteira mestre; (b) para cada carteira derivada — suas 24 palavras, seu bloco de conta sem passphrase e, para cada passphrase informada, a passphrase em texto claro seguida do bloco de conta correspondente; (c) os dados da carteira multisig, quando criada — o esquema de assinaturas e, para cada participante, a identificação da carteira derivada de origem, a passphrase usada e sua chave pública estendida, seguidos do bloco de conta do esquema. Cada bloco de conta é composto conforme FR-020 e FR-021.
- **FR-024**: O relatório final DEVE ser estruturado em seções com títulos e separadores claros, permitindo ao usuário localizar visualmente cada carteira e cada conjunto de endereços.
- **FR-023a**: A seção multisig NÃO DEVE repetir as 24 palavras dos participantes: elas já constam da carteira derivada correspondente, exibida anteriormente no mesmo relatório. Cada participante DEVE referenciar sua carteira de origem pelo número, de modo que o usuário saiba a qual semente ele corresponde.
- **FR-024a**: Toda a interface — perguntas, mensagens de erro, avisos de segurança e títulos do relatório — DEVE ser apresentada em inglês, usando o vocabulário consagrado das carteiras de referência (`seed`, `passphrase`, `derivation path`, `extended public key`). Não há seleção de idioma nem tradução alternativa.

**Não persistência e segurança operacional**

- **FR-025**: O sistema NÃO DEVE gravar sementes, passphrases, chaves ou endereços em arquivos, bancos de dados, logs, variáveis de ambiente, área de transferência ou qualquer meio persistente — a única saída permitida é a exibição na tela.
- **FR-026**: O sistema NÃO DEVE realizar qualquer comunicação de rede durante a execução.
- **FR-027**: O sistema DEVE liberar da memória o material sensível ao término da execução, incluindo em caso de erro ou interrupção.
- **FR-028**: O sistema DEVE exibir, antes da geração, um aviso sobre boas práticas de uso (ambiente offline, terminal sem gravação de histórico) e, ao final, uma orientação para limpar a tela e o buffer de rolagem do terminal.
- **FR-029**: O sistema DEVE detectar quando sua saída não está sendo direcionada a um terminal interativo (redirecionamento para arquivo ou pipe) e alertar o usuário de que isso persiste material sensível.

### Key Entities

- **Entropia**: material aleatório bruto coletado para originar a carteira mestre. Atributos: método de origem, quantidade de bits coletados, força efetiva. Existe apenas em memória durante a geração.
- **Carteira Mestre**: raiz determinística da sessão. Atributos: semente mnemônica de 24 palavras, chave estendida raiz. Origina todas as carteiras derivadas.
- **Carteira Derivada**: carteira filha produzida deterministicamente a partir da mestre. Atributos: índice sequencial, semente mnemônica de 24 palavras, chave pública estendida da conta, lista de endereços de recebimento, lista de passphrases associadas.
- **Passphrase**: segredo textual adicional que, combinado a uma semente, define uma carteira distinta. Atributos: texto informado, carteira oculta resultante (chave pública estendida e endereços).
- **Endereço de Recebimento**: endereço público derivado de uma chave estendida. Atributos: índice, caminho de derivação, representação textual.
- **Configuração Multisig**: esquema de assinatura conjunta. Atributos: número de assinaturas exigidas, número de participantes, conjunto de participantes (cada um com semente e chave pública estendida), lista de endereços multisig resultantes.
- **Relatório de Sessão**: agregação de tudo o que foi gerado na execução, existente apenas como saída de tela.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Um usuário familiarizado com carteiras Bitcoin completa o fluxo padrão (1 carteira mestre, 3 carteiras derivadas, 1 passphrase cada, sem multisig) em menos de 5 minutos, sem consultar documentação externa.
- **SC-002**: 100% das sementes mnemônicas geradas são aceitas como válidas ao serem importadas em pelo menos duas carteiras de referência independentes e amplamente utilizadas.
- **SC-003**: Para qualquer semente e passphrase gerada, os 10 endereços exibidos coincidem, em conteúdo e ordem, com os apresentados por uma carteira de referência independente, em 100% dos casos verificados.
- **SC-004**: Após uma execução completa, uma auditoria do sistema de arquivos, dos logs do sistema, das variáveis de ambiente e da área de transferência não localiza nenhum fragmento de semente, passphrase ou chave — zero ocorrências.
- **SC-005**: Em 1.000 execuções independentes, nenhuma semente mestre se repete e nenhuma sequência previsível é identificável por análise estatística padrão de aleatoriedade — inclusive quando a mesma entropia manual degenerada é fornecida em todas as execuções.
- **SC-006**: Executar o fluxo duas vezes a partir da mesma carteira mestre e das mesmas respostas produz exatamente as mesmas carteiras derivadas e os mesmos endereços, em 100% das repetições.
- **SC-007**: 100% das entradas inválidas testadas (texto onde se espera número, zero, negativos, valores acima do limite, confirmação de passphrase divergente, esquema multisig impossível) resultam em mensagem explicativa e nova solicitação, sem encerramento abrupto e sem geração de dados incorretos.
- **SC-008**: O relatório final de uma sessão com 5 carteiras e 2 passphrases cada permite ao usuário localizar qualquer carteira ou conjunto de endereços específico em menos de 15 segundos de leitura.

## Assumptions

- **Público-alvo**: usuários com conhecimento prévio de carteiras Bitcoin, sementes mnemônicas e passphrases. O aplicativo não ensina os conceitos; assume familiaridade.
- **Ambiente de uso**: máquina pessoal do usuário, preferencialmente offline. O aplicativo é operado diretamente por quem receberá as chaves — não há múltiplos usuários, autenticação ou controle de acesso.
- **Rede Bitcoin**: todos os endereços são gerados para a rede principal (mainnet). Redes de teste estão fora de escopo.
- **Tipo de endereço fixo**: Native SegWit para toda a sessão, sem opção de escolha. Legacy e Taproot estão fora de escopo — Taproot foi rejeitado por ter suporte multisig ainda irregular entre as carteiras usadas para verificação cruzada.
- **Endereços de troco**: apenas a cadeia de recebimento (`.../0/i`) é exibida. A cadeia de troco (`.../1/i`) é derivável pela carteira que importar a chave estendida e não é apresentada no relatório.
- **Tamanho da semente**: fixado em 24 palavras (256 bits de entropia) em todo o fluxo, incluindo carteiras derivadas e participantes multisig, conforme descrito pelo usuário. Não há opção de 12 palavras.
- **Sem armazenamento e sem rede**: o aplicativo não persiste nada e não consulta saldos, taxas ou o estado da blockchain. Consequentemente, não valida se os endereços gerados já possuem histórico de uso.
- **Sem exportação**: não há geração de arquivo de backup, QR code em imagem, PDF ou descritor exportável em arquivo. A única saída é a tela.
- **Escopo de operação**: o aplicativo apenas gera e exibe chaves. Não assina transações, não constrói PSBTs e não interage com dispositivos de hardware.
- **Modelo de confiança do multisig**: o esquema é montado sobre as carteiras derivadas da própria sessão, todas oriundas da mesma mestre via BIP85. Ele protege contra a **perda** de backups individuais (M de N bastam para gastar), mas não contra o **comprometimento** da mestre — quem a possuir satisfaz o esquema sozinho. Multisig com cossignatários independentes está fora de escopo.
- **Composição do multisig**: o número de participantes é sempre igual ao número de carteiras derivadas criadas, e cada participante entra com a sua **última** passphrase. Não há seleção de quais carteiras participam nem de qual passphrase usar — se o usuário quiser um esquema diferente, ele muda quantas carteiras cria e quais passphrases informa.
- **Passphrases em texto claro**: as passphrases são exibidas em texto claro no relatório final, conforme solicitado pelo usuário, para que possam ser anotadas junto das sementes correspondentes.
- **Idioma**: toda a interface é em inglês, e as sementes mnemônicas usam a lista de palavras em inglês — padrão de interoperabilidade entre carteiras. Não há suporte a outros idiomas.
- **Limite de volume**: máximo de 10 carteiras derivadas e 3 passphrases por carteira. O multisig não tem limite próprio: seu número de participantes é a quantidade de carteiras criadas, logo no máximo 10. Os limites existem para manter o relatório legível e transcritível à mão dentro da própria sessão — como nada é salvo, uma saída maior do que o usuário consegue conferir não tem utilidade.
- **Restrição de plataforma**: a plataforma de execução indicada pelo usuário é uma decisão de implementação e será registrada no plano técnico (`plan.md`), não nesta especificação.
