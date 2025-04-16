using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia;

namespace Romanenko_FSE_lab11_2_a 
{
    public partial class InputDialog : Window 
    {
        public string Prompt { get; set; }
        public string InputText => InputTextBox.Text ?? string.Empty;

        public InputDialog() : this("Prompt:", "Input Dialog")
        {
            #if DEBUG
                this.AttachDevTools();
            #endif
        }

        public InputDialog(string prompt, string title)
        {
            InitializeComponent();
            Prompt = prompt;
            Title = title;
            DataContext = this;
            InputTextBox = this.FindControl<TextBox>("InputTextBox");
            #if DEBUG
            this.AttachDevTools();
            #endif
        }

        private void InitializeComponent() 
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close(InputText);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close(null);
        }
    }
}