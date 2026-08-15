namespace BtcVaultProtocol.Wallet.Console.Prompts;

/// <summary>
/// Pergunta de sim/não tolerante a maiúsculas/minúsculas, com padrão explícito (FR-003).
/// </summary>
public static class YesNoPrompt
{
    public static bool Ask(IConsoleIo io, string question, bool defaultAnswer)
    {
        ArgumentNullException.ThrowIfNull(io);

        var hint = defaultAnswer ? "[Y/n]" : "[y/N]";

        while (true)
        {
            io.WriteLine($"{question} {hint}");
            io.Write("> ");

            var answer = io.ReadLine()?.Trim() ?? string.Empty;

            if (answer.Length == 0)
            {
                return defaultAnswer;
            }

            switch (answer.ToLowerInvariant())
            {
                case "y":
                case "yes":
                    return true;
                case "n":
                case "no":
                    return false;
                default:
                    io.WriteLine("Please answer y or n.");
                    io.WriteLine();
                    break;
            }
        }
    }
}
