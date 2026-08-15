using NBitcoin;

namespace BtcVaultProtocol.Wallet.Core.Derivation;

/// <summary>
/// Caminhos de derivação fixos da sessão (R-005). Native SegWit para assinatura única,
/// BIP48 P2WSH para participantes multisig.
/// </summary>
public static class DerivationPaths
{
    /// <summary>Conta de assinatura única — m/84'/0'/0'.</summary>
    public static KeyPath SingleSigAccount { get; } =
        new(WalletConstants.SINGLE_SIG_ACCOUNT_PATH);

    /// <summary>Conta de participante multisig — m/48'/0'/0'/2'.</summary>
    public static KeyPath MultisigAccount { get; } =
        new(WalletConstants.MULTISIG_ACCOUNT_PATH);

    /// <summary>
    /// Caminho relativo do i-ésimo endereço da cadeia de recebimento — 0/i.
    /// </summary>
    public static KeyPath ReceiveAddress(int index) =>
        new($"{WalletConstants.RECEIVE_CHAIN}/{index}");
}
