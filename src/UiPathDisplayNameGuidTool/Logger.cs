using System;
using System.IO;
using System.Text;

namespace UiPathDisplayNameGuidTool
{
    /// <summary>
    /// ログを管理するクラス
    /// 2種類のログファイルを作成します：
    /// 1. guid_changes.csv - DisplayNameの変更内容を記録
    /// 2. guid_changes.log - 処理の進行状況を記録
    /// </summary>
    public class Logger
    {
        // CSVログファイルのパスを保持する変数
        private readonly string _csvLogFilePath;
        // テキストログファイルのパスを保持する変数
        private readonly string _textLogFilePath;
        // 複数のスレッドから同時にログを書き込むのを防ぐためのロックオブジェクト
        private readonly object _lockObject = new();
        // CSVログの内容を一時的に保持する変数
        private readonly StringBuilder _csvLogContent = new();
        // テキストログの内容を一時的に保持する変数
        private readonly StringBuilder _textLogContent = new();

        /// <summary>
        /// コンストラクタ - ログファイルの初期設定を行います
        /// </summary>
        /// <param name="targetDirectory">ログファイルを保存するディレクトリのパス</param>
        public Logger(string targetDirectory)
        {
            // CSVログファイルの設定
            // ファイル名を設定
            string csvLogFileName = "guid_changes.csv";
            // 保存先のフルパスを作成
            _csvLogFilePath = Path.Combine(targetDirectory, csvLogFileName);
            // CSVのヘッダー行を追加
            _csvLogContent.AppendLine("FileName,Before,After");

            // テキストログファイルの設定
            // ファイル名を設定
            string textLogFileName = "guid_changes.log";
            // 保存先のフルパスを作成
            _textLogFilePath = Path.Combine(targetDirectory, textLogFileName);
        }

        /// <summary>
        /// DisplayNameの変更内容をCSVログに記録します
        /// </summary>
        /// <param name="filePath">変更されたファイルのパス</param>
        /// <param name="before">変更前のDisplayName</param>
        /// <param name="after">変更後のDisplayName</param>
        public void LogChange(string filePath, string before, string after)
        {
            // 複数のスレッドから同時に書き込むのを防ぐ
            lock (_lockObject)
            {
                // CSV形式でログ行を作成
                // 例: "Framework/Test.xaml","元の表示名","新しい表示名 [GUID]"
                string csvLogLine = $"\"{filePath}\",\"{before}\",\"{after}\"";
                // ログ内容に追加
                _csvLogContent.AppendLine(csvLogLine);
            }
        }

        /// <summary>
        /// 情報レベルのログを記録します
        /// </summary>
        /// <param name="message">ログメッセージ</param>
        public void LogInfo(string message)
        {
            // タイムスタンプ付きのログ行を作成
            string logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [INFO] {message}";
            // コンソールに出力
            Console.WriteLine(logLine);
            // テキストログに追加
            _textLogContent.AppendLine(logLine);
        }

        /// <summary>
        /// エラーレベルのログを記録します
        /// </summary>
        /// <param name="message">ログメッセージ</param>
        public void LogError(string message)
        {
            // タイムスタンプ付きのログ行を作成
            string logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [ERROR] {message}";
            // コンソールに出力
            Console.WriteLine(logLine);
            // テキストログに追加
            _textLogContent.AppendLine(logLine);
        }

        /// <summary>
        /// 警告レベルのログを記録します
        /// </summary>
        /// <param name="message">ログメッセージ</param>
        public void LogWarning(string message)
        {
            // タイムスタンプ付きのログ行を作成
            string logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [WARNING] {message}";
            // コンソールに出力
            Console.WriteLine(logLine);
            // テキストログに追加
            _textLogContent.AppendLine(logLine);
        }

        /// <summary>
        /// ログファイルを保存します
        /// </summary>
        public void SaveLogToFile()
        {
            // 複数のスレッドから同時に書き込むのを防ぐ
            lock (_lockObject)
            {
                try
                {
                    // CSVログをファイルに保存
                    File.WriteAllText(_csvLogFilePath, _csvLogContent.ToString());
                    // テキストログをファイルに保存
                    File.WriteAllText(_textLogFilePath, _textLogContent.ToString());
                }
                catch (Exception ex)
                {
                    // エラーが発生した場合はコンソールにエラーメッセージを出力
                    Console.WriteLine($"ログファイルの保存中にエラーが発生しました: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// CSVログファイルのパスを取得します
        /// </summary>
        /// <returns>CSVログファイルのパス</returns>
        public string GetLogFilePath()
        {
            return _csvLogFilePath;
        }
    }
} 