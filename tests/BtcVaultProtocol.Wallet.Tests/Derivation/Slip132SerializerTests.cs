using BtcVaultProtocol.Wallet.Core;
using BtcVaultProtocol.Wallet.Core.Derivation;
using BtcVaultProtocol.Wallet.Tests.Vectors;
using NBitcoin;

namespace BtcVaultProtocol.Wallet.Tests.Derivation;

public sealed class Slip132SerializerTests
{
    [Fact]
    public void ToZpub_Bip84AccountKey_MatchesOfficialVector()
    {
        var mnemonic = new Mnemonic(Bip84Vectors.Mnemonic, Wordlist.English);
        var accountExtPubKey = mnemonic
            .DeriveExtKey(Bip84Vectors.Passphrase)
            .Derive(DerivationPaths.SingleSigAccount)
            .Neuter();

        var zpub = Slip132Serializer.ToZpub(accountExtPubKey);

        Assert.Equal(Bip84Vectors.AccountZpub, zpub);
    }

    [Fact]
    public void ToZpub_ProducesZpubPrefix()
    {
        var zpub = Slip132Serializer.ToZpub(SampleAccountExtPubKey());

        Assert.StartsWith("zpub", zpub, StringComparison.Ordinal);
    }

    [Fact]
    public void ToMultisigZpub_ProducesCapitalZpubPrefix()
    {
        var serialized = Slip132Serializer.ToMultisigZpub(SampleAccountExtPubKey());

        Assert.StartsWith("Zpub", serialized, StringComparison.Ordinal);
    }

    [Fact]
    public void VersionBytes_MatchSlip132Registry()
    {
        Assert.Equal(0x04B24746u, WalletConstants.ZPUB_VERSION);
        Assert.Equal(0x02AA7ED3u, WalletConstants.ZPUB_MULTISIG_VERSION);
    }

    [Fact]
    public void ToZpub_And_ToMultisigZpub_DifferForTheSameKey()
    {
        var accountExtPubKey = SampleAccountExtPubKey();

        Assert.NotEqual(
            Slip132Serializer.ToZpub(accountExtPubKey),
            Slip132Serializer.ToMultisigZpub(accountExtPubKey));
    }

    private static ExtPubKey SampleAccountExtPubKey() =>
        new Mnemonic(Bip84Vectors.Mnemonic, Wordlist.English)
            .DeriveExtKey()
            .Derive(DerivationPaths.MultisigAccount)
            .Neuter();
}
