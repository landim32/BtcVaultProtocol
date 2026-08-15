# Phase 1 — CLI Contract: BtcVaultProtocol Wallet

**Feature**: `001-bitcoin-wallet-generator` | **Date**: 2026-08-06

Este é o contrato externo do produto: a superfície que o usuário vê e contra a qual os testes de
fluxo são escritos. Toda a interface é em inglês (FR-024a). O aplicativo não aceita argumentos
de linha de comando, não lê variáveis de ambiente e não possui arquivos de configuração — a
única entrada é o teclado, a única saída é `stdout`.

**Convenção de leitura**: `>` marca a linha digitada pelo usuário. Reticências indicam repetição.

---

## 1. Invocação

```text
dotnet run --project src/BtcVaultProtocol.Wallet.Console
```

Sem flags, sem subcomandos. Códigos de saída:

| Código | Significado |
|---|---|
| `0` | Fluxo concluído e relatório exibido |
| `1` | Interrompido pelo usuário (`Ctrl+C`) ou erro inesperado — buffers zerados antes de sair |
| `2` | CSPRNG do sistema indisponível — nada foi gerado (FR-007) |

## 2. Abertura

Exibida antes de qualquer coleta.

```text
=== BtcVaultProtocol Wallet — offline Bitcoin key generator ===

WARNING: this tool prints private key material to the screen.
  - Run it on an offline machine you trust.
  - Make sure your terminal does not record or stream its output.
  - Nothing is saved: when this window closes, everything is gone.
```

Quando `Console.IsOutputRedirected` é verdadeiro, uma linha adicional precede o aviso (FR-029):

```text
WARNING: output is being redirected to a file or pipe.
         Sensitive material WILL be written to disk. Press Ctrl+C to abort.
```

## 3. Coleta de entropia (FR-004, FR-005)

```text
--- Step 1 of 4: entropy ---

Entropy comes from two mandatory sources: your dice rolls and the operating
system CSPRNG. Both are combined; neither can be skipped.

Roll a 6-sided die and type each result (1-6). Press Enter when finished.

Progress: 0 / 256 bits (0 rolls)
> 4 1 6 2 5 3 ...
Progress: 258 / 256 bits (100 rolls) — minimum reached
> [Enter]
```

Contrato de comportamento:

| Situação | Resposta |
|---|---|
| Caractere fora de `1`–`6` | `Invalid input: only digits 1-6 are accepted.` e repete o prompt |
| `Enter` antes de 256 bits | `Not enough entropy yet: 129 more bits needed (50 more rolls).` e continua |
| Sequência degenerada (todos iguais) | `WARNING: your rolls look non-random. The CSPRNG still protects this seed.` e continua |
| CSPRNG indisponível na mistura | `FATAL: the operating system CSPRNG is unavailable. No wallet was generated.` → exit `2` |

## 4. Carteiras derivadas (FR-008, FR-009)

```text
--- Step 2 of 4: derived wallets (BIP85) ---

Derive child wallets from this master seed? [Y/n]
> y
How many wallets? (1-10)
> 3
```

| Situação | Resposta |
|---|---|
| Resposta vazia | Assume o padrão indicado em maiúscula (`Y`) |
| Resposta fora de `y`/`yes`/`n`/`no` (case-insensitive) | `Please answer y or n.` e repete |
| Valor não numérico, `0`, negativo ou `> 10` | `Please enter a whole number between 1 and 10.` e repete |
| Resposta `n` | Pula direto para a Etapa 4 (multisig) |

## 5. Passphrases (FR-012 … FR-016, R-010)

Repetido para cada carteira derivada.

```text
--- Step 3 of 4: passphrases ---

Wallet 1 of 3
How many passphrases for this wallet? (0-3)
> 2

Passphrase 1 of 2:
> [no echo]
Confirm passphrase 1 of 2:
> [no echo]
  OK.
```

| Situação | Resposta |
|---|---|
| Confirmação diferente | `Passphrases do not match. Try again.` e repete a mesma passphrase |
| Passphrase vazia confirmada | `An empty passphrase is the same as no passphrase, which is already shown. Enter a non-empty one.` e repete |
| Passphrase igual a outra da mesma carteira | `WARNING: identical to passphrase 1 — it produces the same wallet.` e aceita |
| Valor fora de `0`–`3` | `Please enter a whole number between 0 and 3.` e repete |
| Resposta `0` | Nenhum prompt adicional para essa carteira |

