using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Gemini.Framework;
using Gemini.Framework.Threading;
using Gemini.Modules.CodeEditor.Views;

namespace Gemini.Modules.CodeEditor.ViewModels
{
    [Export(typeof(CodeEditorViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
#pragma warning disable 659
    public class CodeEditorViewModel : PersistedDocument
#pragma warning restore 659
    {
        private readonly LanguageDefinitionManager _languageDefinitionManager;
        private string _originalText;
        private ICodeEditorView _view;

        [ImportingConstructor]
        public CodeEditorViewModel(LanguageDefinitionManager languageDefinitionManager)
        {
            _languageDefinitionManager = languageDefinitionManager;
        }

        protected override void OnViewReady(object view)
        {
            base.OnViewReady(view);
            (view as CodeEditorView).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        public override bool ShouldReopenOnStart
        {
            get { return true; }
        }

        public override void SaveState(BinaryWriter writer)
        {
            writer.Write(FilePath);
        }

        public override void LoadState(BinaryReader reader)
        {
            Load(reader.ReadString());
        }

        protected override void OnViewLoaded(object view)
        {
            _view = (ICodeEditorView) view;
        }

        public override bool Equals(object obj)
        {
            var other = obj as CodeEditorViewModel;
            return other != null
                && string.Equals(FilePath, other.FilePath, StringComparison.InvariantCultureIgnoreCase)
                && string.Equals(FileName, other.FileName, StringComparison.InvariantCultureIgnoreCase);
        }

        protected override Task DoNew()
        {
            _originalText = string.Empty;
            ApplyOriginalText();
            return TaskUtility.Completed;
        }

        protected override async Task DoLoad(string filePath)
        {
            if (File.Exists(filePath))
            {
                _originalText = await File.ReadAllTextAsync(filePath);
                ApplyOriginalText();
            }
        }

        protected override async Task DoSave(string filePath)
        {
            var newText = _view.TextEditor.Text;
            await File.WriteAllTextAsync(filePath, newText);
            _originalText = newText;
        }

        private void ApplyOriginalText()
        {
            _view.TextEditor.Text = _originalText;

            _view.TextEditor.TextChanged += delegate
            {
                IsDirty = string.Compare(_originalText, _view.TextEditor.Text) != 0;
            };

            var fileExtension = Path.GetExtension(FileName).ToLower();

            ILanguageDefinition languageDefinition = _languageDefinitionManager.GetDefinitionByExtension(fileExtension);

            SetLanguage(languageDefinition);
        }

        private void SetLanguage(ILanguageDefinition languageDefinition)
        {
            _view.TextEditor.SyntaxHighlighting = (languageDefinition != null)
                ? languageDefinition.SyntaxHighlighting
                : null;
        }
    }
}
