using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_rpg.Services.WeaponsService
{
    public class WeaponService : IWeaponService
    {
        public readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        
        public WeaponService(DataContext context, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            
        }
        public async Task<ServiceResponse<GetCharacterDto>> AddWeapon(AddWeaponDto newWeapon)
        {
            var response = new ServiceResponse<GetCharacterDto>();
            
            try
            {
                // luam caracterul corect din baza de date
                // accesam caracterele din context, gasim prima entitate cu un anumit characterId
                // gasim userul corect ca sa stim ca acest caracter apartine de userul autorizat/autentificat
                // luam id-ul user-ului curent, accesand valuarea NameIndentifier ClaimsTypes din JSON web token
                var character = await _context.Characters
                    .FirstOrDefaultAsync(c => c.Id == newWeapon.CharacterId && 
                        c.User!.Id == int.Parse(_httpContextAccessor.HttpContext!.User
                            .FindFirstValue(ClaimTypes.NameIdentifier)!));
                if (character is null)
                {
                    response.Success = false;
                    response.Message = "Character not found";
                    return response;
                }
                // creeam o noua instanta weapon
                var weapon = new Weapon
                {
                    Name = newWeapon.Name,
                    Damage = newWeapon.Damage,
                    Character = character
                };

                // adaugam weapon la baza de date
                // salvam schimbarile
                // returnam caracterul
                _context.Weapons.Add(weapon);
                await _context.SaveChangesAsync();

                response.Data = _mapper.Map<GetCharacterDto>(character);
            }
            catch (Exception ex)
            {
                
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}