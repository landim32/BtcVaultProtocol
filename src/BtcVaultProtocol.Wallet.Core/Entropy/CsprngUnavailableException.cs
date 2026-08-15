namespace BtcVaultProtocol.Wallet.Core.Entropy;

/// <summary>
/// O CSPRNG do sistema operacional está indisponível. Nenhuma semente é gerada nesse caso: a
/// entropia manual jamais é usada sozinha (FR-007, Princípio I).
/// </summary>
public sealed class CsprngUnavailableException : Exception
{
    public CsprngUnavailableException()
        : base("The operating system CSPRNG is unavailable.")
    {
    }

    public CsprngUnavailableException(Exception innerException)
        : base("The operating system CSPRNG is unavailable.", innerException)
    {
    }
}
