using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;

namespace Romanenko_FSE_lab11_2_a
{
    public enum OperationType
    {
        Delete,
        Add
    }

    public sealed class Logger
    {
        private static readonly Lazy<Logger> _lazyInstance = new Lazy<Logger>(() => new Logger());
        public static Logger Instance => _lazyInstance.Value;

        private readonly string? _logDirectory;
        private readonly string? _logFilePath;
        private DateTime _recordDateTime;
        private int _lineNumber = 0;
        private int _symbolNumber = 0;
        private OperationType _operationType;

        private Logger()
        {
            _logDirectory = "/media/yaroslav/Docs R.Y/Docs 2 course, 2 semester/Fundamentals of Software Engineering";
            try
            {
                if (!Directory.Exists(_logDirectory))
                {
                    Directory.CreateDirectory(_logDirectory);
                }
                _logFilePath = Path.Combine(_logDirectory, "Log.txt");
                if (File.Exists(_logFilePath))
                {
                    File.Delete(_logFilePath);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"FATAL: Could not create/access log directory or file. Target directory: '{_logDirectory}'. Logging disabled. Error: {e.GetType().Name} - {e.Message}");
                _logFilePath = null;
            }
        }

        public string? FilePath => _logFilePath;

        public void OnKeyUp(object? sender, KeyEventArgs e)
        {
            if (_logFilePath == null) return;

            if (sender is not TextBox textBox)
            {
                return;
            }
            if (textBox.IsReadOnly)
            {
                return;
            }
            var txtSource = textBox;

            var text = txtSource.Text ?? string.Empty;
            var caretIndex = txtSource.CaretIndex;

            var textUpToCaret = text.Substring(0, Math.Min(caretIndex, text.Length));
            _lineNumber = textUpToCaret.Count(c => c == '\n');
            var lastNewlinePos = textUpToCaret.LastIndexOf('\n');
            _symbolNumber = (lastNewlinePos >= 0) ? caretIndex - (lastNewlinePos + 1) : caretIndex;
            _recordDateTime = DateTime.Now;

            bool shouldLog = false;
            string logMessage = string.Empty;

            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                _operationType = OperationType.Delete;
                logMessage = $"{_recordDateTime:yyyy-MM-dd HH:mm:ss.fff} | DELETE | Key: {e.Key} | Line Number: {_lineNumber + 1}, Symbol Number: {_symbolNumber + 1}";
                shouldLog = true;
            }
            else if (IsPrintableKey(e.Key))
            {
                _operationType = OperationType.Add;
                logMessage = $"{_recordDateTime:yyyy-MM-dd HH:mm:ss.fff} | ADD | Key: {e.Key} | Line Number: {_lineNumber + 1}, Symbol Number: {_symbolNumber}";
                shouldLog = true;
            }

            if (shouldLog)
            {
                WriteLog(logMessage);
            }
        }

        private bool IsPrintableKey(Key key)
        {
            if (key >= Key.A && key <= Key.Z) return true;
            if (key >= Key.NumPad0 && key <= Key.NumPad9) return true;
            if (key == Key.Space || key == Key.OemComma || key == Key.OemPeriod || key == Key.OemMinus || key == Key.OemPlus ||
                key == Key.OemOpenBrackets || key == Key.OemCloseBrackets || key == Key.OemPipe || key == Key.OemQuestion ||
                key == Key.OemQuotes || key == Key.OemSemicolon || key == Key.OemTilde || key == Key.OemBackslash || key == Key.Oem2) return true;
            if (key == Key.Decimal || key == Key.Add || key == Key.Subtract || key == Key.Multiply || key == Key.Divide) return true;

            return false;
        }

        private void WriteLog(string message)
        {
            if (_logFilePath == null) return;

            try
            {
                using (var fs = new FileStream(_logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                using (var writer = new StreamWriter(fs, System.Text.Encoding.UTF8))
                {
                    writer.WriteLine(message);
                    writer.Flush();
                }
            }
            catch (IOException ioEx)
            {
                Console.WriteLine($"IO Error writing to log file '{_logFilePath}': {ioEx.GetType().Name} - {ioEx.Message}");
            }
            catch (UnauthorizedAccessException authEx)
            {
                Console.WriteLine($"Authorization Error writing to log file '{_logFilePath}': {authEx.GetType().Name} - {authEx.Message}. Check write permissions.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Generic error writing to log file '{_logFilePath}': {ex.GetType().Name} - {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }
    }
}