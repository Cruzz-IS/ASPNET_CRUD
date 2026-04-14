namespace WebApiRRHH.Configuration
{
    public class JwtSettings
    {
        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int AccessTokenExpirationMinutes { get; set; }
        public int RefreshTokenExpirationDays { get; set; }
    }

    public class SecuritySettings
    {
        public bool RequireEmailConfirmation { get; set; } = false;
        public int MaxLoginAttempts { get; set; } = 5;
        public int LockoutMinutes { get; set; } = 15;
        public JwtSettings Jwt { get; set; } = new();
    }
}
