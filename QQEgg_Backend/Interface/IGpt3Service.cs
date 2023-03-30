namespace QQEgg_Backend.Interface
{
    public interface IGpt3Service
    {
        Task<string> GetResponseAsync(string input);
    }
}
