namespace BtcVaultProtocol.Wallet.Core.Entropy;

/// <summary>
/// Fonte de aleatoriedade do sistema operacional. Existe como abstração apenas para permitir
/// testar o caminho de indisponibilidade (FR-007) — em produção há uma única implementação,
/// sobre <see cref="System.Security.Cryptography.RandomNumberGenerator"/>.
/// </summary>
public interface IRandomSource
{
    /// <summary>Preenche <paramref name="buffer"/> com bytes criptograficamente seguros.</summary>
    /// <exception cref="CsprngUnavailableException">Se o CSPRNG do sistema falhar.</exception>
    void Fill(Span<byte> buffer);
}
