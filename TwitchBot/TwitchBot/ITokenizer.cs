namespace TwitchBot {
    public interface ITokenizer<T> {
        bool HasToken();
        T GetToken();
    }
}
