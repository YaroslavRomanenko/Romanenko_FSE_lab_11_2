using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using HtmlAgilityPack;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System.Diagnostics;

namespace Romanenko_FSE_lab11_2_a
{
    public class Data
    {
        public double fileSizeInKiloBytes { get; set; }
        public int symCount { get; set; }
        public int paragraphCount { get; set; }
        public int emptyRowCount { get; set; }
        public int authorPageCount { get; set; }
        public int vowelCyrillicCount { get; set; }
        public int consonantCyrillicCount { get; set; }
        public int vowelLatinCount { get; set; }
        public int consonantLatinCount { get; set; }
        public int numberCount { get; set; }
        public int specialSymCount { get; set; }
        public int punctuationMarkCount { get; set; }
    }

    public partial class MainWindow : Window
    {
        private FormDetails? formDetails = null;
        private FormListBoxResult? formListBoxResult = null;
        private FormTextBoxResult? formTextBoxResult = null;
        private Data data = new Data();

        public MainWindow()
        {
            InitializeComponent();

            if (TxtSource is null)
            {
                Console.WriteLine("FATAL: TxtSource is null after InititalizeComponent!");
                return;
            }

            TxtSource.KeyUp += Logger.Instance.OnKeyUp;

            Dispatcher.UIThread.Post(() =>
            {
                TxtSource.Focus();
            }, DispatcherPriority.Input);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            TxtFilename = this.FindControl<TextBox>("TxtFilename");
            TxtSource = this.FindControl<TextBox>("TxtSource");
            this.Closing += MainWindow_Closing;
        }

        private async void TsmiOpenFile_Click(object sender, RoutedEventArgs e)
        {
            if (formDetails is not null)
            {
                formDetails.Close();
            }

            var storageProvider = this.StorageProvider;

            if (storageProvider is null)
            {
                await ShowMessageAsync("Error", "Cannot access storage provider");
                return;
            }

            var filePickerOpenOptions = new FilePickerOpenOptions
            {
                Title = "Open Text File",
                AllowMultiple = false,
                FileTypeFilter = new[] { FilePickerFileTypes.TextPlain }
            };

            try
            {
                var result = await storageProvider.OpenFilePickerAsync(filePickerOpenOptions);

                if (result is not null && result.Count > 0)
                {
                    IStorageFile selectedFile = result[0];
                    await ProcessSelectedFile(selectedFile);
                    TxtSource.Focus();
                }
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Error", $"Failed during file selection: {ex.Message}");
            }


        }

        private async Task ProcessSelectedFile(IStorageFile file)
        {
            string content;
            long fileSizeInBytes = 0;

            try
            {
                var properties = await file.GetBasicPropertiesAsync();
                fileSizeInBytes = (long)properties.Size;
                TxtFilename.Text = file.Name;
                await using (Stream stream = await file.OpenReadAsync())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        content = await reader.ReadToEndAsync();
                    }
                }

                TxtSource.Text = content;
                TxtSource.IsReadOnly = true;
                string lowerText = content.ToLower();

