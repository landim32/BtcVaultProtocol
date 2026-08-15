using BtcVaultProtocol.Wallet.Tests.Vectors;
using NBitcoin;

namespace BtcVaultProtocol.Wallet.Tests.Derivation;

/// <summary>
/// Canário da cadeia BIP32 sobre a qual BIP84 e BIP85 se apoiam.
/// </summary>
public sealed class Bip32ChainTests
{
    [Fact]
    public void MasterKey_FromSeed_MatchesOfficialVector()
    {
        var masterKey = ExtKey.CreateFromSeed(Convert.FromHexString(Bip32Vectors.SeedHex));

        Assert.Equal(Bip32Vectors.MasterXprv, masterKey.GetWif(Network.Main).ToString());
        Assert.Equal(Bip32Vectors.MasterXpub, masterKey.Neuter().GetWif(Network.Main).ToString());
    }

    [Fact]
    public void HardenedThenNormalDerivation_MatchesOfficialVector()
    {
        var derived = ExtKey
            .CreateFromSeed(Convert.FromHexString(Bip32Vectors.SeedHex))
            .Derive(new KeyPath(Bip32Vectors.ChainPath));

        Assert.Equal(Bip32Vectors.ChainXprv, derived.GetWif(Network.Main).ToString());
        Assert.Equal(Bip32Vectors.ChainXpub, derived.Neuter().GetWif(Network.Main).ToString());
    }
}
