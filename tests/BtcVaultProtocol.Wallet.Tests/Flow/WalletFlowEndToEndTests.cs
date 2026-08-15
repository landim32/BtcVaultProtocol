using BtcVaultProtocol.Wallet.Console;
using BtcVaultProtocol.Wallet.Console.Flow;
using BtcVaultProtocol.Wallet.Console.Rendering;
using BtcVaultProtocol.Wallet.Core;
using BtcVaultProtocol.Wallet.Core.Entropy;
using BtcVaultProtocol.Wallet.Core.Models;

namespace BtcVaultProtocol.Wallet.Tests.Flow;

/// <summary>
/// Percorre o fluxo inteiro com um console falso e confere as regras verificáveis do relatório
/// (contrato §7) e a ordem fixa das perguntas (FR-001).
/// </summary>
public sealed class WalletFlowEndToEndTests
{
    [Fact]
    public void FullFlow_RendersEverySectionInOrder()
    {
        var io = BuildConsole(derivedWallets: 2, passphrasesPerWallet: 1, requiredSignatures: 2);

        var session = RunFlow(io);
        ReportRenderer.Render(io, session);

        var output = io.Output;

        var masterIndex = output.IndexOf("MASTER WALLET", StringComparison.Ordinal);
        var firstDerivedIndex = output.IndexOf("DERIVED WALLET 1 of 2", StringComparison.Ordinal);
        var secondDerivedIndex = output.IndexOf("DERIVED WALLET 2 of 2", StringComparison.Ordinal);
        var multisigIndex = output.IndexOf("MULTISIG WALLET  —  2 of 2", StringComparison.Ordinal);

        Assert.True(masterIndex >= 0);
        Assert.True(masterIndex < firstDerivedIndex);
        Assert.True(firstDerivedIndex < secondDerivedIndex);
        Assert.True(secondDerivedIndex < multisigIndex);
    }

    [Fact]
    public void FullFlow_ShowsNoPassphraseBlockBeforeEachPassphraseBlock()
    {
        var io = BuildConsole(derivedWallets: 2, passphrasesPerWallet: 2, requiredSignatures: 2);

        ReportRenderer.Render(io, RunFlow(io));

        var output = io.Output;
        var noPassphrase = output.IndexOf("[no passphrase]", StringComparison.Ordinal);
        var first = output.IndexOf("[passphrase 1]", StringComparison.Ordinal);
        var second = output.IndexOf("[passphrase 2]", StringComparison.Ordinal);

        Assert.True(noPassphrase >= 0 && noPassphrase < first && first < second);
    }

    [Fact]
    public void FullFlow_EveryAccountShowsTenNumberedAddresses()
    {
        var io = BuildConsole(derivedWallets: 2, passphrasesPerWallet: 1, requiredSignatures: 2);

        var session = RunFlow(io);

        var accounts = new List<WalletAccount> { session.Master.Account };
        accounts.AddRange(session.DerivedWallets.Select(w => w.Account));
        accounts.AddRange(session.DerivedWallets.SelectMany(w => w.PassphraseWallets).Select(p => p.Account));

        Assert.All(accounts, account =>
        {
            Assert.Equal(WalletConstants.ADDRESSES_PER_ACCOUNT, account.Addresses.Count);
            Assert.Equal(Enumerable.Range(0, WalletConstants.ADDRESSES_PER_ACCOUNT), account.Addresses.Select(a => a.Index));
        });

        Assert.Equal(WalletConstants.ADDRESSES_PER_ACCOUNT, session.Multisig!.Addresses.Count);
    }

    [Fact]
    public void FullFlow_UsesZpubForSingleSigAndCapitalZpubForMultisig()
    {
        var io = BuildConsole(derivedWallets: 2, passphrasesPerWallet: 0, requiredSignatures: 2);

        var session = RunFlow(io);

        Assert.StartsWith("zpub", session.Master.Account.Serialized, StringComparison.Ordinal);
        Assert.StartsWith("zpub", session.DerivedWallets[0].Account.Serialized, StringComparison.Ordinal);
        Assert.All(session.Multisig!.Participants,
            p => Assert.StartsWith("Zpub", p.Account.Serialized, StringComparison.Ordinal));
    }

