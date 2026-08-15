using BtcVaultProtocol.Wallet.Core;
using BtcVaultProtocol.Wallet.Core.Models;
using NBitcoin;

namespace BtcVaultProtocol.Wallet.Console.Rendering;

/// <summary>
/// Relatório final consolidado (FR-020 … FR-024a). Ordem fixa: mestre → carteiras derivadas →
/// multisig, terminando sempre pela orientação de limpeza do terminal.
/// </summary>
public static class ReportRenderer
{
    public const int SeparatorWidth = 80;
    private const int WORDS_PER_LINE = 4;

    public static void Render(IConsoleIo io, WalletSession session)
    {
        ArgumentNullException.ThrowIfNull(io);
        ArgumentNullException.ThrowIfNull(session);

        io.WriteLine();
        RenderMaster(io, session.Master);

        foreach (var wallet in session.DerivedWallets)
        {
            RenderDerived(io, wallet, session.DerivedWallets.Count);
        }

        if (session.Multisig is not null)
        {
            RenderMultisig(io, session.Multisig);
        }

        WriteSeparator(io);
        Warnings.ShowClearTerminalGuidance(io);
    }

    private static void RenderMaster(IConsoleIo io, MasterWallet master)
    {
        WriteSectionHeader(io, "MASTER WALLET");

        WriteSeed(io, master.Mnemonic);
        io.WriteLine();
        io.WriteLine(" Address type : Native SegWit (P2WPKH, bech32)");
        WriteAccount(io, master.Account);
    }

    private static void RenderDerived(IConsoleIo io, DerivedWallet wallet, int total)
    {
        WriteSectionHeader(io, $"DERIVED WALLET {wallet.DisplayNumber} of {total}          (BIP85  {wallet.Bip85Path})");

        WriteSeed(io, wallet.Mnemonic);
        io.WriteLine();
        io.WriteLine(" [no passphrase]");
        WriteAccount(io, wallet.Account);

        for (var i = 0; i < wallet.PassphraseWallets.Count; i++)
        {
            var passphraseWallet = wallet.PassphraseWallets[i];

            io.WriteLine();
            io.WriteLine($" [passphrase {i + 1}] \"{passphraseWallet.Passphrase}\"");
            WriteAccount(io, passphraseWallet.Account);
        }
    }

    private static void RenderMultisig(IConsoleIo io, MultisigWallet multisig)
    {
        WriteSectionHeader(io, $"MULTISIG WALLET  —  {multisig.RequiredSignatures} of {multisig.ParticipantCount}");

        io.WriteLine(" This scheme combines the derived wallets above — no new wallet was created.");
        io.WriteLine(" Each participant is one of those wallets, taken with its last passphrase.");
        io.WriteLine();
        io.WriteLine(" WARNING: every participant key below is derived from the SAME master seed.");
        io.WriteLine("          This protects you against losing individual backups, NOT against");
        io.WriteLine("          someone who obtains the master seed — they can spend alone.");
        io.WriteLine();

        foreach (var participant in multisig.Participants)
        {
            // As 24 palavras NÃO são repetidas aqui: são exatamente as da carteira derivada
            // correspondente, já exibidas acima. Repetir só aumentaria o material sensível na
            // tela e o volume a transcrever.
            io.WriteLine($" Participant {participant.DisplayNumber} of {multisig.ParticipantCount}             (from {participant.Origin})");

            io.WriteLine(participant.Passphrase is null
                ? " Passphrase   : (none)"
                : $" Passphrase   : \"{participant.Passphrase}\"");

            io.WriteLine($" Account path : m/{participant.Account.DerivationPath}");
            io.WriteLine($" Account Zpub : {participant.Account.Serialized}");
            io.WriteLine();
        }

        io.WriteLine(" Descriptor:");
        io.WriteLine($"   {multisig.Descriptor}");
        io.WriteLine();
        io.WriteLine(" Receive addresses:");
        WriteAddresses(io, multisig.Addresses);
    }

    private static void WriteAccount(IConsoleIo io, WalletAccount account)
    {
        var label = account.Serialized.StartsWith("Zpub", StringComparison.Ordinal) ? "Zpub" : "zpub";

        io.WriteLine($" Account path : m/{account.DerivationPath}");
        io.WriteLine($" Account {label} : {account.Serialized}");
        io.WriteLine();
        io.WriteLine(" Receive addresses:");
        WriteAddresses(io, account.Addresses);
    }

    private static void WriteAddresses(IConsoleIo io, IReadOnlyList<AddressEntry> addresses)
    {
        foreach (var entry in addresses)
        {
            io.WriteLine($"   #{entry.Index}  {entry.FullPath,-20}  {entry.Address}");
        }
    }

    private static void WriteSeed(IConsoleIo io, Mnemonic mnemonic)
    {
        io.WriteLine($" Seed ({WalletConstants.MNEMONIC_WORD_COUNT} words):");

        var words = mnemonic.Words;

        for (var i = 0; i < words.Length; i += WORDS_PER_LINE)
        {
            var line = new System.Text.StringBuilder("  ");

            for (var j = i; j < Math.Min(i + WORDS_PER_LINE, words.Length); j++)
            {
                line.Append($"{j + 1,3}. {words[j],-14}");
            }

            io.WriteLine(line.ToString().TrimEnd());
        }
    }

    private static void WriteSectionHeader(IConsoleIo io, string title)
    {
        WriteSeparator(io);
        io.WriteLine($" {title}");
        WriteSeparator(io);
    }

    private static void WriteSeparator(IConsoleIo io) =>
        io.WriteLine(new string('=', SeparatorWidth));
}
