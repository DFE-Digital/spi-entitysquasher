namespace Dfe.Spi.EntitySquasher.Domain.Profiles
{
    public class EntityFieldProfile
    {
        public string Name { get; set; }
        public string[] Sources { get; set; }
        public bool TreatWhitespaceAsNull { get; set; }
    }
}