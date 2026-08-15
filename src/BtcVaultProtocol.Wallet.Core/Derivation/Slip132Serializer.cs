using NBitcoin;
using NBitcoin.DataEncoders;

namespace BtcVaultProtocol.Wallet.Core.Derivation;

/// <summary>
/// Serializa chaves públicas estendidas no formato SLIP-132 (zpub/Zpub) — R-006.
///
/// A NBitcoin deliberadamente não suporta SLIP-132: para a biblioteca, um zpub é um ExtPubKey
/// acrescido de informação sobre o tipo de script, que não faz parte do BIP32. Como FR-022a
/// exige essas serializações, trocamos os 4 bytes de versão da serialização BIP32 de 78 bytes
/// e recodificamos com o Base58Check da própria NBitcoin.
///
/// Isso é CODIFICAÇÃO, não primitiva criptográfica: nenhum hash, HMAC ou operação de curva é
/// reimplementado aqui (Princípio I).
/// </summary>
public static class Slip132Serializer
{
    private const int SERIALIZED_LENGTH = 78;
    private const int VERSION_LENGTH = 4;

    /// <summary>Serializa como zpub (P2WPKH, m/84').</summary>
    public static string ToZpub(ExtPubKey accountExtPubKey) =>
        Serialize(accountExtPubKey, WalletConstants.ZPUB_VERSION);

    /// <summary>Serializa como Zpub (P2WSH multisig, m/48'/.../2').</summary>
    public static string ToMultisigZpub(ExtPubKey accountExtPubKey) =>
        Serialize(accountExtPubKey, WalletConstants.ZPUB_MULTISIG_VERSION);

    private static string Serialize(ExtPubKey accountExtPubKey, uint version)
    {
        ArgumentNullException.ThrowIfNull(accountExtPubKey);

        var payload = accountExtPubKey.ToBytes();
        var serialized = new byte[SERIALIZED_LENGTH];

        WriteVersion(serialized, version);

        // A NBitcoin serializa o ExtPubKey com ou sem os bytes de versão dependendo da
        // versão do pacote. Tratamos os dois formatos para não depender desse detalhe.
        if (payload.Length == SERIALIZED_LENGTH)
        {
            Buffer.BlockCopy(payload, VERSION_LENGTH, serialized, VERSION_LENGTH, SERIALIZED_LENGTH - VERSION_LENGTH);
        }
        else if (payload.Length == SERIALIZED_LENGTH - VERSION_LENGTH)
        {
            Buffer.BlockCopy(payload, 0, serialized, VERSION_LENGTH, payload.Length);
        }
        else
        {
            throw new InvalidOperationException(
                $"Unexpected extended key serialization length: {payload.Length} bytes.");
        }

        return Encoders.Base58Check.EncodeData(serialized);
    }

    private static void WriteVersion(byte[] target, uint version)
    {
        target[0] = (byte)(version >> 24);
        target[1] = (byte)(version >> 16);
        target[2] = (byte)(version >> 8);
        target[3] = (byte)version;
    }
}
