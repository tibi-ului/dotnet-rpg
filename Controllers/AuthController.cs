
namespace dotnet_rpg.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        
        private readonly IAuthRepository _authRepo;
        public AuthController(IAuthRepository authRepo)   // injectam IAuthRepository
        {
            _authRepo = authRepo;
            
        }
        
        [HttpPost("Register")]
        public async Task<ActionResult<ServiceResponse<int>>> Register(UserRegisterDto request)
        {
            var response = await _authRepo.Register(
                new User { Username = request.Username }, request.Password  
            );                                                                  // apelam metoda si salvam raspunsul
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<ServiceResponse<int>>> Login(UserLoginDto request)
        {
            var response = await _authRepo.Login(request.Username, request.Password);                                                                
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}