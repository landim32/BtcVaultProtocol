using System.Security.Cryptography;
using BtcVaultProtocol.Wallet.Core;
using BtcVaultProtocol.Wallet.Core.Entropy;

namespace BtcVaultProtocol.Wallet.Tests.Entropy;

public sealed class EntropyMixerTests
{
    [Fact]
    public void Mix_ProducesExactly32Bytes()
    {
        var mixed = EntropyMixer.Mix(ValidRolls(), SystemRandomSource.Instance);

        Assert.Equal(WalletConstants.ENTROPY_BYTES, mixed.Length);
    }

    [Fact]
    public void Mix_IsTheXorOfSha256AndCsprng()
    {
        var rolls = ValidRolls();
        var fixedSystemBytes = Enumerable.Range(0, WalletConstants.ENTROPY_BYTES)
            .Select(i => (byte)i)
            .ToArray();

        var mixed = EntropyMixer.Mix(rolls, new FixedRandomSource(fixedSystemBytes));

        var expectedDigest = SHA256.HashData(rolls.ToArray());
        var expected = expectedDigest.Zip(fixedSystemBytes, (a, b) => (byte)(a ^ b)).ToArray();

        Assert.Equal(expected, mixed);
    }

    [Fact]
    public void Mix_WithDegenerateManualEntropy_StillProducesDistinctResults()
    {
        // SC-005: mesmo com a mesma entrada manual degenerada, o CSPRNG garante unicidade.
        var degenerate = Enumerable.Repeat((byte)1, WalletConstants.MIN_DICE_ROLLS).ToArray();

        var results = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < 1_000; i++)
        {
            results.Add(Convert.ToHexString(EntropyMixer.Mix(degenerate, SystemRandomSource.Instance)));
        }

        Assert.Equal(1_000, results.Count);
    }

    [Fact]
    public void Mix_WithCompromisedCsprng_StillDependsOnManualEntropy()
    {
        // Se o CSPRNG for previsível, entradas manuais distintas ainda produzem sementes distintas.
        var compromised = new FixedRandomSource(new byte[WalletConstants.ENTROPY_BYTES]);

        var first = EntropyMixer.Mix(ValidRolls(seed: 1), compromised);
        var second = EntropyMixer.Mix(ValidRolls(seed: 2), compromised);

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Mix_WithInsufficientManualEntropy_Throws()
    {
        var tooFew = Enumerable.Repeat((byte)3, WalletConstants.MIN_DICE_ROLLS - 1).ToArray();

        Assert.Throws<ArgumentException>(() => EntropyMixer.Mix(tooFew, SystemRandomSource.Instance));
    }

    private static byte[] ValidRolls(int seed = 0) =>
        Enumerable.Range(0, WalletConstants.MIN_DICE_ROLLS)
            .Select(i => (byte)((i + seed) % WalletConstants.DICE_FACES + 1))
            .ToArray();

    private sealed class FixedRandomSource(byte[] value) : IRandomSource
    {
        public void Fill(Span<byte> buffer) => value.AsSpan(0, buffer.Length).CopyTo(buffer);
    }
}
