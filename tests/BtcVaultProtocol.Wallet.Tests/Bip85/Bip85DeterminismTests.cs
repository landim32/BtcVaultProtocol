using BtcVaultProtocol.Wallet.Core;
using BtcVaultProtocol.Wallet.Core.Bip85;
using BtcVaultProtocol.Wallet.Tests.Vectors;
using NBitcoin;

namespace BtcVaultProtocol.Wallet.Tests.Bip85;

/// <summary>
/// FR-010 (determinismo) e FR-019 / R-004 (separação dos espaços de índices).
/// </summary>
public sealed class Bip85DeterminismTests
{
    [Fact]
    public void SameMasterAndIndex_AlwaysProduceTheSameMnemonic()
    {
        for (uint index = 0; index < WalletConstants.MAX_DERIVED_WALLETS; index++)
        {
            var first = Bip85Deriver.DeriveMnemonic(MasterKey(), index);
            var second = Bip85Deriver.DeriveMnemonic(MasterKey(), index);

            Assert.Equal(first.ToString(), second.ToString());
        }
    }

    [Fact]
    public void AllDerivedWallets_AreDistinct()
    {
        var mnemonics = Enumerable
            .Range(0, WalletConstants.MAX_DERIVED_WALLETS)
            .Select(i => Bip85Deriver.DeriveMnemonic(MasterKey(), (uint)i).ToString())
            .ToList();

        Assert.Equal(mnemonics.Count, mnemonics.Distinct(StringComparer.Ordinal).Count());
    }

    private static ExtKey MasterKey() =>
        ExtKey.Parse(Bip85Vectors.MasterRootXprv, Network.Main);
}
