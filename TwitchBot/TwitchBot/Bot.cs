using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace TwitchBot {
    public class Bot {
        private IrcClient ircClient;
        private Settings settings;

        private Random random;

        private List<string> mods;
        private Dictionary<string, string> userCommands;
        private Dictionary<string, Command> commands;

        private Stopwatch stopwatch;

        public Bot() {
            random = new Random();

            mods = new List<string>();
            userCommands = new Dictionary<string, string>();
            commands = new Dictionary<string, Command>();

            stopwatch = new Stopwatch();
            stopwatch.Start();

            Initialize();
        }

        private void Initialize() {
            settings = Settings.Load("settings.xml");

            Account account = Account.Load(settings.AccountFile);
            ircClient = new IrcClient("irc.twitch.tv", 6667, account.Username, account.Password);

            SetupCommands();
            LoadCommands();
        }

        public void Run() {
            ircClient.JoinRoom(settings.Channel);

            ircClient.SendChatMessage(".color " + settings.Color);

            if (settings.Greeting != "")
                ircClient.SendChatMessage(settings.Greeting);

            while (true) {
                IrcMessage ircMessage = ircClient.ReadIrcMessage();

                System.Console.WriteLine(ircMessage.ToString());

                if (ircMessage.Command == "MODE") {
                    if (ircMessage.Args[1] == "+o")
                        mods.Add(ircMessage.Args[2]);
                    else if (ircMessage.Args[1] == "-o")
                        mods.Add(ircMessage.Args[2]);
                }

                if (stopwatch.Elapsed.TotalMilliseconds < settings.Interval)
                    continue;

                ChatMessage chatMessage = new ChatMessage(ircMessage);

                string message = chatMessage.Message;
                string username = chatMessage.Username;

                if (settings.Name != "" && Regex.Match(message, @"\b(?i)" + settings.Name + @"\b").Success)
                    Say(ReadRandomLine(settings.PhrasesFile));
                else if (userCommands.ContainsKey(message)) {
                    string value;
                    userCommands.TryGetValue(message, out value);
                    Say(value);
                } else
                    foreach (KeyValuePair<string, Command> command in commands)
                        if (message == command.Key || message.StartsWith(command.Key + " ")) {
                            if (command.Value.Mod && !mods.Contains(username))
                                Say(username + " -> " + command.Key + " is a mod-only command.");
                            else
                                command.Value.Body(username, message.Split(new[] { ' ' }, command.Value.Argc + 1, StringSplitOptions.RemoveEmptyEntries));

                            break;
                        }
            }
        }

        private void Say(string message) {
            stopwatch.Restart();
            ircClient.SendChatMessage(message);
        }

        private string ReadRandomLine(string fileName) {
            try {
                string[] lines = File.ReadAllLines(fileName);
                return lines.Length == 0 ? "" : lines[random.Next() % lines.Length];
            } catch (Exception e) {
                System.Console.WriteLine(e.Message);
                return "";
            }
        }

        private void SetupCommands() {
            commands.Add("!addcom", new Command(2, true, (string username, string[] args) => {
                if (args.Length == 1) {
                    Say(username + " -> No command specified.");
                    return 1;
                }

                if (!args[1].StartsWith("!")) {
                    Say(username + " -> A command must start with !.");
                    return 1;
                }

                if (args.Length == 2) {
                    Say(username + " -> A command cannot be empty.");
                    return 1;
                }

                if (userCommands.ContainsKey(args[1])) {
                    Say(username + " -> Command " + args[1] + " already exists.");
                    return 1;
                }

                userCommands.Add(args[1], args[2]);

                SaveCommands();

                Say(username + " -> Command " + args[1] + " added.");

                return 0;
            }));

            commands.Add("!delcom", new Command(1, true, (string username, string[] args) => {
                if (args.Length == 1) {
                    Say(username + " -> No command specified.");
                    return 1;
                }

                if (!userCommands.ContainsKey(args[1])) {
                    Say(username + " -> Command " + args[1] + " doesn't exist.");
                    return 1;
                }

                userCommands.Remove(args[1]);

                SaveCommands();

                Say(username + " -> Command " + args[1] + " deleted.");

                return 0;
            }));

            commands.Add("!quote", new Command(0, false, (string username, string[] args) => {
                Say(ReadRandomLine(settings.QuotesFile));

                return 0;
            }));

            commands.Add("!addquote", new Command(1, true, (string username, string[] args) => {
                if (args.Length == 1) {
                    Say(username + " -> A quote cannot be empty.");
                    return 1;
                }

                File.AppendAllText(settings.QuotesFile, args[1] + "\n");

                Say(username + " -> Quote added.");

                return 0;
            }));
        }

        private void LoadCommands() {
            if (!File.Exists(settings.CommandsFile)) {
                File.Create(settings.CommandsFile);
                return;
            }

            string[] lines = File.ReadAllLines(settings.CommandsFile);

            foreach (string str in lines) {
                string[] s = str.Split(new[] { ' ' }, 2);

                if (!s[0].StartsWith("!") || s.Length < 2)
                    throw new Exception("error: invalid commands");

                userCommands.Add(s[0], s[1]);
            }
        }

        private void SaveCommands() {
            int c = 0;
            string[] lines = new string[userCommands.Count];

            foreach (KeyValuePair<string, string> command in userCommands)
                lines[c++] = command.Key + " " + command.Value;

            File.WriteAllLines(settings.CommandsFile, lines);
        }
    }
}