    [Fact]
    public void FullFlow_MultisigSectionAlwaysCarriesTheSharedRootWarning()
    {
        var io = BuildConsole(derivedWallets: 2, passphrasesPerWallet: 0, requiredSignatures: 2);

        ReportRenderer.Render(io, RunFlow(io));

        Assert.Contains("derived from the SAME master seed", io.Output, StringComparison.Ordinal);
        Assert.Contains("they can spend alone", io.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void FullFlow_ClearTerminalGuidanceIsTheLastThingPrinted()
    {
        var io = BuildConsole(derivedWallets: 2, passphrasesPerWallet: 0, requiredSignatures: 2);

        ReportRenderer.Render(io, RunFlow(io));

        var trimmed = io.Output.TrimEnd('\n');
        var lastLines = trimmed.Split('\n')[^3..];

        Assert.Contains("Clear your screen and scrollback now:", string.Join('\n', lastLines), StringComparison.Ordinal);
    }

    [Fact]
    public void DecliningEverything_ProducesMasterOnlyReport()
    {
        var io = new FakeConsoleIo([DiceRolls(), "", "n", "n"]);

        var session = RunFlow(io);
        ReportRenderer.Render(io, session);

        Assert.Empty(session.DerivedWallets);
        Assert.Null(session.Multisig);
        Assert.DoesNotContain("DERIVED WALLET", io.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("MULTISIG WALLET", io.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void RedirectedOutput_IsWarnedAbout()
    {
        var io = new FakeConsoleIo([DiceRolls(), "", "n", "n"]) { IsOutputRedirected = true };

        RunFlow(io);

        Assert.Contains("output is being redirected", io.Output, StringComparison.Ordinal);
        Assert.Contains("WILL be written to disk", io.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void EveryGeneratedSeedHasTwentyFourWords()
    {
        var io = BuildConsole(derivedWallets: 2, passphrasesPerWallet: 1, requiredSignatures: 2);

        var session = RunFlow(io);

        Assert.Equal(WalletConstants.MNEMONIC_WORD_COUNT, session.Master.Mnemonic.Words.Length);
        Assert.All(session.DerivedWallets,
            w => Assert.Equal(WalletConstants.MNEMONIC_WORD_COUNT, w.Mnemonic.Words.Length));
        Assert.All(session.Multisig!.Participants,
            p => Assert.Equal(WalletConstants.MNEMONIC_WORD_COUNT, p.Mnemonic.Words.Length));
    }

    [Fact]
    public void Multisig_ReusesTheDerivedWalletsWithTheirLastPassphrase()
    {
        var io = BuildConsole(derivedWallets: 3, passphrasesPerWallet: 2, requiredSignatures: 2);

        var session = RunFlow(io);
        var multisig = session.Multisig!;

        // Nenhuma carteira nova: cada participante é uma das carteiras derivadas...
        Assert.Equal(
            session.DerivedWallets.Select(w => w.Mnemonic.ToString()),
            multisig.Participants.Select(p => p.Mnemonic.ToString()));

        // ...tomada com a ÚLTIMA passphrase dela.
        Assert.Equal(
            session.DerivedWallets.Select(w => w.PassphraseWallets[^1].Passphrase),
            multisig.Participants.Select(p => p.Passphrase));
    }

    [Fact]
    public void Multisig_DoesNotRepeatTheSeedWordsAlreadyShownAbove()
    {
        var io = BuildConsole(derivedWallets: 2, passphrasesPerWallet: 1, requiredSignatures: 2);

        var session = RunFlow(io);
        ReportRenderer.Render(io, session);

        var multisigSection = io.Output[io.Output.IndexOf("MULTISIG WALLET", StringComparison.Ordinal)..];

        // A seção multisig identifica a carteira de origem, mas não reimprime as 24 palavras.
        Assert.DoesNotContain("Seed (24 words)", multisigSection, StringComparison.Ordinal);
        Assert.Contains("from derived wallet 1", multisigSection, StringComparison.Ordinal);
        Assert.Contains("Account Zpub :", multisigSection, StringComparison.Ordinal);

        // Cada semente aparece exatamente uma vez no relatório inteiro — na sua carteira.
        foreach (var wallet in session.DerivedWallets)
        {
            var firstWord = wallet.Mnemonic.Words[0];
            var occurrences = multisigSection.Split(firstWord).Length - 1;
            Assert.Equal(0, occurrences);
        }
    }

    [Fact]
    public void Multisig_ParticipantCountAlwaysEqualsTheDerivedWalletCount()
    {
        var io = BuildConsole(derivedWallets: 4, passphrasesPerWallet: 1, requiredSignatures: 2);

        var session = RunFlow(io);

        Assert.Equal(session.DerivedWallets.Count, session.Multisig!.ParticipantCount);

        // O usuário nunca é perguntado quantos participantes existem — apenas quantas assinaturas.
        Assert.DoesNotContain("How many participants?", io.Output, StringComparison.Ordinal);
        Assert.Contains("How many signatures are required to spend?", io.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Multisig_WithFewerThanTwoWallets_IsSkippedWithAnExplanation()
    {
        var io = new FakeConsoleIo([DiceRolls(), "", "y", "1", "0"]);

        var session = RunFlow(io);

        Assert.Null(session.Multisig);
        Assert.Contains("Multisig needs at least 2 derived wallets", io.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("Create a multisig wallet?", io.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Multisig_RequiredSignaturesIsBoundedByTheWalletCount()
    {
        // 3 carteiras → o prompt aceita de 1 a 3 assinaturas.
        var io = BuildConsole(derivedWallets: 3, passphrasesPerWallet: 0, requiredSignatures: 3);

        var session = RunFlow(io);

        Assert.Equal(3, session.Multisig!.RequiredSignatures);
        Assert.Equal(3, session.Multisig.ParticipantCount);
        Assert.Contains("(1-3)", io.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void InsufficientEntropy_KeepsAskingInsteadOfGenerating()
    {
        var io = new FakeConsoleIo(["123456", "", DiceRolls(), "", "n", "n"]);

        RunFlow(io);

        Assert.Contains("Not enough entropy yet", io.Output, StringComparison.Ordinal);
    }

    private static WalletSession RunFlow(FakeConsoleIo io) =>
        new WalletFlow(io, SystemRandomSource.Instance, new SecureExit()).Run();

    private static FakeConsoleIo BuildConsole(
        int derivedWallets,
        int passphrasesPerWallet,
        int requiredSignatures)
    {
        var lines = new List<string>
        {
            DiceRolls(),
            "",                              // encerra a coleta de entropia
            "y",                             // derivar carteiras
            derivedWallets.ToString(),
        };

        for (var wallet = 0; wallet < derivedWallets; wallet++)
        {
            lines.Add(passphrasesPerWallet.ToString());
        }

        lines.Add("y");                      // multisig — o número de participantes NÃO é
        lines.Add(requiredSignatures.ToString());  // perguntado: é a quantidade de carteiras

        var io = new FakeConsoleIo([.. lines]);

        for (var wallet = 0; wallet < derivedWallets; wallet++)
        {
            for (var number = 1; number <= passphrasesPerWallet; number++)
            {
                var passphrase = $"passphrase-{wallet}-{number}";
                io.TypeSecret(passphrase);
                io.TypeSecret(passphrase);
            }
        }

        return io;
    }

    private static string DiceRolls() =>
        string.Concat(Enumerable
            .Range(0, WalletConstants.MIN_DICE_ROLLS)
            .Select(i => (char)('1' + (i % WalletConstants.DICE_FACES))));
}
