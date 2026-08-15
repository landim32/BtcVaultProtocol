using BtcVaultProtocol.Wallet.Core;
using BtcVaultProtocol.Wallet.Core.Multisig;
using BtcVaultProtocol.Wallet.Tests.Vectors;
using NBitcoin;

namespace BtcVaultProtocol.Wallet.Tests.Multisig;

public sealed class MultisigBuilderTests
{
    [Theory]
    [MemberData(nameof(Bip67Vectors.All), MemberType = typeof(Bip67Vectors))]
    public void SortByBip67_MatchesOfficialVectors(Bip67Vectors.Vector vector)
    {
        var sorted = MultisigBuilder.SortByBip67(vector.UnsortedKeys.Select(hex => new PubKey(hex)));

        Assert.Equal(vector.SortedKeys, sorted.Select(key => key.ToHex()));
    }

    [Theory]
    [MemberData(nameof(Bip67Vectors.All), MemberType = typeof(Bip67Vectors))]
    public void BuildWitnessScript_MatchesOfficialBip67Script(Bip67Vectors.Vector vector)
    {
        // O script multisig é idêntico em P2SH e P2WSH — muda apenas o wrapper. Estes vetores
        // oficiais validam, portanto, o witnessScript do nosso multisig P2WSH.
        var script = MultisigBuilder.BuildWitnessScript(
            vector.RequiredSignatures,
            vector.UnsortedKeys.Select(hex => new PubKey(hex)));

        Assert.Equal(vector.ScriptHex, script.ToHex());
    }

    [Fact]
    public void BuildWitnessScript_IsIndependentOfInputOrder()
    {
        var keys = Bip67Vectors.ThreeKeysUnsorted.UnsortedKeys.Select(hex => new PubKey(hex)).ToArray();

        var script = MultisigBuilder.BuildWitnessScript(2, keys);
        var scriptFromReversedInput = MultisigBuilder.BuildWitnessScript(2, keys.Reverse());

        Assert.Equal(script.ToHex(), scriptFromReversedInput.ToHex());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public void BuildWitnessScript_WithImpossibleThreshold_Throws(int requiredSignatures)
    {
        var keys = Bip67Vectors.ThreeKeysUnsorted.UnsortedKeys.Select(hex => new PubKey(hex));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => MultisigBuilder.BuildWitnessScript(requiredSignatures, keys));
    }

    [Fact]
    public void DeriveAddresses_ProducesTenDistinctP2wshAddresses()
    {
        var participants = MultisigParticipantSelector.FromDerivedWallets(MultisigParticipantTests.BuildWallets(3));

        var addresses = MultisigBuilder.DeriveAddresses(requiredSignatures: 2, participants);

        Assert.Equal(WalletConstants.ADDRESSES_PER_ACCOUNT, addresses.Count);
        Assert.Equal(addresses.Count, addresses.Select(a => a.Address).Distinct().Count());

        // P2WSH usa um programa de testemunha de 32 bytes: bech32 mais longo que o P2WPKH.
        Assert.All(addresses, a => Assert.StartsWith("bc1q", a.Address, StringComparison.Ordinal));
        Assert.All(addresses, a => Assert.Equal(62, a.Address.Length));
    }

    [Fact]
    public void DeriveAddresses_AreDeterministic()
    {
        var first = MultisigBuilder.DeriveAddresses(2, MultisigParticipantSelector.FromDerivedWallets(MultisigParticipantTests.BuildWallets(3)));
        var second = MultisigBuilder.DeriveAddresses(2, MultisigParticipantSelector.FromDerivedWallets(MultisigParticipantTests.BuildWallets(3)));

        Assert.Equal(first.Select(a => a.Address), second.Select(a => a.Address));
    }

    [Fact]
    public void DeriveAddresses_DifferentThreshold_ProducesDifferentAddresses()
    {
        var participants = MultisigParticipantSelector.FromDerivedWallets(MultisigParticipantTests.BuildWallets(3));

        var twoOfThree = MultisigBuilder.DeriveAddresses(2, participants);
        var threeOfThree = MultisigBuilder.DeriveAddresses(3, participants);

        Assert.NotEqual(twoOfThree[0].Address, threeOfThree[0].Address);
    }

    [Fact]
    public void DeriveAddresses_CarryTheBip48Path()
    {
        var addresses = MultisigBuilder.DeriveAddresses(2, MultisigParticipantSelector.FromDerivedWallets(MultisigParticipantTests.BuildWallets(2)));

        Assert.Equal("m/48'/0'/0'/2'/0/0", addresses[0].FullPath);
        Assert.Equal("m/48'/0'/0'/2'/0/9", addresses[9].FullPath);
    }

    private static ExtKey MasterKey() => ExtKey.Parse(Bip85Vectors.MasterRootXprv, Network.Main);
}
