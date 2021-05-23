using System;
using Microsoft.AspNetCore.Authorization;

namespace DidacticalEnigma.Mem.Authentication
{
    public class JwtPermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public JwtPermissionRequirement(string permission)
        {
            Permission = permission ?? throw new ArgumentNullException(nameof(permission));
        }
    }
}