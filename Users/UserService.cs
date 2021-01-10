using Microsoft.IdentityModel.Tokens;
using OpenAlprWebhookProcessor.Users.Data;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace OpenAlprWebhookProcessor.Users
{
    public interface IUserService
    {
        AuthenticateResponse Authenticate(
            AuthenticateRequest request,
            string ipAddress);

        IEnumerable<User> GetAll();

        User GetById(int id);

        User Create(User user, string password);

        void Update(User requestedUser, string password = null);

        void Delete(int id);

        AuthenticateResponse RefreshToken(
            string token,
            string ipAddress);

        bool RevokeToken(
            string token,
            string ipAddress);
    }

    public class UserService : IUserService
    {
        private readonly UsersContext _usersContext;

        private readonly JwtConfiguration _jwtConfiguration;

        public UserService(
            UsersContext context,
            JwtConfiguration jwtConfiguration)
        {
            _usersContext = context;
            _jwtConfiguration = jwtConfiguration;
        }

        public User Create(User user, string password)
        {
            // validation
            if (string.IsNullOrWhiteSpace(password))
                throw new AppException("Password is required");

            if (_usersContext.Users.Any(x => x.Username == user.Username))
                throw new AppException("Username \"" + user.Username + "\" is already taken");

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _usersContext.Users.Add(user);
            _usersContext.SaveChanges();

            return user;
        }

        public void Update(
            User requestedUser,
            string password = null)
        {
            var user = _usersContext.Users.Find(requestedUser.Id);

            if (user == null)
                throw new AppException("User not found");

            if (!string.IsNullOrWhiteSpace(user.Username) && user.Username != user.Username)
            {
                if (_usersContext.Users.Any(x => x.Username == user.Username))
                    throw new AppException("Username " + user.Username + " is already taken");

                user.Username = user.Username;
            }

            if (!string.IsNullOrWhiteSpace(user.FirstName))
            {
                user.FirstName = user.FirstName;
            }

            if (!string.IsNullOrWhiteSpace(user.LastName))
            {
                user.LastName = user.LastName;
            }

            // update password if provided
            if (!string.IsNullOrWhiteSpace(password))
            {
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash(password, out passwordHash, out passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            _usersContext.Users.Update(user);
            _usersContext.SaveChanges();
        }

        public void Delete(int id)
        {
            var user = _usersContext.Users.Find(id);
            if (user != null)
            {
                _usersContext.Users.Remove(user);
                _usersContext.SaveChanges();
            }
        }

        public AuthenticateResponse Authenticate(
            AuthenticateRequest model,
            string ipAddress)
        {
            var user = _usersContext.Users
                .SingleOrDefault(x => x.Username == model.Username);

            if (user == null)
            {
                return null;
            }

            if (!VerifyPasswordHash(
                model.Password,
                user.PasswordHash,
                user.PasswordSalt))
            {
                return null; 
            }

            var jwtToken = generateJwtToken(user);
            var refreshToken = generateRefreshToken(ipAddress);

            user.RefreshTokens.Add(refreshToken);
            _usersContext.Update(user);
            _usersContext.SaveChanges();

            return new AuthenticateResponse(
                user,
                jwtToken,
                refreshToken.Token);
        }

        public AuthenticateResponse RefreshToken(string token, string ipAddress)
        {
            var user = _usersContext.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

            // return null if no user found with token
            if (user == null) return null;

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            // return null if token is no longer active
            if (!refreshToken.IsActive) return null;

            // replace old refresh token with a new one and save
            var newRefreshToken = generateRefreshToken(ipAddress);
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            user.RefreshTokens.Add(newRefreshToken);
            _usersContext.Update(user);
            _usersContext.SaveChanges();

            // generate new jwt
            var jwtToken = generateJwtToken(user);

            return new AuthenticateResponse(user, jwtToken, newRefreshToken.Token);
        }

        public bool RevokeToken(string token, string ipAddress)
        {
            var user = _usersContext.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

            // return false if no user found with token
            if (user == null) return false;

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            // return false if token is not active
            if (!refreshToken.IsActive) return false;

            // revoke token and save
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            _usersContext.Update(user);
            _usersContext.SaveChanges();

            return true;
        }

        public IEnumerable<User> GetAll()
        {
            return _usersContext.Users;
        }

        public User GetById(int id)
        {
            return _usersContext.Users.Find(id);
        }

        // helper methods

        private string generateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtConfiguration.SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private RefreshToken generateRefreshToken(string ipAddress)
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
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

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
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }
    }
}
