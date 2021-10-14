using System;
using System.IO;

namespace BlackByteDecryptor
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: BlackByteDecryptor <keyfile> <encrypted file or directory> [-r (recursive)]");
                return;
            }

            try
            {
                var decryptor = new BlackByteDecryptor();
                decryptor.LoadKeysFromFile(args[0]);
                var input = args[1];
                bool recursive = false;
                if (args.Length > 2)
                    recursive = args[2] == "-r";

                var dirInfo = new DirectoryInfo(input);
                if (dirInfo.Exists)
                    decryptor.DecryptDirectory(dirInfo, recursive);
                else
                    decryptor.DecryptFile(input);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Decryption error: {ex.Message}");
            }
        }
    }
}
