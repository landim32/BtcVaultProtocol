using System.Globalization;

namespace BtcVaultProtocol.Wallet.Console.Prompts;

/// <summary>
/// Leitura de inteiro dentro de uma faixa fechada. Entrada inválida repete a pergunta com
/// mensagem explicativa, sem encerrar a execução (FR-002, SC-007).
/// </summary>
public static class IntegerPrompt
{
    public static int Ask(IConsoleIo io, string question, int min, int max)
    {
        ArgumentNullException.ThrowIfNull(io);

        while (true)
        {
            io.WriteLine($"{question} ({min}-{max})");
            io.Write("> ");

            var answer = io.ReadLine()?.Trim() ?? string.Empty;

            if (int.TryParse(answer, NumberStyles.None, CultureInfo.InvariantCulture, out var value) &&
                value >= min && value <= max)
            {
                return value;
            }

            io.WriteLine($"Please enter a whole number between {min} and {max}.");
            io.WriteLine();
        }
    }
}
