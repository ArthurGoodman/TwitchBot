using System;
using System.Text.RegularExpressions;

namespace TwitchBot {
    class Program {
        static void Main(string[] args) {
            Account account;

            try {
                account = Account.Load("account");
            } catch (Exception e) {
                System.Console.WriteLine(e.Message);
                System.Console.ReadKey();
                return;
            }

            IrcClient ircClient = new IrcClient("irc.twitch.tv", 6667, account.Username, account.Password);
            ircClient.JoinRoom("arthurgoodman");

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
