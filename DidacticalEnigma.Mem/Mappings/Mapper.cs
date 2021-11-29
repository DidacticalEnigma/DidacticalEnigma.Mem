using System.Linq;
using DidacticalEnigma.Mem.DatabaseModels;
using DidacticalEnigma.Mem.Translation.IoModels;

namespace DidacticalEnigma.Mem.Mappings
{
    public static class Mapper
    {
        public static QueryTranslationNotesResult? Map(NotesCollection? notes)
        {
            if (notes == null)
                return null;

            return new QueryTranslationNotesResult()
            {
                Gloss = notes.GlossNotes
                    .Select(n => new IoGlossNote()
                    {
                        Explanation = n.Text,
                        Foreign = n.Foreign
                    })
                    .ToList(),
                Normal = notes.NormalNote
                    .Select(n => new IoNormalNote()
                    {
                        Text = n.Text,
                        SideText = n.SideComment
                    })
                    .ToList()
            };
        }
        
        public static NotesCollection? Map(AddTranslationNotesParams? notes)
        {
            if (notes == null)
                return null;

            return new NotesCollection()
            {
                GlossNotes = notes.Gloss
                    .Select(n => new GlossNote()
                    {
                        Foreign = n.Foreign,
                        Text = n.Explanation
                    })
                    .ToList(),
                NormalNote = notes.Normal
                    .Select(n => new NormalNote()
                    {
                        Text = n.Text,
                        SideComment = n.SideText
                    })
                    .ToList()
            };
        }

        public static NotesCollection? Map(UpdateTranslationNotesParams? notes)
        {
            if (notes == null)
                return null;

            return new NotesCollection()
            {
                GlossNotes = notes.Gloss
                    .Select(n => new GlossNote()
                    {
                        Foreign = n.Foreign,
                        Text = n.Explanation
                    })
                    .ToList(),
                NormalNote = notes.Normal
                    .Select(n => new NormalNote()
                    {
                        Text = n.Text,
                        SideComment = n.SideText
                    })
                    .ToList()
            };
        }
    }
}