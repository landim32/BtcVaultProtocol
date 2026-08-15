using BtcVaultProtocol.Wallet.Core;
using BtcVaultProtocol.Wallet.Core.Derivation;
using BtcVaultProtocol.Wallet.Tests.Vectors;
using NBitcoin;

namespace BtcVaultProtocol.Wallet.Tests.Derivation;

public sealed class AddressDeriverTests
{
    [Fact]
    public void DeriveSingleSigAddresses_MatchOfficialBip84Vector()
    {
        var addresses = Bip84Addresses();

        Assert.Equal(Bip84Vectors.Address0, addresses[0].Address);
        Assert.Equal(Bip84Vectors.Address1, addresses[1].Address);
    }

    [Fact]
    public void DeriveSingleSigAddresses_ReturnsExactlyTenEntriesNumberedZeroToNine()
    {
        var addresses = Bip84Addresses();

        Assert.Equal(WalletConstants.ADDRESSES_PER_ACCOUNT, addresses.Count);
        Assert.Equal(Enumerable.Range(0, WalletConstants.ADDRESSES_PER_ACCOUNT), addresses.Select(a => a.Index));
    }

    [Fact]
    public void DeriveSingleSigAddresses_CarryFullDerivationPath()
    {
        var addresses = Bip84Addresses();

        Assert.Equal("m/84'/0'/0'/0/0", addresses[0].FullPath);
        Assert.Equal("m/84'/0'/0'/0/9", addresses[9].FullPath);
    }

    [Fact]
    public void DeriveSingleSigAddresses_AreAllNativeSegwitAndDistinct()
    {
        var addresses = Bip84Addresses();

        Assert.All(addresses, a => Assert.StartsWith("bc1q", a.Address, StringComparison.Ordinal));
        Assert.Equal(addresses.Count, addresses.Select(a => a.Address).Distinct().Count());
    }

    [Fact]
    public void ReceivePubKey_MatchesOfficialBip84Vector()
    {
        var accountExtPubKey = AccountDeriver
            .DeriveSingleSig(new Mnemonic(Bip84Vectors.Mnemonic, Wordlist.English), Bip84Vectors.Passphrase)
            .AccountExtPubKey;

        Assert.Equal(Bip84Vectors.PubKey0, AddressDeriver.ReceivePubKey(accountExtPubKey, 0).ToHex());
        Assert.Equal(Bip84Vectors.PubKey1, AddressDeriver.ReceivePubKey(accountExtPubKey, 1).ToHex());
    }

    private static IReadOnlyList<Core.Models.AddressEntry> Bip84Addresses() =>
        AccountDeriver
            .DeriveSingleSig(new Mnemonic(Bip84Vectors.Mnemonic, Wordlist.English), Bip84Vectors.Passphrase)
            .Addresses;
}
