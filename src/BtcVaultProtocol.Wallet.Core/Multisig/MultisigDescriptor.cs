using BtcVaultProtocol.Wallet.Core.Models;

namespace BtcVaultProtocol.Wallet.Core.Multisig;

/// <summary>
/// Monta o output descriptor do esquema — wsh(sortedmulti(M, ...)) (R-007).
///
/// No multisig, a chave estendida sozinha não define o esquema: sem M, sem o conjunto de
/// cossignatários e sem a regra de ordenação, o usuário não consegue reproduzir os endereços em
/// outra carteira. O descriptor é o que torna a seção efetivamente verificável.
/// </summary>
public static class MultisigDescriptor
{
    public static string Build(int requiredSignatures, IReadOnlyList<MultisigParticipant> participants)
    {
        ArgumentNullException.ThrowIfNull(participants);

        var keys = participants.Select(participant =>
        {
            var origin = $"{participant.Account.Fingerprint}{FormatPath(participant.Account.DerivationPath.ToString())}";
            return $"[{origin}]{participant.Account.Serialized}/{WalletConstants.RECEIVE_CHAIN}/*";
        });

        return $"wsh(sortedmulti({requiredSignatures},{string.Join(',', keys)}))";
    }

    /// <summary>
    /// Converte o caminho para a notação com "h" usada em descriptors — 48'/0'/0'/2' vira
    /// /48h/0h/0h/2h.
    /// </summary>
    private static string FormatPath(string accountPath) =>
        "/" + accountPath.Replace("'", "h", StringComparison.Ordinal);
}
