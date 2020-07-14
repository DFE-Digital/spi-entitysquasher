using System;

namespace Dfe.Spi.EntitySquasher.Application.Squash
{
    public class ProfileMisconfiguredException : Exception
    {
        public string ProfileName { get; }
        public Type EntityType { get; }

        public ProfileMisconfiguredException(string profileName, Type entityType)
            : base($"Profile {profileName} does not contain definition for entity {entityType.Name}")
        {
            ProfileName = profileName;
            EntityType = entityType;
        }
    }
}