using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DidacticalEnigma.Mem.Translation.Categories;
using DidacticalEnigma.Mem.Translation.Contexts;
using DidacticalEnigma.Mem.Translation.IoModels;
using DidacticalEnigma.Mem.Translation.Projects;
using DidacticalEnigma.Mem.Translation.Translations;

namespace DidacticalEnigma.Mem.Translation
{
    public class TranslationMemory : ITranslationMemory
    {
        private readonly AddTranslations addTranslations;
        private readonly QueryTranslations queryTranslations;
        private readonly GetContexts getContexts;
        private readonly DeleteContext deleteContext;
        private readonly UpdateTranslation updateTranslation;
        private readonly AddProject addProject;
        private readonly AddContext addContext;
        private readonly GetContextData getContextData;
        private readonly DeleteTranslation deleteTranslation;
        private readonly DeleteProject deleteProject;
        private readonly QueryCategories queryCategories;
        private readonly AddCategories addCategories;
        private readonly DeleteCategory deleteCategory;
        private readonly ListProjects listProjects;

        public TranslationMemory(
            AddTranslations addTranslations,
            QueryTranslations queryTranslations,
            GetContexts getContexts,
            DeleteContext deleteContext,
            UpdateTranslation updateTranslation,
            AddProject addProject,
            AddContext addContext,
            GetContextData getContextData,
            DeleteTranslation deleteTranslation,
            DeleteProject deleteProject,
            QueryCategories queryCategories,
            AddCategories addCategories,
            DeleteCategory deleteCategory,
            ListProjects listProjects)
        {
            this.addTranslations = addTranslations;
            this.queryTranslations = queryTranslations;
            this.getContexts = getContexts;
            this.deleteContext = deleteContext;
            this.updateTranslation = updateTranslation;
            this.addProject = addProject;
            this.addContext = addContext;
            this.getContextData = getContextData;
            this.deleteTranslation = deleteTranslation;
            this.deleteProject = deleteProject;
            this.queryCategories = queryCategories;
            this.addCategories = addCategories;
            this.deleteCategory = deleteCategory;
            this.listProjects = listProjects;
        }

        public async Task<Result<QueryTranslationsResult, Unit>> Query(
            string? projectName,
            string? correlationIdStart,
            string? queryText,
            string? category,
            string? paginationToken = null,
            int? limit = null)
        {
            return await this.queryTranslations.Query(
                projectName,
                correlationIdStart,
                queryText,
                category,
                paginationToken,
                limit);
        }
        
        public async Task<Result<QueryContextsResult, Unit>> GetContexts(Guid? id, string? projectName, string? correlationId)
        {
            return await this.getContexts.Get(id, projectName, correlationId);
        }

        public async Task<Result<Unit, Unit>> DeleteContext(Guid id)
        {
            return await this.deleteContext.Delete(id);
        }

        public async Task<Result<Unit, Unit>> DeleteTranslation(string projectName, string correlationId)
        {
            return await this.deleteTranslation.Delete(projectName, correlationId);
        }

        public async Task<Result<Unit, Unit>> DeleteProject(string projectName)
        {
            return await this.deleteProject.Delete(projectName);
        }

        public async Task<Result<Unit, QueryTranslationResult>> UpdateTranslation(
            string projectName,
            string correlationId,
            UpdateTranslationParams uploadParams)
        {
            return await this.updateTranslation.Update(projectName, correlationId, uploadParams);
        }

        public async Task<Result<QueryCategoriesResult, Unit>> QueryCategories(string projectName)
        {
            return await this.queryCategories.Query(projectName);
        }

        public async Task<Result<Unit, Unit>> AddCategories(
            string projectName,
            AddCategoriesParams categoriesParams)
        {
            return await this.addCategories.Add(projectName, categoriesParams);
        }

        public async Task<Result<Unit, Unit>> DeleteCategory(Guid categoryId)
        {
            return await this.deleteCategory.Delete(categoryId);
        }

        public async Task<Result<QueryProjectsResult, object>> ListProjects()
        {
            return await this.listProjects.Query();
        }

        public async Task<Result<FileResult, Unit>> GetContextData(Guid id)
        {
            return await this.getContextData.Get(id);
        }

        public async Task<Result<Unit, Unit>> AddProject(string projectName)
        {
            return await this.addProject.Add(projectName);
        }

        public async Task<Result<AddTranslationsResult, Unit>> AddTranslations(
            string projectName,
            IReadOnlyCollection<AddTranslationParams> translations,
            bool allowPartialAdd = false)
        {
            return await this.addTranslations.Add(projectName, translations, allowPartialAdd);
        }

        public async Task<Result<Unit, Unit>> AddContext(
            Guid id,
            string correlationId,
            string projectName,
            Stream? content,
            string? mediaType,
            string? text)
        {
            return await this.addContext.Add(id, correlationId, projectName, content, mediaType, text);
        }
    }
}