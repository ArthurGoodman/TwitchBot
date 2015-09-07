using System.IO;
using System.Xml.Serialization;

namespace TwitchBot {
    public class Settings {
        [XmlElement("account")]
        public string AccountFile { get; set; }

        [XmlElement("channel")]
        public string Channel { get; set; }

        [XmlElement("commands")]
        public string CommandsFile { get; set; }

        [XmlElement("quotes")]
        public string QuotesFile { get; set; }

        [XmlElement("phrases")]
        public string PhrasesFile { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("greeting")]
        public string Greeting { get; set; }

        [XmlElement("interval")]
        public int Interval { get; set; }

        [XmlElement("color")]
        public string Color { get; set; }

        public Settings() {
            AccountFile = "account.txt";
            Channel = "";

            CommandsFile = "commands.txt";
            QuotesFile = "quotes.txt";
            PhrasesFile = "phrases.txt";

            Greeting = "";
            Name = "";

            Interval = 1000;

            Color = "green";
        }

        public static Settings Load(string fileName) {
            Settings settings = new Settings();

            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            Stream stream;

            try {
                stream = File.OpenRead(fileName);
                settings = (Settings)serializer.Deserialize(stream);
                stream.Close();
            } catch {
            }

            stream = File.OpenWrite(fileName);
            serializer.Serialize(stream, settings);
            stream.Close();

            return settings;
        }
    }
}
