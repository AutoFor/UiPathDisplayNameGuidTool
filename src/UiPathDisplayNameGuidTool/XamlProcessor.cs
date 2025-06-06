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

                    // アクティビティタイプと位置情報を取得
                    string activityType = GetActivityType(content, match.Index);
                    string position = GetActivityPosition(content, match.Index);
                    string displayNameValue = GetDisplayNameValue(displayName);
                    
                    if (string.IsNullOrEmpty(activityType))
                    {
                        _logger.LogWarning($"アクティビティタイプを取得できませんでした。位置: {position}");
                        continue;
                    }

                    // 新しいGUIDを生成
                    string guid = _guidGenerator.GenerateGuid(activityType, position, displayNameValue);

                    // DisplayNameを置換
                    string newDisplayName = $"{displayName} [{guid}]";
                    content = ReplaceDisplayName(content, displayName, newDisplayName);
                    hasChanges = true;

                    // ログに変更を記録
                    _logger.LogChange(filePath, displayName, newDisplayName);
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
        /// アクティビティタイプを取得します
        /// </summary>
        /// <param name="content">XAMLファイルの内容</param>
        /// <param name="displayNameIndex">DisplayNameの位置</param>
        /// <returns>アクティビティタイプ</returns>
        private string GetActivityType(string content, int displayNameIndex)
        {
            try
            {
                // DisplayNameの前の部分を取得
                string beforeDisplayName = content.Substring(0, displayNameIndex);
                
                // 最後の開始タグを検索
                int lastOpenTagIndex = beforeDisplayName.LastIndexOf('<');
                if (lastOpenTagIndex == -1)
                {
                    return string.Empty;
                }

                // タグ名を取得
                string tagContent = beforeDisplayName.Substring(lastOpenTagIndex);
                var tagMatch = Regex.Match(tagContent, @"<([^\s>]+)");
                if (!tagMatch.Success)
                {
                    return string.Empty;
                }

                return tagMatch.Groups[1].Value;
            }
            catch (Exception ex)
            {
                _logger.LogError($"アクティビティタイプの取得中にエラーが発生しました: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// アクティビティの位置情報を取得します
        /// </summary>
        /// <param name="content">XAMLファイルの内容</param>
        /// <param name="displayNameIndex">DisplayNameの位置</param>
        /// <returns>アクティビティの位置情報</returns>
        private string GetActivityPosition(string content, int displayNameIndex)
        {
            try
            {
                // DisplayNameの前の部分を取得
                string beforeDisplayName = content.Substring(0, displayNameIndex);
                
                // 親アクティビティの位置を特定
                int parentStartIndex = beforeDisplayName.LastIndexOf("<Sequence");
                if (parentStartIndex == -1)
                {
                    parentStartIndex = beforeDisplayName.LastIndexOf("<Flowchart");
                }
                if (parentStartIndex == -1)
                {
                    parentStartIndex = beforeDisplayName.LastIndexOf("<State");
                }

                if (parentStartIndex == -1)
                {
                    return "root";
                }

                // 親アクティビティのIDを取得
                string parentContent = beforeDisplayName.Substring(parentStartIndex);
                var idMatch = Regex.Match(parentContent, @"sap2010:WorkflowViewState.IdRef=""([^""]+)""");
                if (!idMatch.Success)
                {
                    return "unknown";
                }

                // 親アクティビティの表示名を取得
                var displayNameMatch = Regex.Match(parentContent, @"DisplayName\s*=\s*""([^""]*)""");
                string parentDisplayName = displayNameMatch.Success ? displayNameMatch.Groups[1].Value : "unnamed";

                return $"{idMatch.Groups[1].Value}_{parentDisplayName}";
            }
            catch (Exception ex)
            {
                _logger.LogError($"アクティビティの位置情報の取得中にエラーが発生しました: {ex.Message}");
                return "error";
            }
        }

        /// <summary>
        /// DisplayNameの値を取得します
        /// </summary>
        /// <param name="displayName">DisplayNameの文字列</param>
        /// <returns>DisplayNameの値</returns>
        private string GetDisplayNameValue(string displayName)
        {
            try
            {
                // DisplayNameからGUIDを除去
                var pattern = @"\s*\[[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\]$";
                return Regex.Replace(displayName, pattern, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError($"DisplayNameの値の取得中にエラーが発生しました: {ex.Message}");
                return displayName;
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