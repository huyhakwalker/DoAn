using Microsoft.Extensions.Caching.Memory;

namespace ProCoder.Services;

public interface IPasswordResetService
{
    string GenerateResetToken(int coderId);
    bool ValidateResetToken(int coderId, string token);
    void RemoveResetToken(int coderId);
}

public class PasswordResetService : IPasswordResetService
{
    private readonly IMemoryCache _cache;
    private const int TOKEN_EXPIRY_HOURS = 24;

    public PasswordResetService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public string GenerateResetToken(int coderId)
    {
        var token = Guid.NewGuid().ToString();
        var cacheKey = $"reset_token_{coderId}";
        
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromHours(TOKEN_EXPIRY_HOURS));

        _cache.Set(cacheKey, token, cacheEntryOptions);
        
        return token;
    }

    public bool ValidateResetToken(int coderId, string token)
    {
        var cacheKey = $"reset_token_{coderId}";
        if (_cache.TryGetValue(cacheKey, out string? storedToken))
        {
            return token == storedToken;
        }
        return false;
    }

    public void RemoveResetToken(int coderId)
    {
        var cacheKey = $"reset_token_{coderId}";
        _cache.Remove(cacheKey);
    }
} 