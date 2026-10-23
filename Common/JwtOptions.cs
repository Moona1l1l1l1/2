namespace UniversityApi.Common;

public class JwtOptions
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    /// <summary>Ключ подписи. Хранится ВНЕ репозитория: user-secrets / переменная окружения Jwt__Key.</summary>
    public string Key { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 30;
}
