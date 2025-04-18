using System.Text;

namespace UiPathDisplayNameGuidTool
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            // デバッグ用のコード
            // デフォルトのディレクトリパスを設定
            string[] debugArgs = new string[] { @"C:\Users\SeiyaKawashima\OneDrive - AutoFor\ドキュメント\UiPath\!AutoFor会計" };
            ProcessFiles(debugArgs);
#else
            // 本番用のコード
            try
            {
                // 本番時は引数が必須
                if (args.Length == 0)
                {
                    throw new ArgumentException("対象ディレクトリを指定してください。使用方法: UiPathDisplayNameGuidTool <フォルダパス>");
                }
                ProcessFiles(args);
            }
            catch (Exception ex)
            {
                // エラーメッセージとスタックトレースを表示
                Console.WriteLine($"予期せぬエラーが発生しました: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
#endif
        }

        private static void ProcessFiles(string[] args)
        {
            // 引数から対象ディレクトリを取得
            string targetDirectory = args[0];

            // ログを保存するディレクトリのパスを設定
            string logDirectory = Path.Combine(Environment.CurrentDirectory, "Logs");
            // ロガーを初期化
            var logger = new Logger(logDirectory);

            // 処理開始のログを出力
            logger.LogInfo("処理を開始します");
            // 対象ディレクトリのログを出力
            logger.LogInfo($"対象ディレクトリ: {targetDirectory}");

            // 指定されたディレクトリが存在するかチェック
            if (!Directory.Exists(targetDirectory))
            {
                // ディレクトリが存在しない場合のエラーログを出力
                logger.LogError($"指定されたディレクトリが存在しません: {targetDirectory}");
                return;
            }

            // バックアップ管理クラスを初期化
            var backupManager = new BackupManager(logger);
            // GUID生成クラスを初期化
            var guidGenerator = new GuidGenerator(logger);
            // XAMLファイル処理クラスを初期化
            var xamlProcessor = new XamlProcessor(logger, guidGenerator);

            // バックアップディレクトリを作成
            string backupDirectory = backupManager.CreateBackupDirectory(targetDirectory);
            // バックアップの作成を実行
            if (!backupManager.CopyFilesToBackup(targetDirectory, backupDirectory))
            {
                // バックアップ作成失敗のエラーログを出力
                logger.LogError("バックアップの作成に失敗しました");
                return;
            }

            // XAMLファイルを検索
            var xamlFiles = xamlProcessor.FindXamlFiles(targetDirectory);
            // 処理成功したファイル数をカウント
            int processedFiles = 0;
            // 処理失敗したファイル数をカウント
            int failedFiles = 0;

            // 各XAMLファイルに対して処理を実行
            foreach (string file in xamlFiles)
            {
                // ファイルの処理が成功した場合
                if (xamlProcessor.ProcessXamlFile(file))
                {
                    // 成功カウンタをインクリメント
                    processedFiles++;
                }
                else
                {
                    // 失敗カウンタをインクリメント
                    failedFiles++;
                }
            }

            // 処理結果のログを出力
            logger.LogInfo($"処理が完了しました");
            logger.LogInfo($"処理対象ファイル数: {xamlFiles.Count}");
            logger.LogInfo($"正常に処理したファイル数: {processedFiles}");
            logger.LogInfo($"処理に失敗したファイル数: {failedFiles}");
            logger.LogInfo($"バックアップディレクトリ: {backupDirectory}");
            logger.LogInfo($"ログファイル: {Path.Combine(logDirectory, Path.GetFileName(logger.GetLogFilePath()))}");
        }
    }
}
