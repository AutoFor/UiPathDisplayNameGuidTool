namespace UiPathDisplayNameGuidTool
{
    /// <summary>
    /// バックアップを管理するクラス
    /// </summary>
    public class BackupManager
    {
        private readonly Logger _logger;

        public BackupManager(Logger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// バックアップディレクトリを作成します
        /// </summary>
        /// <param name="sourceDirectory">元のディレクトリパス</param>
        /// <returns>作成されたバックアップディレクトリのパス</returns>
        public string CreateBackupDirectory(string sourceDirectory)
        {
            try
            {
                string backupDirName = GenerateBackupDirectoryName(sourceDirectory);
                string backupPath = Path.Combine(Path.GetDirectoryName(sourceDirectory)!, backupDirName);

                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                    _logger.LogInfo($"バックアップディレクトリを作成: {backupPath}");
                }

                return backupPath;
            }
            catch (Exception ex)
            {
                _logger.LogError($"バックアップディレクトリの作成中にエラーが発生しました: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// ファイルをバックアップディレクトリにコピーします
        /// </summary>
        /// <param name="sourceDirectory">元のディレクトリパス</param>
        /// <param name="backupDirectory">バックアップディレクトリのパス</param>
        /// <returns>コピーが成功したかどうか</returns>
        public bool CopyFilesToBackup(string sourceDirectory, string backupDirectory)
        {
            try
            {
                // 元のディレクトリ内の全XAMLファイルを取得
                var xamlFiles = Directory.GetFiles(sourceDirectory, "*.xaml", SearchOption.AllDirectories);

                foreach (string file in xamlFiles)
                {
                    // バックアップ先のパスを生成
                    string relativePath = Path.GetRelativePath(sourceDirectory, file);
                    string backupPath = Path.Combine(backupDirectory, relativePath);

                    // バックアップ先のディレクトリが存在しない場合は作成
                    string backupDir = Path.GetDirectoryName(backupPath)!;
                    if (!Directory.Exists(backupDir))
                    {
                        Directory.CreateDirectory(backupDir);
                    }

                    // ファイルをコピー
                    File.Copy(file, backupPath, true);
                    _logger.LogInfo($"ファイルをバックアップ: {file} -> {backupPath}");
                }

                _logger.LogInfo($"バックアップが完了しました。合計 {xamlFiles.Length} ファイルをコピーしました。");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"バックアップ中にエラーが発生しました: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// バックアップディレクトリ名を生成します
        /// </summary>
        /// <param name="sourceDirectory">元のディレクトリパス</param>
        /// <returns>生成されたバックアップディレクトリ名</returns>
        public string GenerateBackupDirectoryName(string sourceDirectory)
        {
            string directoryName = Path.GetFileName(sourceDirectory);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return $"{directoryName}_backup_{timestamp}";
        }
    }
} 