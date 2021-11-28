using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using DidacticalEnigma.Mem.Configurations;
using DidacticalEnigma.Mem.Translation.IoModels;
using DidacticalEnigma.Mem.Translation.StoredModels;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace DidacticalEnigma.Mem.IntegrationTests
{
    public class QueryTranslationsTests : IClassFixture<MemApplicationFactory>
    {
        private readonly MemApplicationFactory webApplicationFactory;

        public QueryTranslationsTests(
            MemApplicationFactory webApplicationFactory)
        {
            this.webApplicationFactory = webApplicationFactory;
            this.webApplicationFactory.PrepareDatabase();
        }
        
        [Fact]
        public async Task Test1()
        {
            var client = this.webApplicationFactory.CreateClientWithAuth(
                "read:translations",
                "modify:projects",
                "modify:translations");

            var projectName = "QueryTestProject";
            var query = "言う";

            var addProjectResponse = await client.PostAsync($"mem/projects?projectName={HttpUtility.UrlEncode(projectName)}", null);

            addProjectResponse.EnsureSuccessStatusCode();

            var addTranslationsResponse = await client.PostAsJsonAsync($"mem/translations?projectName={HttpUtility.UrlEncode(projectName)}", new AddTranslationsParams()
            {
                AllowPartialAdd = false,
                Translations = new AddTranslationParams[]
                {
                    new AddTranslationParams()
                    {
                        Source = "違うなら言って\n欲しいんだよ…",
                        Target = "Please tell me I'm wrong...",
                        CorrelationId = "My Test Manga Volume 1, Chapter 1, Caption 42",
                        TranslationNotes = new AddTranslationNotesParams()
                        {
                            Normal = Array.Empty<IoNormalNote>(),
                            Gloss = new IoGlossNote[]
                            {
                                new IoGlossNote()
                                {
                                    Foreign = "違う",
                                    Explanation = "to not match the correct (answer, etc.)"
                                },
                                new IoGlossNote()
                                {
                                    Foreign = "なら",
                                    Explanation = "if/in case/if it is the case that/if it is true that"
                                },
                                new IoGlossNote()
                                {
                                    Foreign = "言っ て",
                                    Explanation = "to say/to utter/to declare,\n-te form"
                                },
                                new IoGlossNote()
                                {
                                    Foreign = "欲しい",
                                    Explanation = "(after the -te form of a verb) I want (you) to"
                                },
                                new IoGlossNote()
                                {
                                    Foreign = "ん だ",
                                    Explanation =
                                        "(の and ん add emphasis) the expectation is that .../the reason is that .../the fact is that .../the explanation is that .../it is that ..."
                                },
                                new IoGlossNote()
                                {
                                    Foreign = "よ",
                                    Explanation = ""
                                }
                            }
                        }
                    }
                }
            });
            
            addTranslationsResponse.EnsureSuccessStatusCode();
            
            var response = await client.GetAsync(
                $"mem/translations?projectName={HttpUtility.UrlEncode(projectName)}&query={HttpUtility.UrlEncode(query)}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var rawResponseText = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<QueryTranslationsResult>(
                rawResponseText,
                new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                });
            
            Assert.Equal("違うなら|言っ|て欲しいんだよ…", result.Translations.Single().Source);
            Assert.Equal("Please tell me I'm wrong...", result.Translations.Single().Target);
        }
        
        public static void Initialize(MemContext db)
        {
            
        }
    }
}