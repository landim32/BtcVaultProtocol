using BtcVaultProtocol.Wallet.Console.Prompts;
using BtcVaultProtocol.Wallet.Core;

namespace BtcVaultProtocol.Wallet.Tests.Flow;

/// <summary>
/// FR-018: combinações M-de-N impossíveis são rejeitadas com nova solicitação.
/// </summary>
public sealed class MultisigValidationTests
{
    [Theory]
    [InlineData("0")]
    [InlineData("4")]
    [InlineData("-1")]
    public void RequiredSignatures_OutsideOneToParticipantCount_ReAsks(string invalid)
    {
        var io = new FakeConsoleIo(invalid, "2");

        var required = IntegerPrompt.Ask(io, "How many signatures are required to spend?", 1, 3);

        Assert.Equal(2, required);
        Assert.Contains("Please enter a whole number between 1 and 3.", io.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void RequiredSignatures_MayEqualParticipantCount()
    {
        var io = new FakeConsoleIo("3");

        Assert.Equal(3, IntegerPrompt.Ask(io, "How many signatures are required to spend?", 1, 3));
    }

    [Fact]
    public void MinimumParticipantCount_MatchesTheDocumentedLimit()
    {
        // O usuário não escolhe o número de participantes: ele é a quantidade de carteiras
        // criadas. Só o mínimo para que um esquema exista é fixo.
        Assert.Equal(2, WalletConstants.MIN_MULTISIG_PARTICIPANTS);
    }
}
