using BtcVaultProtocol.Wallet.Core.Bip85;
using BtcVaultProtocol.Wallet.Tests.Vectors;
using NBitcoin;

namespace BtcVaultProtocol.Wallet.Tests.Bip85;

public sealed class Bip85DeriverTests
{
    [Fact]
    public void DeriveEntropy_24Words_MatchesOfficialVector()
    {
        var entropy = Bip85Deriver.DeriveEntropy(MasterKey(), index: 0);

        Assert.Equal(Bip85Vectors.Entropy24WordsHex, Convert.ToHexString(entropy).ToLowerInvariant());
    }

    [Fact]
    public void DeriveMnemonic_24Words_MatchesOfficialVector()
    {
        var mnemonic = Bip85Deriver.DeriveMnemonic(MasterKey(), index: 0);

        Assert.Equal(Bip85Vectors.Mnemonic24Words, mnemonic.ToString());
    }

    [Fact]
    public void DeriveEntropy_12Words_MatchesOfficialVector()
    {
        // Prova que o truncamento da saída do HMAC depende da contagem de palavras.
        var entropy = Bip85Deriver.DeriveEntropy(MasterKey(), index: 0, wordCount: 12);

        Assert.Equal(Bip85Vectors.Entropy12WordsHex, Convert.ToHexString(entropy).ToLowerInvariant());
    }

    [Fact]
    public void DeriveEntropy_24Words_ReturnsExactly32Bytes()
    {
        Assert.Equal(32, Bip85Deriver.DeriveEntropy(MasterKey(), index: 0).Length);
    }

    [Fact]
    public void DeriveMnemonic_DifferentIndexes_ProduceDifferentWallets()
    {
        var first = Bip85Deriver.DeriveMnemonic(MasterKey(), index: 0);
        var second = Bip85Deriver.DeriveMnemonic(MasterKey(), index: 1);

        Assert.NotEqual(first.ToString(), second.ToString());
    }

    [Theory]
    [InlineData(11)]
    [InlineData(25)]
    public void DeriveEntropy_UnsupportedWordCount_Throws(int wordCount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Bip85Deriver.DeriveEntropy(MasterKey(), index: 0, wordCount: wordCount));
    }

    private static ExtKey MasterKey() =>
        ExtKey.Parse(Bip85Vectors.MasterRootXprv, Network.Main);
}
