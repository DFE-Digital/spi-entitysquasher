namespace Dfe.Spi.EntitySquasher.Domain.Profiles
{
    public class Profile
    {
        public string Name { get; set; }
        public EntityProfile[] Entities { get; set; }
    }
}