using System;
using NpgsqlTypes;

namespace DidacticalEnigma.Mem.DatabaseModels
{
    public class Translation
    {
        public Guid Id { get; set; }
        
        public string CorrelationId { get; set; }

        public string Source { get; set; }
        
        public NpgsqlTsVector SearchVector { get; set; }

        public string? Target { get; set; }
        
        public Category? Category { get; set; }
        
        public Guid? CategoryId { get; set; }
        
        public Project Parent { get; set; }

        public int ParentId { get; set; }
        
        public DateTime CreationTime { get; set; }
        
        public DateTime ModificationTime { get; set; }
        
        public NotesCollection? Notes { get; set; }
        
        public string? AssociatedData { get; set; }
    }
}