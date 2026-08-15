using BtcVaultProtocol.Wallet.Core;
using BtcVaultProtocol.Wallet.Core.Mnemonics;
using BtcVaultProtocol.Wallet.Tests.Vectors;
using NBitcoin;

namespace BtcVaultProtocol.Wallet.Tests.Mnemonics;

public sealed class MnemonicFactoryTests
{
    [Theory]
    [MemberData(nameof(Bip39Vectors.EntropyToMnemonic), MemberType = typeof(Bip39Vectors))]
    public void FromEntropy_MatchesOfficialBip39Vectors(string entropyHex, string expectedMnemonic)
    {
        var mnemonic = MnemonicFactory.FromEntropy(Convert.FromHexString(entropyHex));

        Assert.Equal(expectedMnemonic, mnemonic.ToString());
    }

    [Fact]
    public void FromEntropy_ProducesTwentyFourValidWords()
    {
        var mnemonic = MnemonicFactory.FromEntropy(Convert.FromHexString(Bip39Vectors.AllZeros.EntropyHex));

        Assert.Equal(WalletConstants.MNEMONIC_WORD_COUNT, mnemonic.Words.Length);
        Assert.True(mnemonic.IsValidChecksum);
    }

    [Fact]
    public void FromEntropy_RoundTripsThroughTheWordlist()
    {
        var mnemonic = MnemonicFactory.FromEntropy(Convert.FromHexString(Bip39Vectors.Eighty.EntropyHex));

        // Reimportar as palavras geradas deve reproduzir a mesma semente — é exatamente o que
        // uma carteira de referência fará com o que o usuário transcrever.
        var reparsed = new Mnemonic(mnemonic.ToString(), Wordlist.English);

        Assert.Equal(mnemonic.ToString(), reparsed.ToString());
        Assert.True(reparsed.IsValidChecksum);
        Assert.Equal(
            Convert.ToHexString(mnemonic.DeriveSeed()),
            Convert.ToHexString(reparsed.DeriveSeed()));
    }

    [Theory]
    [InlineData(16)]
    [InlineData(31)]
    [InlineData(33)]
    public void FromEntropy_WithWrongLength_Throws(int length)
    {
        Assert.Throws<ArgumentException>(() => MnemonicFactory.FromEntropy(new byte[length]));
    }
}
