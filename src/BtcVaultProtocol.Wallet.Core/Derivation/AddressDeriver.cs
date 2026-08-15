using BtcVaultProtocol.Wallet.Core.Models;
using NBitcoin;

namespace BtcVaultProtocol.Wallet.Core.Derivation;

/// <summary>
/// Deriva os endereços de recebimento exibidos para cada conta. Apenas a cadeia de recebimento
/// (.../0/i) é produzida — a cadeia de troco é derivável por quem importar a chave estendida.
/// </summary>
public static class AddressDeriver
{
    /// <summary>
    /// Os <see cref="WalletConstants.ADDRESSES_PER_ACCOUNT"/> primeiros endereços Native SegWit
    /// (P2WPKH) da conta.
    /// </summary>
    public static IReadOnlyList<AddressEntry> DeriveSingleSigAddresses(
        ExtPubKey accountExtPubKey,
        KeyPath accountPath)
    {
        ArgumentNullException.ThrowIfNull(accountExtPubKey);
        ArgumentNullException.ThrowIfNull(accountPath);

        var entries = new List<AddressEntry>(WalletConstants.ADDRESSES_PER_ACCOUNT);

        for (var index = 0; index < WalletConstants.ADDRESSES_PER_ACCOUNT; index++)
        {
            var relativePath = DerivationPaths.ReceiveAddress(index);
            var address = accountExtPubKey
                .Derive(relativePath)
                .PubKey
                .GetAddress(ScriptPubKeyType.Segwit, Network.Main);

            // KeyPath.ToString() da NBitcoin omite o prefixo "m/" — reinserido aqui para que o
            // caminho exibido possa ser colado direto em uma carteira de referência.
            entries.Add(new AddressEntry(index, $"m/{accountPath}/{relativePath}", address.ToString()));
        }

        return entries;
    }

    /// <summary>
    /// Chave pública do i-ésimo endereço de recebimento de uma conta — usada para compor o
    /// script multisig.
    /// </summary>
    public static PubKey ReceivePubKey(ExtPubKey accountExtPubKey, int index)
    {
        ArgumentNullException.ThrowIfNull(accountExtPubKey);

        return accountExtPubKey.Derive(DerivationPaths.ReceiveAddress(index)).PubKey;
    }
}
