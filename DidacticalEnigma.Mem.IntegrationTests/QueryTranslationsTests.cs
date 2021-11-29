using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using DidacticalEnigma.Mem.Configurations;
using DidacticalEnigma.Mem.Translation.IoModels;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace DidacticalEnigma.Mem.IntegrationTests
{
    public class QueryTranslationsTests : IClassFixture<MemApplicationFactory>
    {
        private const string ProjectName = "QueryTestProject";
        
        private readonly MemApplicationFactory webApplicationFactory;

        public QueryTranslationsTests(
            MemApplicationFactory webApplicationFactory)
        {
            this.webApplicationFactory = webApplicationFactory;
            this.webApplicationFactory.PrepareDatabase();
        }
        
        [Fact]
        public async Task GetByQuery()
        {
            var client = this.webApplicationFactory.CreateClientWithAuth(
                "read:translations",
                "modify:projects",
                "modify:translations",
                "modify:categories");

            var query = "言う";

            await this.webApplicationFactory.CallOnce(setUpPrecondition, client);

            var response = await client.GetAsync(
                $"mem/translations?projectName={HttpUtility.UrlEncode(ProjectName)}&query={HttpUtility.UrlEncode(query)}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var rawResponseText = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<QueryTranslationsResult>(
                rawResponseText,
                new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                });
            
            result = result ?? throw new InvalidOperationException();
            var tl = result.Translations.Single();
            Assert.Equal("違うなら|言っ|て欲しいんだよ…", tl.Source);
            Assert.Equal("Please tell me I'm wrong...", tl.Target);
            Assert.Equal(new Guid("7EE2147C-94CD-4CFF-AC14-5D68971A46B6"), tl.CategoryId);
            Assert.Equal("Broski", tl.Category);
            Assert.Equal(ProjectName, tl.ProjectName);
            Assert.Equal("違う", tl.TranslationNotes.Gloss.First().Foreign);
        }
        
        [Fact]
        public async Task GetByCorrelationId()
        {
            var client = this.webApplicationFactory.CreateClientWithAuth(
                "read:translations",
                "modify:projects",
                "modify:translations",
                "modify:categories");

            var correlationId = "My Test Manga Volume 1, Chapter 1";

            await this.webApplicationFactory.CallOnce(setUpPrecondition, client);

            var response = await client.GetAsync(
                $"mem/translations?projectName={HttpUtility.UrlEncode(ProjectName)}&correlationId={HttpUtility.UrlEncode(correlationId)}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var rawResponseText = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<QueryTranslationsResult>(
                rawResponseText,
                new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                });
            
            result = result ?? throw new InvalidOperationException();
            Assert.Equal(
                expected: new[]
                {
                    "My Test Manga Volume 1, Chapter 1, Caption 43",
                    "My Test Manga Volume 1, Chapter 1, Caption 42",
                    "My Test Manga Volume 1, Chapter 1, Caption 30",
                    "My Test Manga Volume 1, Chapter 1, Caption 29",
                }.OrderBy(x => x),
                actual: result.Translations.Select(tl => tl.CorrelationId).OrderBy(x => x));
        }
        
        [Fact]
        public async Task Paging()
        {
            var client = this.webApplicationFactory.CreateClientWithAuth(
                "read:translations",
                "modify:projects",
                "modify:translations",
                "modify:categories");

            await this.webApplicationFactory.CallOnce(setUpPrecondition, client);

            var allTranslations = new List<QueryTranslationResult>();
            string? paginationToken = null;

            Func<Task> callWithPagination = async () =>
            {
                var url = paginationToken != null
                    ? $"mem/translations?projectName={HttpUtility.UrlEncode(ProjectName)}&paginationToken={HttpUtility.UrlEncode(paginationToken)}&limit=5"
                    : $"mem/translations?projectName={HttpUtility.UrlEncode(ProjectName)}&limit=5";

                var response = await client.GetAsync(url);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var rawResponseText = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<QueryTranslationsResult>(
                    rawResponseText,
                    new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true
                    });

                result = result ?? throw new InvalidOperationException();
                allTranslations.AddRange(result.Translations ?? Enumerable.Empty<QueryTranslationResult>());
                paginationToken = result.PaginationToken;
            };

            do
            {
                await callWithPagination();
            } while (paginationToken != null);

            
            Assert.Equal(
                expected: new[]
                {
                    "My Test Manga Volume 1, Chapter 2, Caption 15",
                    "My Test Manga Volume 1, Chapter 2, Caption 5",
                    "My Test Manga Volume 1, Chapter 2, Caption 1",
                    "My Test Manga Volume 1, Chapter 1, Caption 43",
                    "My Test Manga Volume 1, Chapter 1, Caption 42",
                    "My Test Manga Volume 1, Chapter 1, Caption 30",
                    "My Test Manga Volume 1, Chapter 1, Caption 29",
                }.OrderBy(x => x),
                actual: allTranslations.Select(tl => tl.CorrelationId).OrderBy(x => x));
        }

        private static readonly Func<HttpClient, Task> setUpPrecondition = async client =>
        {
            var addProjectResponse =
                await client.PostAsync($"mem/projects?projectName={HttpUtility.UrlEncode(ProjectName)}", null);

            addProjectResponse.EnsureSuccessStatusCode();

            var addCategoryResponse =
                await client.PostAsJsonAsync($"mem/categories?projectName={HttpUtility.UrlEncode(ProjectName)}",
                    new AddCategoriesParams()
                    {
                        Categories = new AddCategoryParams[]
                        {
                            new AddCategoryParams()
                            {
                                Id = new Guid("7EE2147C-94CD-4CFF-AC14-5D68971A46B6"),
                                Name = "Broski"
                            }
                        }
                    });

            addCategoryResponse.EnsureSuccessStatusCode();

            var addTranslationsResponse = await client.PostAsJsonAsync(
                $"mem/translations?projectName={HttpUtility.UrlEncode(ProjectName)}", new AddTranslationsParams()
                {
                    AllowPartialAdd = false,
                    Translations = new AddTranslationParams[]
                    {
                        new AddTranslationParams()
                        {
                            Source = "……兄さんは…\n世界で一番\n格好いいと思うよ…",
                            Target = "...Bro, I think you're the coolest person in the world.",
                            CorrelationId = "My Test Manga Volume 1, Chapter 2, Caption 15",
                        },
                        new AddTranslationParams()
                        {
                            Source = "な、何だよ\nいきなりぃ…",
                            Target = "Wh-what's up with you, all of a sudden...",
                            CorrelationId = "My Test Manga Volume 1, Chapter 2, Caption 5",
                        },
                        new AddTranslationParams()
                        {
                            Source = "噓だろ……",
                            Target = "it can't be...",
                            CorrelationId = "My Test Manga Volume 1, Chapter 2, Caption 1",
                        },
                        new AddTranslationParams()
                        {
                            Source = "からっぽな部屋",
                            Target = "Empty apartment",
                            CorrelationId = "My Test Manga Volume 1, Chapter 1, Caption 43",
                        },
                        new AddTranslationParams()
                        {
                            Source = "違うなら言って\n欲しいんだよ…",
                            Target = "Please tell me I'm wrong...",
                            CorrelationId = "My Test Manga Volume 1, Chapter 1, Caption 42",
                            CategoryId = new Guid("7EE2147C-94CD-4CFF-AC14-5D68971A46B6"),
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
                            },
                        },
                        new AddTranslationParams()
                        {
                            Source = "本当にオレは\n情けないよ",
                            Target = "I really am just pathetic.",
                            CorrelationId = "My Test Manga Volume 1, Chapter 1, Caption 30",
                        },
                        new AddTranslationParams()
                        {
                            Source = "昨日妻が出て\n行っちゃってさ、",
                            Target = "Yesterday my wife left me,",
                            CorrelationId = "My Test Manga Volume 1, Chapter 1, Caption 29",
                        },
                    }
                });

            addTranslationsResponse.EnsureSuccessStatusCode();
        };

        public static void Initialize(MemContext db)
        {
            
        }
    }
}