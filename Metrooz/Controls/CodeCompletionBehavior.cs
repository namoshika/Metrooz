using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Interactivity;
//using Microsoft.Expression.Interactivity.Core;

namespace Metrooz
{
    public class CodeCompletionBehavior : Behavior<ICSharpCode.AvalonEdit.TextEditor>
    {
        public CodeCompletionBehavior() { }
        CompletionWindow completionWindow;
        protected override void OnAttached()
        {
            base.OnAttached();
        }
        protected override void OnDetaching()
        {
            base.OnDetaching();
        }

        void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null && !char.IsLetterOrDigit(e.Text[0]))
                completionWindow.CompletionList.RequestInsertion(e);
        }
        void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "+")
            {
                completionWindow = new CompletionWindow(AssociatedObject.TextArea);
                var data = completionWindow.CompletionList.CompletionData;
                data.Add(new MentionCompletionData("yuki miyabi+111505920302662271472", new Button() { Content = "yuki miyabi" }));
                data.Add(new MentionCompletionData("Hidetaka Kawase+110176194700379954027", new Button() { Content = "Hidetaka Kawase" }));
                data.Add(new MentionCompletionData("機械犬+105562819149620818862", new Button() { Content = "機械犬" }));
                completionWindow.Show();
                completionWindow.Closed += delegate { completionWindow = null; };
            }
        }
        public class MentionCompletionData : ICompletionData
        {
            public MentionCompletionData(string text, object displayText)
            {
                Text = text;
                Content = displayText;
            }
            public object Content { get; private set; }
            public object Description { get; private set; }
            public string Text { get; private set; }
            public double Priority { get { return 0.0; } }
            public System.Windows.Media.ImageSource Image { get { return null; } }

            public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
            {
                textArea.Document.Replace(completionSegment, this.Text);
            }


        }
        public class MentionElementGenerator : VisualLineElementGenerator
        {
            readonly static Regex mentionFormatRegex = new Regex("\\+(?<name>[^+]+)\\+(?<plusid>\\d{21})", RegexOptions.IgnoreCase);
            public override int GetFirstInterestedOffset(int startOffset)
            {
                var match = FindMatch(startOffset);
                return match.Success ? (startOffset + match.Index) : -1;
            }
            public override VisualLineElement ConstructElement(int offset)
            {
                var match = FindMatch(offset);
                if (match.Success && match.Index == 0)
                    return new InlineObjectElement(
                        match.Length, new System.Windows.Controls.Button() { Content = match.Value });
                return null;
            }
            Match FindMatch(int startOffset)
            {
                var endOffset = CurrentContext.VisualLine.LastDocumentLine.EndOffset;
                var findRangeText = CurrentContext.Document.GetText(startOffset, endOffset - startOffset);
                return mentionFormatRegex.Match(findRangeText);
            }
        }
    }
}