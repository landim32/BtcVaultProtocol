using NBitcoin;

namespace BtcVaultProtocol.Wallet.Core.Mnemonics;

/// <summary>
/// Converte 32 bytes de entropia em uma semente mnemônica de 24 palavras em inglês (FR-006).
/// </summary>
public static class MnemonicFactory
{
    public static Mnemonic FromEntropy(byte[] entropy)
    {
        ArgumentNullException.ThrowIfNull(entropy);

        if (entropy.Length != WalletConstants.ENTROPY_BYTES)
        {
            throw new ArgumentException(
                $"Entropy must be exactly {WalletConstants.ENTROPY_BYTES} bytes " +
                $"({WalletConstants.ENTROPY_BITS} bits), got {entropy.Length}.",
                nameof(entropy));
        }

        return new Mnemonic(Wordlist.English, entropy);
    }
}
