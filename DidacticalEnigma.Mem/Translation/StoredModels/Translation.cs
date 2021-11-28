using System;
using System.Linq;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Mem.Translation.Extensions;
using JDict;
using NpgsqlTypes;
using Optional;

namespace DidacticalEnigma.Mem.Translation.StoredModels
{
    public class Translation
    {
        public Guid Id { get; set; }
        
        public string CorrelationId { get; set; }

        public string Source { get; set; }
        
        public NpgsqlTsVector SearchVector { get; set; }

        public string? Target { get; set; }
        
        public Project Parent { get; set; }

        public int ParentId { get; set; }
        
        public DateTime CreationTime { get; set; }
        
        public DateTime ModificationTime { get; set; }
        
        public NotesCollection? Notes { get; set; }
        
        public string? AssociatedData { get; set; }
    }
}