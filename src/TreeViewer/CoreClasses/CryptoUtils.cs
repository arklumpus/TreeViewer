/*
    TreeViewer - Cross-platform software to draw phylogenetic trees
    Copyright (C) 2021  Giorgio Bianchini
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TreeViewer
{
    public class CryptoUtils
    {
        public const string FileSignatureGuid = "2597d3f2-83e5-4242-8616-4fbe09446f96";

        static CryptoUtils()
        {
            RSACryptoServiceProvider[] providers = new RSACryptoServiceProvider[ModulePublicKeys.Length];

            for (int i = 0; i < ModulePublicKeys.Length; i++)
            {
                providers[i] = new RSACryptoServiceProvider();
                providers[i].ImportParameters(ModulePublicKeys[i]);
            }
            ModuleRSADecrypters = providers;

            if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "Keys")))
            {
                try
                {
                    Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "Keys"));
                }
                catch (Exception ex)
                {
                    ConsoleWrapperUI.WriteLine("An error occurred during initialisation!\n" + ex.Message);

                    if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
                    {
                        ConsoleWrapperUI.WriteLine("\nPlease try running the following command (NOT as root) and restart the program:\n\n\t\tmkdir $HOME/.local/share\n\n" + ex.Message);
                    }
                }
            }

            string publicKeyFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "Keys", "publicKeys.json");

            if (File.Exists(publicKeyFile))
            {
                FilePublicKeys = new List<RSAParameters>();
                List<PublicKeyHolder> holders = JsonSerializer.Deserialize<List<PublicKeyHolder>>(File.ReadAllText(publicKeyFile), Modules.DefaultSerializationOptions);
                for (int i = 0; i < holders.Count; i++)
                {
                    RSAParameters publicKey = new RSAParameters()
                    {
                        Exponent = Convert.FromBase64String(holders[i].Exponent),
                        Modulus = Convert.FromBase64String(holders[i].Modulus)
                    };

                    FilePublicKeys.Add(publicKey);
                }
            }
            else
            {
                FilePublicKeys = new List<RSAParameters>()
                {
                    new RSAParameters()
                    {
                        Exponent = Convert.FromBase64String("AQAB"),
                        Modulus = Convert.FromBase64String("3aIbJNiueDXQ22oMsuH2eFxqNRmwG35bpcY5LgBdiHfN8abJWiZOOiV\u002Bj7Am3wtmLunm8lS4wZk76yfnhHg/K0SvAO5iWS8y\u002BxKGBpdy62OqRjpndVQc9MTvM56BfviLW3voAOLLFIzGKJm7BS4PA8wIa\u002BWrYx/hZQIje67WaMmpOq44wUkKvrVM2SEKsWUAkGjGkEOrshE1j1UKGgBsnxz8lBkaMp\u002B3H5RTHCyl8YwJ2a3SM5S2Nx64YBNF4bn4TTFvv0fRtQlxlEzSchAcBsejjg0SEnGVVoF7B\u002BLzjx2TIrbIrylibq4vYfDDXXpccze5vPH7twYZqW8sC3xIB44aoTi6V7b/rvpL4Qb9756Fq94npkfGTV9RoKN6Tn4nJAbR4hK4nnbCIjuNIgFAbLs6X\u002BfWXb2sX0cygSbzR4D0erLs4iOEkeD3f0fAsj2xWzG4n/vuUUeEEaWW/vAQ5kgddchi8kRjkJrVNBaPSq3QeR4gekUV4TFF4a\u002B\u002B5/zUzWnwSi4GjHb8X63w6pAkYzBqzT4Q0T5Xg\u002BcJZFvmc4o6hrXDdjsbVdHsfnPHfrOwrJhiJQixrvqi/qlhwXlCj7GrUnrcAm\u002Bnjy83H3d32\u002BsDVkhValH0p/hx0n/7IDM24ejHfRiALjZz7/WtkjijG/c\u002B\u002B7FlUbqoj2RsVdpYD1U=")
                    }
                };
            }


            string privateKeyFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "Keys", "privateKey.json");
            if (File.Exists(privateKeyFile))
            {
                UserPrivateKey = LoadPrivateKey(privateKeyFile);
            }
            else
            {
                RSACryptoServiceProvider privateKeyProvider = new RSACryptoServiceProvider(4096);

                RSAParameters privateKey = privateKeyProvider.ExportParameters(true);
                RSAParameters publicKey = privateKeyProvider.ExportParameters(false);

                PrivateKeyHolder privateHolder = new PrivateKeyHolder(privateKey);

                string tempPrivateFile = Path.GetTempFileName();
                File.WriteAllText(tempPrivateFile, JsonSerializer.Serialize<PrivateKeyHolder>(privateHolder, Modules.DefaultSerializationOptions));

                FilePublicKeys.Insert(0, publicKey);

                List<PublicKeyHolder> holders = (from el in FilePublicKeys select new PublicKeyHolder(el)).ToList();

                string tempFile = Path.GetTempFileName();
                File.WriteAllText(tempFile, JsonSerializer.Serialize<List<PublicKeyHolder>>(holders, Modules.DefaultSerializationOptions));

                ModuleUtils.CopyFiles(new[] { (tempPrivateFile, privateKeyFile), (tempFile, publicKeyFile) });

                UserPrivateKey = privateKey;

                try
                {
                    File.Delete(tempPrivateFile);
                }
                catch { }

                try
                {
                    File.Delete(tempFile);
                }
                catch { }
            }
        }

        public static void CreateKeyPair(string outputPrefix)
        {
            RSACryptoServiceProvider privateKeyProvider = new RSACryptoServiceProvider(4096);

            RSAParameters privateKey = privateKeyProvider.ExportParameters(true);
            RSAParameters publicKey = privateKeyProvider.ExportParameters(false);

            PrivateKeyHolder privateHolder = new PrivateKeyHolder(privateKey);
            PublicKeyHolder publicHolder = new PublicKeyHolder(publicKey);

            string privateKeyFile = outputPrefix + ".private.json";
            string publicKeyFile = outputPrefix + ".public.json";

            File.WriteAllText(privateKeyFile, JsonSerializer.Serialize<PrivateKeyHolder>(privateHolder, Modules.DefaultSerializationOptions));
            File.WriteAllText(publicKeyFile, JsonSerializer.Serialize<PublicKeyHolder>(publicHolder, Modules.DefaultSerializationOptions));
        }

        public static void AddPublicKey(RSAParameters publicKey)
        {
            FilePublicKeys.Add(publicKey);

            List<PublicKeyHolder> holders = (from el in FilePublicKeys select new PublicKeyHolder(el)).ToList();

            string tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, JsonSerializer.Serialize<List<PublicKeyHolder>>(holders, Modules.DefaultSerializationOptions));

            ModuleUtils.CopyFiles(new[] { (tempFile, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "Keys", "publicKeys.json")) });
        }

        private static readonly RSAParameters UserPrivateKey;

        public static RSACryptoServiceProvider FileRSAEncrypter
        {
            get
            {
                RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
                provider.ImportParameters(UserPrivateKey);
                return provider;
            }
        }

        public static RSAParameters UserPublicKey
        {
            get
            {
                return FilePublicKeys[0];
            }
        }

        private static readonly List<RSAParameters> FilePublicKeys;

        public static IEnumerable<RSACryptoServiceProvider> FileRSADecrypters
        {
            get
            {
                foreach (RSAParameters par in FilePublicKeys)
                {
                    RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
                    provider.ImportParameters(par);
                    yield return provider;
                }
            }
        }

        private static readonly RSAParameters[] ModulePublicKeys = new RSAParameters[]
        {
            new RSAParameters()
            {
                Exponent = Convert.FromBase64String("AQAB"),
                Modulus = Convert.FromBase64String("twvnoj6v9WbCq/MziFHoPP62eFQ7jirLHzPey097B1PqB2pgESfmnmRW/tY1KPVF4nA7DFOuijX84/Heop+IAHYR2a8CTDw1GhxLI9X5S7a7P7ro/cICwA0jhSKUKtkAMo0KY1YgnVtqkAez3euBuYsCj2FQquS1w7JJfJGIDIE+KK5Nud00GkLRrQZBQcwecasubqOmTeuIVzTCYqPxvlZEsV/H+Mz2AzrvF6+3c0kr9GoMegh5mXzdhvEKLqw9599QXLdN4U30Sfj1vH9tfMpoyWm9i8imYXFJZ48m42mmd47qGmUzpG28dXZDqdBT8NGsEykapCzZwWOSzWWG2cTDjIre6rL0xhkuM97ulyn83h52tzEuk95QH+TbaSMjk7ky4oD9BzD7JzqTeBa1oWEtVGIl1XdZ/XAdnF3eIw4FOm9qb9qpqd1y70uv7R0QctJbHTtGCQxVPJe6bzjfoxVRSXyNndqOnn1z2MlKGMIUrelm0saBkqAvCOjJeupIaEcLQyDQo8Dm3EXrlqIwM/PA7cGme4FepZQtkkFvcwhkVjSqidsmpCXQaUgIkd84xR56dGAc1ljZljWTAWchnI4skDSWFtlpn1YrZtbe6VeAWHcbH13l0uMEZllF0TsuJBgjEzFvSaXh1a7ccbip0vvwSvGc0aFBQ/Sk+3N2JQ0=")
            },
            new RSAParameters()
            {
                Exponent = Convert.FromBase64String("AQAB"),
                Modulus = Convert.FromBase64String("urTvbYy6CkEpT4uz4GoiFlS0cRwRVNpLrojiKX/o9rWL/lrEGBCEP7KLlz0Ofza6O0770sTmYOnCllIcj6/k5V81jE2XLieZ4lt62IMpj3uy21JA/IuWuIHifEjMBdzbCYLe7Lf0W/\u002Bve9cT\u002BxCR3ugL2gH2YIWsAnDZmK8HlXABj/p5N9S\u002BRLUcZO358ESUyTOjcrW7DNU5RyhIxKndvX5buEHGEQd38c1DcW\u002B8UmUVvGcP04yZtUuykrh7POWAtgPCcGQmFkTqRwMmpC7zjGOMuTAmzvTju6LHGPr6az/YDBvkZpuSo8uqyY6/cS8mhgp3vRyOAVuJv8ee1dGCGv\u002BNmlXN9BV2hKCqQluejhlBXNOUiYR3nI6DdmQCA0yi0Ju729Tf/jgWwxQXe7HeKBLBiTfOVbA6m4XwjQO8H8whXA9qrS8a95ooL5PBM4ruMYrbaGGFsdQyaHOwDUCcTgOMqDdrrIB6xiYQxXn0cEY3yrg2\u002BX7YCLjOk0ZiezxeEpTi9PD0CjKs8mw76hpud/Nf6mauvVcF/jTKctqfqcYPg\u002BpK/tNkB9hWR0ABkJBwk8b8xQslD18UWIzW0Gm42S\u002Bze3LLf3XzoDD68w1B1w/9ZMkVgvbBAiFPPIxYYxtE9Dpl43eUsOPfPmqs\u002BOdStLyBJLztcx97XprE\u002BMN6/AE=")
            },
        };

        public static readonly RSACryptoServiceProvider[] ModuleRSADecrypters;

        public static RSAParameters LoadPrivateKey(string fileName)
        {
            PrivateKeyHolder holder = JsonSerializer.Deserialize<PrivateKeyHolder>(File.ReadAllText(fileName), Modules.DefaultSerializationOptions);

            return new RSAParameters()
            {
                Exponent = Convert.FromBase64String(holder.Exponent),
                Modulus = Convert.FromBase64String(holder.Modulus),
                P = Convert.FromBase64String(holder.P),
                Q = Convert.FromBase64String(holder.Q),
                DP = Convert.FromBase64String(holder.DP),
                DQ = Convert.FromBase64String(holder.DQ),
                InverseQ = Convert.FromBase64String(holder.InverseQ),
                D = Convert.FromBase64String(holder.D)
            };
        }

        private class PrivateKeyHolder
        {
            public string Exponent { get; set; }
            public string Modulus { get; set; }
            public string P { get; set; }
            public string Q { get; set; }
            public string DP { get; set; }
            public string DQ { get; set; }
            public string InverseQ { get; set; }
            public string D { get; set; }

            public PrivateKeyHolder()
            {
            }

            public PrivateKeyHolder(RSAParameters privateKey)
            {
                this.Exponent = Convert.ToBase64String(privateKey.Exponent);
                this.Modulus = Convert.ToBase64String(privateKey.Modulus);
                this.P = Convert.ToBase64String(privateKey.P);
                this.Q = Convert.ToBase64String(privateKey.Q);
                this.DP = Convert.ToBase64String(privateKey.DP);
                this.DQ = Convert.ToBase64String(privateKey.DQ);
                this.InverseQ = Convert.ToBase64String(privateKey.InverseQ);
                this.D = Convert.ToBase64String(privateKey.D);
            }
        }



        public class PublicKeyHolder
        {
            public string Exponent { get; set; }
            public string Modulus { get; set; }

            public PublicKeyHolder()
            {
            }

            public PublicKeyHolder(RSAParameters publicKey)
            {
                this.Exponent = Convert.ToBase64String(publicKey.Exponent);
                this.Modulus = Convert.ToBase64String(publicKey.Modulus);
            }
        }


        public static bool VerifyStringSignature(string subject, string signature, IEnumerable<RSACryptoServiceProvider> rsaDecrypters)
        {
            if (string.IsNullOrEmpty(signature))
            {
                return false;
            }

            using (MemoryStream codeStream = new MemoryStream())
            using (StreamWriter memoryWriter = new StreamWriter(codeStream, Encoding.UTF8))
            {
                memoryWriter.Write(subject);
                byte[] byteSignature = Convert.FromBase64String(signature);

                foreach (RSACryptoServiceProvider provider in rsaDecrypters)
                {
                    codeStream.Seek(0, SeekOrigin.Begin);
                    if (provider.VerifyData(codeStream, byteSignature, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static string SignString(string subject, RSACryptoServiceProvider rsaEncrypter)
        {
            using (MemoryStream codeStream = new MemoryStream())
            using (StreamWriter memoryWriter = new StreamWriter(codeStream, Encoding.UTF8))
            {
                memoryWriter.Write(subject);
                codeStream.Seek(0, SeekOrigin.Begin);
                byte[] byteSignature = rsaEncrypter.SignData(codeStream, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
                return Convert.ToBase64String(byteSignature);
            }
        }
    }
}
