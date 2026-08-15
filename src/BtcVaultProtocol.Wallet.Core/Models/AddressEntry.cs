namespace BtcVaultProtocol.Wallet.Core.Models;

/// <summary>
/// Um endereço de recebimento exibido no relatório.
/// </summary>
public sealed record AddressEntry(int Index, string FullPath, string Address);
