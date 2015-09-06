using System;
using System.IO;
namespace TwitchBot {
    class Account {
        private const string errorMessage = "error: invalid account information";

        public string Username { get; private set; }
        public string Password { get; private set; }

        private Account(string username, string password) {
            Username = username;
            Password = password;
        }

        public static Account Load(string fileName) {
            StreamReader inputStream = new StreamReader(File.OpenRead(fileName));

            Tokenizer tokenizer = new Tokenizer(inputStream.ReadToEnd());

            string username;
            string password;

            try {
                username = tokenizer.GetToken();
                password = tokenizer.GetToken();
            } catch (Exception e) {
                if (e.Message == "eof")
                    throw new Exception(errorMessage);

                return null;
            }

            try {
                tokenizer.GetToken();
                throw new Exception(errorMessage);
            } catch (Exception e) {
                if (e.Message != "eof")
                    throw e;
            }

            return new Account(username, password);
        }
    }
}
