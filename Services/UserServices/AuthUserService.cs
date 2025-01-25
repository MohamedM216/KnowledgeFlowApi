using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using KnowledgeFlowApi.Data;
using KnowledgeFlowApi.Entities;
using KnowledgeFlowApi.Models;
using KnowledgeFlowApi.Options;
using KnowledgeFlowApi.Requests;
using KnowledgeFlowApi.Requests.UserRequests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace KnowledgeFlowApi.Services.UserServices
{
    public class AuthUserService(ApplicationDbContext dbContext, IOptions<JwtOptions> options)
    {   
        public const int MAX_REFRESH_TOKEN_LIFETIME_IN_DAYS = 7;
        private void MapSignUpRequest(SignUpRequest request, User user)
        {
            user.Email = request.Email;
            user.Username = request.Username;
            user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
            user.MembershipDate = DateTime.UtcNow;
            user.Bio = request.Bio;
            user.ContactEmail = request.ContactEmail;
        }

        public async Task<ResponseAuthModel> SignUpAsync(SignUpRequest request) {
            if (request == null)
                return new ResponseAuthModel { Message = "null sign up request"};

            if (string.IsNullOrEmpty(request.Email)
            || string.IsNullOrEmpty(request.Username)
            || string.IsNullOrEmpty(request.Password))
                return new ResponseAuthModel { Message = "email, username or password is null or empty"};

            var existingUser = dbContext.Users.SingleOrDefault(u => u.Email == request.Email || u.Username == request.Username);
            if (existingUser != null)
                return new ResponseAuthModel { Message = "this email or password is already used" };

            var user = new User();
            using (var transaction = await dbContext.Database.BeginTransactionAsync()) {
                try
                {
                    MapSignUpRequest(request, user);
                    dbContext.Add(user);
                    await dbContext.SaveChangesAsync(); 
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new ResponseAuthModel { Message = $"problem to sign up a user, ERROR: {ex.Message}, SOURCE: {ex.Source} " };
                }
            }

            // send confirmation email

            return new ResponseAuthModel 
            {
                IsAuthenticated = true,
                Message = "added successfully",
                Meta = $"user id: {user.Id}"
            };
        }

        // login (must be after sign up)
        public async Task<JwtAuthModel> GetTokenAsync(AuthRequest request) {
            if (request == null) 
                return new JwtAuthModel { Message = "null or empty credentials" };

            var user = dbContext.Users.SingleOrDefault(u => u.Email == request.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                return new JwtAuthModel { Message = "Invalid Credentials" };
            
            if (string.IsNullOrEmpty(options.Value.SigningKey))
                return new JwtAuthModel { Message = "problem to login a user" };

            try
            {
                var accessToken = await CreateJwtTokenAsync(user);
                var authModel = new JwtAuthModel();


                if (user.UserRefreshTokens != null && user.UserRefreshTokens.Any(x => x.IsActive)) {
                    var activeRefreshToken = user.UserRefreshTokens.FirstOrDefault(x => x.IsActive);
                    authModel.RefreshToken = activeRefreshToken.Token;
                    authModel.RefreshTokenExpiration = activeRefreshToken.ExpiresOn;
                }
                else {
                    var refreshToken = GenerateRefreshToken();
                    refreshToken.UserId = user.Id;
                    authModel.RefreshToken = refreshToken.Token;
                    authModel.RefreshTokenExpiration = refreshToken.ExpiresOn;
                    dbContext.Add(refreshToken);
                    await dbContext.SaveChangesAsync();
                }

                authModel.Message = "jwt token generated successfully";
                authModel.IsAuthenticated = true;
                authModel.AccessToken = accessToken;
                
                return authModel; 
            }
            catch (Exception ex)
            {
                return new JwtAuthModel 
                { 
                    Message = $"Exception: {ex.Message}, InnerException: {ex.InnerException?.Message}, Source: {ex.Source}" 
                };
            }
        }

        public async Task<string> CreateJwtTokenAsync(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = options.Value.Issuer,
                Audience = options.Value.Audience,
                Expires = DateTime.Now.AddMinutes(options.Value.LifeTimeInMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.SigningKey)), SecurityAlgorithms.HmacSha256),
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Email),
                    new Claim(ClaimTypes.Role, "user"),
                    new Claim("username", user.Username),
                    new Claim("email", user.Email),
                    new Claim("userId", user.Id.ToString())
                })
            };

            // Create the token
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            // Serialize the token
            var accessToken = tokenHandler.WriteToken(securityToken);

            return accessToken;
        }

        public async Task<JwtAuthModel> RefreshTokenAsync(string token) {
            var user = await dbContext.Users
            .Include(a => a.UserRefreshTokens)
            .SingleOrDefaultAsync(a => a.UserRefreshTokens.Any(t => t.Token == token));

            if (user == null) {
                return new JwtAuthModel { Message = "Invalid token" }; // Better error message
            }

            var refreshToken = user.UserRefreshTokens.SingleOrDefault(t => t.Token == token);
            if (refreshToken == null) {
                return new JwtAuthModel { Message = "Refresh token is not found" }; // Handle missing token
            }
            // Mark the old token as revoked
            refreshToken.RevokedOn = DateTime.UtcNow;
            dbContext.Update(refreshToken);
            // dbContext.Entry(refreshToken).State = EntityState.Modified; // Explicitly mark as modified

            // Generate a new refresh token
            var newRefreshToken = GenerateRefreshToken();
            newRefreshToken.UserId = user.Id;

            // Add the new refresh token
            dbContext.UserRefreshTokens.Add(newRefreshToken);
            await dbContext.SaveChangesAsync(); // Save changes to revoke the old token and add the new one

            // Generate the JWT token
            var jwtToken = await CreateJwtTokenAsync(user);

            var authModel = new JwtAuthModel {
                IsAuthenticated = true,
                Message = "Refresh token issued successfully",
                AccessToken = jwtToken,
                RefreshToken = newRefreshToken.Token,
                RefreshTokenExpiration = newRefreshToken.ExpiresOn
            };
            return authModel;
        }

        public async Task<bool> RevokeRefreshTokenAsync(string token) {
            var user = await dbContext.Users
            .Include(a => a.UserRefreshTokens)
            .SingleOrDefaultAsync(a => a.UserRefreshTokens.Any(t => t.Token == token));

            if (user == null) {
                return false;
            }

            var refreshToken = user.UserRefreshTokens.SingleOrDefault(t => t.Token == token);
            if (refreshToken == null) {
                return false;
            }
            // Mark the old token as revoked
            refreshToken.RevokedOn = DateTime.UtcNow;
            dbContext.Update(refreshToken);
            // dbContext.Entry(refreshToken).State = EntityState.Modified; // Explicitly mark as modified

            await dbContext.SaveChangesAsync(); // Save changes

            return true;
        }

        public async Task<SendEmailResponse> GetUsernameAndEmail(int userId) {
            var user = await dbContext.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return new SendEmailResponse { Message = "This user is not found" };

            return new SendEmailResponse {
                IsRetreved = true,
                ToEmail = user.Email,
                ToUsername = user.Username,
                Message = "user info retreved successfully"
            };
        }

        private UserRefreshToken GenerateRefreshToken() {
            var randomNumber = new byte[32];
            using var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(randomNumber);

            return new UserRefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                ExpiresOn = DateTime.UtcNow.AddDays(MAX_REFRESH_TOKEN_LIFETIME_IN_DAYS),
                CreatedOn = DateTime.UtcNow,
            };
        }


    }
}