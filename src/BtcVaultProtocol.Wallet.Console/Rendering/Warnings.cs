namespace BtcVaultProtocol.Wallet.Console.Rendering;

/// <summary>
/// Avisos operacionais de abertura e de encerramento (FR-028, FR-029).
/// </summary>
public static class Warnings
{
    public static void ShowOpening(IConsoleIo io)
    {
        ArgumentNullException.ThrowIfNull(io);

        io.WriteLine("=== BtcVaultProtocol Wallet — offline Bitcoin key generator ===");
        io.WriteLine();

        if (io.IsOutputRedirected)
        {
            io.WriteLine("WARNING: output is being redirected to a file or pipe.");
            io.WriteLine("         Sensitive material WILL be written to disk. Press Ctrl+C to abort.");
            io.WriteLine();
        }

        io.WriteLine("WARNING: this tool prints private key material to the screen.");
        io.WriteLine("  - Run it on an offline machine you trust.");
        io.WriteLine("  - Make sure your terminal does not record or stream its output.");
        io.WriteLine("  - Nothing is saved: when this window closes, everything is gone.");
        io.WriteLine();
    }

    public static void ShowClearTerminalGuidance(IConsoleIo io)
    {
        ArgumentNullException.ThrowIfNull(io);

        io.WriteLine(" Nothing above was saved. Clear your screen and scrollback now:");
        io.WriteLine("   Windows : cls          Linux/macOS : clear && printf '\\033[3J'");
        io.WriteLine(new string('=', ReportRenderer.SeparatorWidth));
    }
}
