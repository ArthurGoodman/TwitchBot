using System;

namespace TwitchBot {
    public abstract class Command {
        public enum AccessLevel {
            Regular, Mod, Owner
        };

        private int argc;
        protected AccessLevel accessLevel;

        public Command(int argc, AccessLevel accessLevel) {
            this.argc = argc;
            this.accessLevel = accessLevel;
        }

        public void Execute(string username, string message) {
            if (accessLevel == Command.AccessLevel.Owner && username != Bot.Instance.Channel)
                return;

            if (accessLevel == Command.AccessLevel.Mod && !Bot.Instance.Mods.Contains(username))
                return;

            Perform(username, message.Split(new[] { ' ' }, argc + 1, StringSplitOptions.RemoveEmptyEntries));
        }

        protected abstract void Perform(string username, string[] args);
    }
}
