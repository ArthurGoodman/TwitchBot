using System;

namespace TwitchBot {
    public abstract class Command {
        public enum AccessLevel {
            Regular, Mod, Owner
        };

        private int argc;

        public AccessLevel Level { get; private set; }

        public Command(int argc, AccessLevel accessLevel) {
            this.argc = argc;
            Level = accessLevel;
        }

        public void Execute(string username, string message) {
            if (Level == Command.AccessLevel.Owner && username != Bot.Instance.Channel)
                return;

            if (Level == Command.AccessLevel.Mod && !Bot.Instance.Mods.Contains(username))
                return;

            Perform(username, message.Split(new[] { ' ' }, argc + 1, StringSplitOptions.RemoveEmptyEntries));
        }

        protected abstract void Perform(string username, string[] args);
    }
}
