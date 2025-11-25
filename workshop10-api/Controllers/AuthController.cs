using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CampusWorkshops.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    // Usuários de exemplo (NÃO use plaintext em produção)
    private static readonly Dictionary<string,(string Password, string[] Roles)> Users = new()
    {
        ["admin@campus"] = ("admin123", new[] { "Admin", "Instructor" }),
        ["inst@campus"]  = ("inst123",  new[] { "Instructor" }),
        ["aluno@campus"] = ("aluno123", Array.Empty<string>())
    };

    private readonly IConfiguration _cfg;
    public AuthController(IConfiguration cfg) => _cfg = cfg;

    public record LoginRequest(string Username, string Password);
    public record TokenResponse(string AccessToken, DateTime ExpiresAt);

    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponse), 200)]
    [ProducesResponseType(401)]
    public IActionResult Login([FromBody] LoginRequest body)
    {
        if (!Users.TryGetValue(body.Username, out var u) || u.Password != body.Password)
            return Unauthorized();

        var jwt = _cfg.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, body.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(u.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var expires = DateTime.UtcNow.AddHours(2);
        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );
        var encoded = new JwtSecurityTokenHandler().WriteToken(token);
        return Ok(new TokenResponse(encoded, expires));
    }
}
