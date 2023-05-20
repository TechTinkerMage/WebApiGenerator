using ICSharpCode.AvalonEdit;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml;

namespace WebApiGenerator.Behaviours
{
    public static class AvalonEditBehaviour
    {
        public static readonly DependencyProperty HighlightingDefinitionProperty =
      DependencyProperty.RegisterAttached(
          "HighlightingDefinition",
          typeof(string),
        typeof(AvalonEditBehaviour),
          new FrameworkPropertyMetadata(default(string), SyntaxHighlightingChanged));

        private static void SyntaxHighlightingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textEditor = (TextEditor)d;
            if (!string.IsNullOrEmpty((string)e.NewValue))
            {
                var assembly = Assembly.GetExecutingAssembly();
                using (Stream s = assembly.GetManifestResourceStream((string)e.NewValue))
                {
                    if (s != null)
                    {
                        using (XmlTextReader reader = new XmlTextReader(s))
                        {
                            textEditor.SyntaxHighlighting =
                                ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(
                                    reader,
                                    ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
                        }
                    }
                }
            }
        }

        public static string GetHighlightingDefinition(DependencyObject dependencyObject)
        {
            return (string)dependencyObject.GetValue(HighlightingDefinitionProperty);
        }

        public static void SetHighlightingDefinition(DependencyObject dependencyObject, string value)
        {
            dependencyObject.SetValue(HighlightingDefinitionProperty, value);
        }
    }
}
