

namespace dotnet_rpg.Models
{
    public class Weapon
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Damage { get; set; }
        public Character? Character { get; set; }   // relatie one to one cu character
        public int CharacterId { get; set; }   // Foreign Key pentru Character
    }
}