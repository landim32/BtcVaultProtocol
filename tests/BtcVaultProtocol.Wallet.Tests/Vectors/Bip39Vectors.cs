namespace BtcVaultProtocol.Wallet.Tests.Vectors;

/// <summary>
/// Vetores oficiais do BIP39 (implementação de referência Trezor), embutidos como constantes —
/// o Princípio III proíbe leitura de arquivos externos em testes.
/// Fonte: https://github.com/trezor/python-mnemonic/blob/master/vectors.json
/// Apenas as entradas de 32 bytes (24 palavras) são usadas, conforme o escopo da feature.
/// A passphrase dos vetores de seed é "TREZOR".
/// </summary>
public static class Bip39Vectors
{
    public const string SEED_PASSPHRASE = "TREZOR";

    public sealed record Vector(string EntropyHex, string Mnemonic, string? SeedHex);

    public static readonly Vector AllZeros = new(
        "0000000000000000000000000000000000000000000000000000000000000000",
        "abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon " +
        "abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon " +
        "abandon abandon abandon art",
        "bda85446c68413707090a52022edd26a1c9462295029f2e60cd7c4f2bbd3097170af7a4d73245cafa9" +
        "c3cca8d561a7c3de6f5d4a10be8ed2a5e608d68f92fcc8");

    public static readonly Vector SevenF = new(
        "7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f",
        "legal winner thank year wave sausage worth useful legal winner thank year wave " +
        "sausage worth useful legal winner thank year wave sausage worth title",
        SeedHex: null);

    public static readonly Vector Eighty = new(
        "8080808080808080808080808080808080808080808080808080808080808080",
        "letter advice cage absurd amount doctor acoustic avoid letter advice cage absurd " +
        "amount doctor acoustic avoid letter advice cage absurd amount doctor acoustic bless",
        SeedHex: null);

    public static readonly Vector AllFs = new(
        "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff",
        "zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo " +
        "zoo zoo zoo vote",
        SeedHex: null);

    public static IEnumerable<object[]> EntropyToMnemonic() =>
    [
        [AllZeros.EntropyHex, AllZeros.Mnemonic],
        [SevenF.EntropyHex, SevenF.Mnemonic],
        [Eighty.EntropyHex, Eighty.Mnemonic],
        [AllFs.EntropyHex, AllFs.Mnemonic],
    ];
}
