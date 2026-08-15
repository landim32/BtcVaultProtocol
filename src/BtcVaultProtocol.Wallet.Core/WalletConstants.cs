namespace BtcVaultProtocol.Wallet.Core;

/// <summary>
/// Constantes de domínio da geração de carteiras. Princípio IV: nenhum literal numérico de
/// domínio pode aparecer solto na lógica.
/// </summary>
public static class WalletConstants
{
    // --- Semente ---
    public const int MNEMONIC_WORD_COUNT = 24;
    public const int ENTROPY_BITS = 256;
    public const int ENTROPY_BYTES = 32;

    // --- Coleta de entropia manual (R-002) ---
    public const int DICE_FACES = 6;
    public const int MIN_DICE_ROLLS = 100;

    // --- Limites de volume ---
    public const int MAX_DERIVED_WALLETS = 10;
    public const int MAX_PASSPHRASES_PER_WALLET = 3;
    public const int ADDRESSES_PER_ACCOUNT = 10;

    /// <summary>
    /// Mínimo de carteiras derivadas para que um esquema multisig faça sentido. O número de
    /// participantes NÃO é escolhido pelo usuário: é a quantidade de carteiras criadas.
    /// </summary>
    public const int MIN_MULTISIG_PARTICIPANTS = 2;

    // --- BIP85 (R-003, R-004) ---
    public const uint BIP85_PURPOSE = 83696968;
    public const uint BIP85_APPLICATION_BIP39 = 39;
    public const uint BIP85_LANGUAGE_ENGLISH = 0;
    public const string BIP85_HMAC_KEY = "bip-entropy-from-k";

    // --- Caminhos de derivação (R-005) ---
    public const string SINGLE_SIG_ACCOUNT_PATH = "m/84'/0'/0'";
    public const string MULTISIG_ACCOUNT_PATH = "m/48'/0'/0'/2'";
    public const int RECEIVE_CHAIN = 0;

    // --- Serialização SLIP-132 (R-006) ---
    public const uint ZPUB_VERSION = 0x04B24746;
    public const uint ZPUB_MULTISIG_VERSION = 0x02AA7ED3;
}
