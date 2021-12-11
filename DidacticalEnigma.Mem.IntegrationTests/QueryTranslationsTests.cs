using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DidacticalEnigma.Mem.DatabaseModels;
using DidacticalEnigma.Mem.Translation.Categories;
using DidacticalEnigma.Mem.Translation.IoModels;
using DidacticalEnigma.Mem.Translation.Projects;
using DidacticalEnigma.Mem.Translation.Translations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DidacticalEnigma.Mem.IntegrationTests
{
    public class QueryTranslationsTests : BaseIntegrationTest
    {
        private const string ProjectName = "QueryTestProject";
        
        private const string AuthorName = "QueryTestProjectAuthor";

        public QueryTranslationsTests()
        {
            
        }
        
        [Fact]
        public async Task GetByQuery()
        {
            await this.WebApplicationFactory.CallOnce(setUpPrecondition, this);

            var query = "言う";

            var queryHandler = this.ServiceProvider.GetRequiredService<QueryTranslationsHandler>();

            var response = await queryHandler.Query(
                userName: AuthorName,
                projectName: ProjectName,
                correlationIdStart: null,
                queryText: query,
                category: null,
                paginationToken: null,
                limit: null);

            Assert.Equal(null, response.Error);

            var result = response.Value!;
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
            var correlationId = "My Test Manga Volume 1, Chapter 1";

            await this.WebApplicationFactory.CallOnce(setUpPrecondition, this);

            var queryTranslationsHandler = this.ServiceProvider.GetRequiredService<QueryTranslationsHandler>();
            
            var response = await queryTranslationsHandler.Query(
                AuthorName,
                ProjectName,
                correlationId,
                null,
                null,
                null,
                null);

            Assert.Equal(null, response.Error);

            var result = response.Value ?? throw new InvalidOperationException();
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
            await this.WebApplicationFactory.CallOnce(setUpPrecondition, this);

            var allTranslations = new List<QueryTranslationResult>();
            string? paginationToken = null;

            var queryTranslationsHandler = this.ServiceProvider.GetRequiredService<QueryTranslationsHandler>();

            Func<Task> callWithPagination = async () =>
            {
                var response = await queryTranslationsHandler.Query(
                    AuthorName,
                    ProjectName,
                    null,
                    null,
                    null,
                    paginationToken,
                    limit: 5);

                Assert.Equal(null, response.Error);
                var result = response.Value ?? throw new InvalidOperationException();
                
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

        private static readonly Func<QueryTranslationsTests, Task> setUpPrecondition = async self =>
        {
            var userManager = self.ServiceProvider.GetRequiredService<UserManager<User>>();

            await userManager.CreateAsync(new User()
            {
                UserName = AuthorName
            }, "QWERTYqwerty1!");
            
            var addProjectHandler = self.ServiceProvider.GetRequiredService<AddProjectHandler>();

            var addProjectResult = await addProjectHandler.Add(AuthorName, ProjectName, false);
            
            Assert.Equal(null, addProjectResult.Error);

            var addCategoryHandler = self.ServiceProvider.GetRequiredService<AddCategoriesHandler>();
            
            var addCategoryResult = await addCategoryHandler.Add(
                AuthorName,
                ProjectName,
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
            
            Assert.Equal(null, addCategoryResult.Error);
            
            var addTranslationsHandler = self.ServiceProvider.GetRequiredService<AddTranslationsHandler>();

            var addTranslationsResponse = await addTranslationsHandler.Add(
                AuthorName,
                ProjectName,
                new AddTranslationParams[]
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
                },
                allowPartialAdd: false);

            Assert.Equal(null, addTranslationsResponse.Error);
        };

        public static void Initialize(MemContext db)
        {
            
        }
    }
}