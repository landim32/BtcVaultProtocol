namespace BtcVaultProtocol.Wallet.Tests.Vectors;

/// <summary>
/// Vetores oficiais do BIP67 (ordenação lexicográfica determinística de chaves públicas em
/// scripts multisig).
/// Fonte: https://github.com/bitcoin/bips/blob/master/bip-0067.mediawiki
///
/// O script multisig do BIP67 é o mesmo em P2SH e P2WSH — muda apenas o wrapper. Por isso
/// estes vetores validam também o witnessScript do nosso multisig P2WSH (R-008): o BIP48 não
/// publica vetores de endereço, e o endereço final é coberto pela verificação cruzada manual
/// do quickstart (V-3).
/// </summary>
public static class Bip67Vectors
{
    public sealed record Vector(int RequiredSignatures, string[] UnsortedKeys, string[] SortedKeys, string ScriptHex);

    /// <summary>Duas chaves fora de ordem, 2-de-2.</summary>
    public static readonly Vector TwoKeys = new(
        RequiredSignatures: 2,
        UnsortedKeys:
        [
            "02ff12471208c14bd580709cb2358d98975247d8765f92bc25eab3b2763ed605f8",
            "02fe6f0a5a297eb38c391581c4413e084773ea23954d93f7753db7dc0adc188b2f",
        ],
        SortedKeys:
        [
            "02fe6f0a5a297eb38c391581c4413e084773ea23954d93f7753db7dc0adc188b2f",
            "02ff12471208c14bd580709cb2358d98975247d8765f92bc25eab3b2763ed605f8",
        ],
        ScriptHex:
            "522102fe6f0a5a297eb38c391581c4413e084773ea23954d93f7753db7dc0adc188b2f2102ff12" +
            "471208c14bd580709cb2358d98975247d8765f92bc25eab3b2763ed605f852ae");

    /// <summary>Três chaves já ordenadas, 2-de-3.</summary>
    public static readonly Vector ThreeKeysAlreadySorted = new(
        RequiredSignatures: 2,
        UnsortedKeys:
        [
            "02632b12f4ac5b1d1b72b2a3b508c19172de44f6f46bcee50ba33f3f9291e47ed0",
            "027735a29bae7780a9755fae7a1c4374c656ac6a69ea9f3697fda61bb99a4f3e77",
            "02e2cc6bd5f45edd43bebe7cb9b675f0ce9ed3efe613b177588290ad188d11b404",
        ],
        SortedKeys:
        [
            "02632b12f4ac5b1d1b72b2a3b508c19172de44f6f46bcee50ba33f3f9291e47ed0",
            "027735a29bae7780a9755fae7a1c4374c656ac6a69ea9f3697fda61bb99a4f3e77",
            "02e2cc6bd5f45edd43bebe7cb9b675f0ce9ed3efe613b177588290ad188d11b404",
        ],
        ScriptHex:
            "522102632b12f4ac5b1d1b72b2a3b508c19172de44f6f46bcee50ba33f3f9291e47ed021027735" +
            "a29bae7780a9755fae7a1c4374c656ac6a69ea9f3697fda61bb99a4f3e772102e2cc6bd5f45edd" +
            "43bebe7cb9b675f0ce9ed3efe613b177588290ad188d11b40453ae");

    /// <summary>Três chaves fora de ordem, 2-de-3.</summary>
    public static readonly Vector ThreeKeysUnsorted = new(
        RequiredSignatures: 2,
        UnsortedKeys:
        [
            "022df8750480ad5b26950b25c7ba79d3e37d75f640f8e5d9bcd5b150a0f85014da",
            "03e3818b65bcc73a7d64064106a859cc1a5a728c4345ff0b641209fba0d90de6e9",
            "021f2f6e1e50cb6a953935c3601284925decd3fd21bc445712576873fb8c6ebc18",
        ],
        SortedKeys:
        [
            "021f2f6e1e50cb6a953935c3601284925decd3fd21bc445712576873fb8c6ebc18",
            "022df8750480ad5b26950b25c7ba79d3e37d75f640f8e5d9bcd5b150a0f85014da",
            "03e3818b65bcc73a7d64064106a859cc1a5a728c4345ff0b641209fba0d90de6e9",
        ],
        ScriptHex:
            "5221021f2f6e1e50cb6a953935c3601284925decd3fd21bc445712576873fb8c6ebc1821022df8" +
            "750480ad5b26950b25c7ba79d3e37d75f640f8e5d9bcd5b150a0f85014da2103e3818b65bcc73a" +
            "7d64064106a859cc1a5a728c4345ff0b641209fba0d90de6e953ae");

    public static IEnumerable<object[]> All() =>
    [
        [TwoKeys], [ThreeKeysAlreadySorted], [ThreeKeysUnsorted],
    ];
}
