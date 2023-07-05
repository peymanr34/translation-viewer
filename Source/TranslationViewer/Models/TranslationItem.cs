namespace TranslationViewer.Models
{
    public class TranslationItem
    {
        public string? Key { get; set; }

        public string? Original { get; set; }

        public string? Translation { get; set; }

        public bool HasErrors { get; set; }
    }
}
