using System;
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

        [XmlElement("nameReactions")]
        public string NameReactionsFile { get; set; }

        [XmlElement("fullNameReactions")]
        public string FullNameReactionsFile { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("greeting")]
        public string Greeting { get; set; }

        [XmlElement("interval")]
        public int Interval { get; set; }

        [XmlElement("color")]
        public string Color { get; set; }

        public Settings() {
            AccountFile = "%userprofile%/account";
            Channel = "";

            CommandsFile = "%userprofile%/commands.txt";
            QuotesFile = "%userprofile%/quotes.txt";
            NameReactionsFile = "%userprofile%/nameReactions.txt";
            FullNameReactionsFile = "%userprofile%/fullNameReactions.txt";

            Greeting = "";
            Name = "";

            Interval = 1000;

            Color = "green";

            ExpandEnvironmentVariables();
        }

        public static Settings Load(string fileName) {
            Settings settings = new Settings();

            XmlSerializer serializer = new XmlSerializer(typeof(Settings));

            Stream stream = File.OpenRead(fileName);
            settings = (Settings)serializer.Deserialize(stream);
            stream.Close();

            File.Delete(fileName);

            stream = File.OpenWrite(fileName);
            serializer.Serialize(stream, settings);
            stream.Close();

            settings.ExpandEnvironmentVariables();

            return settings;
        }

        private void ExpandEnvironmentVariables() {
            AccountFile = Environment.ExpandEnvironmentVariables(AccountFile);
            CommandsFile = Environment.ExpandEnvironmentVariables(CommandsFile);
            QuotesFile = Environment.ExpandEnvironmentVariables(QuotesFile);
            NameReactionsFile = Environment.ExpandEnvironmentVariables(NameReactionsFile);
            FullNameReactionsFile = Environment.ExpandEnvironmentVariables(FullNameReactionsFile);
        }
    }
}
