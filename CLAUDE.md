# BtcVaultProtocol Development Guidelines

Auto-generated from all feature plans. Last updated: 2026-08-06

## Active Technologies

- .NET 8.0 LTS (fixed by Constitution Principle II) + NBitcoin + BCL only; xUnit for tests (fixed by Principle II) (001-bitcoin-wallet-generator)

## Project Structure

```text
src/
tests/
```

## Commands

# Add commands for .NET 8.0 LTS (fixed by Constitution Principle II)

## Code Style

.NET 8.0 LTS (fixed by Constitution Principle II): Follow standard conventions

## Recent Changes

- 001-bitcoin-wallet-generator: Added .NET 8.0 LTS (fixed by Constitution Principle II) + NBitcoin + BCL only; xUnit for tests (fixed by Principle II)

<!-- MANUAL ADDITIONS START -->

## Regras não-negociáveis (`.specify/memory/constitution.md` v2.0.1)

- **Nunca** escrever primitiva criptográfica à mão. NBitcoin/BCL apenas. Aleatoriedade só via
  `RandomNumberGenerator` — `System.Random` é proibido perto de material de chave.
- **Nunca** gravar em arquivo, log, clipboard, variável de ambiente ou rede. A única saída é
  `System.Console`.
- **Nunca** considerar uma derivação pronta sem teste contra o vetor oficial do BIP. Valor
  esperado de teste jamais é ajustado para acomodar a saída do código.
- Pacotes permitidos: **NBitcoin** (produção) e **xUnit** + `Microsoft.NET.Test.Sdk` +
  `xunit.runner.visualstudio` (somente no projeto de testes). Nada mais sem emenda formal.
- `BtcVaultProtocol.Wallet.Core` não pode usar `System.Console` nem referenciar o projeto Console.

## Comandos

```powershell
dotnet build --configuration Release
dotnet test
dotnet run --project src/BtcVaultProtocol.Wallet.Console
```

## Convenções

- `namespace` file-scoped, `nullable` enabled, warnings as errors.
- Records imutáveis para todo resultado de geração; sem literais mágicos — constantes nomeadas em
  `Core/WalletConstants.cs`.
- Buffers sensíveis em `byte[]`/`char[]`, zerados com `CryptographicOperations.ZeroMemory` em
  `try/finally`, inclusive nos caminhos de erro.
- Interface do aplicativo em **inglês**; documentação de specs em português.

<!-- MANUAL ADDITIONS END -->