Nenhum caractere é ecoado durante a digitação; nem asteriscos.

## 6. Multisig (FR-017 … FR-019b)

O esquema é montado sobre as carteiras **já criadas** — nenhuma carteira nova é gerada. O número
de participantes não é perguntado: é a quantidade de carteiras derivadas.

```text
--- Step 4 of 4: multisig ---

The multisig scheme will combine all 3 wallets above,
each one taken with its last passphrase.

Create a multisig wallet? [y/N]
> y
How many signatures are required to spend? (1-3)
> 2
```

| Situação | Resposta |
|---|---|
| Menos de 2 carteiras derivadas | `Multisig needs at least 2 derived wallets to combine, and only N exist. Skipping this step.` — não pergunta nada |
| Assinaturas fora de `1`–N | `Please enter a whole number between 1 and 3.` e repete |
| Resposta `n` | Vai direto ao relatório, sem seção multisig |

## 7. Relatório final (FR-020 … FR-024a)

Estrutura fixa e ordem obrigatória. Larguras alinhadas para transcrição manual.

```text
================================================================================
 MASTER WALLET
================================================================================
 Seed (24 words):
   1. abandon      2. ability      3. able        4. about
   ...
  21. zone        22. zoo         23. zero       24. wrong

 Address type : Native SegWit (P2WPKH, bech32)
 Account path : m/84'/0'/0'
 Account zpub : zpub6r...

 Receive addresses:
   #0  m/84'/0'/0'/0/0   bc1q...
   ...
   #9  m/84'/0'/0'/0/9   bc1q...

================================================================================
 DERIVED WALLET 1 of 3          (BIP85  m/83696968'/39'/0'/24'/0')
================================================================================
 Seed (24 words):
   ...

 [no passphrase]
 Account zpub : zpub6s...
   #0  m/84'/0'/0'/0/0   bc1q...
   ...

 [passphrase 1] "correct horse battery staple"
 Account zpub : zpub6t...
   #0  m/84'/0'/0'/0/0   bc1q...
   ...

================================================================================
 MULTISIG WALLET  —  2 of 3
================================================================================
 This scheme combines the derived wallets above — no new wallet was created.
 Each participant is one of those wallets, taken with its last passphrase.

 WARNING: every participant key below is derived from the SAME master seed.
          This protects you against losing individual backups, NOT against
          someone who obtains the master seed — they can spend alone.

 Participant 1 of 3             (from derived wallet 1 (last passphrase))
 Passphrase   : "correct horse battery staple"
 Account path : m/48'/0'/0'/2'
 Account Zpub : Zpub74...

 ... (participants 2 and 3)

 Descriptor:
   wsh(sortedmulti(2,[a1b2c3d4/48h/0h/0h/2h]Zpub74.../0/*,...))

 Receive addresses:
   #0  .../0/0   bc1q...
   ...

================================================================================
 Nothing above was saved. Clear your screen and scrollback now:
   Windows : cls          Linux/macOS : clear && printf '\033[3J'
================================================================================
```

Regras verificáveis do relatório:

1. A ordem das seções é sempre: master → carteiras derivadas (crescente) → multisig.
2. Dentro de uma carteira derivada: bloco sem passphrase primeiro, depois cada passphrase na
   ordem em que foi informada.
3. Toda conta exibe exatamente 10 endereços, numerados `#0`–`#9`, cada um com o caminho completo.
4. `zpub` para contas `m/84'`; `Zpub` para contas `m/48'/…/2'`.
5. O aviso de raiz compartilhada é obrigatório sempre que a seção multisig existir, junto da
   declaração de que o esquema reusa as carteiras derivadas.
5a. Cada participante identifica a carteira derivada de origem e a passphrase usada — `(none)`
   quando a carteira não tiver passphrase alguma. O número de participantes é sempre igual ao
   número de carteiras derivadas exibidas acima.
5b. A seção multisig **não** reimprime as 24 palavras: cada semente aparece uma única vez no
   relatório, na sua carteira derivada.
6. A orientação de limpeza de tela é a última coisa impressa, sempre.

## 8. Interrupção

`Ctrl+C` em qualquer momento:

```text

Interrupted. Sensitive data wiped from memory. Nothing was saved.
```

Saída com código `1`. Nenhum resultado parcial é exibido.
