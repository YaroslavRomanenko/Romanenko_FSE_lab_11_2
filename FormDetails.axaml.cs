using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Romanenko_FSE_lab11_2_a
{
    public partial class FormDetails : Window
    {
        public FormDetails()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void SetAllData(Data data)
        {
            this.FindControl<TextBlock>("lblShowSize").Text = data.fileSizeInKiloBytes.ToString();
            this.FindControl<TextBlock>("lblShowSymCount").Text = data.symCount.ToString();
            this.FindControl<TextBlock>("lblShowParCount").Text = data.paragraphCount.ToString();
            this.FindControl<TextBlock>("lblShowEmptyRowCount").Text = data.emptyRowCount.ToString();
            this.FindControl<TextBlock>("lblShowAuthorPageCount").Text = data.authorPageCount.ToString();
            this.FindControl<TextBlock>("lblShowVolCyrillicCount").Text = data.vowelCyrillicCount.ToString();
            this.FindControl<TextBlock>("lblShowConCyrillicCount").Text = data.consonantCyrillicCount.ToString();
            this.FindControl<TextBlock>("lblShowVolLatinCount").Text = data.vowelLatinCount.ToString();
            this.FindControl<TextBlock>("lblShowConLatinCount").Text = data.consonantLatinCount.ToString();
            this.FindControl<TextBlock>("lblShowNumCount").Text = data.numberCount.ToString();
            this.FindControl<TextBlock>("lblShowSpecSymCount").Text = data.specialSymCount.ToString();
            this.FindControl<TextBlock>("lblShowPunctMarkCount").Text = data.punctuationMarkCount.ToString();
        }
    }
}