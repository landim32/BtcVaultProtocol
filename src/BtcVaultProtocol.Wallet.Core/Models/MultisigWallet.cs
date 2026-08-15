namespace BtcVaultProtocol.Wallet.Core.Models;

/// <summary>
/// Esquema multisig M-de-N. Todos os participantes derivam da mesma carteira mestre — o aviso
/// de FR-019b acompanha obrigatoriamente esta seção no relatório.
/// </summary>
public sealed record MultisigWallet(
    int RequiredSignatures,
    IReadOnlyList<MultisigParticipant> Participants,
    string Descriptor,
    IReadOnlyList<AddressEntry> Addresses)
{
    public int ParticipantCount => Participants.Count;
}
