using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace UiPathDisplayNameGuidTool
{
    /// <summary>
    /// XAMLファイルを処理するクラス
    /// </summary>
    public class XamlProcessor
    {
        private readonly Logger _logger;
        private readonly GuidGenerator _guidGenerator;

        public XamlProcessor(Logger logger, GuidGenerator guidGenerator)
        {
            _logger = logger;
            _guidGenerator = guidGenerator;
        }

        /// <summary>
        /// 指定されたディレクトリ内のXAMLファイルを再帰的に検索します
        /// </summary>
        /// <param name="directory">検索対象のディレクトリパス</param>
        /// <returns>見つかったXAMLファイルのパスのリスト</returns>
        public List<string> FindXamlFiles(string directory)
        {
            var xamlFiles = new List<string>();
            try
            {
                // 指定されたディレクトリ内のXAMLファイルを検索
                xamlFiles.AddRange(Directory.GetFiles(directory, "*.xaml", SearchOption.AllDirectories));
                _logger.LogInfo($"検出されたXAMLファイル数: {xamlFiles.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"XAMLファイルの検索中にエラーが発生しました: {ex.Message}");
                throw;
            }
            return xamlFiles;
        }

        /// <summary>
        /// XAMLファイルを処理し、DisplayNameにGUIDを付与します
        /// </summary>
        /// <param name="filePath">処理対象のXAMLファイルパス</param>
        /// <returns>処理が成功したかどうか</returns>
        public bool ProcessXamlFile(string filePath)
        {
            try
            {
                // ファイルの内容を読み込む
                string content = File.ReadAllText(filePath);

                // DisplayNameのパターンを検索
                var pattern = @"DisplayName\s*=\s*""([^""]*)""";
                var matches = Regex.Matches(content, pattern);

                if (matches.Count == 0)
                {
                    _logger.LogInfo($"ファイル {filePath} にはDisplayNameが含まれていません");
                    return true;
                }

                bool hasChanges = false;
                foreach (Match match in matches)
                {
                    string displayName = match.Groups[1].Value;

                    // 既にGUIDが付与されている場合はスキップ
                    if (_guidGenerator.IsGuidAlreadyAssigned(displayName))
                    {
                        continue;
                    }

                    // 新しいGUIDを生成
                    string guid = _guidGenerator.GenerateGuid();

                    // DisplayNameを置換
                    string newDisplayName = $"{displayName} [{guid}]";
                    content = ReplaceDisplayName(content, displayName, newDisplayName);
                    hasChanges = true;

                    _logger.LogInfo($"DisplayNameを更新: {displayName} -> {newDisplayName}");
                }

                if (hasChanges)
                {
                    // 変更をファイルに保存
                    File.WriteAllText(filePath, content);
                    _logger.LogInfo($"ファイル {filePath} を更新しました");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ファイル {filePath} の処理中にエラーが発生しました: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// DisplayNameの値を置換します
        /// </summary>
        /// <param name="content">XAMLファイルの内容</param>
        /// <param name="oldDisplayName">置換前のDisplayName</param>
        /// <param name="newDisplayName">置換後のDisplayName</param>
        /// <returns>置換後のXAMLファイルの内容</returns>
        private string ReplaceDisplayName(string content, string oldDisplayName, string newDisplayName)
        {
            var pattern = $@"DisplayName\s*=\s*""{Regex.Escape(oldDisplayName)}""";
            var replacement = $"DisplayName = \"{newDisplayName}\"";
            return Regex.Replace(content, pattern, replacement);
        }
    }
} 