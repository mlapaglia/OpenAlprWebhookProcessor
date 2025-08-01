using Microsoft.IdentityModel.Tokens;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users.Data.Repositories;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Services
{
    public class JwtService : IJwtService
    {
        private readonly IUsersUnitOfWork _usersUnitOfWork;

        public JwtService(IUsersUnitOfWork usersUnitOfWork)
        {
            _usersUnitOfWork = usersUnitOfWork ?? throw new ArgumentNullException(nameof(usersUnitOfWork));
        }

        public async Task<string> GenerateJwtTokenAsync(User user, bool rememberMe = false, CancellationToken cancellationToken = default)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var jwtSecretKey = await GetJwtSecretKeyAsync(cancellationToken);

            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = rememberMe ? null : DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(jwtSecretKey),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<byte[]> GetJwtSecretKeyAsync(CancellationToken cancellationToken = default)
        {
            var jwtKey = await _usersUnitOfWork.JwtKeys.GetFirstAsync(cancellationToken);

            if (jwtKey == null)
            {
                jwtKey = new JwtKey
                {
                    Key = GenerateJwtSecretKey(128)
                };

                await _usersUnitOfWork.JwtKeys.AddAsync(jwtKey, cancellationToken);
                await _usersUnitOfWork.SaveChangesAsync(cancellationToken);
            }
            else if (jwtKey.Key.Length < 128)
            {
                jwtKey.Key = GenerateJwtSecretKey(128);
                await _usersUnitOfWork.SaveChangesAsync(cancellationToken);
            }

            return Convert.FromBase64String(jwtKey.Key);
        }

        public RefreshToken GenerateRefreshToken(string ipAddress)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var randomBytes = new byte[128];
                rng.GetBytes(randomBytes);
                return new RefreshToken
                {
                    Token = Convert.ToBase64String(randomBytes),
                    Expires = DateTime.UtcNow.AddDays(7),
                    Created = DateTime.UtcNow,
                    CreatedByIp = ipAddress
                };
            }
        }

        private static string GenerateJwtSecretKey(int keyLength)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] randomBytes = new byte[keyLength];
                rng.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }
    }
} 