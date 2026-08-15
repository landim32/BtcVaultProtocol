using BtcVaultProtocol.Wallet.Console;
using BtcVaultProtocol.Wallet.Console.Flow;
using BtcVaultProtocol.Wallet.Console.Rendering;
using BtcVaultProtocol.Wallet.Core.Entropy;
using SysConsole = System.Console;

var io = SystemConsoleIo.Instance;
var secureExit = new SecureExit();

// Ctrl+C também limpa o material sensível antes de encerrar (Princípio VI, FR-027).
SysConsole.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    secureExit.WipeAll();
    io.WriteLine();
    io.WriteLine("Interrupted. Sensitive data wiped from memory. Nothing was saved.");
    Environment.Exit(ExitCodes.Interrupted);
};

try
{
    var session = new WalletFlow(io, SystemRandomSource.Instance, secureExit).Run();
    ReportRenderer.Render(io, session);

    return ExitCodes.Success;
}
catch (CsprngUnavailableException)
{
    io.WriteLine();
    io.WriteLine("FATAL: the operating system CSPRNG is unavailable. No wallet was generated.");
    return ExitCodes.CsprngUnavailable;
}
catch (Exception)
{
    // Mensagem genérica de propósito: nem stack trace nem fragmento de material sensível pode
    // escapar por aqui ("Restrições Adicionais → Tratamento de Erros").
    io.WriteLine();
    io.WriteLine("FATAL: unexpected error. Nothing was saved and sensitive data was wiped.");
    return ExitCodes.Interrupted;
}
finally
{
    secureExit.WipeAll();
}

internal static class ExitCodes
{
    public const int Success = 0;
    public const int Interrupted = 1;
    public const int CsprngUnavailable = 2;
}
