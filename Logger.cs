using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Input;

namespace Romanenko_FSE_lab11_2_a
{
    enum OperationType
    {
        Delete,
        Add
    }

    internal sealed class Logger
    {
        private static readonly Lazy<Logger> _instance = new Lazy<Logger>(() => new Logger());
        private static string _filePath = "/media/yaroslav/Docs R.Y/Docs 2 course, 2 semester/Fundamentals of Software Engineering/";
        private DateTime _recordDateTime;
        private int _lineNumber = 0;
        private int _symbolNumber = 0;
        private OperationType _operationType;
        private static TextBox? txtSource = null;

        private Logger()
        {
            try
            {
                if (!Directory.Exists(_filePath))
                {
                    Directory.CreateDirectory(_filePath);
                    Console.WriteLine($"Created log directory: {_filePath}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"FATAL: could not create log directory '{_filePath}'. Logging disabled. Error: {e.Message}");
            }
        }

        public static Logger Instance { get { return _instance.Value; } }
        public static string FilePath { get { return _filePath + "Log.txt"; } }
        public static TextBox SetTxtSource { set { txtSource = value; } }

        public void OnTextInput(object? sender, TextInputEventArgs e)
        {
            if (_filePath is null) return;

            Console.WriteLine("Event OnTextInput");
            if (sender is not TextBox txtSource || txtSource.IsReadOnly == true)
                return;


            string inputText = e.Text ?? string.Empty;
            if (string.IsNullOrEmpty(inputText))
                return;

            var text = txtSource.Text ?? string.Empty;
            var caretIndex = txtSource.CaretIndex;

            var textUpToCaret = text.Substring(0, caretIndex);
            _lineNumber = textUpToCaret.Count(c => c == '\n');

            var lastNewlinePos = textUpToCaret.LastIndexOf('\n');
            _symbolNumber = lastNewlinePos >= 0 ? caretIndex - lastNewlinePos - 1 : caretIndex;

            _recordDateTime = DateTime.Now;
            _operationType = OperationType.Add;

            string logMessage = $"{_recordDateTime:yyyy-MM-dd HH:mm:ss.fff} | ADD | Char: '{inputText.Replace("\r", "\\r").Replace("\n", "\\n")}' | Pos: L{_lineNumber + 1}, C{_symbolNumber + 1}";
            WriteLog(logMessage);
            Console.WriteLine($"Logged: {logMessage}");
        }

        public void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (_filePath is null) return;

            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                Console.WriteLine($"Event OnKeyDown triggered: {e.Key}");
                if (sender is not TextBox txtSource || txtSource.IsReadOnly)
                    return;

                var text = txtSource.Text ?? string.Empty;
                var caretIndex = txtSource.CaretIndex;
                var selectionStart = txtSource.SelectionStart;
                var selectionEnd = txtSource.SelectionEnd;

                if (selectionStart != selectionEnd)
                {
                    var textUpToSelection = text.Substring(0, selectionStart);
                    _lineNumber = textUpToSelection.Count(c => c == '\n');
                    var lastNewlinePos = textUpToSelection.LastIndexOf('\n');
                    _symbolNumber = (lastNewlinePos >= 0) ? selectionStart - (lastNewlinePos + 1) : selectionStart;

                    _recordDateTime = DateTime.Now;
                    _operationType = OperationType.Delete;
                    string deletedText = text.Substring(selectionStart, selectionEnd - selectionStart);
                    string logMessage = $"{_recordDateTime:yyyy-MM-dd HH:mm:ss.fff} | DELETE | Selection: '{deletedText.Replace("\r", "\\r").Replace("\n", "\\n")}' | StartPos: L{_lineNumber + 1}, C{_symbolNumber + 1}";
                    WriteLog(logMessage);
                    Console.WriteLine($"Logged: {logMessage}");
                }
                else
                {
                    if (e.Key == Key.Back && caretIndex > 0)
                    {
                        var textUpToCaret = text.Substring(0, caretIndex - 1);
                        _lineNumber = textUpToCaret.Count(c => c == '\n');
                        var lastNewlinePos = textUpToCaret.LastIndexOf('\n');
                        _symbolNumber = (lastNewlinePos >= 0) ? (caretIndex - 1) - (lastNewlinePos + 1) : (caretIndex - 1);

                        _recordDateTime = DateTime.Now;
                        _operationType = OperationType.Delete;
                        string logMessage = $"{_recordDateTime:yyyy-MM-dd HH:mm:ss.fff} | DELETE | Key: {e.Key} | Pos: L{_lineNumber + 1}, C{_symbolNumber + 1}";
                        WriteLog(logMessage);
                        Console.WriteLine($"Logged: {logMessage}");
                    }
                    else if (e.Key == Key.Delete && caretIndex < text.Length)
                    {
                        var textUpToCaret = text.Substring(0, caretIndex);
                        _lineNumber = textUpToCaret.Count(c => c == '\n');
                        var lastNewlinePos = textUpToCaret.LastIndexOf('\n');
                        _symbolNumber = (lastNewlinePos >= 0) ? caretIndex - (lastNewlinePos + 1) : caretIndex;

                        _recordDateTime = DateTime.Now;
                        _operationType = OperationType.Delete;
                        string logMessage = $"{_recordDateTime:yyyy-MM-dd HH:mm:ss.fff} | DELETE | Key: {e.Key} | Pos: L{_lineNumber + 1}, C{_symbolNumber + 1}";
                        WriteLog(logMessage);
                        Console.WriteLine($"Logged: {logMessage}");
                    }
                    else
                    {
                        Console.WriteLine($"OnKeyDown: {e.Key} pressed but no character to delete at caret index {caretIndex}.");
                    }
                }
            }
        }
        private void WriteLog(string message)
        {
            if (_filePath is null) return;
            try
            {
                using (StreamWriter writer = new StreamWriter(_filePath, append: true, System.Text.Encoding.UTF8))
                {
                    writer.WriteLine(message);
                }
            }
            catch (IOException ioEx)
            {
                Console.WriteLine($"Error writing to log file '{_filePath}': {ioEx.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Generic error writing to log: {e.Message}");
            }
        }
    }
}