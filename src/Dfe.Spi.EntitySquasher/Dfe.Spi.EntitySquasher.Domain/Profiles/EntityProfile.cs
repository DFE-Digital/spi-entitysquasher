namespace Dfe.Spi.EntitySquasher.Domain.Profiles
{
    public class EntityProfile
    {
        public string Name { get; set; }
        public string[] Sources { get; set; }
        public EntityFieldProfile[] Fields { get; set; }
    }
}