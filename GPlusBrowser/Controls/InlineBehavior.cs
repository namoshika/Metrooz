using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Interactivity;
using SunokoLibrary.Web.GooglePlus;
//using Microsoft.Expression.Interactivity.Core;

namespace GPlusBrowser.Controls
{
    public class SetInlineBehavior : Behavior<TextBlock>
	{
		protected override void OnAttached()
		{
			base.OnAttached();
            OnChangedBinding(Element);
		}
		protected override void OnDetaching()
		{
			base.OnDetaching();
            AssociatedObject.Inlines.Clear();
		}

        public SunokoLibrary.Web.GooglePlus.ContentElement Element
        {
            get { return (SunokoLibrary.Web.GooglePlus.ContentElement)GetValue(ElementProperty); }
            set { SetValue(ElementProperty, value); }
        }
        public static readonly DependencyProperty ElementProperty = DependencyProperty.Register(
            "Element", typeof(SunokoLibrary.Web.GooglePlus.ContentElement), typeof(SetInlineBehavior),
            new UIPropertyMetadata(null, (sender, e) => ((SetInlineBehavior)sender).OnChangedBinding((SunokoLibrary.Web.GooglePlus.ContentElement)e.NewValue)));
        void OnChangedBinding(SunokoLibrary.Web.GooglePlus.ContentElement newElement)
        {
            if (AssociatedObject == null)
                return;

            var textBlock = AssociatedObject;
            if (newElement != null)
            {
                textBlock.Inlines.Clear();
                textBlock.Inlines.Add(PrivateConvertInlines(newElement));
            }
            else
                textBlock.Inlines.Clear();
        }

        static System.Windows.Documents.Inline PrivateConvertInlines(SunokoLibrary.Web.GooglePlus.ContentElement tree)
        {
            if (tree == null)
                return null;
            System.Windows.Documents.Inline inline = null;
            switch (tree.Type)
            {
                case ElementType.Style:
                    var styleEle = ((StyleElement)tree);
                    switch (styleEle.Style)
                    {
                        case StyleType.Bold:
                            inline = new System.Windows.Documents.Bold();
                            ((System.Windows.Documents.Bold)inline).Inlines.AddRange(
                                ((StyleElement)tree).Children.Select(ele => PrivateConvertInlines(ele)));
                            break;
                        case StyleType.Italic:
                            inline = new System.Windows.Documents.Italic();
                            ((System.Windows.Documents.Italic)inline).Inlines.AddRange(
                                ((StyleElement)tree).Children.Select(ele => PrivateConvertInlines(ele)));
                            break;
                        case StyleType.Middle:
                            inline = new System.Windows.Documents.Span();
                            inline.TextDecorations.Add(System.Windows.TextDecorations.Strikethrough);
                            ((System.Windows.Documents.Span)inline).Inlines.AddRange(
                                ((StyleElement)tree).Children.Select(ele => PrivateConvertInlines(ele)));
                            break;
                        default:
                            inline = new System.Windows.Documents.Span();
                            ((System.Windows.Documents.Span)inline).Inlines.AddRange(
                                ((StyleElement)tree).Children.Select(ele => PrivateConvertInlines(ele)));
                            break;
                    }
                    break;
                case ElementType.Hyperlink:
                    var hyperEle = (HyperlinkElement)tree;
                    var target = hyperEle.Target;
                    var hyperLink = new System.Windows.Documents.Hyperlink(new System.Windows.Documents.Run(hyperEle.Text)) { Focusable = false };
                    hyperLink.Click += (sender, e) => { System.Diagnostics.Process.Start(target.AbsoluteUri); };
                    inline = hyperLink;
                    break;
                case ElementType.Mension:
                    var spanInline = new System.Windows.Documents.Span();
                    spanInline.Inlines.AddRange(
                        new System.Windows.Documents.Inline[]
                        {
                            new System.Windows.Documents.Run("+"),
                            new System.Windows.Documents.Hyperlink(
                                new System.Windows.Documents.Run(((MensionElement)tree).Text.Substring(1)))
                                { TextDecorations = null, Focusable = false }
                        });
                    inline = spanInline;
                    break;
                case ElementType.Text:
                    inline = new System.Windows.Documents.Run(((TextElement)tree).Text);
                    break;
                case ElementType.Break:
                    inline = new System.Windows.Documents.LineBreak();
                    break;
                default:
                    throw new Exception();
            }
            return inline;
        }
	}
}