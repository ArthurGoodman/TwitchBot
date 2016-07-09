using System;

namespace TwitchBot {
    class UserCommand : Command {
        private string message;

        public UserCommand(AccessLevel accessLevel, string message)
            : base(0, accessLevel) {
            this.message = message;
        }

        protected override void Perform(string username, string[] args) {
            Bot.Instance.Say(message);
        }

        public override string ToString() {
            string s = "";

            switch (Level) {
                case AccessLevel.Regular:
                    s = "#reg";
                    break;

                case AccessLevel.Mod:
                    s = "#mod";
                    break;

                case AccessLevel.Owner:
                    s = "#own";
                    break;
            }


            return s + " " + message;
        }

        public static UserCommand FromString(string str) {
            string[] s = str.Split(new[] { ' ' }, 2);

            if (s.Length == 1)
                throw new Exception("error: invalid commands");

            AccessLevel accessLevel;

            if (s[0] == "#reg")
                accessLevel = AccessLevel.Regular;
            else if (s[0] == "#mod")
                accessLevel = AccessLevel.Mod;
            else if (s[0] == "#own")
                accessLevel = AccessLevel.Owner;
            else
                throw new Exception("error: invalid commands");

            return new UserCommand(accessLevel, s[1]);
        }
    }
}
