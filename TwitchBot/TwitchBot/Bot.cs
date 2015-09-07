
namespace TwitchBot {
    public class Bot {
        private IrcClient ircClient;
        private Settings settings;

        public Bot() {
            settings = Settings.Load("settings.xml");

            Account account = Account.Load(settings.AccountFile);
            ircClient = new IrcClient("irc.twitch.tv", 6667, account.Username, account.Password);
        }

        public void Run() {
            ircClient.JoinRoom(settings.Channel);

            while (true) {
                string message = ircClient.ReadIrcMessage().ToString();

                System.Console.WriteLine(message);

                //if (message.EndsWith("!hello"))
                //    ircClient.SendChatMessage("Hey, man!");

                //if (Regex.Match(message, @"\b(?i)tom\b").Success)
                //    ircClient.SendChatMessage("I'm not Tom! DansGame");
            }
        }
    }
}
