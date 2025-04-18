# UiPath DisplayName GUID付与ツール 仕様書

## 1. 文書情報
### 1.1 文書管理情報
// この仕様書の管理情報です。文書のバージョン管理に使用します。
- 文書番号: SPEC-2024-001
- 版数: 1.0
- 作成日: 2024年3月1日
- 最終更新日: 2024年3月1日
- 作成者: システム開発部

### 1.2 変更履歴
// この仕様書の変更履歴を記録します。いつ、誰が、何を変更したかを管理します。
| 版数 | 変更日 | 変更内容 | 変更者 |
|------|--------|----------|--------|
| 1.0  | 2024/3/1 | 初版作成 | システム開発部 |

## 2. 概要
### 2.1 目的
// このツールが何のために作られるのかを説明します。
本ツールは、UiPathのxamlファイル内のDisplayNameプロパティに一意のGUIDを付与することを目的とする。

### 2.2 適用範囲
// このツールがどのような環境で使用されるかを説明します。
- UiPath Studioで作成されたxamlファイル
- Windows環境での実行

### 2.3 用語定義
// この仕様書で使用される専門用語の意味を説明します。
| 用語 | 定義 |
|------|------|
| GUID | グローバル一意識別子。RFC 4122に準拠した32桁の16進数 |
| DisplayName | UiPathのアクティビティに設定される表示名プロパティ |

## 3. 機能要件
### 3.1 基本機能
// このツールが持つべき基本的な機能を説明します。
1. ファイル検索機能
   - ユーザーが指定したフォルダパス内のxamlファイルを再帰的に検索
   - 検索条件：拡張子が.xamlのファイル

2. DisplayName検出機能
   - 各xamlファイル内の`DisplayName = "任意の文字列"`形式の文字列を検出
   - 例：`DisplayName = "ファイルを開く"`、`DisplayName = "エクセルデータを読み込む"`

3. GUID付与機能
   - 検出したDisplayNameの末尾に一意のGUIDを付与
   - 形式：`DisplayName = "任意の文字列 [GUID]"`
   - 既にGUIDが付与されている場合はスキップ

4. バックアップ機能
   - 処理前に元のフォルダの完全なバックアップを作成
   - バックアップには以下の内容が含まれます：
     - すべてのファイル（XAMLファイル以外も含む）
     - すべてのディレクトリ（.から始まる隠しディレクトリも含む）
     - 空のディレクトリも含む
   - バックアップフォルダ名：`[フォルダ名]_backup_YYYYMMDD_HHMMSS`
   - バックアップは元のフォルダと同じ階層に作成

### 3.2 処理フロー
// このツールがどのような順序で処理を行うかを説明します。
1. 入力
   - フォルダパスの指定（コマンドライン引数）

2. バックアップ作成
   - 元のフォルダと同じ階層にバックアップフォルダを作成
   - バックアップフォルダ名の形式：`[元のフォルダ名]_backup_YYYYMMDD_HHMMSS`
     - 例：`UiPathProject_backup_20240301_123456`
   - バックアップの内容：
     - すべてのファイル（XAMLファイル以外も含む）
     - すべてのディレクトリ（.から始まる隠しディレクトリも含む）
     - 空のディレクトリも含む
   - バックアップの作成方法：
     - ディレクトリ構造を維持
     - ファイルの属性（読み取り専用、隠しファイルなど）を保持
     - サブディレクトリも再帰的にコピー
   - エラー処理：
     - バックアップ作成中にエラーが発生した場合はログに記録
     - エラーの詳細（ファイルパス、エラーメッセージなど）を記録
     - エラーが発生した場合は処理を中断

3. ファイル処理
   - 指定フォルダ内の全xamlファイルを再帰的に検索
   - 各xamlファイルに対して以下を実行:
     a. ファイルを読み込み
     b. `DisplayName = "任意の文字列"`パターンを検索
     c. 各マッチに対して新しいGUIDを生成
     d. `DisplayName = "任意の文字列 [GUID]"`形式で置換
     e. 変更をファイルに保存

4. 出力
   - 処理結果のログ（変更したファイル数、変更箇所数、生成されたGUIDなど）
   - 変更されたxamlファイル

