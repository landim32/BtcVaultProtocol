using NBitcoin;

namespace BtcVaultProtocol.Wallet.Core.Models;

/// <summary>
/// Raiz determinística da sessão. Origina todas as carteiras derivadas e todos os participantes
/// multisig. A mestre nunca recebe passphrase.
/// </summary>
public sealed record MasterWallet(Mnemonic Mnemonic, ExtKey RootKey, WalletAccount Account);
