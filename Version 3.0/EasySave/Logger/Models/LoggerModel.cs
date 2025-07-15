using System;
using System.Xml.Serialization;

namespace Logger.Models
{
    [XmlRoot("LogEntry")]
    public class LoggerModel
    {
        [XmlElement("TimeStamp")]
        public DateTime TimeStamp { get; set; }

        [XmlElement("SaveName")]
        public string SaveName { get; set; } = "";

        [XmlElement("FileSize")]
        public long FileSize { get; set; }

        [XmlElement("FileTransferTime")]
        public double FileTransferTime { get; set; }

        [XmlElement("FileSource")]
        public string FileSource { get; set; } = "";

        [XmlElement("FileTarget")]
        public string FileTarget { get; set; } = "";

        [XmlElement("EncryptionTime")]
        public double EncryptionTime { get; set; }
    }
}
