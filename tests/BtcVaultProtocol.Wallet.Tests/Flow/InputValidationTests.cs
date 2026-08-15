using BtcVaultProtocol.Wallet.Console.Prompts;

namespace BtcVaultProtocol.Wallet.Tests.Flow;

/// <summary>
/// SC-007: toda entrada inválida produz mensagem explicativa e nova solicitação, sem
/// encerramento abrupto.
/// </summary>
public sealed class InputValidationTests
{
    [Theory]
    [InlineData("y", true)]
    [InlineData("Y", true)]
    [InlineData("yes", true)]
    [InlineData("YES", true)]
    [InlineData("n", false)]
    [InlineData("N", false)]
    [InlineData("no", false)]
    public void YesNoPrompt_IsCaseInsensitive(string answer, bool expected)
    {
        var io = new FakeConsoleIo(answer);

        Assert.Equal(expected, YesNoPrompt.Ask(io, "Question?", defaultAnswer: false));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void YesNoPrompt_EmptyAnswer_UsesTheDefault(bool defaultAnswer)
    {
        var io = new FakeConsoleIo("");

        Assert.Equal(defaultAnswer, YesNoPrompt.Ask(io, "Question?", defaultAnswer));
    }

    [Fact]
    public void YesNoPrompt_ShowsWhichAnswerIsTheDefault()
    {
        YesNoPrompt.Ask(new FakeConsoleIo(""), "Question?", defaultAnswer: true);
        var withDefaultYes = new FakeConsoleIo("");
        YesNoPrompt.Ask(withDefaultYes, "Question?", defaultAnswer: true);

        var withDefaultNo = new FakeConsoleIo("");
        YesNoPrompt.Ask(withDefaultNo, "Question?", defaultAnswer: false);

        Assert.Contains("[Y/n]", withDefaultYes.Output, StringComparison.Ordinal);
        Assert.Contains("[y/N]", withDefaultNo.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void YesNoPrompt_InvalidAnswer_ReAsksWithoutTerminating()
    {
        var io = new FakeConsoleIo("maybe", "42", "y");

        var answer = YesNoPrompt.Ask(io, "Question?", defaultAnswer: false);

        Assert.True(answer);
        Assert.Contains("Please answer y or n.", io.Output, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("11")]
    [InlineData("1.5")]
    [InlineData("")]
    public void IntegerPrompt_InvalidValue_ReAsksWithExplanation(string invalid)
    {
        var io = new FakeConsoleIo(invalid, "3");

        var value = IntegerPrompt.Ask(io, "How many?", 1, 10);

        Assert.Equal(3, value);
        Assert.Contains("Please enter a whole number between 1 and 10.", io.Output, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void IntegerPrompt_AcceptsTheWholeRange(int value)
    {
        var io = new FakeConsoleIo(value.ToString());

        Assert.Equal(value, IntegerPrompt.Ask(io, "How many?", 1, 10));
    }

    [Fact]
    public void IntegerPrompt_ZeroIsValidWhenTheRangeAllowsIt()
    {
        var io = new FakeConsoleIo("0");

        Assert.Equal(0, IntegerPrompt.Ask(io, "How many passphrases?", 0, 3));
    }

    [Fact]
    public void IntegerPrompt_StatesTheRangeInTheQuestion()
    {
        var io = new FakeConsoleIo("2");

        IntegerPrompt.Ask(io, "How many participants?", 2, 9);

        Assert.Contains("(2-9)", io.Output, StringComparison.Ordinal);
    }
}
