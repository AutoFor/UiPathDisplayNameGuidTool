using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UiPathDisplayNameGuidTool
{
    /// <summary>
    /// GUIDを生成するクラス
    /// </summary>
    public class GuidGenerator
    {
        private readonly Logger _logger;
        private readonly Dictionary<string, HashSet<string>> _generatedGuidsByType;
        private readonly object _lockObject = new object();

        public GuidGenerator(Logger logger)
        {
            _logger = logger;
            _generatedGuidsByType = new Dictionary<string, HashSet<string>>();
        }

        /// <summary>
        /// 新しいGUIDを生成します
        /// </summary>
        /// <param name="activityType">アクティビティタイプ</param>
        /// <param name="position">アクティビティの位置情報</param>
        /// <returns>生成されたGUID</returns>
        public string GenerateGuid(string activityType, string position)
        {
            lock (_lockObject)
            {
                // アクティビティタイプごとのGUIDセットを取得または作成
                if (!_generatedGuidsByType.ContainsKey(activityType))
                {
                    _generatedGuidsByType[activityType] = new HashSet<string>();
                }

                string guid;
                int retryCount = 0;
                const int maxRetries = 10;

                do
                {
                    // アクティビティタイプと位置情報を組み合わせてGUIDを生成
                    string seed = $"{activityType}_{position}_{retryCount}";
                    byte[] seedBytes = System.Text.Encoding.UTF8.GetBytes(seed);
                    guid = Guid.NewGuid().ToString();

                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        throw new Exception($"GUIDの生成に失敗しました。アクティビティタイプ: {activityType}, 位置: {position}");
                    }
                } while (IsGuidAlreadyUsed(activityType, guid));

                _generatedGuidsByType[activityType].Add(guid);
                _logger.LogInfo($"新しいGUIDを生成: {guid} (アクティビティタイプ: {activityType}, 位置: {position})");
                return guid;
            }
        }

        /// <summary>
        /// 指定されたGUIDが既に使用されているかどうかを確認します
        /// </summary>
        /// <param name="activityType">アクティビティタイプ</param>
        /// <param name="guid">確認するGUID</param>
        /// <returns>GUIDが既に使用されている場合はtrue、そうでない場合はfalse</returns>
        private bool IsGuidAlreadyUsed(string activityType, string guid)
        {
            return _generatedGuidsByType.ContainsKey(activityType) && 
                   _generatedGuidsByType[activityType].Contains(guid);
        }

        /// <summary>
        /// 指定された文字列が有効なGUIDかどうかを確認します
        /// </summary>
        /// <param name="guid">確認する文字列</param>
        /// <returns>有効なGUIDの場合はtrue、そうでない場合はfalse</returns>
        public bool IsValidGuid(string guid)
        {
            return Guid.TryParse(guid, out _);
        }

        /// <summary>
        /// 指定されたDisplayNameに既にGUIDが付与されているかどうかを確認します
        /// </summary>
        /// <param name="displayName">確認するDisplayName</param>
        /// <returns>GUIDが付与されている場合はtrue、そうでない場合はfalse</returns>
        public bool IsGuidAlreadyAssigned(string displayName)
        {
            var pattern = @"\[([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})\]";
            return Regex.IsMatch(displayName, pattern);
        }
    }
} 