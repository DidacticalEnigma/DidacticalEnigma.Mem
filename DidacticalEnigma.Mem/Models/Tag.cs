namespace DidacticalEnigma.Mem.Models
{
    public class Tag
    {
        public enum TagType
        {
            Text,
            Highlighted,
            Newline
        }
        
        public TagType Type { get; }
        
        public string Text { get; }

        private Tag(TagType type, string text)
        {
            Type = type;
            Text = text;
        }

        public static Tag CreateText(string text)
        {
            return new Tag(TagType.Text, text);
        }

        public static Tag CreateHighlighted(string text)
        {
            return new Tag(TagType.Highlighted, text);
        }
        
        public static Tag CreateNewline()
        {
            return new Tag(TagType.Newline, "");
        }
    }
}