using ICSharpCode.AvalonEdit;
using System.Windows.Controls;

namespace Gemini.Modules.CodeEditor.Views
{
    public partial class CodeEditorView : UserControl, ICodeEditorView
    {
        public TextEditor TextEditor
        {
            get { return CodeEditor; }
        }

        public CodeEditorView()
        {
            InitializeComponent();
        }
    }
}
