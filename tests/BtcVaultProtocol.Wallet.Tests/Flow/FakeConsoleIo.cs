using System.Text;
using BtcVaultProtocol.Wallet.Console;

namespace BtcVaultProtocol.Wallet.Tests.Flow;

/// <summary>
/// Console falso para exercitar as regras de validação sem um terminal real.
/// </summary>
public sealed class FakeConsoleIo(params string[] lines) : IConsoleIo
{
    private readonly Queue<string> _lines = new(lines);
    private readonly Queue<ConsoleKeyInfo> _keys = new();
    private readonly StringBuilder _output = new();

    public bool IsOutputRedirected { get; set; }

    public string Output => _output.ToString();

    public int RemainingLines => _lines.Count;

    /// <summary>Enfileira uma "digitação" seguida de Enter, para prompts sem eco.</summary>
    public FakeConsoleIo TypeSecret(string secret)
    {
        foreach (var character in secret)
        {
            _keys.Enqueue(new ConsoleKeyInfo(character, ConsoleKey.NoName, false, false, false));
        }

        _keys.Enqueue(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        return this;
    }

    /// <summary>Enfileira caracteres sem o Enter final.</summary>
    public FakeConsoleIo Type(string text)
    {
        foreach (var character in text)
        {
            _keys.Enqueue(new ConsoleKeyInfo(character, ConsoleKey.NoName, false, false, false));
        }

        return this;
    }

    public FakeConsoleIo Backspace()
    {
        _keys.Enqueue(new ConsoleKeyInfo('\b', ConsoleKey.Backspace, false, false, false));
        return this;
    }

    public FakeConsoleIo Enter()
    {
        _keys.Enqueue(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        return this;
    }

    public string? ReadLine() => _lines.Count > 0 ? _lines.Dequeue() : null;

    public ConsoleKeyInfo ReadKey(bool intercept) =>
        _keys.Count > 0
            ? _keys.Dequeue()
            : throw new InvalidOperationException("No key was queued for this prompt.");

    public void Write(string text) => _output.Append(text);

    public void WriteLine(string text = "") => _output.Append(text).Append('\n');
}