### 3.3 GUID生成の仕様

#### 3.3.1 GUIDの形式
- RFC 4122に準拠したバージョン4のGUIDを使用
- 形式: `xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx`
  - x: 16進数（0-9, a-f）
  - y: 8, 9, a, bのいずれか
- 例: `383d880c-be99-4de3-8717-a8ca9fa82b9c`

#### 3.3.2 GUIDの生成方法
1. 基本的な生成
   - .NET Frameworkの`Guid.NewGuid()`メソッドを使用
   - スレッドセーフな実装（ロックオブジェクトを使用）
   - アクティビティの情報（タイプ、位置、表示名）はログ記録のみに使用

2. 既存GUIDのチェック
   - DisplayNameに既にGUIDが付与されているかどうかを確認
   - 正規表現パターン: `\[[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\]$`
   - 既にGUIDが付与されている場合はスキップ

#### 3.3.3 ログ記録
1. GUID生成時のログ
   - 生成されたGUID
   - アクティビティタイプ
   - 位置情報
   - 表示名
   - 生成日時

2. エラー時のログ
   - エラーの種類
   - エラーが発生したアクティビティの情報
   - エラーの詳細
   - エラー発生日時

### 3.4 ディレクトリ指定
- コマンドライン引数で対象ディレクトリを指定可能
- デバッグモード（DEBUGビルド時）
  - 常にデフォルトのディレクトリパスを使用
    - パス: `C:\Users\SeiyaKawashima\OneDrive - AutoFor\ドキュメント\Cursor\UiPathDisplayName\UiPathDisplayNameGuidTool\UiPathDisplayNameGuidTool`
  - コマンドライン引数は無視される
- 本番モード（RELEASEビルド時）
  - 引数が必須
  - 引数が指定されていない場合はエラーを表示
    - エラーメッセージ: "対象ディレクトリを指定してください。使用方法: UiPathDisplayNameGuidTool <フォルダパス>"

## 4. 技術要件
### 4.1 開発環境
// このツールを開発するために必要な環境を説明します。
1. 開発ツール
   - Visual Studio 2022
   - .NET 7.0

2. 必要なNuGetパッケージ
   // NuGetパッケージとは、.NETで使用できるライブラリの集まりです。
   - `System.Xml.Linq`: XAMLファイル解析用
   - `System.IO`: ファイル操作用
   - `System.Text.RegularExpressions`: 正規表現処理用
   - `System.Guid`: GUID生成用

### 4.2 ソリューション構成
// プロジェクトのファイル構成を説明します。
```
UiPathDisplayNameGuidTool/
├── UiPathDisplayNameGuidTool.sln
├── UiPathDisplayNameGuidTool/
│   ├── Program.cs                 # メイン処理
│   ├── XamlProcessor.cs           # XAMLファイル処理クラス
│   ├── BackupManager.cs           # バックアップ管理クラス
│   ├── GuidGenerator.cs           # GUID生成クラス
│   ├── Logger.cs                  # ログ管理クラス
│   └── Properties/
│       └── AssemblyInfo.cs
├── UiPathDisplayNameGuidTool.Tests/
│   └── UnitTest1.cs
└── README.md
```

### 4.3 クラス仕様
// 各クラスの役割と機能を説明します。
#### 4.3.1 Program.cs
- 責務：アプリケーションのエントリーポイント
- 主要メソッド：
  - `Main`: コマンドライン引数の処理と実行モードの分岐
    - デバッグモード（DEBUGビルド時）
      - エラーハンドリングを行わない
      - エラー発生時は即座に例外をスロー
    - 本番モード（RELEASEビルド時）
      - エラーハンドリングを実装
      - エラー発生時はログに記録し、スタックトレースを出力
  - `ProcessFiles`: メイン処理フローの実装
    - ディレクトリパスの設定
    - ログの初期化
    - バックアップの作成
    - XAMLファイルの処理
    - 処理結果のログ出力

#### 4.3.2 XamlProcessor.cs
// XAMLファイルを処理するクラスです。
```csharp
public class XamlProcessor
{
    public List<string> FindXamlFiles(string directory);
    public bool ProcessXamlFile(string filePath);
    private string ReplaceDisplayName(string content, string displayName, string guid);
}
```

