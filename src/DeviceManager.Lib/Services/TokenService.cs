using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DeviceManager.Lib.Helpers.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DeviceManager.Lib.Services;

public class TokenService : ITokenService
{
    private readonly JwtOptions _jwtOptions;
    private const int DefaultTokenValidityMinutes = 60;

    public TokenService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
        ValidateJwtOptions();
    }

    private void ValidateJwtOptions()
    {
        if (string.IsNullOrEmpty(_jwtOptions.Key))
            throw new InvalidOperationException("JWT Key is not configured.");
        if (string.IsNullOrEmpty(_jwtOptions.Issuer))
            throw new InvalidOperationException("JWT Issuer is not configured.");
        if (string.IsNullOrEmpty(_jwtOptions.Audience))
            throw new InvalidOperationException("JWT Audience is not configured.");
        if (_jwtOptions.ValidityInMinutes <= 0)
            _jwtOptions.ValidityInMinutes = DefaultTokenValidityMinutes;
    }

    public string GenerateToken(string username, string role)
    {
        if (string.IsNullOrEmpty(username))
            throw new ArgumentNullException(nameof(username));
        if (string.IsNullOrEmpty(role))
            throw new ArgumentNullException(nameof(role));

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtOptions.Key);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, username),
            new(ClaimTypes.Role, role),
            new("Role", role)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            Expires = DateTime.UtcNow.Add(TimeSpan.FromMinutes(_jwtOptions.ValidityInMinutes)),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}