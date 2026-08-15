using BtcVaultProtocol.Wallet.Core;
using BtcVaultProtocol.Wallet.Core.Entropy;

namespace BtcVaultProtocol.Wallet.Tests.Entropy;

/// <summary>
/// FR-007: sem CSPRNG não há semente. A entropia manual nunca é usada sozinha.
/// </summary>
public sealed class CsprngUnavailableTests
{
    [Fact]
    public void Mix_WhenCsprngFails_ThrowsAndProducesNothing()
    {
        var failing = new FailingRandomSource();

        Assert.Throws<CsprngUnavailableException>(
            () => EntropyMixer.Mix(ValidRolls(), failing));
    }

    [Fact]
    public void Mix_WhenCsprngFails_NeverFallsBackToManualEntropyAlone()
    {
        var failing = new FailingRandomSource();
        byte[]? produced = null;

        try
        {
            produced = EntropyMixer.Mix(ValidRolls(), failing);
        }
        catch (CsprngUnavailableException)
        {
            // esperado
        }

        Assert.Null(produced);
        Assert.True(failing.WasCalled);
    }

    private static byte[] ValidRolls() =>
        Enumerable.Range(0, WalletConstants.MIN_DICE_ROLLS)
            .Select(i => (byte)(i % WalletConstants.DICE_FACES + 1))
            .ToArray();

    private sealed class FailingRandomSource : IRandomSource
    {
        public bool WasCalled { get; private set; }

        public void Fill(Span<byte> buffer)
        {
            WasCalled = true;
            throw new CsprngUnavailableException();
        }
    }
}
