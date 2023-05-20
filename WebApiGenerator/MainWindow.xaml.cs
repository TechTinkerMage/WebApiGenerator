using ICSharpCode.AvalonEdit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WebApiGenerator.ViewModels;

namespace WebApiGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }

        private void textEditor_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            TextEditor textEditor = (TextEditor)sender;
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                double fontSize = textEditor.FontSize + (e.Delta > 0 ? 1 : -1);
                if (fontSize >= 8 && fontSize <= 72)
                {
                    textEditor.FontSize = fontSize;
                }
                e.Handled = true;
            }
        }

        private void textEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D && (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                var textEditor = (TextEditor)sender;
                try
                {
                    var json = JsonConvert.DeserializeObject(textEditor.Text);
                    textEditor.Text = JsonConvert.SerializeObject(json, Formatting.Indented);
                }
                catch (JsonReaderException)
                {

                }
            }
        }

        public void DisplayJsonAsTree(string json, TreeView treeView)
        {
            treeView.Items.Clear();
            var token = JToken.Parse(json);
            if (token is JObject jsonObject)
            {
                AddObjectNodes(jsonObject, "Root", treeView.Items);
            }
            else if (token is JArray jsonArray)
            {
                AddArrayNodes(jsonArray, "Root", treeView.Items);
            }
        }

        private void AddObjectNodes(JObject @object, string name, ItemCollection itemCollection)
        {
            var node = new TreeViewItem { Header = name };
            itemCollection.Add(node);

            foreach (var property in @object.Properties())
            {
                AddTokenNodes(property.Value, property.Name, node.Items);
            }
        }

        private void AddArrayNodes(JArray array, string name, ItemCollection itemCollection)
        {
            var node = new TreeViewItem { Header = name };
            itemCollection.Add(node);

            for (var i = 0; i < array.Count; i++)
            {
                AddTokenNodes(array[i], $"[{i}]", node.Items);
            }
        }

        private void AddTokenNodes(JToken token, string name, ItemCollection itemCollection)
        {
            switch (token)
            {
                case JValue jValue:
                    itemCollection.Add(new TreeViewItem { Header = $"{name}: {jValue.Value}" });
                    break;
                case JArray jArray:
                    AddArrayNodes(jArray, name, itemCollection);
                    break;
                case JObject jObject:
                    AddObjectNodes(jObject, name, itemCollection);
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DisplayJsonAsTree(jsonEditor.Document.Text, treeView);
        }
    }
}
