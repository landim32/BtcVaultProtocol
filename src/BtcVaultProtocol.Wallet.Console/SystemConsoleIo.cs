using SysConsole = System.Console;

namespace BtcVaultProtocol.Wallet.Console;

/// <summary>Única implementação usada em produção — o terminal do sistema.</summary>
public sealed class SystemConsoleIo : IConsoleIo
{
    public static SystemConsoleIo Instance { get; } = new();

    public bool IsOutputRedirected => SysConsole.IsOutputRedirected;

    public string? ReadLine() => SysConsole.ReadLine();

    public ConsoleKeyInfo ReadKey(bool intercept) => SysConsole.ReadKey(intercept);

    public void Write(string text) => SysConsole.Write(text);

    public void WriteLine(string text = "") => SysConsole.WriteLine(text);
}
