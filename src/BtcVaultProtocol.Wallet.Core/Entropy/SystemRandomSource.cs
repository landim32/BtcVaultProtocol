using System.Security.Cryptography;

namespace BtcVaultProtocol.Wallet.Core.Entropy;

/// <summary>
/// Única fonte de aleatoriedade usada em produção — o CSPRNG do sistema operacional.
/// Princípio I: <c>System.Random</c> não é usado em nenhum caminho que influencie material de
/// chave.
/// </summary>
public sealed class SystemRandomSource : IRandomSource
{
    public static SystemRandomSource Instance { get; } = new();

    public void Fill(Span<byte> buffer)
    {
        try
        {
            RandomNumberGenerator.Fill(buffer);
        }
        catch (Exception ex)
        {
            throw new CsprngUnavailableException(ex);
        }
    }
}
