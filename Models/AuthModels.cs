namespace dotnet9_jwt_concept.Models
{
    public class AuthModels
    {
        public class AuthRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public class AuthResult
        {
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
        }
    }
}
