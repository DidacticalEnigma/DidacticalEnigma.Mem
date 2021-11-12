using System.Collections.Generic;
using System.Linq;
using DidacticalEnigma.Core.Models.LanguageService;
using Utility.Utils;

namespace DidacticalEnigma.Mem.Translation.Extensions
{
    public static class AnalyzerExtensions
    {
        public static string Normalize(this IMorphologicalAnalyzer<IpadicEntry> analyzer, string input)
        {
            var morphemes = analyzer
                .ParseToEntries(input)
                .Where(e => e.IsRegular)
                .Where(IsNotStopWord)
                .Select(e => e.DictionaryForm);
            var text = string.Join(" ", morphemes);
            return text;
        }
        
        public static (string sentence, string? highlighter) Highlight(this IMorphologicalAnalyzer<IpadicEntry> analyzer, string haystack, string needle)
        {
            var needleMorphemes = analyzer
                .ParseToEntries(needle)
                .Where(e => e.IsRegular)
                .ToList();
            var haystackMorphemes = analyzer
                .ParseToEntries(haystack)
                .Where(e => e.IsRegular)
                .ToList();

            if (haystackMorphemes.Count == 0 || needleMorphemes.Count == 0)
                return (haystack, null);
            
            var values = new List<int>(haystackMorphemes.Count);
            for (int i = 0; i < haystackMorphemes.Count; ++i)
            {
                var count = Enumerable
                    .Zip(haystackMorphemes.Skip(i), needleMorphemes)
                    .Count(pair => pair.First.DictionaryForm == pair.Second.DictionaryForm);
                values.Add(count);
            }

            var start = EnumerableExt.MaxBy(values.Indexed(), p => p.element).index;
            var highlighter = GetHighlighterFor(haystack);
            var haystackStrings = haystackMorphemes.Select(m => m.SurfaceForm).Materialize();
            var strings = haystackStrings
                .Take(start)
                .Append(highlighter)
                .Concat(haystackStrings.Skip(start).Take(values[start]))
                .Append(highlighter)
                .Concat(haystackStrings.Skip(start + values[start]));
            return (string.Concat(strings), highlighter);
        }

        private static bool IsNotStopWord(IpadicEntry entry)
        {
            return entry.PartOfSpeech != PartOfSpeech.Particle;
        }
        
        private static string GetHighlighterFor(string haystack)
        {
            return "|";
        }
    }
}