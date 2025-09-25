# otak-agent

クラシックなAgentTalkフローティングアシスタントを、ネイティブ依存関係のない単一の.NET 10 WinFormsアプリケーションに最新化しました。

## 主な特徴
- .NET 10 RC1の最新機能とパフォーマンス改善を活用する`net10.0-windows`をターゲットとしています
- AgentTalkの特徴であったClippy/Kairuペルソナ、フローティングウィンドウ、ダブルCtrl+Cキャプチャを維持しています
- 1つの実行ファイル、隣接するJSON設定、オプションの履歴（`%AppData%/AgentTalk`）にデプロイメントを簡素化しました
- より長い入力のための5倍垂直拡張機能を備えた拡張可能なテキストエリア
- 統一された右クリックコンテキストメニュー（キャラクターとシステムトレイで同一機能）
- ダブルクリックでバブル表示切り替え、拡張ボタンでテキストエリア拡大
- システムリソース（CPU/メモリ）使用率のリアルタイム監視
- システムプロンプトプリセット機能（組み込みおよびカスタム）
- GPT-5およびGPT-5 Codexモデルをサポート（最新の/v1/responsesエンドポイント使用）
- Webサーチ機能の統合（GPT-5シリーズ、GPT-4.1シリーズで利用可能）
- システムプロンプトに現在時刻を自動埋め込み（日本語UIではJST、英語UIではUTC）
- **会話継続モード**: 応答表示中に入力ボタンまたはCtrl+Enterで会話を継続

