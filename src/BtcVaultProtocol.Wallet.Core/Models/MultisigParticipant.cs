using NBitcoin;

namespace BtcVaultProtocol.Wallet.Core.Models;

/// <summary>
/// Um cossignatário do esquema multisig. NÃO é uma carteira nova: é uma das carteiras derivadas
/// já apresentadas no relatório, tomada com a sua ÚLTIMA passphrase (ou sem passphrase, quando a
/// carteira não tiver nenhuma).
/// </summary>
/// <param name="WalletIndex">Índice da carteira derivada de origem.</param>
/// <param name="Mnemonic">As 24 palavras dessa carteira.</param>
/// <param name="Passphrase">A última passphrase da carteira, ou <c>null</c> se não houver.</param>
/// <param name="Account">Conta m/48'/0'/0'/2' derivada dessa semente com essa passphrase.</param>
public sealed record MultisigParticipant(
    int WalletIndex,
    Mnemonic Mnemonic,
    string? Passphrase,
    WalletAccount Account)
{
    /// <summary>Numeração exibida ao usuário — igual à da carteira derivada de origem.</summary>
    public int DisplayNumber => WalletIndex + 1;

    /// <summary>Descreve qual carteira e qual passphrase originaram este participante.</summary>
    public string Origin =>
        Passphrase is null
            ? $"derived wallet {DisplayNumber} (no passphrase)"
            : $"derived wallet {DisplayNumber} (last passphrase)";
}
