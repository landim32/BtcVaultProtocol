using BtcVaultProtocol.Wallet.Console.Prompts;
using BtcVaultProtocol.Wallet.Console.Rendering;
using BtcVaultProtocol.Wallet.Core;
using BtcVaultProtocol.Wallet.Core.Bip85;
using BtcVaultProtocol.Wallet.Core.Derivation;
using BtcVaultProtocol.Wallet.Core.Entropy;
using BtcVaultProtocol.Wallet.Core.Mnemonics;
using BtcVaultProtocol.Wallet.Core.Models;
using BtcVaultProtocol.Wallet.Core.Multisig;
using NBitcoin;

namespace BtcVaultProtocol.Wallet.Console.Flow;

/// <summary>
/// Orquestra a sequência fixa de perguntas (FR-001): entropia → carteiras derivadas →
/// passphrases → multisig → relatório. Nenhuma derivação acontece aqui — todas vivem no Core
/// (Princípio IV).
/// </summary>
public sealed class WalletFlow(IConsoleIo io, IRandomSource randomSource, SecureExit secureExit)
{
    private readonly IConsoleIo _io = io ?? throw new ArgumentNullException(nameof(io));
    private readonly IRandomSource _randomSource = randomSource ?? throw new ArgumentNullException(nameof(randomSource));
    private readonly SecureExit _secureExit = secureExit ?? throw new ArgumentNullException(nameof(secureExit));

    public WalletSession Run()
    {
        Warnings.ShowOpening(_io);

        var master = BuildMasterWallet();
        var derivedWallets = BuildDerivedWallets(master);
        var multisig = BuildMultisig(derivedWallets);

        return new WalletSession(master, derivedWallets, multisig);
    }

    private MasterWallet BuildMasterWallet()
    {
        var rolls = _secureExit.Track(DiceEntropyPrompt.Collect(_io));
        var entropy = _secureExit.Track(EntropyMixer.Mix(rolls, _randomSource));

        var mnemonic = MnemonicFactory.FromEntropy(entropy);
        var rootKey = mnemonic.DeriveExtKey();

        return new MasterWallet(mnemonic, rootKey, AccountDeriver.DeriveSingleSig(mnemonic));
    }

    private IReadOnlyList<DerivedWallet> BuildDerivedWallets(MasterWallet master)
    {
        _io.WriteLine("--- Step 2 of 4: derived wallets (BIP85) ---");
        _io.WriteLine();

        if (!YesNoPrompt.Ask(_io, "Derive child wallets from this master seed?", defaultAnswer: true))
        {
            _io.WriteLine();
            return [];
        }

        var count = IntegerPrompt.Ask(_io, "How many wallets?", 1, WalletConstants.MAX_DERIVED_WALLETS);
        _io.WriteLine();

        _io.WriteLine("--- Step 3 of 4: passphrases ---");
        _io.WriteLine();

        var wallets = new List<DerivedWallet>(count);

        for (var index = 0; index < count; index++)
        {
            var mnemonic = Bip85Deriver.DeriveMnemonic(master.RootKey, (uint)index);
            var account = AccountDeriver.DeriveSingleSig(mnemonic);

            _io.WriteLine($"Wallet {index + 1} of {count}");
            var passphraseWallets = CollectPassphrases(mnemonic);

            wallets.Add(new DerivedWallet(index, mnemonic, account, passphraseWallets));
        }

        return wallets;
    }

    private IReadOnlyList<PassphraseWallet> CollectPassphrases(Mnemonic mnemonic)
    {
        var count = IntegerPrompt.Ask(
            _io,
            "How many passphrases for this wallet?",
            0,
            WalletConstants.MAX_PASSPHRASES_PER_WALLET);

        _io.WriteLine();

        var entered = new List<string>(count);
        var wallets = new List<PassphraseWallet>(count);

        for (var number = 1; number <= count; number++)
        {
            var passphrase = PassphrasePrompt.Read(_io, number, count, entered);
            entered.Add(passphrase);

            wallets.Add(new PassphraseWallet(passphrase, AccountDeriver.DeriveSingleSig(mnemonic, passphrase)));
        }

        return wallets;
    }

    private MultisigWallet? BuildMultisig(IReadOnlyList<DerivedWallet> derivedWallets)
    {
        _io.WriteLine("--- Step 4 of 4: multisig ---");
        _io.WriteLine();

        // O esquema é montado com as carteiras já criadas — nenhuma carteira nova é gerada.
        if (derivedWallets.Count < WalletConstants.MIN_MULTISIG_PARTICIPANTS)
        {
            _io.WriteLine(
                $"Multisig needs at least {WalletConstants.MIN_MULTISIG_PARTICIPANTS} derived wallets to " +
                $"combine, and only {derivedWallets.Count} exist. Skipping this step.");
            _io.WriteLine();
            return null;
        }

        _io.WriteLine($"The multisig scheme will combine all {derivedWallets.Count} wallets above,");
        _io.WriteLine("each one taken with its last passphrase.");
        _io.WriteLine();

        if (!YesNoPrompt.Ask(_io, "Create a multisig wallet?", defaultAnswer: false))
        {
            _io.WriteLine();
            return null;
        }

        // O usuário escolhe apenas M: N é a quantidade de carteiras criadas.
        var requiredSignatures = IntegerPrompt.Ask(
            _io,
            "How many signatures are required to spend?",
            1,
            derivedWallets.Count);

        var participants = MultisigParticipantSelector.FromDerivedWallets(derivedWallets);

        return new MultisigWallet(
            requiredSignatures,
            participants,
            MultisigDescriptor.Build(requiredSignatures, participants),
            MultisigBuilder.DeriveAddresses(requiredSignatures, participants));
    }
}