## 始めるには
### 前提条件
- デスクトップ開発ツールが有効になったWindows 11
- .NET 10 SDK RC1 (10.0.100-rc.1以降) - [ダウンロード](https://dotnet.microsoft.com/download/dotnet/10.0)
- チャットコンプリーション用のOpenAI互換APIキー（または同等のエンドポイント）

### ビルドと実行
1. .NET 10 SDK RC1がインストールされていない場合はインストールします
2. ソリューションをリストアしてビルド: `dotnet build otak-agent.sln`
3. WinFormsフロントエンドを実行: `dotnet run --project src/OtakAgent.App`
4. 配布用にパブリッシュ: `dotnet publish -c Release -r win-x64 --self-contained false`
5. リソース（GIF/PNG/WAV）はビルド中に`src/OtakAgent.App/Resources`から自動的にコピーされます

### インストールオプション

#### ビルド済みパッケージのダウンロード
- **GitHubリリース**: [最新リリース](https://github.com/tsuyoshi-otake/otak-agent/releases/latest)からダウンロード
  - `OtakAgent-Portable.zip` - ポータブル版（推奨）
  - `OtakAgent.msix` - Microsoft Store形式パッケージ（開発者モード必要）
  - `OtakAgent.msi` - Windowsインストーラー（WiXツールセットで生成）

#### ポータブル版の使用方法
1. リリースから`OtakAgent-Portable.zip`をダウンロード
2. ZIPファイルを任意のフォルダに解凍
3. `OtakAgent.App.exe`を実行
4. .NET 10ランタイムが必要です

## パッケージ作成方法

### 統合ビルドスクリプト
`build-packages.ps1`を使用して、すべての配布形式を作成できます：

```powershell
# すべてのパッケージを作成（推奨）
powershell -ExecutionPolicy Bypass -File build-packages.ps1 -All

# 個別パッケージの作成
powershell -ExecutionPolicy Bypass -File build-packages.ps1 -Portable  # ポータブル版のみ
powershell -ExecutionPolicy Bypass -File build-packages.ps1 -MSI       # MSIインストーラーのみ
powershell -ExecutionPolicy Bypass -File build-packages.ps1 -MSIX      # MSIXパッケージのみ
```

### パッケージの種類と要件

#### 1. ポータブル版 (ZIP)
- **要件**: .NET 10 SDK
- **出力**: `publish/OtakAgent-Portable.zip`
- **用途**: インストール不要で使いたい場合

#### 2. MSIインストーラー
- **要件**: WiX Toolset v5（`dotnet tool install -g wix`でインストール）
- **出力**: `publish/OtakAgent.msi`
- **用途**: 標準的なWindowsインストーラーが必要な場合

#### 3. MSIXパッケージ（Microsoft Store用）
- **要件**: Visual Studio 2022 + Windows Application Packaging Project拡張
- **出力**: `OtakAgent.Package/AppPackages/`配下
- **用途**: Microsoft Store配布用

### 手動ビルド手順（上級者向け）

#### ポータブル版
```powershell
dotnet publish src/OtakAgent.App -c Release -r win-x64 --self-contained false -o ./publish/portable -p:PublishSingleFile=true
Compress-Archive -Path './publish/portable/*' -DestinationPath './publish/OtakAgent-Portable.zip' -Force
```

#### MSIインストーラー
```powershell
# WiX v5がインストール済みの場合
cd installer
wix build OtakAgent.wxs -o ../publish/OtakAgent.msi
cd ..
```

#### MSIXパッケージ
```powershell
# Visual Studio 2022必須
msbuild OtakAgent.Package\OtakAgent.Package.wapproj /p:Configuration=Release /p:Platform=x64
```

## 設定
- 設定は実行ファイルの隣に`agenttalk.settings.json`として存在し、`OtakAgent.Core`の`SettingsService`によって管理されます
- 初回起動時、レガシーの`agenttalk.ini`と`SystemPrompt.ini`が検出されると`IniSettingsImporter`が移行します
- 会話履歴はデフォルトでメモリに残り、オプションで`%AppData%/AgentTalk/history.json`に永続化できます
- モデル選択はドロップダウンメニューから簡単に変更可能
- APIキー、ホスト、エンドポイントは設定画面から簡単に設定可能

## デフォルト設定
- **言語**: 日本語UIがデフォルト
- **モデル**: GPT-5 Codexがデフォルト
- **キャラクター人格**: 有効がデフォルト
- **会話履歴**: 保持がデフォルト
- **最大トークン数**: 32768トークン
- **自動クリップボードコピー**: 無効がデフォルト

## サポートされているAIモデル
- GPT-5シリーズ: GPT-5、GPT-5 Codex（デフォルト）
- GPT-4シリーズ: GPT-4.1、GPT-4.1-mini
- その他のOpenAI互換モデル（従来の/v1/chat/completionsエンドポイント使用）
- ドロップダウンメニューから簡単にモデルを選択可能
- GPT-5シリーズとGPT-4.1シリーズは自動的に最新のresponses APIを使用

## 操作方法とショートカット

### 基本操作
- **ダブルクリック**: バブル表示切り替え
- **右クリック**: コンテキストメニュー表示
- **ドラッグ**: ウィンドウ移動
- **拡張ボタン（▼/▲）**: テキストエリアの5倍拡張

### キーボードショートカット
- **Ctrl+Enter**:
  - 入力モード時：メッセージ送信
  - 応答表示時：会話継続モードで新規入力
- **Ctrl+Backspace**: 会話履歴リセット
- **ダブルCtrl+C**: クリップボード内容を自動送信（設定で有効化時）

### 会話継続モード
応答が表示されている状態で「入力」ボタンをクリックまたはCtrl+Enterを押すと：
- 会話履歴を保持したまま新規入力が可能
- プレースホルダーテキストは表示されない
- 「リセット」ボタンが常に表示される
- プロンプトが「会話を続けてください...」に変わる

## ペルソナとプロンプト
- `PersonalityPromptBuilder`は、人格が有効な場合にClippyとKairuのプロンプトを再構築し、カスタムペルソナテキストを許可します
- システムプロンプトに現在時刻が自動的に含まれます（日本語UIではJST形式、英語UIではUTC形式）
- プリセット機能で簡単にプロンプトを切り替え可能
- カスタムプロンプトの保存と読み込みに対応

## API統合の詳細
- **最新のResponses API**: GPT-5シリーズ、GPT-4.1シリーズは自動的に`/v1/responses`エンドポイントを使用
- **従来のChat Completions API**: その他のモデルは標準の`/v1/chat/completions`エンドポイントを使用
- **Webサーチ統合**: Responses API使用時に自動的にWebサーチ機能が利用可能
- **エラーハンドリング**: 不完全な応答やトークン制限の自動処理
- **最大出力トークン**: 32768トークンまでの長い応答をサポート
- **タイムアウト処理**: 30秒でタイムアウト、適切なエラーメッセージ表示

## プロジェクト構造
```
otak-agent/
├── build-packages.ps1      # 統合パッケージビルドスクリプト
├── otak-agent.sln          # Visual Studioソリューション
├── README.md               # プロジェクトドキュメント（日本語）
├── CLAUDE.md               # Claude Code用ガイドライン
├── LICENSE                 # MITライセンスファイル
├── .gitignore              # Git除外設定
│
├── src/                    # ソースコード
│   ├── OtakAgent.Core/     # ビジネスロジック層
│   │   ├── Configuration/  # 設定管理とINI移行
│   │   └── Services/       # チャット、クリップボード、システムリソース監視
│   └── OtakAgent.App/      # プレゼンテーション層
│       ├── Forms/          # WinForms UI（メイン、設定、バブル）
│       └── Resources/      # アセット（GIF、PNG、WAVファイル）
│
├── installer/              # MSIインストーラー定義
│   ├── OtakAgent.wxs       # WiX v5定義ファイル（日本語対応）
│   └── license.rtf        # インストーラー用ライセンス
│
├── OtakAgent.Package/      # MSIX/Microsoft Storeパッケージング
│   ├── Package.appxmanifest    # MSIXマニフェスト
│   ├── OtakAgent.Package.wapproj # Visual Studioパッケージングプロジェクト
│   ├── Images/                  # Storeアセット（タイル、アイコン）
│   ├── create-certificate.ps1   # 自己署名証明書生成
│   └── generate-assets.ps1      # Storeアセット生成
│
├── docs/                   # GitHub Pagesドキュメント
│   ├── index.md            # トップページ（日本語）
│   ├── privacy.md          # プライバシーポリシー（日本語）
│   └── _config.yml         # Jekyll設定
│
└── publish/                # ビルド出力（.gitignore対象）
    ├── OtakAgent-Portable.zip  # ポータブル版パッケージ
    ├── OtakAgent.msi           # MSIインストーラー
    └── portable/               # ポータブル版作業ディレクトリ
```

## 開発ノート
- 設定インポートとプロンプトビルダー動作のユニットテストは`OtakAgent.Core.Tests`で計画されています
- トレースロギングとオプションのファイルログは、診断を支援するためにポリッシュフェーズ中に追加されます
- パブリッシュビルドは`dotnet publish`を使用し、配布用のセルフコンテンド対応フォルダーを生成します

## 技術仕様
- **フレームワーク**: .NET 10 RC1 (10.0.100-rc.1以降)
- **ターゲット**: net10.0-windows
- **UI**: Windows Forms
- **依存性注入**: Microsoft.Extensions.DependencyInjection
- **JSON処理**: System.Text.Json
- **HTTP通信**: System.Net.Http
- **ビルドツール**: MSBuild、WiX Toolset（オプション）

## システム要件
- **OS**: Windows 11（Windows 10でも動作可能）
- **アーキテクチャ**: x64、x86、ARM64
- **ランタイム**: .NET 10ランタイム（self-containedビルドの場合は不要）
- **メモリ**: 最小512MB RAM
- **ストレージ**: 約50MBの空き容量（ポータブル版）

## トラブルシューティング

### よくある問題と解決方法
1. **APIキーエラー**: 設定画面でAPIキーを正しく入力してください
2. **接続エラー**: インターネット接続とプロキシ設定を確認してください
3. **文字化け**: Windows 11の日本語言語パックをインストールしてください
4. **起動しない**: .NET 10ランタイムがインストールされているか確認してください

### ログとデバッグ
- アプリケーションログは将来のアップデートで追加予定
- 現在はVisual Studioのデバッガーを使用してデバッグ可能

## ライセンスとクレジット
- このプロジェクトはMITライセンスのオープンソースです
- クラシックなAgentTalk実装を.NET 10で最新化
- アイコンとキャラクターアセットは独自デザイン

## お問い合わせ
- **Issues**: [GitHub Issues](https://github.com/tsuyoshi-otake/otak-agent/issues)
- **Pull Requests**: 歓迎します！

---
最終更新: 2025年9月