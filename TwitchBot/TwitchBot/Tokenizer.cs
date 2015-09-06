using System;
namespace TwitchBot {
    class Tokenizer {
        private string str;
        private int pos;

        public Tokenizer(string str) {
            Initialize(str);
        }

        public void Initialize(string str) {
            this.str = str;
            pos = 0;
        }

        public string GetToken() {
            string token = "";

            while (pos < str.Length && char.IsWhiteSpace(str[pos]))
                pos++;

            if (pos == str.Length)
                throw new Exception("eof");
            else if (str[pos] == '\"') {
                pos++;

                while (pos < str.Length && str[pos] != '\"')
                    token += str[pos++];

                if (str[pos] == '\"')
                    pos++;
            } else
                while (pos < str.Length && !char.IsWhiteSpace(str[pos]))
                    token += str[pos++];

            return token;
        }
    }
}
