using BtcVaultProtocol.Wallet.Core.Derivation;
using BtcVaultProtocol.Wallet.Core.Models;

namespace BtcVaultProtocol.Wallet.Core.Multisig;

/// <summary>
/// Monta os participantes do esquema multisig a partir das carteiras JÁ criadas (FR-019).
///
/// Nenhuma carteira nova é gerada: cada carteira derivada vira um cossignatário, usando a sua
/// ÚLTIMA passphrase. Carteira sem passphrase entra com a conta sem passphrase. O número de
/// participantes é, portanto, a quantidade de carteiras criadas — o usuário escolhe apenas
/// quantas assinaturas serão exigidas.
/// </summary>
public static class MultisigParticipantSelector
{
    public static IReadOnlyList<MultisigParticipant> FromDerivedWallets(IReadOnlyList<DerivedWallet> wallets)
    {
        ArgumentNullException.ThrowIfNull(wallets);

        if (wallets.Count < WalletConstants.MIN_MULTISIG_PARTICIPANTS)
        {
            throw new ArgumentException(
                $"A multisig scheme needs at least {WalletConstants.MIN_MULTISIG_PARTICIPANTS} derived wallets, " +
                $"got {wallets.Count}.",
                nameof(wallets));
        }

        return [.. wallets.Select(ToParticipant)];
    }

    private static MultisigParticipant ToParticipant(DerivedWallet wallet)
    {
        var passphrase = wallet.PassphraseWallets.Count > 0
            ? wallet.PassphraseWallets[^1].Passphrase
            : null;

        return new MultisigParticipant(
            wallet.Index,
            wallet.Mnemonic,
            passphrase,
            AccountDeriver.DeriveMultisigParticipant(wallet.Mnemonic, passphrase));
    }
}
