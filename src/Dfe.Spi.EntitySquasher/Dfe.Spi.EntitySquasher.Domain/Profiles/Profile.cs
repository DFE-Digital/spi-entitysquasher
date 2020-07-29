namespace Dfe.Spi.EntitySquasher.Domain.Profiles
{
    public class Profile
    {
        public string Name { get; set; }
        public EntityProfile[] Entities { get; set; }

        public override string ToString()
        {
            return $"{nameof(Profile)} (Name={Name})";
        }
    }
}