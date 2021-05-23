using System;
using DidacticalEnigma.Mem.Configurations;
using Microsoft.AspNetCore.Authorization;

namespace DidacticalEnigma.Mem.Authentication
{
    public class AuthConfigurationPolicyRequirement : IAuthorizationRequirement
    {
        private readonly Predicate<AuthConfiguration> configCheck;

        public AuthConfigurationPolicyRequirement(
            Predicate<AuthConfiguration> configCheck)
        {
            this.configCheck = configCheck;
        }

        public bool SatisfiesCheck(AuthConfiguration configuration)
        {
            return configCheck(configuration);
        }
    }
}