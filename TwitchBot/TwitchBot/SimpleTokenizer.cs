namespace TwitchBot {
    public class SimpleTokenizer : ITokenizer<string> {
        private string str;
        private int pos;

        public SimpleTokenizer(string str) {
            Initialize(str);
        }

        public void Initialize(string str) {
            this.str = str;
            pos = 0;

            SkipSpaces();
        }

        public bool HasToken() {
            return pos < str.Length;
        }

        public string GetToken() {
            if (!HasToken())
                return null;

            string token = "";

            while (HasToken() && !char.IsWhiteSpace(str[pos]))
                token += str[pos++];

            SkipSpaces();

            return token;
        }

        private void SkipSpaces() {
            while (HasToken() && char.IsWhiteSpace(str[pos]))
                pos++;
        }
    }
}
