using System.IO;
using System.Net.Sockets;

namespace TwitchBot {
    class IrcClient {
        private string username;
        private string channel;

        private TcpClient tcpClient;
        private StreamReader inputStream;
        private StreamWriter outputStream;

        public IrcClient(string ip, int port, string username, string password) {
            this.username = username;

            tcpClient = new TcpClient(ip, port);
            inputStream = new StreamReader(tcpClient.GetStream());
            outputStream = new StreamWriter(tcpClient.GetStream());

            outputStream.WriteLine("PASS " + password);
            outputStream.WriteLine("NICK " + username);
            outputStream.WriteLine("USER " + username + " 8 * :" + username);
            outputStream.Flush();
        }

        public void JoinRoom(string channel) {
            this.channel = channel;

            outputStream.WriteLine("JOIN #" + channel);
            outputStream.Flush();
        }

        public void SendIrcMessage(string message) {
            outputStream.WriteLine(message);
            outputStream.Flush();
        }

        public void SendChatMessage(string message) {
            SendIrcMessage(":" + username + "!" + username + "@" + username + ".tmi.twitch.tv PRIVMSG #" + channel + " :" + message);
        }

        public IrcMessage ReadIrcMessage() {
            string message = inputStream.ReadLine();
            return new IrcMessage(message);
        }

        //public ChatMessage ReadChatMessage() {
        //    return new ChatMessage(ReadIrcMessage());
        //}
    }
}
