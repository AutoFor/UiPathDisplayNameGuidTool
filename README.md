# UiPath XAML DisplayName GUID Appender

UiPathのxamlファイル内のDisplayNameプロパティに一意のGUIDを付与するツールです。

## 機能概要

- 指定されたディレクトリ内のすべてのXAMLファイルを検索
- 各XAMLファイル内のDisplayNameプロパティにGUIDを付加
- 処理前のバックアップを自動的に作成
- 処理結果をログファイルに記録

## 使用方法

### 本番環境での使用

```bash
UiPathDisplayNameGuidTool.exe <フォルダパス>
```

例：
```bash
UiPathDisplayNameGuidTool.exe "C:\UiPath\Projects\MyProject"
```

### デバッグ環境での使用

デバッグ環境では、デフォルトのディレクトリパスが使用されます。
コマンドライン引数は無視されます。

## 出力例

変更前：
```xml
<Activity DisplayName="ファイルを開く">
```

変更後：
```xml
<Activity DisplayName="ファイルを開く [550e8400-e29b-41d4-a716-446655440000]">
```

## 機能詳細

### バックアップ機能
- 処理前に元のフォルダのバックアップを作成
- バックアップフォルダ名：`[フォルダ名]_backup_YYYYMMDD_HHMMSS`

### ログ機能
- 処理結果をログファイルに記録
- ログファイル名：`UiPathDisplayNameGuidTool_YYYYMMDD_HHMMSS.log`
- ログ内容：
  - 処理開始・終了時刻
  - 処理対象ファイル数
  - 成功・失敗ファイル数
  - バックアップディレクトリパス

### エラーハンドリング
- 本番環境では適切なエラーメッセージを表示
- デバッグ環境では詳細なエラー情報を出力

## 開発環境

- Visual Studio 2022
- .NET 7.0
- C#

## フォルダ構成

```
root/
├── docs/
│   └── specification.md     # 詳細仕様書
├── src/
│   └── UiPathDisplayNameGuidTool/
│       ├── Program.cs           # メインプログラム
│       ├── BackupManager.cs     # バックアップ管理
│       ├── GuidGenerator.cs     # GUID生成
│       ├── Logger.cs            # ログ管理
│       └── XamlProcessor.cs     # XAML処理
├── .gitignore
└── README.md                # このファイル

```

## ライセンス

このプロジェクトはMITライセンスの下で公開されています。 