using System;

namespace TwitchBot {
    class Command {
        public int Argc { get; private set; }
        public bool Mod { get; private set; }
        public Func<string, string[], int> Body { get; private set; }

        public Command(int argc, bool mod, Func<string, string[], int> body) {
            Argc = argc;
            Mod = mod;
            Body = body;
        }
    }
}
