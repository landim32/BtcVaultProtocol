using System.Text;

namespace BtcVaultProtocol.Wallet.Console.Prompts;

/// <summary>
/// Leitura de passphrase sem eco, com confirmação obrigatória (FR-013, FR-014, R-010).
/// </summary>
public static class PassphrasePrompt
{
    /// <summary>
    /// Lê uma passphrase não vazia, exigindo confirmação idêntica. Repete até obter as duas
    /// digitações coincidentes.
    /// </summary>
    /// <param name="alreadyEntered">Passphrases já informadas para a mesma carteira, usadas
    /// apenas para avisar sobre redundância (FR-016).</param>
    public static string Read(IConsoleIo io, int number, int total, IReadOnlyList<string> alreadyEntered)
    {
        ArgumentNullException.ThrowIfNull(io);
        ArgumentNullException.ThrowIfNull(alreadyEntered);

        while (true)
        {
            io.WriteLine($"Passphrase {number} of {total}:");
            var first = ReadSecret(io);

            if (first.Length == 0)
            {
                io.WriteLine("An empty passphrase is the same as no passphrase, which is already shown. Enter a non-empty one.");
                io.WriteLine();
                continue;
            }

            io.WriteLine($"Confirm passphrase {number} of {total}:");
            var confirmation = ReadSecret(io);

            if (!string.Equals(first, confirmation, StringComparison.Ordinal))
            {
                io.WriteLine("Passphrases do not match. Try again.");
                io.WriteLine();
                continue;
            }

            var duplicateIndex = alreadyEntered
                .Select((value, index) => (value, index))
                .Where(item => string.Equals(item.value, first, StringComparison.Ordinal))
                .Select(item => item.index + 1)
                .FirstOrDefault();

            if (duplicateIndex > 0)
            {
                io.WriteLine($"WARNING: identical to passphrase {duplicateIndex} — it produces the same wallet.");
            }

            io.WriteLine("  OK.");
            io.WriteLine();
            return first;
        }
    }

    /// <summary>
    /// Lê caracteres sem eco algum — nem asteriscos. Backspace apaga o último caractere.
    /// </summary>
    private static string ReadSecret(IConsoleIo io)
    {
        var builder = new StringBuilder();

        io.Write("> ");

        while (true)
        {
            var key = io.ReadKey(intercept: true);

            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    io.WriteLine();
                    return builder.ToString();

                case ConsoleKey.Backspace when builder.Length > 0:
                    builder.Length--;
                    break;

                default:
                    if (!char.IsControl(key.KeyChar))
                    {
                        builder.Append(key.KeyChar);
                    }

                    break;
            }
        }
    }
}
