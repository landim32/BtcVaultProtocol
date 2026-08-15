using System.Security.Cryptography;

namespace BtcVaultProtocol.Wallet.Console;

/// <summary>
/// Zeroing centralizado do material sensível (Princípio VI, FR-027).
///
/// Limitação conhecida e aceita: NBitcoin.Mnemonic mantém as palavras como string imutável
/// internamente, fora do alcance do zeroing — o mesmo vale para as strings que chegam à tela.
/// Zeramos o que está sob nosso controle: entropia manual, entropia mestre e passphrases.
/// </summary>
public sealed class SecureExit
{
    private readonly List<byte[]> _buffers = [];
    private readonly List<char[]> _charBuffers = [];

    /// <summary>Registra um buffer para ser zerado ao final da execução.</summary>
    public byte[] Track(byte[] buffer)
    {
        _buffers.Add(buffer);
        return buffer;
    }

    /// <summary>Registra um buffer de caracteres (passphrase) para ser zerado.</summary>
    public char[] Track(char[] buffer)
    {
        _charBuffers.Add(buffer);
        return buffer;
    }

    /// <summary>
    /// Zera tudo o que foi registrado. Idempotente: pode ser chamado por mais de um caminho
    /// (fim normal, erro, Ctrl+C) sem efeito colateral.
    /// </summary>
    public void WipeAll()
    {
        foreach (var buffer in _buffers)
        {
            CryptographicOperations.ZeroMemory(buffer);
        }

        foreach (var buffer in _charBuffers)
        {
            Array.Clear(buffer);
        }

        _buffers.Clear();
        _charBuffers.Clear();
    }
}
