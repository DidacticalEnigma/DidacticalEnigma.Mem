using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace DidacticalEnigma.Mem.Authentication
{
    public class CompositeOrRequirement : IAuthorizationRequirement
    {
        public IReadOnlyCollection<IAuthorizationRequirement> Requirements { get; }

        public CompositeOrRequirement(
            params IAuthorizationRequirement[] requirements)
        {
            this.Requirements = requirements;
        }
    }
}