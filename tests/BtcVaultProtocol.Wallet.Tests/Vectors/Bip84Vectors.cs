namespace BtcVaultProtocol.Wallet.Tests.Vectors;

/// <summary>
/// Vetores oficiais do BIP84 (Native SegWit, m/84'/0'/0').
/// Fonte: https://github.com/bitcoin/bips/blob/master/bip-0084.mediawiki
/// </summary>
public static class Bip84Vectors
{
    public const string Mnemonic =
        "abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon " +
        "abandon about";

    /// <summary>Os vetores do BIP84 não usam passphrase.</summary>
    public const string Passphrase = "";

    public const string AccountZpub =
        "zpub6rFR7y4Q2AijBEqTUquhVz398htDFrtymD9xYYfG1m4wAcvPhXNfE3EfH1r1ADqtfSdVCToUG868Rv" +
        "UUkgDKf31mGDtKsAYz2oz2AGutZYs";

    public const string AccountZprv =
        "zprvAdG4iTXWBoARxkkzNpNh8r6Qag3irQB8PzEMkAFeTRXxHpbF9z4QgEvBRmfvqWvGp42t42nvgGpNgY" +
        "SJA9iefm1yYNZKEm7z6qUWCroSQnE";

    public const string RootZprv =
        "zprvAWgYBBk7JR8Gjrh4UJQ2uJdG1r3WNRRfURiABBE3RvMXYSrRJL62XuezvGdPvG6GFBZduosCc1YP5w" +
        "ixPox7zhZLfiUm8aunE96BBa4Kei5";

    public const string Address0 = "bc1qcr8te4kr609gcawutmrza0j4xv80jy8z306fyu";
    public const string Address1 = "bc1qnjg0jd8228aq7egyzacy8cys3knf9xvrerkf9g";

    public const string PubKey0 =
        "0330d54fd0dd420a6e5f8d3624f5f3482cae350f79d5f0753bf5beef9c2d91af3c";
    public const string PubKey1 =
        "03e775fd51f0dfb8cd865d9ff1cca2a158cf651fe997fdc9fee9c1d3b5e995ea77";
}
