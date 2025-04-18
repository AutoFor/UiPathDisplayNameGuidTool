using System.Text.RegularExpressions;

namespace UiPathDisplayNameGuidTool
{
    /// <summary>
    /// GUIDを生成するクラス
    /// </summary>
    public class GuidGenerator
    {
        private readonly Logger _logger;
        private readonly HashSet<string> _generatedGuids = new();

        public GuidGenerator(Logger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 新しいGUIDを生成します
        /// </summary>
        /// <returns>生成されたGUID</returns>
        public string GenerateGuid()
        {
            string guid = Guid.NewGuid().ToString();
            _generatedGuids.Add(guid);
            _logger.LogInfo($"新しいGUIDを生成: {guid}");
            return guid;
        }

        /// <summary>
        /// 指定された文字列が有効なGUIDかどうかをチェックします
        /// </summary>
        /// <param name="guid">チェックするGUID文字列</param>
        /// <returns>有効なGUIDの場合はtrue、そうでない場合はfalse</returns>
        public bool IsValidGuid(string guid)
        {
            return Guid.TryParse(guid, out _);
        }

        /// <summary>
        /// 指定されたDisplayNameに既にGUIDが付与されているかどうかをチェックします
        /// </summary>
        /// <param name="displayName">チェックするDisplayName</param>
        /// <returns>GUIDが付与されている場合はtrue、そうでない場合はfalse</returns>
        public bool IsGuidAlreadyAssigned(string displayName)
        {
            // DisplayNameの末尾にGUIDが付与されているかチェック
            var pattern = @"\[([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})\]";
            var match = Regex.Match(displayName, pattern);
            
            if (match.Success)
            {
                string guid = match.Groups[1].Value;
                if (IsValidGuid(guid))
                {
                    _logger.LogInfo($"DisplayNameに既にGUIDが付与されています: {displayName}");
                    return true;
                }
            }

            return false;
        }
    }
} 