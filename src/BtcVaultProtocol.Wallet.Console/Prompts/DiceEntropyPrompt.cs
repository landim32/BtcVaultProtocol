using BtcVaultProtocol.Wallet.Core;
using BtcVaultProtocol.Wallet.Core.Entropy;

namespace BtcVaultProtocol.Wallet.Console.Prompts;

/// <summary>
/// Coleta guiada da entropia manual — lançamentos de dado de 6 faces (FR-004, FR-005, R-002).
/// </summary>
public static class DiceEntropyPrompt
{
    public static byte[] Collect(IConsoleIo io)
    {
        ArgumentNullException.ThrowIfNull(io);

        io.WriteLine("--- Step 1 of 4: entropy ---");
        io.WriteLine();
        io.WriteLine("Entropy comes from two mandatory sources: your dice rolls and the operating");
        io.WriteLine("system CSPRNG. Both are combined; neither can be skipped.");
        io.WriteLine();
        io.WriteLine($"Roll a {WalletConstants.DICE_FACES}-sided die and type each result (1-{WalletConstants.DICE_FACES}).");
        io.WriteLine("Press Enter on an empty line when finished.");
        io.WriteLine();

        var rolls = new List<byte>(WalletConstants.MIN_DICE_ROLLS);

        while (true)
        {
            ReportProgress(io, rolls.Count);
            io.Write("> ");

            var line = io.ReadLine();

            if (line is null)
            {
                throw new InvalidOperationException("Input stream closed before enough entropy was collected.");
            }

            var trimmed = line.Trim();

            if (trimmed.Length == 0)
            {
                if (EntropyStrength.IsSufficient(rolls.Count))
                {
                    break;
                }

                io.WriteLine(
                    $"Not enough entropy yet: {EntropyStrength.RemainingBits(rolls.Count)} more bits needed " +
                    $"({EntropyStrength.RemainingRolls(rolls.Count)} more rolls).");
                io.WriteLine();
                continue;
            }

            if (!TryParseRolls(trimmed, out var parsed))
            {
                io.WriteLine($"Invalid input: only digits 1-{WalletConstants.DICE_FACES} are accepted.");
                io.WriteLine();
                continue;
            }

            rolls.AddRange(parsed);
        }

        if (EntropyStrength.LooksDegenerate(rolls))
        {
            io.WriteLine("WARNING: your rolls look non-random. The CSPRNG still protects this seed.");
        }

        io.WriteLine();
        return [.. rolls];
    }

    private static void ReportProgress(IConsoleIo io, int rollCount)
    {
        var bits = EntropyStrength.BitsFor(rollCount);
        var suffix = EntropyStrength.IsSufficient(rollCount) ? " — minimum reached" : string.Empty;

        io.WriteLine($"Progress: {bits} / {WalletConstants.ENTROPY_BITS} bits ({rollCount} rolls){suffix}");
    }

    private static bool TryParseRolls(string input, out List<byte> rolls)
    {
        rolls = [];

        foreach (var character in input)
        {
            if (char.IsWhiteSpace(character))
            {
                continue;
            }

            if (character < '1' || character > '0' + WalletConstants.DICE_FACES)
            {
                rolls = [];
                return false;
            }

            rolls.Add((byte)(character - '0'));
        }

        return rolls.Count > 0;
    }
}
