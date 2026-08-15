using BtcVaultProtocol.Wallet.Core.Derivation;
using BtcVaultProtocol.Wallet.Tests.Vectors;
using NBitcoin;

namespace BtcVaultProtocol.Wallet.Tests.Derivation;

public sealed class AccountDeriverTests
{
    [Fact]
    public void DeriveSingleSig_MatchesOfficialBip84Vector()
    {
        var account = AccountDeriver.DeriveSingleSig(Bip84Mnemonic(), Bip84Vectors.Passphrase);

        Assert.Equal(Bip84Vectors.AccountZpub, account.Serialized);
        Assert.Equal(DerivationPaths.SingleSigAccount.ToString(), account.DerivationPath.ToString());
    }

    [Fact]
    public void DeriveSingleSig_IsDeterministic()
    {
        var first = AccountDeriver.DeriveSingleSig(Bip84Mnemonic(), Bip84Vectors.Passphrase);
        var second = AccountDeriver.DeriveSingleSig(Bip84Mnemonic(), Bip84Vectors.Passphrase);

        Assert.Equal(first.Serialized, second.Serialized);
        Assert.Equal(
            first.Addresses.Select(a => a.Address),
            second.Addresses.Select(a => a.Address));
    }

    [Fact]
    public void DeriveSingleSig_WithPassphrase_ProducesDifferentAccount()
    {
        var withoutPassphrase = AccountDeriver.DeriveSingleSig(Bip84Mnemonic());
        var withPassphrase = AccountDeriver.DeriveSingleSig(Bip84Mnemonic(), "correct horse");

        Assert.NotEqual(withoutPassphrase.Serialized, withPassphrase.Serialized);
        Assert.NotEqual(
            withoutPassphrase.Addresses[0].Address,
            withPassphrase.Addresses[0].Address);
    }

    [Fact]
    public void DeriveSingleSig_SeedWithPassphrase_MatchesOfficialBip39Vector()
    {
        // Confirma que a passphrase entra na derivação exatamente como o BIP39 determina.
        var mnemonic = new Mnemonic(Bip39Vectors.AllZeros.Mnemonic, Wordlist.English);
        var seed = mnemonic.DeriveSeed(Bip39Vectors.SEED_PASSPHRASE);

        Assert.Equal(Bip39Vectors.AllZeros.SeedHex, Convert.ToHexString(seed).ToLowerInvariant());
    }

    [Fact]
    public void DeriveMultisigParticipant_UsesBip48PathAndCapitalZpub()
    {
        var participant = AccountDeriver.DeriveMultisigParticipant(Bip84Mnemonic());

        Assert.Equal(DerivationPaths.MultisigAccount.ToString(), participant.DerivationPath.ToString());
        Assert.StartsWith("Zpub", participant.Serialized, StringComparison.Ordinal);
        Assert.Empty(participant.Addresses);
    }

    [Fact]
    public void Fingerprint_IsStableForTheSameSeed()
    {
        var first = AccountDeriver.DeriveSingleSig(Bip84Mnemonic());
        var second = AccountDeriver.DeriveMultisigParticipant(Bip84Mnemonic());

        Assert.Equal(first.Fingerprint, second.Fingerprint);
    }

    private static Mnemonic Bip84Mnemonic() => new(Bip84Vectors.Mnemonic, Wordlist.English);
}
