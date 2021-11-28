using System;
using System.Linq;
using System.Threading.Tasks;
using DidacticalEnigma.Mem.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace DidacticalEnigma.Mem.Authentication
{
    public class MemAuthorizationHandler : AuthorizationHandler<CompositeOrRequirement>
    {
        private readonly AuthConfiguration authConfiguration;

        public MemAuthorizationHandler(
            IOptions<AuthConfiguration> authConfiguration)
        {
            this.authConfiguration = authConfiguration.Value;
        }

        private Task HandleRequirementAsync(AuthorizationHandlerContext context,
            AuthConfigurationPolicyRequirement requirement,
            CompositeOrRequirement compositeOrRequirement)
        {
            if (requirement.SatisfiesCheck(authConfiguration))
            {
                context.Succeed(compositeOrRequirement);
            }

            return Task.CompletedTask;
        }

        private Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            JwtPermissionRequirement requirement,
            CompositeOrRequirement compositeOrRequirement)
        {
            var issuer = authConfiguration.Authority;

            // Split the scopes string into an array
            var scopes = context.User.FindAll(c => c.Type == "permissions" && c.Issuer == issuer);

            // Succeed if the scope array contains the required scope
            if (scopes.Any(s => s.Value == requirement.Permission))
                context.Succeed(compositeOrRequirement);

            return Task.CompletedTask;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            CompositeOrRequirement requirement)
        {
            foreach (var singleRequirement in requirement.Requirements)
            {
                switch (singleRequirement)
                {
                    case JwtPermissionRequirement jwtPermissionRequirement:
                        await HandleRequirementAsync(context, jwtPermissionRequirement, requirement);
                        break;
                    case AuthConfigurationPolicyRequirement authConfigurationPolicyRequirement:
                        await HandleRequirementAsync(context, authConfigurationPolicyRequirement, requirement);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}