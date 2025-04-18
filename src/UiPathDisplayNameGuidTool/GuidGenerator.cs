using System;
using System.Text.RegularExpressions;

namespace UiPathDisplayNameGuidTool
{
    /// <summary>
    /// GUIDを生成・管理するクラス
    /// </summary>
    public class GuidGenerator
    {
        private readonly Logger _logger;
        private readonly object _lockObject = new();

        public GuidGenerator(Logger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 新しいGUIDを生成します
        /// </summary>
        /// <param name="activityType">アクティビティタイプ</param>
        /// <param name="position">アクティビティの位置情報</param>
        /// <param name="displayName">アクティビティの表示名</param>
        /// <returns>生成されたGUID</returns>
        public string GenerateGuid(string activityType, string position, string displayName)
        {
            lock (_lockObject)
            {
                string guidString = Guid.NewGuid().ToString();
                _logger.LogInfo($"新しいGUIDを生成: {guidString} (アクティビティタイプ: {activityType}, 位置: {position}, 表示名: {displayName})");
                return guidString;
            }
        }

        /// <summary>
        /// 指定されたDisplayNameに既にGUIDが付与されているかどうかを確認します
        /// </summary>
        /// <param name="displayName">確認するDisplayName</param>
        /// <returns>DisplayNameにGUIDが付与されている場合はtrue、そうでない場合はfalse</returns>
        public bool IsGuidAlreadyAssigned(string displayName)
        {
            var pattern = @"\[[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\]$";
            return Regex.IsMatch(displayName, pattern);
        }
    }
} 