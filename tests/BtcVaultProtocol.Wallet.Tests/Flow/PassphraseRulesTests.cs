using BtcVaultProtocol.Wallet.Console.Prompts;

namespace BtcVaultProtocol.Wallet.Tests.Flow;

/// <summary>
/// FR-013, FR-014, FR-016 e R-010 (passphrase vazia rejeitada).
/// </summary>
public sealed class PassphraseRulesTests
{
    [Fact]
    public void Read_RequiresMatchingConfirmation()
    {
        var io = new FakeConsoleIo();
        io.TypeSecret("correct horse");
        io.TypeSecret("correct horse");

        var passphrase = PassphrasePrompt.Read(io, 1, 1, []);

        Assert.Equal("correct horse", passphrase);
    }

    [Fact]
    public void Read_MismatchedConfirmation_ReAsksTheSamePassphrase()
    {
        var io = new FakeConsoleIo();
        io.TypeSecret("first");
        io.TypeSecret("second");
        io.TypeSecret("third");
        io.TypeSecret("third");

        var passphrase = PassphrasePrompt.Read(io, 1, 1, []);

        Assert.Equal("third", passphrase);
        Assert.Contains("Passphrases do not match. Try again.", io.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Read_EmptyPassphrase_IsRejected()
    {
        var io = new FakeConsoleIo();
        io.TypeSecret("");
        io.TypeSecret("valid one");
        io.TypeSecret("valid one");

        var passphrase = PassphrasePrompt.Read(io, 1, 1, []);

        Assert.Equal("valid one", passphrase);
        Assert.Contains("An empty passphrase is the same as no passphrase", io.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Read_DuplicatePassphrase_WarnsButAccepts()
    {
        var io = new FakeConsoleIo();
        io.TypeSecret("same");
        io.TypeSecret("same");

        var passphrase = PassphrasePrompt.Read(io, 2, 2, ["same"]);

        Assert.Equal("same", passphrase);
        Assert.Contains("WARNING: identical to passphrase 1", io.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Read_NeverEchoesTheTypedCharacters()
    {
        var io = new FakeConsoleIo();
        io.TypeSecret("s3cr3t");
        io.TypeSecret("s3cr3t");

        PassphrasePrompt.Read(io, 1, 1, []);

        Assert.DoesNotContain("s3cr3t", io.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("*", io.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Read_BackspaceRemovesTheLastCharacter()
    {
        var io = new FakeConsoleIo();
        io.Type("abcx").Backspace().Enter();  // vira "abc"
        io.TypeSecret("abc");

        Assert.Equal("abc", PassphrasePrompt.Read(io, 1, 1, []));
    }

    [Fact]
    public void Read_PreservesLeadingAndTrailingSpaces()
    {
        // Espaços nas bordas mudam a carteira resultante — não podem ser aparados.
        var io = new FakeConsoleIo();
        io.TypeSecret(" spaced ");
        io.TypeSecret(" spaced ");

        Assert.Equal(" spaced ", PassphrasePrompt.Read(io, 1, 1, []));
    }
}
