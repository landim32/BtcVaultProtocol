using System.Security.Cryptography;

namespace BtcVaultProtocol.Wallet.Core.Entropy;

/// <summary>
/// Combina as duas fontes obrigatórias de entropia (FR-004, R-001):
///
///     entropia_final = SHA256(entrada_manual) XOR CSPRNG(32)
///
/// O XOR garante a propriedade exigida: o resultado é uniforme se QUALQUER UMA das parcelas
/// for uniforme. Se o CSPRNG estiver comprometido, quem não conhece os lançamentos de dado não
/// prevê a semente; se o usuário digitar uma sequência degenerada, o CSPRNG sozinho já garante
/// a imprevisibilidade.
/// </summary>
public static class EntropyMixer
{
    /// <summary>
    /// Produz os 32 bytes de entropia da semente mestre. O chamador é dono do array retornado e
    /// deve zerá-lo após o uso (Princípio VI).
    /// </summary>
    /// <exception cref="ArgumentException">Se a entropia manual for insuficiente.</exception>
    /// <exception cref="CsprngUnavailableException">Se o CSPRNG estiver indisponível.</exception>
    public static byte[] Mix(IReadOnlyList<byte> manualRolls, IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(manualRolls);
        ArgumentNullException.ThrowIfNull(randomSource);

        if (!EntropyStrength.IsSufficient(manualRolls.Count))
        {
            throw new ArgumentException(
                $"Manual entropy is insufficient: {EntropyStrength.BitsFor(manualRolls.Count)} of " +
                $"{WalletConstants.ENTROPY_BITS} bits.",
                nameof(manualRolls));
        }

        var manualBytes = manualRolls.ToArray();
        var manualDigest = new byte[WalletConstants.ENTROPY_BYTES];
        var systemBytes = new byte[WalletConstants.ENTROPY_BYTES];

        try
        {
            SHA256.HashData(manualBytes, manualDigest);

            // Se o CSPRNG falhar, a exceção sobe e nada é gerado — jamais recorremos apenas à
            // entropia manual (FR-007).
            randomSource.Fill(systemBytes);

            var mixed = new byte[WalletConstants.ENTROPY_BYTES];
            for (var i = 0; i < WalletConstants.ENTROPY_BYTES; i++)
            {
                mixed[i] = (byte)(manualDigest[i] ^ systemBytes[i]);
            }

            return mixed;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(manualBytes);
            CryptographicOperations.ZeroMemory(manualDigest);
            CryptographicOperations.ZeroMemory(systemBytes);
        }
    }
}
