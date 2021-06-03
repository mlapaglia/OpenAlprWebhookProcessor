using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenAlprWebhookProcessor.Users.Data;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Users
{
    public interface IUserService
    {
        Task<AuthenticateResponse> AuthenticateAsync(
            AuthenticateRequest request,
            string ipAddress,
            CancellationToken cancellationToken);

        Task<List<User>> GetAllAsync(CancellationToken cancellationToken);

        Task<User> GetByIdAsync(
            int id,
            CancellationToken cancellationToken);

        Task<User> CreateAsync(
            User user,
            string password);

        Task UpdateAsync(
            User requestedUser,
            string password = null);

        Task DeleteAsync(int id);

        Task<byte[]> GetJwtSecretKeyAsync();

        Task<AuthenticateResponse> RefreshTokenAsync(
            string token,
            string ipAddress,
            CancellationToken cancellationToken);

        Task<bool> RevokeTokenAsync(
            string token,
            string ipAddress,
            CancellationToken cancellationToken);
    }

    public class UserService : IUserService
    {
        private readonly UsersContext _usersContext;

        public UserService(UsersContext context)
        {
            _usersContext = context;
        }

        public async Task<User> CreateAsync(User user, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new AppException("Password is required");
            }

            if (_usersContext.Users.Any(x => x.Username == user.Username))
            {
                throw new AppException("Username \"" + user.Username + "\" is already taken");
            }

            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _usersContext.Users.Add(user);
            await _usersContext.SaveChangesAsync();

            return user;
        }

        public async Task UpdateAsync(
            User requestedUser,
            string password = null)
        {
            var user = await _usersContext.Users.FindAsync(requestedUser.Id);

            if (user == null)
            {
                throw new AppException("User not found");
            }

            if (!string.IsNullOrWhiteSpace(user.Username) && user.Username != requestedUser.Username)
            {
                if (_usersContext.Users.Any(x => x.Username == user.Username))
                    throw new AppException("Username " + user.Username + " is already taken");

                user.Username = requestedUser.Username;
            }

            if (!string.IsNullOrWhiteSpace(user.FirstName))
            {
                user.FirstName = requestedUser.FirstName;
            }

            if (!string.IsNullOrWhiteSpace(user.LastName))
            {
                user.LastName = requestedUser.LastName;
            }

            if (!string.IsNullOrWhiteSpace(password))
            {
                CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            _usersContext.Users.Update(user);
            await _usersContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _usersContext.Users.FindAsync(id);

            if (user != null)
            {
                _usersContext.Users.Remove(user);
                await _usersContext.SaveChangesAsync();
            }
        }

        public async Task<AuthenticateResponse> AuthenticateAsync(
            AuthenticateRequest request,
            string ipAddress,
            CancellationToken cancellationToken)
        {
            var user = await _usersContext.Users
                .SingleOrDefaultAsync(x =>
                    x.Username == request.Username,
                    cancellationToken);

            if (user == null)
            {
                return null;
            }

            if (!VerifyPasswordHash(
                request.Password,
                user.PasswordHash,
                user.PasswordSalt))
            {
                return null; 
            }

            var jwtToken = await GenerateJwtTokenAsync(user);
            var refreshToken = GenerateRefreshToken(ipAddress);

            user.RefreshTokens.Add(refreshToken);

            _usersContext.Update(user);
            await _usersContext.SaveChangesAsync(cancellationToken);

            return new AuthenticateResponse(
                user,
                jwtToken,
                refreshToken.Token);
        }

        public async Task<AuthenticateResponse> RefreshTokenAsync(
            string token,
            string ipAddress,
            CancellationToken cancellationToken)
        {
            var user = await _usersContext.Users.SingleOrDefaultAsync(u => 
                u.RefreshTokens.Any(t => t.Token == token),
                cancellationToken);

            if (user == null)
            {
                throw new ArgumentException("unknown user");
            }

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            if (!refreshToken.IsActive)
            {
                throw new ArgumentException("user is already inactive");
            }

            var newRefreshToken = GenerateRefreshToken(ipAddress);

            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.ReplacedByToken = newRefreshToken.Token;

            user.RefreshTokens.Add(newRefreshToken);
            _usersContext.Update(user);

            await _usersContext.SaveChangesAsync(cancellationToken);

            var jwtToken = await GenerateJwtTokenAsync(user);

            return new AuthenticateResponse(
                user,
                jwtToken,
                newRefreshToken.Token);
        }

        public async Task<bool> RevokeTokenAsync(
            string token,
            string ipAddress,
            CancellationToken cancellationToken)
        {
            var user = await _usersContext.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token), cancellationToken);

            if (user == null) return false;

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            if (!refreshToken.IsActive) return false;

            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;

            _usersContext.Update(user);
            await _usersContext.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _usersContext.Users.ToListAsync(cancellationToken);
        }

        public async Task<User> GetByIdAsync(
            int id,
            CancellationToken cancellationToken)
        {
            return await _usersContext.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<byte[]> GetJwtSecretKeyAsync()
        {
            var jwtKey = await _usersContext.JwtKeys.FirstOrDefaultAsync();

            if (jwtKey == null)
            {
                jwtKey = new JwtKey
                {
                    Key = GenerateJwtSecretKey(30)
                };

                _usersContext.Add(jwtKey);
                await _usersContext.SaveChangesAsync();
            }

            return Convert.FromBase64String(jwtKey.Key);
        }

        private async Task<string> GenerateJwtTokenAsync(User user)
        {
            var jwtSecretKey = await GetJwtSecretKeyAsync();

            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(jwtSecretKey),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static RefreshToken GenerateRefreshToken(string ipAddress)
        {
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[64];
                rngCryptoServiceProvider.GetBytes(randomBytes);
                return new RefreshToken
                {
                    Token = Convert.ToBase64String(randomBytes),
                    Expires = DateTime.UtcNow.AddDays(7),
                    Created = DateTime.UtcNow,
                    CreatedByIp = ipAddress
                };
            }
        }

        private static void CreatePasswordHash(
            string password,
            out byte[] passwordHash,
            out byte[] passwordSalt)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Value cannot be empty or whitespace only string.", nameof(password));
            }

            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private static bool VerifyPasswordHash(
            string password,
            byte[] storedHash,
            byte[] storedSalt)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Value cannot be empty or whitespace only string.", nameof(password));
            }

            if (storedHash.Length != 64)
            {
                throw new ArgumentException("Invalid length of password hash (64 bytes expected).", nameof(storedHash));
            }

            if (storedSalt.Length != 128)
            {
                throw new ArgumentException("Invalid length of password salt (128 bytes expected).", nameof(storedSalt));
            }

            using (var hmac = new HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }

        private static string GenerateJwtSecretKey(int keyLength)
        {
            RNGCryptoServiceProvider rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            byte[] randomBytes = new byte[keyLength];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
