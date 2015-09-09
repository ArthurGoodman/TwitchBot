using System;
using System.IO;
using System.Net.Sockets;

namespace TwitchBot {
    public class IrcClient {
        private string username;

        private TcpClient tcpClient;
        private StreamReader inputStream;
        private StreamWriter outputStream;

        public string Channel { get; private set; }

        public IrcClient(string ip, int port, string username, string password) {
            this.username = username;

            tcpClient = new TcpClient(ip, port);
            inputStream = new StreamReader(tcpClient.GetStream());
            outputStream = new StreamWriter(tcpClient.GetStream());

            outputStream.WriteLine("PASS " + password);
            outputStream.WriteLine("NICK " + username);
            outputStream.WriteLine("USER " + username + " 8 * :" + username);
            outputStream.WriteLine("CAP REQ :twitch.tv/membership");
            outputStream.Flush();
        }

        public void JoinRoom(string channel) {
            this.Channel = channel;

            outputStream.WriteLine("JOIN #" + channel);
            outputStream.Flush();
        }

        public void SendIrcMessage(string message) {
            outputStream.WriteLine(message);
            outputStream.Flush();
        }

        public void SendChatMessage(string message) {
            SendIrcMessage(":" + username + "!" + username + "@" + username + ".tmi.twitch.tv PRIVMSG #" + Channel + " :" + message);
        }

        public IrcMessage ReadIrcMessage() {
            string message = inputStream.ReadLine();

            if (message == null)
                throw new Exception("error: failed to connect");

            return new IrcMessage(message);
        }
    }
}
