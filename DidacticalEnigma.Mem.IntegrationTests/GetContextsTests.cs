using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using DidacticalEnigma.Mem.Translation.IoModels;
using Xunit;

namespace DidacticalEnigma.Mem.IntegrationTests
{
    public class GetContextsTests : IClassFixture<MemApplicationFactory>
    {
        private const string ProjectName = "QueryTestProject";
        
        private readonly MemApplicationFactory webApplicationFactory;

        public GetContextsTests(
            MemApplicationFactory webApplicationFactory)
        {
            this.webApplicationFactory = webApplicationFactory;
            this.webApplicationFactory.PrepareDatabase();
        }
        
        [Fact]
        public async Task GetByQuery()
        {
            var client = this.webApplicationFactory.CreateClientWithAuth(
                "modify:contexts",
                "read:contexts",
                "modify:projects");

            await this.webApplicationFactory.CallOnce(setUpPrecondition, client);

            
        }
        
        private static readonly Func<HttpClient, Task> setUpPrecondition = SetUpPrecondition;

        private static async Task SetUpPrecondition(HttpClient client)
        {
            
        }
    }
}