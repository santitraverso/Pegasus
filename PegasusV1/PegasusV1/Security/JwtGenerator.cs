using Microsoft.IdentityModel.Tokens;
using PegasusV1.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PegasusV1.Security
{
    public class JwtGenerator
    {
        private readonly IConfiguration _configuration;

        public JwtGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(Usuario usuario)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, usuario.Mail),
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{usuario.Nombre} {usuario.Apellido}"),
            };

            if (usuario.Perfil != null && usuario.Id_Perfil.HasValue)
            {
                claims.Add(new Claim(ClaimTypes.Role, usuario.Perfil.Nombre));
                claims.Add(new Claim("PerfilId", usuario.Id_Perfil.ToString()));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key no configurada")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}