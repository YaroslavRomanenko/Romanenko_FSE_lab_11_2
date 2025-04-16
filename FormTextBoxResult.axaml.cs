using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Romanenko_FSE_lab11_2_a
{
    public partial class FormTextBoxResult : Window
    {
        public TextBox TxtResultsControl => this.FindControl<TextBox>("txtResult");
        
        public FormTextBoxResult()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            
            TxtResultsControl.Text = string.Empty;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}