#### 4.3.3 BackupManager.cs
// バックアップを管理するクラスです。
```csharp
public class BackupManager
{
    public string CreateBackupDirectory(string sourceDirectory);
    public bool CopyFilesToBackup(string sourceDirectory, string backupDirectory);
    public string GenerateBackupDirectoryName(string sourceDirectory);
    private void CopyDirectory(string sourceDir, string destDir);
}
```

#### 4.3.4 GuidGenerator.cs
// GUIDを生成するクラスです。
```csharp
public class GuidGenerator
{
    public string GenerateGuid();
    public bool IsValidGuid(string guid);
    public bool IsGuidAlreadyAssigned(string displayName);
}
```

#### 4.3.5 Logger.cs
// ログを管理するクラスです。
```csharp
public class Logger
{
    public void LogInfo(string message);
    public void LogWarning(string message);
    public void LogError(string message);
    public void SaveLogToFile();
}
```

### 4.4 コーディング規約
// コードを書く際のルールを説明します。
1. 基本方針
   - C#コーディング規約に準拠
   - 型の明示的な指定
   - XMLドキュメントコメントの記述

2. 命名規則
   // 変数やクラスなどの名前の付け方のルールです。
   - クラス名：PascalCase
   - メソッド名：PascalCase
   - 変数名：camelCase
   - 定数：UPPER_CASE

3. コメント規約
   // コードにコメントを付ける際のルールです。
   - クラス、メソッドにはXMLドキュメントコメントを記述
   - 複雑なロジックには適切なコメントを記述

### 4.5 エラーハンドリング
// エラーが発生した場合の処理方法を説明します。
1. 例外処理
   - カスタム例外クラスの定義
   - エラーメッセージのリソース化
   - リカバリー処理の実装

2. ログ出力
   // エラー情報を記録する方法を説明します。
   - エラーレベルに応じたログ出力
   - ログファイルへの保存
   - エラー発生時のスタックトレース出力 

### 3.5 バックアップ機能の仕様

#### 3.5.1 バックアップの作成
1. バックアップフォルダの作成
   - 元のフォルダと同じ階層に作成
   - フォルダ名の形式：`[元のフォルダ名]_backup_YYYYMMDD_HHMMSS`
   - 例：`UiPathProject_backup_20240301_123456`

2. バックアップの内容
   - すべてのファイルとディレクトリをコピー
   - ファイルの属性を保持
   - ディレクトリ構造を維持
   - 空のディレクトリも含む

3. コピー処理の詳細
   - ファイルのコピー：
     - バイナリコピーを実行
     - ファイルの属性（読み取り専用、隠しファイルなど）を保持
     - コピー先に同名ファイルが存在する場合は上書き
   - ディレクトリのコピー：
     - サブディレクトリも再帰的にコピー
     - ディレクトリの属性を保持
     - 空のディレクトリも作成

#### 3.5.2 エラー処理
1. バックアップ作成時のエラー
   - エラーの種類：
     - ファイルアクセス権限エラー
     - ディスク容量不足
     - ファイルロック
     - その他のIOエラー
   - エラー発生時の処理：
     - エラーの詳細をログに記録
     - 処理を中断
     - ユーザーにエラー内容を通知

2. ログ記録
   - バックアップ開始時のログ：
     - バックアップ元のパス
     - バックアップ先のパス
     - 開始日時
   - バックアップ完了時のログ：
     - コピーしたファイル数
     - コピーしたディレクトリ数
     - 完了日時
   - エラー時のログ：
     - エラーの種類
     - エラーが発生したファイル/ディレクトリのパス
     - エラーの詳細
     - エラー発生日時

#### 3.5.3 バックアップの検証
1. バックアップの整合性チェック
   - コピーしたファイル数の確認
   - コピーしたディレクトリ数の確認
   - ファイルサイズの比較
   - 必要に応じてチェックサムの検証

2. バックアップの利用
   - 処理中にエラーが発生した場合：
     - バックアップから元の状態に復元可能
     - 復元手順の提供
   - 処理完了後の確認：
     - バックアップの保持期間の設定
     - 不要なバックアップの削除 