using BtcVaultProtocol.Wallet.Core.Models;
using NBitcoin;

namespace BtcVaultProtocol.Wallet.Core.Derivation;

/// <summary>
/// Deriva a conta de uma semente mnemônica, opcionalmente com passphrase, e monta a
/// <see cref="WalletAccount"/> correspondente com seus endereços de recebimento.
/// </summary>
public static class AccountDeriver
{
    /// <summary>
    /// Conta de assinatura única — m/84'/0'/0', serializada como zpub, com endereços P2WPKH.
    /// </summary>
    public static WalletAccount DeriveSingleSig(Mnemonic mnemonic, string? passphrase = null)
    {
        ArgumentNullException.ThrowIfNull(mnemonic);

        var (accountKey, fingerprint) = DeriveAccountKey(mnemonic, passphrase, DerivationPaths.SingleSigAccount);
        var accountExtPubKey = accountKey.Neuter();

        return new WalletAccount(
            DerivationPaths.SingleSigAccount,
            accountExtPubKey,
            Slip132Serializer.ToZpub(accountExtPubKey),
            fingerprint,
            AddressDeriver.DeriveSingleSigAddresses(accountExtPubKey, DerivationPaths.SingleSigAccount));
    }

    /// <summary>
    /// Conta de participante multisig — m/48'/0'/0'/2', serializada como Zpub. Os endereços do
    /// esquema não pertencem a um participante isolado: são compostos por
    /// <see cref="Multisig.MultisigBuilder"/> e ficam vazios aqui.
    /// </summary>
    public static WalletAccount DeriveMultisigParticipant(Mnemonic mnemonic, string? passphrase = null)
    {
        ArgumentNullException.ThrowIfNull(mnemonic);

        var (accountKey, fingerprint) = DeriveAccountKey(mnemonic, passphrase, DerivationPaths.MultisigAccount);
        var accountExtPubKey = accountKey.Neuter();

        return new WalletAccount(
            DerivationPaths.MultisigAccount,
            accountExtPubKey,
            Slip132Serializer.ToMultisigZpub(accountExtPubKey),
            fingerprint,
            Addresses: []);
    }

    private static (ExtKey AccountKey, HDFingerprint Fingerprint) DeriveAccountKey(
        Mnemonic mnemonic,
        string? passphrase,
        KeyPath accountPath)
    {
        // A passphrase BIP39 vale exatamente como digitada: espaços nas bordas são
        // significativos e nenhuma normalização adicional é aplicada além do NFKD do padrão,
        // já implementado pela NBitcoin.
        var rootKey = mnemonic.DeriveExtKey(passphrase);
        var fingerprint = rootKey.Neuter().PubKey.GetHDFingerPrint();

        return (rootKey.Derive(accountPath), fingerprint);
    }
}
