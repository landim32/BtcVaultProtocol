namespace BtcVaultProtocol.Wallet.Tests.Vectors;

/// <summary>
/// Vetores oficiais do BIP85, aplicação BIP39 (R-003).
/// Fonte: https://github.com/bitcoin/bips/blob/master/bip-0085.mediawiki
/// </summary>
public static class Bip85Vectors
{
    /// <summary>Master BIP32 root key usada por todos os vetores do BIP85.</summary>
    public const string MasterRootXprv =
        "xprv9s21ZrQH143K2LBWUUQRFXhucrQqBpKdRRxNVq2zBqsx8HVqFk2uYo8kmbaLLHRdqtQpUm98uKfu3v" +
        "ca1LqdGhUtyoFnCNkfmXRyPXLjbKb";

    public const string HmacKey = "bip-entropy-from-k";

    // --- 24 palavras, inglês, índice 0 ---
    public const string Path24Words = "m/83696968'/39'/0'/24'/0'";

    public const string Entropy24WordsHex =
        "ae131e2312cdc61331542efe0d1077bac5ea803adf24b313a4f0e48e9c51f37f";

    public const string Mnemonic24Words =
        "puppy ocean match cereal symbol another shed magic wrap hammer bulb intact gadget " +
        "divorce twin tonight reason outdoor destroy simple truth cigar social volcano";

    // --- 12 palavras, inglês, índice 0 — usado apenas para provar que o truncamento
    //     da entropia depende da contagem de palavras ---
    public const string Path12Words = "m/83696968'/39'/0'/12'/0'";

    public const string Entropy12WordsHex = "6250b68daf746d12a24d58b4787a714b";

    public const string Mnemonic12Words =
        "girl mad pet galaxy egg matter matrix prison refuse sense ordinary nose";
}
