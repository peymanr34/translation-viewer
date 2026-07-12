using MvvmGen;

namespace TranslationViewer.ViewModels
{
    [ViewModel]
    public partial class ItemViewModel
    {
        [Property]
        private string _key;

        [Property]
        private string? _original;

        [Property]
        private string? _translation;

        [Property]
        private long _originalLineNumber;

        [Property]
        private long? _translationLineNumber;

        [Property]
        private bool _hasErrors;
    }
}
