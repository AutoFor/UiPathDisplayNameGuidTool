using System.IO;

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
                string backupDirectory = GenerateBackupDirectoryName(sourceDirectory);
                Directory.CreateDirectory(backupDirectory);
                _logger.LogInfo($"バックアップディレクトリを作成: {backupDirectory}");
                return backupDirectory;
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
                // すべてのファイルとディレクトリを再帰的にコピー
                CopyDirectory(sourceDirectory, backupDirectory);
                _logger.LogInfo($"バックアップの作成が完了しました: {backupDirectory}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"バックアップの作成中にエラーが発生しました: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// ディレクトリを再帰的にコピーします
        /// </summary>
        /// <param name="sourceDir">元のディレクトリパス</param>
        /// <param name="destDir">コピー先のディレクトリパス</param>
        private void CopyDirectory(string sourceDir, string destDir)
        {
            // コピー先ディレクトリが存在しない場合は作成
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            // すべてのファイルをコピー
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
                _logger.LogInfo($"ファイルをコピー: {file} -> {destFile}");
            }

            // すべてのサブディレクトリを再帰的にコピー
            foreach (string dir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
                CopyDirectory(dir, destSubDir);
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
            return Path.Combine(Path.GetDirectoryName(sourceDirectory) ?? string.Empty, $"{directoryName}_backup_{timestamp}");
        }
    }
} 