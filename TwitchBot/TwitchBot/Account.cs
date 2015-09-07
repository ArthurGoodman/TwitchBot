using System;
using System.IO;

namespace TwitchBot {
    public class Account {
        public string Username { get; private set; }
        public string Password { get; private set; }

        private Account(string username, string password) {
            Username = username;
            Password = password;
        }

        public static Account Load(string fileName) {
            StreamReader inputStream = new StreamReader(File.OpenRead(fileName));

            ITokenizer<string> tokenizer = new SimpleTokenizer(inputStream.ReadToEnd());

            string username, password;

            username = tokenizer.GetToken();
            password = tokenizer.GetToken();

            if (username == null || password == null || tokenizer.HasToken())
                throw new Exception("error: invalid account information");

            return new Account(username, password);
        }
    }
}
