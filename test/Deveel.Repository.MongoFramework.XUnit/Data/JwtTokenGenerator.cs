using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.IdentityModel.Tokens;

namespace Deveel.Data {
	public class JwtTokenGenerator {
		private readonly string _key;
		private readonly string _issuer;

		public JwtTokenGenerator(string key, string issuer) {
			_key = key;
			_issuer = issuer;
		}

		public string GenerateToken(string tenant, string username, string role) {
			var tokenHandler = new JwtSecurityTokenHandler();
			var key = Encoding.UTF8.GetBytes(_key);
			var tokenDescriptor = new SecurityTokenDescriptor {
				Subject = new ClaimsIdentity(new[]
				{
				new Claim(ClaimTypes.Name, username),
				new Claim(ClaimTypes.Role, role),
				new Claim("tenant", tenant)
			}),
				Expires = DateTime.UtcNow.AddDays(7),
				Issuer = _issuer,
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
			};
			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}
	}
}