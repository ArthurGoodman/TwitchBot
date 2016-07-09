using System;
using System.Collections.Generic;

namespace TwitchBot {
    public class IrcMessage {
        public string Preffix { get; private set; }
        public string Command { get; private set; }
        public string[] Args { get; private set; }
        public string Trailing { get; private set; }

        public IrcMessage(string message) {
            Preffix = "";
            Trailing = "";

            if (message[0] == ':') {
                string[] s = message.Substring(1).Split(new[] { ' ' }, 2);

                Preffix = s[0];
                message = s[1];
            }

            List<string> args = new List<string>();

            if (message.IndexOf(':') != -1) {
                string[] s = message.Split(new[] { ":" }, 2, StringSplitOptions.None);

                args.AddRange(s[0].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                Trailing = s[1];
            } else
                args.AddRange(message.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

            Command = args[0];
            args.RemoveAt(0);

            Args = args.ToArray();
        }

        public override string ToString() {
            string message = "";

            if (Preffix != "")
                message += ":" + Preffix + " ";

            message += Command;

            foreach (string arg in Args)
                message += " " + arg;

            if (Trailing != "")
                message += " :" + Trailing;

            return message;
        }

        public string Inspect() {
            string message = "";

            message += "{ preffix = \"" + Preffix + "\"";
            message += ", command = \"" + Command + "\"";

            message += ", args = [";

            if (Args.Length > 0)
                message += "\"" + Args[0] + "\"";

            for (int i = 1; i < Args.Length; i++)
                message += ", \"" + Args[i] + "\"";

            message += "], trailing = \"" + Trailing + "\" }";

            return message;
        }
    }
}
