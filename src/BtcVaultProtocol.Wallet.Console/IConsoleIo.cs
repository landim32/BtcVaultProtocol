namespace BtcVaultProtocol.Wallet.Console;

/// <summary>
/// Abstração mínima sobre o terminal. Existe para que as regras de validação de entrada possam
/// ser testadas sem um console real — em produção há uma única implementação,
/// <see cref="SystemConsoleIo"/>.
/// </summary>
public interface IConsoleIo
{
    bool IsOutputRedirected { get; }

    string? ReadLine();

    ConsoleKeyInfo ReadKey(bool intercept);

    void Write(string text);

    void WriteLine(string text = "");
}