                data.fileSizeInKiloBytes = Math.Round(fileSizeInBytes / 1024.0, 4);
                data.symCount = content.Length;
                string[] paragraphs = content.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                data.paragraphCount = paragraphs.Length;
                string[] emptyRows = content.Split(new[] { "\r\n" }, StringSplitOptions.None);
                data.emptyRowCount = emptyRows.Length - data.paragraphCount;
                data.authorPageCount = data.symCount / 1800;
                string vowelsCyrillic = "аеёиоуыэюяіїє";
                data.vowelCyrillicCount = lowerText.Count(sym => vowelsCyrillic.Contains(sym));
                string consonantsCyrillic = "бвгджзйклмнпрстфхцчшщьъ";
                data.consonantCyrillicCount = lowerText.Count(sym => consonantsCyrillic.Contains(sym));
                string vowelsLatin = "aeiou";
                data.vowelLatinCount = lowerText.Count(sym => vowelsLatin.Contains(sym));
                string consonantsLatin = "bcdfghjklmnpqrstvwxyz";
                data.consonantLatinCount = lowerText.Count(sym => consonantsLatin.Contains(sym));
                data.numberCount = lowerText.Count(sym => Regex.IsMatch(sym.ToString(), @"\d"));
                data.specialSymCount = lowerText.Count(sym => !char.IsLetterOrDigit(sym) && !char.IsWhiteSpace(sym) && !char.IsPunctuation(sym));
                data.punctuationMarkCount = lowerText.Count(char.IsPunctuation);
            }
            catch (IOException ioEx)
            {
                await ShowMessageAsync("File Error", $"Error reading file '{file.Name}': {ioEx.Message}");
                TxtSource.Text = string.Empty;
                TxtFilename.Text = string.Empty;
                data = new Data();
            }
            catch (Exception e)
            {
                await ShowMessageAsync("Error", $"An unexpected error occurred processing file '{file.Name}': {e.Message}");
                TxtSource.Text = string.Empty;
                TxtFilename.Text = string.Empty;
                data = new Data();
            }
        }

        private async void TsmiDetails_Click(object sender, RoutedEventArgs e)
        {
            if (await IsTextFieldEmptyAsync(TxtSource.Text)) return;

            if (formDetails != null)
            {
                formDetails.Close();
            }

            formDetails = new FormDetails();
            formDetails.SetAllData(data);
            formDetails.Show();
        }

        private async void TsmiFormat_Click(object sender, RoutedEventArgs e)
        {
            if (await IsTextFieldEmptyAsync(TxtSource.Text)) return;

            string cleanedInput = Regex.Replace(TxtSource.Text, @" {2,}", " ");
            cleanedInput = Regex.Replace(cleanedInput, @"\t{2,}", "\t");
            cleanedInput = Regex.Replace(cleanedInput, @"(\r\n){2,}", "\r\n");

            TxtSource.Text = cleanedInput;
        }

        private async void TsmiSearchByInitials_Click(object sender, RoutedEventArgs e)
        {
            string pattern = @"\b[A-ZА-ЯІЇЄ][a-zа-яіїє]+ [A-ZА-ЯІЇЄ]\.[A-ZА-ЯІЇЄ]\.(?=\s|$)";
            await SearchAndInsertToResultAsync(pattern);
        }

        private async void TsmiFindTheMailingAddress_Click(object sender, RoutedEventArgs e)
        {
            string pattern = @"\b\d{5}, (м\.|c\.|смт) [А-ЯІЇЄ][а-яіїє]+, (вул|проcп|пров)\. [А-ЯІЇЄ][а-яіїє]+, буд\. \d+(, кв\. \d+)?\b";
            await SearchAndInsertToResultAsync(pattern);
        }

        private async void TsmiSearchForMobileNumbers_Click(object sender, RoutedEventArgs e)
        {
            string pattern = @"(?<=\s|^)(\+380|0)(67|68|96|97|98|50|66|95|99|63|73|93|91|92|94)\d{7}\b";
            await SearchAndInsertToResultAsync(pattern);
        }

        private async Task SearchAndInsertToResultAsync(string pattern)
        {
            if (await IsTextFieldEmptyAsync(TxtSource.Text)) return;

            MatchCollection matches = Regex.Matches(TxtSource.Text, pattern);

            if (formListBoxResult != null)
            {
                formListBoxResult.Close();
            }

            formListBoxResult = new FormListBoxResult();

            foreach (Match match in matches)
            {
                formListBoxResult.LsbResultsControl.Items.Add(match.Value);
            }

            formListBoxResult.Show();
        }

        private async void TsmiFindTheMostFreqWordsLatin_Click(object sender, RoutedEventArgs e)
        {
            if (await IsTextFieldEmptyAsync(TxtSource.Text)) return;

            string pattern = @"\b[A-Za-z]+\b";
            MatchCollection matches = Regex.Matches(TxtSource.Text, pattern);

            Dictionary<string, int> wordCounter = new Dictionary<string, int>();

            if (formListBoxResult != null)
            {
                formListBoxResult.Close();
            }

            formListBoxResult = new FormListBoxResult();

            foreach (Match match in matches)
            {
                string word = match.Value.ToLower();
                if (wordCounter.ContainsKey(word))
                {
                    wordCounter[word]++;
                }
                else
                {
                    wordCounter[word] = 1;
                }
            }

            if (wordCounter.Count == 0)
            {
                await ShowMessageAsync("Information", "Latin words not found!");
                return;
            }

            int maxCount = wordCounter.Values.Max();

            var mostFrequentWords = wordCounter.Where(pair => pair.Value == maxCount)
                                              .Select(pair => $"{pair.Key} ({pair.Value} time(s))")
                                              .ToList();

            foreach (var word in mostFrequentWords)
            {
                formListBoxResult.LsbResultsControl.Items.Add(word);
            }

            formListBoxResult.Show();
        }

        private async void TsmiReplaceSeqLettersWithAnother_Click(object sender, RoutedEventArgs e)
        {
            if (await IsTextFieldEmptyAsync(TxtSource.Text)) return;

            string oldSequenceOfLetters = await ShowInputDialogAsync("Enter the sequence of letters you want to replace: ", "Input field");
            if (await IsInputTextFieldEmptyAsync(oldSequenceOfLetters)) return;

            string newSequenceOfLetters = await ShowInputDialogAsync("Enter the sequence of letters you want to replace with: ", "Input field");
            if (await IsInputTextFieldEmptyAsync(newSequenceOfLetters)) return;

            TxtSource.Text = TxtSource.Text.Replace(oldSequenceOfLetters, newSequenceOfLetters);
        }

        private async void TsmiSearchByLetterSequence_Click(object sender, RoutedEventArgs e)
        {
            string text = TxtSource.Text;

            if (await IsTextFieldEmptyAsync(text)) return;

            string sequenceToSearchFor = await ShowInputDialogAsync("Enter the sequence of letters you want to find: ", "Input field");

            if (await IsInputTextFieldEmptyAsync(sequenceToSearchFor)) return;

            string[] lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            bool found = false;

            if (formTextBoxResult != null)
            {
                formTextBoxResult.Close();
            }

            formTextBoxResult = new FormTextBoxResult();

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(sequenceToSearchFor))
                {
                    formTextBoxResult.TxtResultsControl.Text += $"Line {i + 1}: {lines[i]}" + Environment.NewLine;
                    found = true;
                }
            }

            if (!found)
            {
                await ShowMessageAsync("Information", "Sequence not found");
                return;
            }

            formTextBoxResult.Show();
        }

        private async Task<bool> IsTextFieldEmptyAsync(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                await ShowMessageAsync("Error!", "There's no text in text field!");
                return true;
            }
            return false;
        }

        private async Task<bool> IsInputTextFieldEmptyAsync(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                await ShowMessageAsync("Error!", "You haven't entered anything!");
                return true;
            }
            return false;
        }

        private async void TsmiLoadNews_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://www.znu.edu.ua/cms/index.php?action=news/view&start=0&site_id=27&lang=ukr";

            string amountOfNewsStr = await ShowInputDialogAsync("Enter the correct amount of news: ", "Input field");
            if (await IsInputTextFieldEmptyAsync(amountOfNewsStr)) return;

            if (!int.TryParse(amountOfNewsStr, out int amountOfNews))
            {
                await ShowMessageAsync("Error!", "You only need to enter numbers");
                return;
            }

            if (amountOfNews <= 0)
            {
                await ShowMessageAsync("Error!", "Amount must be natural");
                return;
            }

            try
            {
                string html = await DownloadHtmlCodeAsync(url);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                var newsNodes = doc.DocumentNode.SelectNodes("//div[@class='znu-2016-news']/div[@class='znu-2016-new']");

                if (newsNodes is null)
                {
                    await ShowMessageAsync("Error!", "Failed to find any news");
                    return;
                }

                int count = 0;
                int totalNewsCount = newsNodes.Count;

                int newsToProcess = (amountOfNews == 0 || amountOfNews > totalNewsCount) ? totalNewsCount : amountOfNews;

                if (TxtSource != null)
                {
                    StringBuilder newsBuilder = new StringBuilder();
                    foreach (var node in newsNodes)
                    {
                        if (amountOfNews > 0 && count >= amountOfNews)
                            break;

                        var heading = node.SelectSingleNode(".//div[@class='znu-2016-new-img-list-info']//h4/a");
                        var paragraph = node.SelectSingleNode(".//div[@class='znu-2016-new-img-list-info']//div[@class='text']/p[1]");

                        if (heading != null)
                        {
                            string header = WebUtility.HtmlDecode(heading.InnerText.Trim());
                            string par = paragraph is not null ? WebUtility.HtmlDecode(paragraph.InnerText.Trim()) : "There's no annotation";

                            count++;
                            newsBuilder.AppendLine("------------------------");
                            newsBuilder.AppendLine($"News #{count}");
                            newsBuilder.AppendLine("------------------------");
                            newsBuilder.AppendLine($"{header}");
                            newsBuilder.AppendLine("------------------------");
                            newsBuilder.AppendLine($"{par}");
                            newsBuilder.AppendLine();
                        }
                    }
                    TxtSource.Text = newsBuilder.ToString();
                    TxtSource.IsReadOnly = false;

                    Dispatcher.UIThread.Post(() =>
                    {
                        TxtSource.Focus();
                    }, DispatcherPriority.Input);
                }

                TxtFilename.Text = url;
                await ShowMessageAsync("Information", $"Found total news: {totalNewsCount}\nDownloaded news: {count}");
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Error!", $"An error occurred: {ex.Message}");
            }
        }


        private async Task<string> DownloadHtmlCodeAsync(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Avalonia Application");
                    return await client.GetStringAsync(url);
                }
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Error!", $"Failed to download html: {ex.Message}");
                return string.Empty;
            }
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            string? logPath = Logger.Instance.FilePath;
            if (logPath != null && File.Exists(logPath))
            {
                try
                {
                    File.Delete(logPath);
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Could not delete log file {logPath}: {ex.Message}");
                }
                catch (UnauthorizedAccessException uaEx)
                {
                    Console.WriteLine($"No permission to delete log file {logPath}: {uaEx.Message}");
                }
            }
        }

        private Task ShowMessageAsync(string title, string message)
        {
            return Dispatcher.UIThread.InvokeAsync(() =>
            {
                var messageBoxStandartWindow = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                return messageBoxStandartWindow.ShowAsync();
            });
        }

        private async Task<string> ShowInputDialogAsync(string prompt, string title)
        {
            var dialog = new InputDialog(prompt, title);
            return await dialog.ShowDialog<string>(this) ?? string.Empty;
        }
    }
}