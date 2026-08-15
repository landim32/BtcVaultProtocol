using BtcVaultProtocol.Wallet.Core.Derivation;
using BtcVaultProtocol.Wallet.Tests.Vectors;
using NBitcoin;

namespace BtcVaultProtocol.Wallet.Tests.Derivation;

/// <summary>
/// FR-015 e o edge case de passphrases não-ASCII.
/// </summary>
public sealed class PassphraseAccountTests
{
    [Fact]
    public void DifferentPassphrases_ProduceDifferentHiddenWallets()
    {
        var accounts = new[] { "alpha", "beta", "gamma" }
            .Select(p => AccountDeriver.DeriveSingleSig(Mnemonic(), p))
            .ToList();

        Assert.Equal(3, accounts.Select(a => a.Serialized).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(3, accounts.Select(a => a.Addresses[0].Address).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void SamePassphrase_AlwaysProducesTheSameWallet()
    {
        var first = AccountDeriver.DeriveSingleSig(Mnemonic(), "repeatable");
        var second = AccountDeriver.DeriveSingleSig(Mnemonic(), "repeatable");

        Assert.Equal(first.Serialized, second.Serialized);
        Assert.Equal(first.Addresses[0].Address, second.Addresses[0].Address);
    }

    [Fact]
    public void SeedWithPassphrase_MatchesOfficialBip39Vector()
    {
        var mnemonic = new Mnemonic(Bip39Vectors.AllZeros.Mnemonic, Wordlist.English);

        var seed = mnemonic.DeriveSeed(Bip39Vectors.SEED_PASSPHRASE);

        Assert.Equal(Bip39Vectors.AllZeros.SeedHex, Convert.ToHexString(seed).ToLowerInvariant());
    }

    [Theory]
    [InlineData("café")]
    [InlineData("señor contraseña")]
    [InlineData("🔐 emoji passphrase")]
    [InlineData("日本語")]
    public void NonAsciiPassphrase_IsAcceptedAndProducesAStableWallet(string passphrase)
    {
        var first = AccountDeriver.DeriveSingleSig(Mnemonic(), passphrase);
        var second = AccountDeriver.DeriveSingleSig(Mnemonic(), passphrase);

        Assert.Equal(first.Serialized, second.Serialized);
        Assert.StartsWith("bc1q", first.Addresses[0].Address, StringComparison.Ordinal);
    }

    [Fact]
    public void NonAsciiPassphrase_IsNormalizedToNfkdPerBip39()
    {
        // "é" pré-composto (U+00E9) e decomposto (U+0065 U+0301) devem convergir após o NFKD
        // exigido pelo BIP39 — do contrário a carteira seria irrecuperável em outro software.
        var precomposed = AccountDeriver.DeriveSingleSig(Mnemonic(), "café");
        var decomposed = AccountDeriver.DeriveSingleSig(Mnemonic(), "café");

        Assert.Equal(precomposed.Serialized, decomposed.Serialized);
    }

    [Fact]
    public void LeadingAndTrailingSpaces_AreSignificant()
    {
        var plain = AccountDeriver.DeriveSingleSig(Mnemonic(), "secret");
        var padded = AccountDeriver.DeriveSingleSig(Mnemonic(), " secret ");

        Assert.NotEqual(plain.Serialized, padded.Serialized);
    }

    [Fact]
    public void NoPassphrase_AndEmptyPassphrase_AreEquivalent()
    {
        // É exatamente por isso que a passphrase vazia é rejeitada na interface (R-010):
        // ela não cria carteira nova alguma.
        var none = AccountDeriver.DeriveSingleSig(Mnemonic());
        var empty = AccountDeriver.DeriveSingleSig(Mnemonic(), string.Empty);

        Assert.Equal(none.Serialized, empty.Serialized);
    }

    private static Mnemonic Mnemonic() => new(Bip84Vectors.Mnemonic, Wordlist.English);
}
