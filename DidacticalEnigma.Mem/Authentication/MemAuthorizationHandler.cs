using System;
using System.Linq;
using System.Threading.Tasks;
using DidacticalEnigma.Mem.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace DidacticalEnigma.Mem.Authentication
{
    public class MemAuthorizationHandler : AuthorizationHandler<IAuthorizationRequirement>
    {
        private readonly AuthConfiguration authConfiguration;

        public MemAuthorizationHandler(
            IOptions<AuthConfiguration> authConfiguration)
        {
            this.authConfiguration = authConfiguration.Value;
        }
        
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            switch (requirement)
            {
                case HasPermissionRequirement scopeRequirement:
                    return HandleRequirementImplAsync(context, scopeRequirement);
                case ConfigurationPolicyRequirement configurationPolicyRequirement:
                    return HandleRequirementImplAsync(context, configurationPolicyRequirement);
            }
            
            throw new NotImplementedException();
        }
        
        private Task HandleRequirementImplAsync(AuthorizationHandlerContext context, HasPermissionRequirement requirement)
        {
            var issuer = authConfiguration.Authority;

            // Split the scopes string into an array
            var scopes = context.User.FindAll(c => c.Type == "permissions" && c.Issuer == issuer);

            // Succeed if the scope array contains the required scope
            if (scopes.Any(s => s.Value == requirement.Permission))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
        
        private Task HandleRequirementImplAsync(AuthorizationHandlerContext context, ConfigurationPolicyRequirement requirement)
        {
            if (requirement.SatisfiesCheck(authConfiguration))
            {
                context.Succeed(requirement);
            }
            
            return Task.CompletedTask;
        }
    }
}