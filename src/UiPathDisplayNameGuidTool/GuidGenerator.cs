using System.Text.RegularExpressions;

namespace UiPathDisplayNameGuidTool
{
    /// <summary>
    /// GUIDを生成するクラス
    /// </summary>
    public class GuidGenerator
    {
        private readonly Logger _logger;
        private readonly Dictionary<string, HashSet<string>> _generatedGuidsByType = new();

        public GuidGenerator(Logger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 新しいGUIDを生成します
        /// </summary>
        /// <param name="activityType">アクティビティのタイプ</param>
        /// <returns>生成されたGUID</returns>
        public string GenerateGuid(string activityType)
        {
            string guid;
            do
            {
                guid = Guid.NewGuid().ToString();
            } while (IsGuidAlreadyUsed(activityType, guid));

            if (!_generatedGuidsByType.ContainsKey(activityType))
            {
                _generatedGuidsByType[activityType] = new HashSet<string>();
            }
            _generatedGuidsByType[activityType].Add(guid);
            _logger.LogInfo($"新しいGUIDを生成: {guid} (アクティビティタイプ: {activityType})");
            return guid;
        }

        /// <summary>
        /// 指定されたGUIDが既に使用されているかどうかをチェックします
        /// </summary>
        /// <param name="activityType">アクティビティのタイプ</param>
        /// <param name="guid">チェックするGUID</param>
        /// <returns>使用済みの場合はtrue、そうでない場合はfalse</returns>
        private bool IsGuidAlreadyUsed(string activityType, string guid)
        {
            return _generatedGuidsByType.ContainsKey(activityType) && 
                   _generatedGuidsByType[activityType].Contains(guid);
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