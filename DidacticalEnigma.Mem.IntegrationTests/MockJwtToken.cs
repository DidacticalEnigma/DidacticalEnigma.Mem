using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using DidacticalEnigma.Mem.Configurations;
using Microsoft.IdentityModel.Tokens;

namespace DidacticalEnigma.Mem.IntegrationTests
{
    public static class MockJwtToken
    {
        public static SecurityKey SecurityKey { get; }
        public static SigningCredentials SigningCredentials { get; }

        private static readonly JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        private static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();
        private static readonly byte[] key = new byte[32];

        static MockJwtToken()
        {
            rng.GetBytes(key);
            SecurityKey = new SymmetricSecurityKey(key) { KeyId = Guid.NewGuid().ToString() };
            SigningCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);
        }

        public static string GenerateJwtToken(IEnumerable<Claim> claims, AuthConfiguration authConfiguration)
        {
            return tokenHandler.WriteToken(
                new JwtSecurityToken(
                    authConfiguration.Authority,
                    authConfiguration.Audience,
                    claims,
                    null,
                    DateTime.UtcNow.AddMinutes(20),
                    SigningCredentials));
        }
    }
}