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
    public class GetContextsTests : BaseIntegrationTest
    {
        private const string ProjectName = "QueryTestProject";

        public GetContextsTests()
        {
            
        }
        
        [Fact]
        public async Task GetByQuery()
        {
            await this.WebApplicationFactory.CallOnce(setUpPrecondition, this);

            
        }
        
        private static readonly Func<GetContextsTests, Task> setUpPrecondition = async self =>
        {
            
        };
    }
}