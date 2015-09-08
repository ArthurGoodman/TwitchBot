using System;

namespace TwitchBot {
    class BuiltinCommand : Command {
        private Func<string, string[], int> body;

        public BuiltinCommand(int argc, AccessLevel accessLevel, Func<string, string[], int> body)
            : base(argc, accessLevel) {
            this.body = body;
        }

        protected override void Perform(string username, string[] args) {
            body(username, args);
        }
    }
}
