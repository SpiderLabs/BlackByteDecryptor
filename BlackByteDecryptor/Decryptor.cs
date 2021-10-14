using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BlackByteDecryptor
{
    public class BlackByteDecryptor
    {
        private readonly byte[] salt_ = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };
        private byte[] desKey_;
        private byte[] aesKey_;
        private byte[] iv_;

        public void LoadKeysFromFile(string filename)
        {
            Console.WriteLine($"Loading keys from {filename}");
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                stream.Seek(0, SeekOrigin.End);
                var buffer = new byte[stream.Position];
                stream.Seek(0, SeekOrigin.Begin);

                stream.Read(buffer, 0, buffer.Length);
                desKey_ = DecryptKey(buffer);
            }

            Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(desKey_, salt_, 1000);
            aesKey_ = deriveBytes.GetBytes(16);
            iv_ = deriveBytes.GetBytes(16);
        }

        public int DecryptDirectory(DirectoryInfo dirInfo, bool recursive)
        {
            Console.WriteLine($"Decrypting directory {dirInfo.FullName}");
            int count = 0, totalCount = 0;
            foreach (var file in dirInfo.GetFiles("*.blackbyte"))
            {
                if (DecryptFile(file.FullName))
                {
                    count++;
                    totalCount++;
                }
            }

            if (recursive)
            {
                foreach (var dir in dirInfo.GetDirectories())
                {
                    totalCount += DecryptDirectory(dir, recursive);
                }
            }
            if (count > 0)
            {
                Console.WriteLine($"Decrypted {count} files in {dirInfo.FullName}");
                Console.WriteLine($"Total files decrypted: {totalCount}");
            }
            return totalCount;
        }

        public bool DecryptFile(string inputFile)
        {
            if (!inputFile.EndsWith(".blackbyte"))
            {
                Console.WriteLine($"{inputFile} is not encrypted");
                return false;
            }

            try
            {
                var fileInfo = new FileInfo(inputFile);
                var outputFile = inputFile.Substring(0, inputFile.Length - ".blackbyte".Length);
                var outputFileInfo = new FileInfo(outputFile);
                if (outputFileInfo.Exists)
                {
                    Console.WriteLine($"Warning: Target file {outputFile} exists. Not overwriting.");
                    return false;
                }

                if (fileInfo.Length > 3 * 1024 * 1024)
                {
                    DecryptLargeFile(fileInfo, outputFile);
                }
                else
                {
                    DecryptSmallFile(fileInfo, outputFile);
                }

                outputFileInfo.Refresh();
                Console.WriteLine($"Decrypted {outputFileInfo.FullName} - {outputFileInfo.Length} bytes");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error decrypting {inputFile} - {ex.Message}");
                return false;
            }
        }

        private void DecryptLargeFile(FileInfo fileInfo, string outputFile)
        {
            Console.WriteLine($"Decrypting large file {fileInfo.Name}...");
            const int MB = 1024 * 1024;
            int blockSize = 1 * MB;
            if (fileInfo.Length > 3 * 50 * MB)
            {
                blockSize = 50 * MB;
            }
            else if (fileInfo.Length > 3 * 5 * MB)
            {
                blockSize = 5 * MB;
            }

            File.Copy(fileInfo.FullName, outputFile);
            using (var fileStream = new FileStream(outputFile, FileMode.Open))
            {
                var buf = new byte[blockSize];
                fileStream.Read(buf, 0, blockSize);
                buf = EncryptBlock(buf);
                fileStream.Seek(0, SeekOrigin.Begin);
                fileStream.Write(buf, 0, blockSize);

                fileStream.Seek(-blockSize, SeekOrigin.End);
                buf = EncryptBlock(buf);
                fileStream.Seek(-blockSize, SeekOrigin.End);
                fileStream.Write(buf, 0, blockSize);
            }
        }

        private byte[] EncryptBlock(byte[] block)
        {
            using (var aes = new AesCryptoServiceProvider()
            {
                KeySize = 128,
                Key = aesKey_,
                IV = iv_,
                Padding = PaddingMode.None,
                Mode = CipherMode.CBC
            })
            {
                var decryptor = aes.CreateDecryptor();
                var buf = new byte[block.Length];
                using (var memoryStream = new MemoryStream(block))
                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    cryptoStream.Read(buf);
                    return buf;
                }
            }
        }

        private void DecryptSmallFile(FileInfo fileInfo, string outputFile)
        { 
            Console.WriteLine($"Decrypting file {fileInfo.Name}...");
            using (var aes = new AesCryptoServiceProvider()
            {
                KeySize = 128,
                Key = aesKey_,
                IV = iv_,
                Padding = PaddingMode.PKCS7,
                Mode = CipherMode.CBC
            })
            {
                var decryptor = aes.CreateDecryptor();

                using (var fileStream = new FileStream(fileInfo.FullName, FileMode.Open))
                using (var cryptoStream = new CryptoStream(fileStream, decryptor, CryptoStreamMode.Read))
                using (var output = new FileStream(outputFile, FileMode.Create))
                {
                    int totalBytes = 0;
                    int bytes = 0;
                    var buf = new byte[8192];
                    while ((bytes = cryptoStream.Read(buf)) > 0)
                    {
                        output.Write(buf, 0, bytes);
                        totalBytes += bytes;
                    }
                    Console.WriteLine($"Decrypted to {outputFile}, {totalBytes} bytes");
                }
            }
        }

        private byte[] DecryptKey(byte[] buffer)
        {
            using (var memoryStream = new MemoryStream(buffer))
            {
                byte[] buf = new byte[40];

                memoryStream.Read(buf, 0, 40);
                memoryStream.Position = 1064;

                byte[] rawKey = new byte[32];
                memoryStream.Read(rawKey, 0, rawKey.Length);
                memoryStream.Close();

                MD5 md = new MD5CryptoServiceProvider();
                var des = new TripleDESCryptoServiceProvider()
                {
                    Key = md.ComputeHash(rawKey),
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.PKCS7
                };

                var res = des.CreateDecryptor().TransformFinalBlock(buf, 0, buf.Length);
                des.Clear();
                return res;
            }
        }
    }
}
