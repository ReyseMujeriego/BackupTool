using CryptoSoft;

namespace EasySave.Core.Services
{
    public static class CryptoService
    {
        public static event Action<string>? OnMessageLogged;

        /// <summary>
        /// Encrypts a file and replaces the original one.
        /// Returns encryption time in milliseconds, or -1 on failure.
        /// </summary>
        public static int EncryptFile(string filePath)
        {
            if (!JobManager.Instance.EncryptionSettings.EnableEncryption)
            {
                OnMessageLogged?.Invoke(LanguageManager.GetString("EncryptionDisabled", filePath));
                return 0; // Return 0 to indicate that no encryption was performed
            }

            try
            {
                // 🔹 Encrypt the file
                int encryptionTime = EncryptAndWriteToFile(filePath);
                OnMessageLogged?.Invoke(LanguageManager.GetString("FileEncrypted", filePath, encryptionTime));
                return encryptionTime;
            }
            catch (Exception ex)
            {
                OnMessageLogged?.Invoke(LanguageManager.GetString("EncryptionFailed", filePath, ex.Message));
                return -1;
            }
        }

        /// <summary>
        /// Decrypts a file based on its extension and replaces the original one.
        /// Returns decryption time in milliseconds, or -1 on failure.
        /// </summary>
        public static int DecryptFile(string filePath, string key)
        {
            List<string> encryptedExtensions = JobManager.Instance.EncryptionSettings.EncryptedExtensions;
            Console.WriteLine(encryptedExtensions[0]);
            string fileExtension = Path.GetExtension(filePath);

            if (!encryptedExtensions.Contains(fileExtension))
            {
                OnMessageLogged?.Invoke(LanguageManager.GetString("FileNotEncrypted", filePath));
                return -1;
            }

            try
            {
                //Decrypt the file
                int decryptionTime = DecryptAndWriteToFile(filePath, key);
                OnMessageLogged?.Invoke(LanguageManager.GetString("DecryptionComplete", filePath, decryptionTime));
                return decryptionTime;
            }
            catch (Exception ex)
            {
                OnMessageLogged?.Invoke(LanguageManager.GetString("DecryptionFailed", filePath, ex.Message));
                return -1;
            }
        }
        
        /// <summary>
        /// Decrypts all encrypted files in a given directory.
        /// </summary>
        /// <param name="directoryPath"> Path to the folder.</param>
        /// <param name="key">Key to decrypt the files.</param>
        public static void DecryptFilesInDirectory(string directoryPath, string key)
        {
            if (!Directory.Exists(directoryPath))
            {
                OnMessageLogged?.Invoke(LanguageManager.GetString("InvalidPath", directoryPath));
                return;
            }

            var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories)
                                 .Where(file => JobManager.Instance.EncryptionSettings.EncryptedExtensions.Contains(Path.GetExtension(file)))
                                 .ToList();

            if (!files.Any())
            {
                OnMessageLogged?.Invoke(LanguageManager.GetString("NoEncryptedFiles", directoryPath));
                return;
            }

            OnMessageLogged?.Invoke(LanguageManager.GetString("DecryptingFiles", files.Count));

            foreach (var file in files)
            {
                DecryptFile(file, key);
            }

            OnMessageLogged?.Invoke(LanguageManager.GetString("DecryptionComplete"));
        }

        /// <summary>
        /// Encrypts the file.
        /// Returns encryption time in milliseconds.
        /// </summary>
        private static int EncryptAndWriteToFile(string filePath)
        {
            FileManager fileManager = new FileManager(filePath, JobManager.Instance.EncryptionSettings.EncryptionKey);
            return fileManager.TransformFile();
        }

        /// <summary>
        /// Decrypts a file.
        /// Returns decryption time in milliseconds.
        /// </summary>
        private static int DecryptAndWriteToFile(string filePath, string key)
        {
            FileManager fileManager = new FileManager(filePath, key);
            return fileManager.TransformFile();
        }
    }
}