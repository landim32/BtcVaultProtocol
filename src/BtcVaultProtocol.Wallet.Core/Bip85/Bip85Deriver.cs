using System.Security.Cryptography;
using BtcVaultProtocol.Wallet.Core.Mnemonics;
using NBitcoin;

namespace BtcVaultProtocol.Wallet.Core.Bip85;

/// <summary>
/// Derivação BIP85, aplicação BIP39 (R-003).
///
/// EXCEÇÃO CONTROLADA DO PRINCÍPIO I: a NBitcoin não implementa BIP85, então a derivação é
/// composta aqui — mas exclusivamente a partir de primitivas existentes:
///   * derivação BIP32 endurecida  → NBitcoin (ExtKey.Derive)
///   * HMAC-SHA512                 → BCL (System.Security.Cryptography)
/// Nenhuma primitiva criptográfica nova é escrita. O componente é único e isolado, e é
/// validado contra o vetor oficial do BIP85.
///
/// Caminho: m/83696968'/39'/{language}'/{words}'/{index}'
/// Entropia: HMAC-SHA512(key = "bip-entropy-from-k", msg = chave_privada_derivada), truncada
/// aos primeiros 32 bytes para 24 palavras.
/// </summary>
public static class Bip85Deriver
{
    private const int HMAC_OUTPUT_BYTES = 64;

    /// <summary>
    /// Deriva a entropia BIP85 do índice informado. O chamador é dono do array retornado e deve
    /// zerá-lo após o uso (Princípio VI).
    /// </summary>
    public static byte[] DeriveEntropy(ExtKey masterKey, uint index, int wordCount = WalletConstants.MNEMONIC_WORD_COUNT)
    {
        ArgumentNullException.ThrowIfNull(masterKey);

        var entropyBytes = EntropyBytesFor(wordCount);

        var path = new KeyPath(
        [
            Harden(WalletConstants.BIP85_PURPOSE),
            Harden(WalletConstants.BIP85_APPLICATION_BIP39),
            Harden(WalletConstants.BIP85_LANGUAGE_ENGLISH),
            Harden((uint)wordCount),
            Harden(index),
        ]);

        var derivedPrivateKey = masterKey.Derive(path).PrivateKey.ToBytes();
        var hmacOutput = new byte[HMAC_OUTPUT_BYTES];

        try
        {
            var hmacKey = System.Text.Encoding.UTF8.GetBytes(WalletConstants.BIP85_HMAC_KEY);
            HMACSHA512.HashData(hmacKey, derivedPrivateKey, hmacOutput);

            return hmacOutput[..entropyBytes];
        }
        finally
        {
            CryptographicOperations.ZeroMemory(derivedPrivateKey);
            CryptographicOperations.ZeroMemory(hmacOutput);
        }
    }

    /// <summary>Deriva a semente mnemônica BIP85 do índice informado.</summary>
    public static Mnemonic DeriveMnemonic(ExtKey masterKey, uint index)
    {
        var entropy = DeriveEntropy(masterKey, index);

        try
        {
            return MnemonicFactory.FromEntropy(entropy);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(entropy);
        }
    }

    private static uint Harden(uint value) => value | 0x80000000u;

    private static int EntropyBytesFor(int wordCount) => wordCount switch
    {
        12 => 16,
        18 => 24,
        24 => WalletConstants.ENTROPY_BYTES,
        _ => throw new ArgumentOutOfRangeException(
            nameof(wordCount), wordCount, "Supported word counts are 12, 18 and 24."),
    };
}
