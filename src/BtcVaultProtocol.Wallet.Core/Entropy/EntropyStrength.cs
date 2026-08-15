namespace BtcVaultProtocol.Wallet.Core.Entropy;

/// <summary>
/// Contabiliza a força da entropia manual coletada (R-002, FR-005).
///
/// Cada lançamento de um dado de <see cref="WalletConstants.DICE_FACES"/> faces contribui
/// log2(6) ≈ 2,585 bits. A contagem é truncada para baixo — nunca superestimamos a entropia
/// disponível.
/// </summary>
public static class EntropyStrength
{
    private static readonly double BitsPerRoll = Math.Log2(WalletConstants.DICE_FACES);

    /// <summary>Bits acumulados por <paramref name="rollCount"/> lançamentos.</summary>
    public static int BitsFor(int rollCount)
    {
        if (rollCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rollCount), rollCount, "Roll count cannot be negative.");
        }

        return (int)Math.Floor(rollCount * BitsPerRoll);
    }

    /// <summary>Indica se os lançamentos já atingem os 256 bits exigidos.</summary>
    public static bool IsSufficient(int rollCount) =>
        BitsFor(rollCount) >= WalletConstants.ENTROPY_BITS;

    /// <summary>Quantos lançamentos ainda faltam para atingir o mínimo.</summary>
    public static int RemainingRolls(int rollCount) =>
        Math.Max(0, WalletConstants.MIN_DICE_ROLLS - rollCount);

    /// <summary>Quantos bits ainda faltam para atingir o mínimo.</summary>
    public static int RemainingBits(int rollCount) =>
        Math.Max(0, WalletConstants.ENTROPY_BITS - BitsFor(rollCount));

    /// <summary>
    /// Indica se a sequência é visivelmente degenerada (todos os valores iguais). A semente
    /// permanece segura por causa da mistura com o CSPRNG, mas o usuário é avisado.
    /// </summary>
    public static bool LooksDegenerate(IReadOnlyList<byte> rolls)
    {
        ArgumentNullException.ThrowIfNull(rolls);

        return rolls.Count > 1 && rolls.Distinct().Count() == 1;
    }
}
