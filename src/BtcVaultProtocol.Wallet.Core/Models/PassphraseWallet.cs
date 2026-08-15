namespace BtcVaultProtocol.Wallet.Core.Models;

/// <summary>
/// Carteira oculta definida por uma passphrase sobre a semente de uma carteira derivada.
/// A passphrase é exibida em texto claro no relatório, por decisão explícita do usuário.
/// </summary>
public sealed record PassphraseWallet(string Passphrase, WalletAccount Account);
