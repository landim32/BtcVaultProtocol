using BtcVaultProtocol.Wallet.Core;
using BtcVaultProtocol.Wallet.Core.Bip85;
using BtcVaultProtocol.Wallet.Core.Derivation;
using BtcVaultProtocol.Wallet.Core.Models;
using BtcVaultProtocol.Wallet.Core.Multisig;
using BtcVaultProtocol.Wallet.Tests.Vectors;
using NBitcoin;

namespace BtcVaultProtocol.Wallet.Tests.Multisig;

/// <summary>
/// FR-019: o esquema multisig combina as carteiras JÁ criadas, cada uma com a sua ÚLTIMA
/// passphrase. Nenhuma carteira nova é gerada, e o número de participantes é a quantidade de
/// carteiras derivadas.
/// </summary>
public sealed class MultisigParticipantTests
{
    [Fact]
    public void FromDerivedWallets_ReusesTheExistingWalletsSeeds()
    {
        var wallets = BuildWallets(3);

        var participants = MultisigParticipantSelector.FromDerivedWallets(wallets);

        Assert.Equal(
            wallets.Select(w => w.Mnemonic.ToString()),
            participants.Select(p => p.Mnemonic.ToString()));
    }

    [Fact]
    public void FromDerivedWallets_ParticipantCountEqualsWalletCount()
    {
        foreach (var count in new[] { 2, 5, WalletConstants.MAX_DERIVED_WALLETS })
        {
            var participants = MultisigParticipantSelector.FromDerivedWallets(BuildWallets(count));

            Assert.Equal(count, participants.Count);
        }
    }

    [Fact]
    public void FromDerivedWallets_UsesTheLastPassphraseOfEachWallet()
    {
        var wallets = BuildWallets(2, passphrasesPerWallet: 3);

        var participants = MultisigParticipantSelector.FromDerivedWallets(wallets);

        Assert.Equal(
            wallets.Select(w => w.PassphraseWallets[^1].Passphrase),
            participants.Select(p => p.Passphrase));
    }

    [Fact]
    public void FromDerivedWallets_LastPassphraseAccount_MatchesDirectDerivation()
    {
        var wallets = BuildWallets(2, passphrasesPerWallet: 2);

        var participant = MultisigParticipantSelector.FromDerivedWallets(wallets)[0];
        var expected = AccountDeriver.DeriveMultisigParticipant(
            wallets[0].Mnemonic,
            wallets[0].PassphraseWallets[^1].Passphrase);

        Assert.Equal(expected.Serialized, participant.Account.Serialized);
    }

    [Fact]
    public void FromDerivedWallets_WalletWithoutPassphrase_UsesNoPassphrase()
    {
        var wallets = BuildWallets(2, passphrasesPerWallet: 0);

        var participants = MultisigParticipantSelector.FromDerivedWallets(wallets);

        Assert.All(participants, p => Assert.Null(p.Passphrase));
        Assert.All(participants, p =>
            Assert.Equal(
                AccountDeriver.DeriveMultisigParticipant(p.Mnemonic).Serialized,
                p.Account.Serialized));
    }

    [Fact]
    public void FromDerivedWallets_DifferentLastPassphrase_ChangesTheParticipantKey()
    {
        var withoutPassphrase = MultisigParticipantSelector.FromDerivedWallets(BuildWallets(2, 0));
        var withPassphrase = MultisigParticipantSelector.FromDerivedWallets(BuildWallets(2, 1));

        Assert.NotEqual(withoutPassphrase[0].Account.Serialized, withPassphrase[0].Account.Serialized);
    }

    [Fact]
    public void FromDerivedWallets_UsesBip48PathSerializedAsCapitalZpub()
    {
        var participants = MultisigParticipantSelector.FromDerivedWallets(BuildWallets(3, 1));

        Assert.All(participants, p =>
        {
            Assert.Equal("48'/0'/0'/2'", p.Account.DerivationPath.ToString());
            Assert.StartsWith("Zpub", p.Account.Serialized, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void FromDerivedWallets_DisplayNumbersMatchTheOriginWallets()
    {
        var participants = MultisigParticipantSelector.FromDerivedWallets(BuildWallets(3));

        Assert.Equal([1, 2, 3], participants.Select(p => p.DisplayNumber));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void FromDerivedWallets_WithFewerThanTwoWallets_Throws(int walletCount)
    {
        Assert.Throws<ArgumentException>(
            () => MultisigParticipantSelector.FromDerivedWallets(BuildWallets(walletCount)));
    }

    [Fact]
    public void Descriptor_DeclaresSortedMultiWithOriginAndReceiveChain()
    {
        var participants = MultisigParticipantSelector.FromDerivedWallets(BuildWallets(3, 1));

        var descriptor = MultisigDescriptor.Build(2, participants);

        Assert.StartsWith("wsh(sortedmulti(2,", descriptor, StringComparison.Ordinal);
        Assert.EndsWith("))", descriptor, StringComparison.Ordinal);
        Assert.Equal(3, descriptor.Split("Zpub").Length - 1);
        Assert.Contains("/48h/0h/0h/2h]", descriptor, StringComparison.Ordinal);
        Assert.Contains("/0/*", descriptor, StringComparison.Ordinal);
    }

    internal static IReadOnlyList<DerivedWallet> BuildWallets(int count, int passphrasesPerWallet = 1)
    {
        var master = ExtKey.Parse(Bip85Vectors.MasterRootXprv, Network.Main);
        var wallets = new List<DerivedWallet>(count);

        for (var index = 0; index < count; index++)
        {
            var mnemonic = Bip85Deriver.DeriveMnemonic(master, (uint)index);

            var passphraseWallets = Enumerable
                .Range(1, passphrasesPerWallet)
                .Select(number =>
                {
                    var passphrase = $"passphrase-{index}-{number}";
                    return new PassphraseWallet(passphrase, AccountDeriver.DeriveSingleSig(mnemonic, passphrase));
                })
                .ToList();

            wallets.Add(new DerivedWallet(
                index,
                mnemonic,
                AccountDeriver.DeriveSingleSig(mnemonic),
                passphraseWallets));
        }

        return wallets;
    }
}
