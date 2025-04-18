using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

namespace UiPathDisplayNameGuidTool
{
    /// <summary>
    /// GUIDを生成・管理するクラス
    /// </summary>
    public class GuidGenerator
    {
        private readonly Logger _logger;
        private readonly HashSet<string> _generatedGuids = new();
        private readonly Dictionary<string, string> _activityTypeToGuid = new();
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
                // アクティビティタイプと位置情報、表示名を組み合わせてシードを生成
                string seed = $"{activityType}_{position}_{displayName}";
                byte[] seedBytes = Encoding.UTF8.GetBytes(seed);
                int seedValue = BitConverter.ToInt32(seedBytes, 0);

                // シード値を使用してGUIDを生成
                Random random = new(seedValue);
                byte[] guidBytes = new byte[16];
                random.NextBytes(guidBytes);

                // GUIDのバージョンとバリアントを設定
                guidBytes[7] = (byte)((guidBytes[7] & 0x0F) | 0x40); // バージョン4
                guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80); // バリアント1

                Guid guid = new(guidBytes);
                string guidString = guid.ToString();

                // 重複チェックと再試行
                int retryCount = 0;
                while (_generatedGuids.Contains(guidString) && retryCount < 10)
                {
                    guidBytes[0] = (byte)random.Next(256);
                    guid = new(guidBytes);
                    guidString = guid.ToString();
                    retryCount++;
                }

                if (retryCount >= 10)
                {
                    _logger.LogError($"GUIDの生成に失敗しました。アクティビティタイプ: {activityType}, 位置: {position}, 表示名: {displayName}");
                    throw new Exception("GUIDの生成に失敗しました。");
                }

                _generatedGuids.Add(guidString);
                _activityTypeToGuid[activityType] = guidString;

                _logger.LogInfo($"新しいGUIDを生成: {guidString} (アクティビティタイプ: {activityType}, 位置: {position}, 表示名: {displayName})");
                return guidString;
            }
        }

        /// <summary>
        /// 指定されたGUIDが既に使用されているかどうかを確認します
        /// </summary>
        /// <param name="guid">確認するGUID</param>
        /// <returns>GUIDが既に使用されている場合はtrue、そうでない場合はfalse</returns>
        public bool IsGuidAlreadyUsed(string guid)
        {
            return _generatedGuids.Contains(guid);
        }

        /// <summary>
        /// 指定されたGUIDが有効な形式かどうかを確認します
        /// </summary>
        /// <param name="guid">確認するGUID</param>
        /// <returns>GUIDが有効な形式の場合はtrue、そうでない場合はfalse</returns>
        public bool IsValidGuid(string guid)
        {
            return Guid.TryParse(guid, out _);
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