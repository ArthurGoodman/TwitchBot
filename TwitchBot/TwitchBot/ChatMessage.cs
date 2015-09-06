//namespace TwitchBot {
//    class ChatMessage {
//        public string Username { get; private set; }
//        public string Message { get; private set; }

//        public ChatMessage(IrcMessage ircMessage) {
//            Username = "";

//            if (ircMessage.Command == "PRIVMSG") {
//                Username = ircMessage.Args[0].Substring(1);
//                Message = ircMessage.Trailing;
//            } else
//                Message = ircMessage.ToString();
//        }

//        public new string ToString() {
//            return (Username != "" ? Username + ": " : "") + Message;
//        }
//    }
//}
