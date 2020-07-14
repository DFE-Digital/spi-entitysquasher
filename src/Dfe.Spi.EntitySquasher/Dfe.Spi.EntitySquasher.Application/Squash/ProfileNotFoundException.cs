namespace Dfe.Spi.EntitySquasher.Application.Squash
{
    public class ProfileNotFoundException : InvalidRequestException
    {
        public string ProfileName { get; }

        public ProfileNotFoundException(string profileName)
            : base($"Cannot find profile with name {profileName}")
        {
            ProfileName = profileName;
        }
    }
}