using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SuperSocket.ProtoBase;

namespace Versatile.Networks.Services;

public class MessagePackageInfo : IKeyedPackageInfo<MessageType>
{
    public MessageType Key { get; set; }

    public DateTime Timestamp { get; set; }

    public Dictionary<string, string> Parameters { get; set; }

    public MessagePackageInfo()
    {
        Parameters = new();
    }

    public string this[string key]
    {
        get => Parameters.TryGetValue(key, out var value) ? value : default;
        set => Parameters[key] = value;
    }

    public string Get(string key)
    {
        return Parameters.TryGetValue(key, out var value) ? value : default;
    }

    public T Get<T>(string key)
    {
        if (Parameters.TryGetValue(key, out var value))
        {
            if(typeof(T) == typeof(string))
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            else
            {
                var obj = JsonSerializer.Deserialize<T>(value);
                return obj;
            }
        }
        else
        {
            return default;
        }
    }

    public bool ContainsKey(string key)
    {
        return Parameters.ContainsKey(key);
    }

    public MessagePackageInfo Add<T>(string key, T value)
    {
        if(value is string s)
        {
            Parameters.Add(key, s);
        }
        else
        {
            var text = JsonSerializer.Serialize(value);
            Parameters.Add(key, text);
        }
        return this;
    }

    public byte[] ToBytes()
    {
        var magic = "VMSG";
        var isCompressed = true;
        var isEncrypted = true;

        var text = JsonSerializer.Serialize(this);
        var textBytes = Encoding.UTF8.GetBytes(text);
        var compressedBytes = isCompressed ? Compress(textBytes) : textBytes;
        var encryptedBytes = isEncrypted ? Encrypt(compressedBytes) : compressedBytes;

        var dataLength = encryptedBytes.Length;
        var flags =
            (isCompressed ? 1 : 0)
            | ((isEncrypted ? 1 : 0) << 1)
            ;

        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        bw.Write(Encoding.ASCII.GetBytes(magic));
        bw.Write((ushort)dataLength);
        bw.Write((ushort)flags);
        bw.Write(encryptedBytes);

        bw.Flush();
        var data = ms.ToArray();

        Debug.WriteLine($"Text length: {textBytes.Length}, Compressed length: {compressedBytes.Length}, Encrypted length: {encryptedBytes.Length}, Text: {text}");

        return data;
    }

    public static MessagePackageInfo FromBytes(ref ReadOnlySequence<byte> buffer)
    {
        var reader = new SequenceReader<byte>(buffer);
        var magic = reader.ReadString(4);
        reader.TryReadLittleEndian(out short encryptedLength);
        reader.TryReadLittleEndian(out short flags);
        reader.TryReadExact(encryptedLength, out var data);

        var isCompressed = (flags & 1) == 1;
        var isEncrypted = ((flags >> 1) & 1) == 1;
        var encryptedBytes = data.ToArray();
        var compressedBytes = isEncrypted ? Decrypt(encryptedBytes) : encryptedBytes;
        var decompressedData = isCompressed ? Decompress(compressedBytes) : compressedBytes;
        var text = Encoding.UTF8.GetString(decompressedData);
        var msg = JsonSerializer.Deserialize<MessagePackageInfo>(text);
        return msg;
    }

    // better than none
    private static byte[] Encrypt(byte[] data)
    {
        using var aesAlg = Aes.Create();
        aesAlg.Key = Enumerable.Repeat((byte)'b', 16).ToArray();
        aesAlg.IV = Enumerable.Repeat((byte)'b', 16).ToArray();
        var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
        using var msOutput = new MemoryStream();
        using (var csCrypto = new CryptoStream(msOutput, encryptor, CryptoStreamMode.Write))
        {
            csCrypto.Write(data, 0, data.Length);
        }
        return msOutput.ToArray();
    }

    private static byte[] Compress(byte[] data)
    {
        using var msInput = new MemoryStream(data);
        using var msOutput = new MemoryStream();
        using (var compressor = new GZipStream(msOutput, CompressionMode.Compress))
        {
            compressor.Write(data, 0, data.Length);
        }
        return msOutput.ToArray();
    }

    private static byte[] Decompress(byte[] data)
    {
        using var msInput = new MemoryStream(data);
        using var msOutput = new MemoryStream();
        using (var decompressor = new GZipStream(msInput, CompressionMode.Decompress))
        {
            decompressor.CopyTo(msOutput);
        }
        return msOutput.ToArray();
    }

    private static byte[] Decrypt(byte[] data)
    {
        using var aesAlg = Aes.Create();
        aesAlg.Key = Enumerable.Repeat((byte)'b', 16).ToArray();
        aesAlg.IV = Enumerable.Repeat((byte)'b', 16).ToArray();
        var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
        using var msInput = new MemoryStream(data);
        using var msOutput = new MemoryStream();
        using (var csDecrypt = new CryptoStream(msInput, decryptor, CryptoStreamMode.Read))
        {
            csDecrypt.CopyTo(msOutput);
        }
        return msOutput.ToArray();
    }
}
