using NBitcoin;

namespace BtcVaultProtocol.Wallet.Core.Models;

/// <summary>
/// Carteira filha derivada da mestre via BIP85.
/// </summary>
public sealed record DerivedWallet(
    int Index,
    Mnemonic Mnemonic,
    WalletAccount Account,
    IReadOnlyList<PassphraseWallet> PassphraseWallets)
{
    /// <summary>Numeração exibida ao usuário (1-based).</summary>
    public int DisplayNumber => Index + 1;

    /// <summary>Caminho BIP85 que originou esta carteira.</summary>
    public string Bip85Path =>
        $"m/{WalletConstants.BIP85_PURPOSE}'/{WalletConstants.BIP85_APPLICATION_BIP39}'/" +
        $"{WalletConstants.BIP85_LANGUAGE_ENGLISH}'/{WalletConstants.MNEMONIC_WORD_COUNT}'/{Index}'";
}
