namespace SignalApi
{
    public interface ITokenRepository
    {
        void StoreRefreshToken(string userId, string deviceId, string refreshToken);
        bool HasRefreshToken(string refreshToken);
    }
}