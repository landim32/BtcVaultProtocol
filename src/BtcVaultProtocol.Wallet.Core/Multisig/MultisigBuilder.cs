using BtcVaultProtocol.Wallet.Core.Derivation;
using BtcVaultProtocol.Wallet.Core.Models;
using NBitcoin;

namespace BtcVaultProtocol.Wallet.Core.Multisig;

/// <summary>
/// Compõe o esquema multisig P2WSH (R-007).
///
/// As chaves públicas de cada índice de endereço são ordenadas lexicograficamente (BIP67) antes
/// de compor o script — é o que Sparrow, Electrum e Specter fazem por padrão (`sortedmulti`).
/// Sem essa ordenação os endereços não reproduziriam em nenhuma carteira de referência.
/// </summary>
public static class MultisigBuilder
{
    /// <summary>Monta o witnessScript multisig de um índice, com as chaves ordenadas por BIP67.</summary>
    public static Script BuildWitnessScript(int requiredSignatures, IEnumerable<PubKey> pubKeys)
    {
        ArgumentNullException.ThrowIfNull(pubKeys);

        var sorted = SortByBip67(pubKeys);

        if (requiredSignatures < 1 || requiredSignatures > sorted.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requiredSignatures),
                requiredSignatures,
                $"Required signatures must be between 1 and {sorted.Length}.");
        }

        return PayToMultiSigTemplate.Instance.GenerateScriptPubKey(requiredSignatures, sorted);
    }

    /// <summary>Ordenação lexicográfica das chaves públicas comprimidas (BIP67).</summary>
    public static PubKey[] SortByBip67(IEnumerable<PubKey> pubKeys)
    {
        ArgumentNullException.ThrowIfNull(pubKeys);

        return pubKeys
            .OrderBy(key => key.ToHex(), StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>
    /// Os <see cref="WalletConstants.ADDRESSES_PER_ACCOUNT"/> primeiros endereços P2WSH do
    /// esquema. Cada índice combina a chave correspondente de todos os participantes.
    /// </summary>
    public static IReadOnlyList<AddressEntry> DeriveAddresses(
        int requiredSignatures,
        IReadOnlyList<MultisigParticipant> participants)
    {
        ArgumentNullException.ThrowIfNull(participants);

        var entries = new List<AddressEntry>(WalletConstants.ADDRESSES_PER_ACCOUNT);

        for (var index = 0; index < WalletConstants.ADDRESSES_PER_ACCOUNT; index++)
        {
            var pubKeys = participants
                .Select(p => AddressDeriver.ReceivePubKey(p.Account.AccountExtPubKey, index));

            var witnessScript = BuildWitnessScript(requiredSignatures, pubKeys);
            var address = witnessScript.WitHash.GetAddress(Network.Main);

            entries.Add(new AddressEntry(
                index,
                $"m/{DerivationPaths.MultisigAccount}/{DerivationPaths.ReceiveAddress(index)}",
                address.ToString()));
        }

        return entries;
    }
}
