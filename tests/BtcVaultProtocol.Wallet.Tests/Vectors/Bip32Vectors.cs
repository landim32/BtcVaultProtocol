namespace BtcVaultProtocol.Wallet.Tests.Vectors;

/// <summary>
/// Vetor de teste 1 do BIP32, usado como canário da cadeia de derivação (seed → chave mestre →
/// derivação endurecida e normal). Se este vetor falhar, o problema está na base sobre a qual
/// BIP84 e BIP85 se apoiam.
/// Fonte: https://github.com/bitcoin/bips/blob/master/bip-0032.mediawiki
/// </summary>
public static class Bip32Vectors
{
    public const string SeedHex = "000102030405060708090a0b0c0d0e0f";

    public const string MasterXprv =
        "xprv9s21ZrQH143K3QTDL4LXw2F7HEK3wJUD2nW2nRk4stbPy6cq3jPPqjiChkVvvNKmPGJxWUtg6LnF5k" +
        "ejMRNNU3TGtRBeJgk33yuGBxrMPHi";

    public const string MasterXpub =
        "xpub661MyMwAqRbcFtXgS5sYJABqqG9YLmC4Q1Rdap9gSE8NqtwybGhePY2gZ29ESFjqJoCu1Rupje8YtG" +
        "qsefD265TMg7usUDFdp6W1EGMcet8";

    /// <summary>Chain m/0'/1 — cobre derivação endurecida seguida de normal.</summary>
    public const string ChainPath = "m/0'/1";

    public const string ChainXprv =
        "xprv9wTYmMFdV23N2TdNG573QoEsfRrWKQgWeibmLntzniatZvR9BmLnvSxqu53Kw1UmYPxLgboyZQaXwT" +
        "Cg8MSY3H2EU4pWcQDnRnrVA1xe8fs";

    public const string ChainXpub =
        "xpub6ASuArnXKPbfEwhqN6e3mwBcDTgzisQN1wXN9BJcM47sSikHjJf3UFHKkNAWbWMiGj7Wf5uMash7Sy" +
        "Yq527Hqck2AxYysAA7xmALppuCkwQ";
}
