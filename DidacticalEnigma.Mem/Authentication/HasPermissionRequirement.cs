using System;
using Microsoft.AspNetCore.Authorization;

namespace DidacticalEnigma.Mem.Authentication
{
    public class HasPermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public HasPermissionRequirement(string permission)
        {
            Permission = permission ?? throw new ArgumentNullException(nameof(permission));
        }
    }
}