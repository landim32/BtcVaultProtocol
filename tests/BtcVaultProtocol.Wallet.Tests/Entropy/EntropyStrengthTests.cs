using BtcVaultProtocol.Wallet.Core;
using BtcVaultProtocol.Wallet.Core.Entropy;

namespace BtcVaultProtocol.Wallet.Tests.Entropy;

public sealed class EntropyStrengthTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 2)]
    [InlineData(10, 25)]
    [InlineData(99, 255)]
    [InlineData(100, 258)]
    [InlineData(200, 516)] // 200 × log2(6) = 516,99… → 516
    public void BitsFor_TruncatesTowardsZero(int rolls, int expectedBits)
    {
        Assert.Equal(expectedBits, EntropyStrength.BitsFor(rolls));
    }

    [Fact]
    public void BitsFor_NeverOverestimates()
    {
        // Nunca podemos alegar mais entropia do que a matemática garante.
        for (var rolls = 0; rolls <= WalletConstants.MIN_DICE_ROLLS * 2; rolls++)
        {
            Assert.True(EntropyStrength.BitsFor(rolls) <= rolls * Math.Log2(WalletConstants.DICE_FACES));
        }
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(98, false)]
    [InlineData(99, false)]
    [InlineData(100, true)]
    [InlineData(150, true)]
    public void IsSufficient_RequiresTheFullMinimum(int rolls, bool expected)
    {
        Assert.Equal(expected, EntropyStrength.IsSufficient(rolls));
    }

    [Fact]
    public void MinimumRollCount_ReachesTheRequiredBits()
    {
        Assert.True(EntropyStrength.BitsFor(WalletConstants.MIN_DICE_ROLLS) >= WalletConstants.ENTROPY_BITS);
        Assert.False(EntropyStrength.BitsFor(WalletConstants.MIN_DICE_ROLLS - 1) >= WalletConstants.ENTROPY_BITS);
    }

    [Fact]
    public void RemainingBitsAndRolls_ReachZeroAtTheMinimum()
    {
        Assert.Equal(0, EntropyStrength.RemainingRolls(WalletConstants.MIN_DICE_ROLLS));
        Assert.Equal(0, EntropyStrength.RemainingBits(WalletConstants.MIN_DICE_ROLLS));
        Assert.True(EntropyStrength.RemainingRolls(50) > 0);
        Assert.True(EntropyStrength.RemainingBits(50) > 0);
    }

    [Fact]
    public void BitsFor_NegativeRollCount_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => EntropyStrength.BitsFor(-1));
    }

    [Fact]
    public void LooksDegenerate_DetectsUniformSequences()
    {
        Assert.True(EntropyStrength.LooksDegenerate(Enumerable.Repeat((byte)1, 100).ToArray()));
        Assert.False(EntropyStrength.LooksDegenerate([1, 2, 3, 4, 5, 6]));
        Assert.False(EntropyStrength.LooksDegenerate([]));
    }
}
