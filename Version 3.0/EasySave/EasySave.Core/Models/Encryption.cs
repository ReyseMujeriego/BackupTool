namespace EasySave.Core.Models
{
    /// <summary>
    /// Stores encryption settings shared by all jobs.
    /// </summary>
    public class EncryptionSettings
    {
        public bool EnableEncryption { get; set; } = false;
        public string EncryptionKey { get; set; } = "";
        public List<string> EncryptedExtensions { get; set; } = new List<string>();
    }
}