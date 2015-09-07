using System;

namespace TwitchBot {
    public class Program {
        static void Main(string[] args) {
            try {
                Bot bot = new Bot();
                bot.Run();
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                Console.ReadKey();
                return;
            }
        }
    }
}
