﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace TwitchBot {
    public class Bot {
        public static Bot Instance { get; private set; }

        private IrcClient ircClient;
        private Settings settings;
        private Account account;

        private Random random = new Random();

        private Dictionary<string, Command> commands = new Dictionary<string, Command>();

        private Stopwatch stopwatch = new Stopwatch();

        private bool raffle;
        private List<string> entrants = new List<string>();

        public string Channel { get { return ircClient.Channel; } }
        public List<string> Mods { get; private set; }

        public Bot() {
            Instance = this;

            stopwatch.Start();

            raffle = false;

            Mods = new List<string>();

            Initialize();
        }

        private void Initialize() {
            settings = Settings.Load("settings.xml");

            account = Account.Load(settings.AccountFile);
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

                if (ircMessage.Command == "MODE") {
                    if (ircMessage.Args[1] == "+o")
                        Mods.Add(ircMessage.Args[2]);
                    else if (ircMessage.Args[1] == "-o")
                        Mods.Add(ircMessage.Args[2]);
                } else if (ircMessage.Command == "PING")
                    ircClient.SendIrcMessage("PONG :" + ircMessage.Trailing);

                if (ircMessage.Command != "PRIVMSG") {
                    Console.WriteLine(ircMessage.ToString());
                    continue;
                }

                ChatMessage chatMessage = new ChatMessage(ircMessage);

                Console.WriteLine(chatMessage.ToString());

                string message = chatMessage.Message;
                string username = chatMessage.Username;

                if (raffle && Regex.Match(message, @"\b(?i)raffle\b").Success)
                    if (!entrants.Contains(username))
                        entrants.Add(username);

                if (stopwatch.Elapsed.TotalMilliseconds < settings.Interval)
                    continue;

                bool isCommand = false;

                foreach (KeyValuePair<string, Command> command in commands)
                    if (message == command.Key || message.StartsWith(command.Key + " ")) {
                        command.Value.Execute(username, message);
                        isCommand = true;
                        break;
                    }

                if (!isCommand) {
                    if (settings.Name != "" && Regex.Match(message, @"\b(?i)" + settings.Name + @"\b").Success)
                        Say(ReadRandomLine(settings.NameReactionsFile));
                    else if (Regex.Match(message, @"\b(?i)" + account.Username + @"\b").Success)
                        Say(ReadRandomLine(settings.FullNameReactionsFile));
                }
            }
        }

        public void Say(string message) {
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
            commands.Add("!addcom", new BuiltinCommand(2, Command.AccessLevel.Mod, (string username, string[] args) => {
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

                if (commands.ContainsKey(args[1])) {
                    Say(username + " -> Command " + args[1] + " already exists.");
                    return 1;
                }

                Command.AccessLevel level = Command.AccessLevel.Regular;

                if (args[2].StartsWith("#")) {
                    string[] s = args[2].Substring(1).Split(new[] { ' ' }, 2);

                    if (s.Length == 1) {
                        Say(username + " -> A command cannot be empty.");
                        return 1;
                    }

                    if (s[0] == "regular")
                        level = Command.AccessLevel.Regular;
                    else if (s[0] == "mod")
                        level = Command.AccessLevel.Mod;
                    else if (s[0] == "owner")
                        level = Command.AccessLevel.Owner;
                    else {
                        Say(username + " -> Invalid access level #" + s[0] + ".");
                        return 1;
                    }

                    args[2] = s[1];
                }

                if (level == Command.AccessLevel.Owner && username != Channel) {
                    Say(username + " -> Mods cannot add #owner commands.");
                    return 1;
                }

                commands.Add(args[1], new UserCommand(level, args[2]));

                SaveCommands();

                Say(username + " -> Command " + args[1] + " added.");
                return 0;
            }));

            commands.Add("!editcom", new BuiltinCommand(2, Command.AccessLevel.Mod, (string username, string[] args) => {
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

                Command.AccessLevel level = Command.AccessLevel.Regular;

                if (args[2].StartsWith("#")) {
                    string[] s = args[2].Substring(1).Split(new[] { ' ' }, 2);

                    if (s.Length == 1) {
                        Say(username + " -> A command cannot be empty.");
                        return 1;
                    }

                    if (s[0] == "regular")
                        level = Command.AccessLevel.Regular;
                    else if (s[0] == "mod")
                        level = Command.AccessLevel.Mod;
                    else if (s[0] == "owner")
                        level = Command.AccessLevel.Owner;
                    else {
                        Say(username + " -> Invalid access level #" + s[0] + ".");
                        return 1;
                    }

                    args[2] = s[1];
                }

                if (level == Command.AccessLevel.Owner && username != Channel) {
                    Say(username + " -> Mods cannot add #owner commands.");
                    return 1;
                }

                if (!commands.ContainsKey(args[1])) {
                    Say(username + " -> Command " + args[1] + " doesn't exist.");
                    return 1;
                }

                Command command;
                commands.TryGetValue(args[1], out command);

                if (command is BuiltinCommand) {
                    Say(username + " -> Cannot edit built-in commands.");
                    return 1;
                }

                if (command.Level == Command.AccessLevel.Owner && username != Channel) {
                    Say(username + " -> Mods cannot edit #owner commands.");
                    return 1;
                }

                commands.Remove(args[1]);
                commands.Add(args[1], new UserCommand(level, args[2]));

                SaveCommands();

                Say(username + " -> Command " + args[1] + " edited.");
                return 0;
            }));

            commands.Add("!delcom", new BuiltinCommand(1, Command.AccessLevel.Mod, (string username, string[] args) => {
                if (args.Length == 1) {
                    Say(username + " -> No command specified.");
                    return 1;
                }

                if (!commands.ContainsKey(args[1])) {
                    Say(username + " -> Command " + args[1] + " doesn't exist.");
                    return 1;
                }

                Command command;
                commands.TryGetValue(args[1], out command);

                if (command is BuiltinCommand) {
                    Say(username + " -> Cannot delete built-in commands.");
                    return 1;
                }

                if (command.Level == Command.AccessLevel.Owner && username != Channel) {
                    Say(username + " -> Mods cannot delete #owner commands.");
                    return 1;
                }

                commands.Remove(args[1]);

                SaveCommands();

                Say(username + " -> Command " + args[1] + " deleted.");
                return 0;
            }));

            commands.Add("!quote", new BuiltinCommand(0, Command.AccessLevel.Regular, (string username, string[] args) => {
                Say(ReadRandomLine(settings.QuotesFile));
                return 0;
            }));

            commands.Add("!addquote", new BuiltinCommand(1, Command.AccessLevel.Mod, (string username, string[] args) => {
                if (args.Length == 1) {
                    Say(username + " -> A quote cannot be empty.");
                    return 1;
                }

                File.AppendAllText(settings.QuotesFile, args[1] + Environment.NewLine);

                Say(username + " -> Quote added.");
                return 0;
            }));

            commands.Add("!startraffle", new BuiltinCommand(0, Command.AccessLevel.Owner, (string username, string[] args) => {
                raffle = true;

                entrants.Clear();

                Say("The raffle has started! Type \"raffle\" to enter.");
                return 0;
            }));

            commands.Add("!closeraffle", new BuiltinCommand(0, Command.AccessLevel.Owner, (string username, string[] args) => {
                if (raffle) {
                    raffle = false;
                    Say("The raffle is closed.");
                }

                return 0;
            }));

            commands.Add("!rafflewinner", new BuiltinCommand(0, Command.AccessLevel.Owner, (string username, string[] args) => {
                if (entrants.Count == 0)
                    Say("No one entered.");
                else {
                    Say("Entrants: " + ListEntrants() + ".");
                    Say("The winner is " + entrants[random.Next() % entrants.Count] + "!");
                }

                raffle = false;

                return 0;
            }));

            commands.Add("!entrants", new BuiltinCommand(0, Command.AccessLevel.Regular, (string username, string[] args) => {
                if (!raffle)
                    return 1;

                if (entrants.Count == 0) {
                    Say("No one entered.");
                    return 0;
                }

                Say("Raffle ongoing, type \"raffle\" to enter. Entrants: " + ListEntrants() + ".");

                return 0;
            }));

            commands.Add("!raid", new BuiltinCommand(1, Command.AccessLevel.Mod, (string username, string[] args) => {
                if (args.Length == 1)
                    return 1;

                Say("Thank you so much for the raid! Please go check out their channel at http://twitch.tv/" + args[1]);
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

                if (s.Length == 1 || !s[0].StartsWith("!"))
                    throw new Exception("error: invalid commands");

                commands.Add(s[0], UserCommand.FromString(s[1]));
            }
        }

        private void SaveCommands() {
            List<string> lines = new List<string>();

            foreach (KeyValuePair<string, Command> command in commands)
                if (command.Value is UserCommand)
                    lines.Add(command.Key + " " + command.Value.ToString());

            File.WriteAllLines(settings.CommandsFile, lines);
        }

        private string ListEntrants() {
            string str = "";

            str += entrants[0];

            for (int i = 1; i < entrants.Count; i++)
                str += ", " + entrants[i];

            return str;
        }
    }
}
