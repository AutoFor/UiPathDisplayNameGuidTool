namespace UiPathDisplayNameGuidTool
{
    /// <summary>
    /// ログを管理するクラス
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// ログファイルのパスを保持するフィールド
        /// </summary>
        private readonly string _logFilePath;
        /// <summary>
        /// ログ書き込み時の排他制御用オブジェクト
        /// </summary>
        private readonly object _lockObject = new();

        /// <summary>
        /// コンストラクタ（初期化処理）
        /// </summary>
        public Logger(string logDirectory)
        {
            /// <summary>
            /// ログディレクトリが存在しない場合の処理
            /// </summary>
            if (!Directory.Exists(logDirectory))
            {
                /// <summary>
                /// ログディレクトリを作成
                /// </summary>
                Directory.CreateDirectory(logDirectory);
            }

            /// <summary>
            /// 現在の日時を取得してログファイル名を生成
            /// </summary>
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            /// <summary>
            /// ログファイルの完全なパスを生成
            /// </summary>
            _logFilePath = Path.Combine(logDirectory, $"UiPathDisplayNameGuidTool_{timestamp}.log");
        }

        /// <summary>
        /// ログファイルのパスを取得するプロパティ
        /// </summary>
        public string LogFilePath => _logFilePath;

        /// <summary>
        /// 情報レベルのログを出力するメソッド
        /// </summary>
        /// <param name="message">ログメッセージ</param>
        public void LogInfo(string message)
        {
            /// <summary>
            /// ログを書き込む
            /// </summary>
            WriteLog("INFO", message);
        }

        /// <summary>
        /// 警告レベルのログを出力するメソッド
        /// </summary>
        /// <param name="message">ログメッセージ</param>
        public void LogWarning(string message)
        {
            /// <summary>
            /// ログを書き込む
            /// </summary>
            WriteLog("WARNING", message);
        }

        /// <summary>
        /// エラーレベルのログを出力するメソッド
        /// </summary>
        /// <param name="message">ログメッセージ</param>
        public void LogError(string message)
        {
            /// <summary>
            /// ログを書き込む
            /// </summary>
            WriteLog("ERROR", message);
        }

        /// <summary>
        /// 実際にログを書き込むプライベートメソッド
        /// </summary>
        private void WriteLog(string level, string message)
        {
            /// <summary>
            /// ログメッセージのフォーマットを設定
            /// </summary>
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";

            /// <summary>
            /// コンソールにログを出力
            /// </summary>
            Console.WriteLine(logMessage);

            /// <summary>
            /// ファイルへの書き込みを排他制御
            /// </summary>
            lock (_lockObject)
            {
                /// <summary>
                /// ログファイルにメッセージを追加
                /// </summary>
                File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
            }
        }

        /// <summary>
        /// ログファイルのパスを取得するメソッド
        /// </summary>
        /// <returns>ログファイルのパス</returns>
        public string GetLogFilePath()
        {
            return _logFilePath;
        }
    }
} 