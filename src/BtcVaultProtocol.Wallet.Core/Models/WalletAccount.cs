using NBitcoin;

namespace BtcVaultProtocol.Wallet.Core.Models;

/// <summary>
/// Conta derivada — unidade comum de exibição para qualquer carteira.
/// </summary>
/// <param name="DerivationPath">Caminho da conta (m/84'/0'/0' ou m/48'/0'/0'/2').</param>
/// <param name="AccountExtPubKey">Chave pública estendida da conta.</param>
/// <param name="Serialized">Serialização SLIP-132 coerente com o tipo de script (zpub/Zpub).</param>
/// <param name="Fingerprint">Fingerprint da chave mestre, usada no descriptor.</param>
/// <param name="Addresses">Os <see cref="WalletConstants.ADDRESSES_PER_ACCOUNT"/> primeiros endereços.</param>
public sealed record WalletAccount(
    KeyPath DerivationPath,
    ExtPubKey AccountExtPubKey,
    string Serialized,
    HDFingerprint Fingerprint,
    IReadOnlyList<AddressEntry> Addresses);
