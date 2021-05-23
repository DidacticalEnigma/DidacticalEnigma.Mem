using System;
using DidacticalEnigma.Mem.Configurations;
using Microsoft.AspNetCore.Authorization;

namespace DidacticalEnigma.Mem.Authentication
{
    public class ConfigurationPolicyRequirement : IAuthorizationRequirement
    {
        private readonly Predicate<AuthConfiguration> configCheck;

        public ConfigurationPolicyRequirement(
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