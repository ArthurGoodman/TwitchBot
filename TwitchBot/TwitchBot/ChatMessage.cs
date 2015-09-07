namespace TwitchBot {
    public class ChatMessage {
        public string Username { get; private set; }
        public string Message { get; private set; }

        public ChatMessage(IrcMessage ircMessage) {
            Username = "";

            if (ircMessage.Command == "PRIVMSG") {
                Username = ircMessage.Preffix.Substring(0, ircMessage.Preffix.IndexOf("!"));
                Message = ircMessage.Trailing;
            } else
                Message = ircMessage.ToString();
        }

        public new string ToString() {
            return (Username != "" ? Username + ": " : "") + Message;
        }

        public string Inspect() {
            return Username != "" ? "{ username = \"" + Username + "\", message = \"" + Message + "\" }" : Message;
        }
    }
}
