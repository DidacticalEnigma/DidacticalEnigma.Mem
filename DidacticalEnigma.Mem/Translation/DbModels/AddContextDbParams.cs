using System;

namespace DidacticalEnigma.Mem.Translation.DbModels
{
    public class AddContextDbParams
    {
        public Guid InputContextId { get; init; }
        
        public string? InputText { get; init; }
        
        public byte[]? InputContent { get; init; }
        
        public string? InputMediaType { get; init; }
    }
}