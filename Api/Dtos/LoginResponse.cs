namespace Api.Dtos
{
    public sealed record LoginResponse(string AccessToken, DateTime AccessTokenExpiresAt, string RefreshToken);
}
