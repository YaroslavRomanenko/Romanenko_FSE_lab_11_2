using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Romanenko_FSE_lab11_2_a
{
    public partial class FormListBoxResult : Window
    {
        public ListBox LsbResultsControl => this.FindControl<ListBox>("lsbResults");

        public FormListBoxResult()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            
            LsbResultsControl.Items.Clear();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}