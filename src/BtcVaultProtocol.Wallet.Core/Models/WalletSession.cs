namespace BtcVaultProtocol.Wallet.Core.Models;

/// <summary>
/// Agregação de tudo o que a execução produziu — entrada única do renderizador. Nunca é
/// serializada nem persistida.
/// </summary>
public sealed record WalletSession(
    MasterWallet Master,
    IReadOnlyList<DerivedWallet> DerivedWallets,
    MultisigWallet? Multisig);
