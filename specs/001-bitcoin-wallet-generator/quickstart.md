# Phase 1 — Quickstart: BtcVaultProtocol Wallet

**Feature**: `001-bitcoin-wallet-generator` | **Date**: 2026-08-06

Guia para construir, executar e — o passo que realmente importa — **verificar** que as chaves
geradas estão corretas.

---

## Pré-requisitos

- .NET SDK 8.0 (LTS)
- Um dado de 6 faces
- Para a verificação cruzada: [Sparrow Wallet](https://sparrowwallet.com) ou Electrum, em modo
  offline

Nenhum banco de dados, nenhum contêiner, nenhuma variável de ambiente. Docker não é usado e não
está disponível no ambiente local.

## Build e testes

```powershell
dotnet restore
dotnet build --configuration Release
dotnet test
```

`dotnet test` deve terminar com **zero falhas** antes de qualquer execução real. Os testes rodam
inteiramente offline: os vetores oficiais estão embutidos como constantes no projeto de testes,
não são baixados nem lidos de arquivo.

## Execução

```powershell
dotnet run --project src/BtcVaultProtocol.Wallet.Console --configuration Release
```

Fluxo típico (3 carteiras, 1 passphrase cada, multisig 2-de-3):

1. Leia o aviso de segurança.
2. Role o dado 100 vezes e digite os resultados até o contador atingir 256 bits.
3. `Y` para derivar carteiras → `3`.
4. Para cada carteira: `1` passphrase, digitada duas vezes.
5. `y` para multisig → `3` participantes → `2` assinaturas.
6. Transcreva o relatório.
7. Limpe a tela e o buffer de rolagem conforme a última linha instrui.

**Atenção**: rodar com a saída redirecionada (`> arquivo.txt` ou `| more`) grava o material
sensível em disco. O aplicativo avisa, mas não impede.

---

## Verificação cruzada (obrigatória)

Os testes automatizados cobrem cada derivação contra vetores oficiais, mas **não existe vetor
oficial publicado para o endereço multisig BIP48 fim-a-fim** (ver `research.md` R-008). Esta
verificação manual fecha essa lacuna e deve ser executada a cada release.

### V-1 — Carteira de assinatura única

1. Gere uma sessão e anote as 24 palavras de uma carteira derivada e seus 10 endereços.
2. No Sparrow (offline): *File → New Wallet → Software Wallet → Enter 24 words*.
3. Script type: **Native SegWit (P2WPKH)**, derivation `m/84'/0'/0'`.
4. Compare os 10 primeiros endereços de recebimento — devem coincidir em conteúdo **e ordem**.
5. Compare o `zpub` exibido pelo Sparrow com o do relatório.
6. Repita os passos 2–5 em uma **segunda** carteira de referência independente (Electrum, se a
   primeira foi Sparrow). SC-002 exige validação em duas carteiras distintas — uma só não
   distingue um erro do app de uma peculiaridade da carteira usada na conferência.

### V-2 — Carteira com passphrase

Repita V-1 informando a passphrase no Sparrow. Os endereços devem bater com o bloco
`[passphrase N]` correspondente — e ser **diferentes** dos do bloco `[no passphrase]`.

### V-3 — Multisig

Lembre-se: os participantes **são** as carteiras derivadas já exibidas, cada uma com a sua última
passphrase — não há sementes novas a importar.

1. No Sparrow: *File → New Wallet → Multi Signature*, política 2-de-3, script type **P2WSH**.
2. Importe cada participante pelas 24 palavras da carteira derivada correspondente, informando a
   passphrase indicada na linha `Passphrase :` daquele participante, derivação `m/48'/0'/0'/2'`.
3. Compare os `Zpub` de cada cossignatário com os do relatório.
4. Compare os 10 endereços multisig.
5. Alternativa mais rápida: cole o descriptor `wsh(sortedmulti(...))` do relatório em
   *File → New Wallet → Import → Output Descriptor* e compare os endereços.

Se qualquer endereço divergir, **é defeito do código** — nunca ajuste o valor esperado para
acomodar a saída (Princípio V).

### V-4 — Determinismo

Executar duas vezes com os mesmos lançamentos de dado **não** reproduz a mesma carteira: o CSPRNG
muda a cada execução, e esse é o comportamento correto. O determinismo que importa é o interno —
mesma carteira mestre ⇒ mesmas carteiras BIP85 e mesmos endereços —, verificado pelos testes
automatizados, que partem de uma seed mestre fixa.

### V-5 — Não persistência

Após uma execução completa, em uma máquina de teste:

```powershell
# nenhum arquivo novo ou modificado no perfil do usuário durante a execução
Get-ChildItem $HOME -Recurse -File -ErrorAction SilentlyContinue |
  Where-Object { $_.LastWriteTime -gt (Get-Date).AddMinutes(-10) }
```

Nenhum resultado deve conter fragmentos de semente ou passphrase. Verifique também que a área de
transferência não foi alterada e que o histórico do shell não registra nada além do comando de
execução.

---

### V-6 — Desempenho do cenário máximo

Execute com 10 carteiras, 3 passphrases cada e multisig 9-de-9 (41 contas, 410 endereços). Do
fim da última resposta até o relatório completo devem passar menos de 5 s.

Medição manual, por observação — deliberadamente fora da suíte de testes: o Princípio V proíbe
testes que dependam do relógio do sistema.

---

## Solução de problemas

| Sintoma | Causa provável |
|---|---|
| `FATAL: the operating system CSPRNG is unavailable` (exit 2) | Ambiente sem fonte de entropia do SO. Não há contorno — por decisão de projeto o app não prossegue apenas com a entropia manual. |
| Endereços não batem com o Sparrow | Confira o script type (**Native SegWit**, não Legacy/Taproot) e a derivação (`m/84'/0'/0'`). |
| Multisig não bate | Confira se a política é P2WSH e se a carteira usa ordenação BIP67 (`sortedmulti`) — é o padrão do Sparrow. |
| Passphrase não reproduz a carteira | Espaços à esquerda/direita são significativos. A passphrase deve ser transcrita exatamente como aparece no relatório. |
