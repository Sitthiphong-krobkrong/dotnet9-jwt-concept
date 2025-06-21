namespace dotnet9_jwt_concept.Models.Core
{
    public class AppSettings
    {
        public JwtSettings Jwt { get; set; } = new();
    }

    public class JwtSettings
    {
        public string SecretKey { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public int AccessTokenExpiration { get; set; }
        public int RefreshTokenExpiration { get; set; }
    }
}
