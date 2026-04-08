
namespace SignalApi
{
    internal class InMemoryTokenRepository : ITokenRepository
    {
        private List<(string UserId, string DeviceId, string RefreshToken)> _refreshTokens = new();

        public bool HasRefreshToken(string refreshToken)
        {
            throw new NotImplementedException();
        }

        public void StoreRefreshToken(string userId, string deviceId, string refreshToken)
        {
            _refreshTokens.Add((userId, deviceId, refreshToken));
        }
    }
}