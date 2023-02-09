using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;


namespace dotnet_rpg.Data
{
        // pentru a inregistra un user nou, practic, trebuie sa creeam un user nou in baza de date 

    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        public AuthRepository(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            
        }
        public async Task<ServiceResponse<string>> Login(string username, string password)
        {
            var response = new ServiceResponse<string>();
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower().Equals(username.ToLower()));
                if (user is null)
                {
                    response.Success = false;
                    response.Message = "User not found.";
                }
                else if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                {
                    response.Success = false;
                    response.Message = "Wrong Password.";
                }
                else 
                {
                    response.Data = CreateToken(user);
                }
                return response;
        }

        public async Task<ServiceResponse<int>> Register(User user, string password)
        {
            var response = new ServiceResponse<int>();    // creeam service response
            if (await UserExists(user.Username))    
            {
                response.Success = false;
                response.Message = "User already exists.";
                return response;
            }

            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);   
            user.PasswordHash = passwordHash;   // setam hash-ul utilizatorului
            user.PasswordSalt = passwordSalt;         
            
            _context.Users.Add(user);   // adaugam user-ul in baza de date        
            await _context.SaveChangesAsync();   // salvam modificarile
            response.Data = user.Id;    // trimitem noul Id inapoi
            return response;
        }

        public async Task<bool> UserExists(string username)
        {
            if (await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower()))   // accesam context si verificam daca user-ul cu acelasi username exista deja
            {
                return true;
            }
            return false;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)  // folosim parametrii out pentru ai seta in metoda Register(). Acestia vor fi stocati in baza de date pentru User
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512())   // instanta genereaza o cheie care poate fi folosita ca o parola salt
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));   // genereaza o parola hash cu metoda ComputeHash() care salveaza parola ca bytes  
            }
        }   // nu vom returna nimic pentru ca folosim parametri out

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var appSettingsToken = _configuration.GetSection("AppSettings:Token").Value;
            if (appSettingsToken is null)
                throw new Exception("AppSettings Token is null!");

                SymmetricSecurityKey key = new SymmetricSecurityKey(System.Text.Encoding.UTF8
                    .GetBytes(appSettingsToken));

                SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature); 

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.Now.AddDays(1),
                    SigningCredentials = creds
                };

                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}