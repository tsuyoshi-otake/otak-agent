# CLAUDE.md

このファイルは、このリポジトリのコードを扱う際のClaude Code (claude.ai/code) のためのガイドラインです。

## 重要事項
2025年9月25日現在、このアプリケーションはGPT-5およびGPT-5 Codexをサポートしています。GPT-5 Codexがデフォルトモデルです。アプリケーションはGPT-5シリーズとGPT-4.1シリーズのモデルに対して自動的に最新の/v1/responsesエンドポイントを使用します。

## フレームワーク要件
- **.NET 10 RC1** (10.0.100-rc.1以降)
- デスクトップ開発ツール付きWindows 11
- ターゲットフレームワーク:
  - `net10.0` - OtakAgent.Core用
  - `net10.0-windows` - OtakAgent.App用

## 共通コマンド

### ビルドと実行
- **ソリューションのビルド**: `dotnet build otak-agent.sln`
- **アプリケーションの実行**: `dotnet run --project src/OtakAgent.App`
- **クリーンビルド**: `dotnet clean && dotnet build`
- **パッケージの復元**: `dotnet restore`

### パッケージ作成
統合ビルドスクリプト`build-packages.ps1`を使用：
- **すべてのパッケージ作成**: `powershell -ExecutionPolicy Bypass -File build-packages.ps1 -All`
- **ポータブル版のみ**: `powershell -ExecutionPolicy Bypass -File build-packages.ps1 -Portable`
- **MSIのみ**: `powershell -ExecutionPolicy Bypass -File build-packages.ps1 -MSI`
- **MSIXのみ**: `powershell -ExecutionPolicy Bypass -File build-packages.ps1 -MSIX`

必要な前提条件：
- ポータブル版: .NET 10 SDK
- MSI: WiX v5（`dotnet tool install -g wix`）
- MSIX: Visual Studio 2022 + Windows Application Packaging Project拡張

### テスト（実装時）
- **全テスト実行**: `dotnet test`
- **特定テストプロジェクト実行**: `dotnet test src/OtakAgent.Core.Tests`

## 最近のUI改善

### バブルウィンドウのレンダリング
- 上部と下部の画像がクロップされることなく完全に表示
- 下部は適切な外観のため5px下に延長
- マゼンタカラーキー（RGB 255,0,255）による適切な透過処理

### 拡張可能なテキストエリア
- バブルの右上角（位置: 200,8）にトグルボタン（▼/▲）
- 有効時にテキストエリアを垂直方向に5倍拡張
- フォームは下部位置を維持したまま上方向に拡大
- 拡張状態に基づいてキャラクター位置を調整

### UI要素の配置
- プロンプトラベル: (8, 8)
- テキスト入力: (8, 32) - 通常モード、拡張時は295px高さ
- ボタン: テキストエリアの下に動的に配置
- キャラクター: 拡張状態に基づいて位置調整

### インタラクティブ機能
- キャラクターをダブルクリックでバブル表示切り替え
- キャラクターを右クリックでコンテキストメニュー
- テキストエリアの拡張/折りたたみボタン
- アニメーショントランジションはフリーズ防止のためBeginInvokeで処理

### 会話継続モード
- 応答表示中に「入力」ボタンまたはCtrl+Enterで会話継続
- 会話履歴を保持したまま新規入力が可能
- プレースホルダーテキストは表示されない
- 「リセット」ボタンが常に表示される

## プロジェクト構造

### ディレクトリ構成
```
otak-agent/
├── build-packages.ps1      # 統合パッケージビルドスクリプト
├── otak-agent.sln          # Visual Studioソリューション
├── README.md               # プロジェクトドキュメント（日本語）
├── CLAUDE.md               # このファイル（Claude Code用ガイドライン）
├── LICENSE                 # ライセンスファイル
├── .gitignore              # Gitignore設定
│
├── src/                    # ソースコード
│   ├── OtakAgent.Core/     # ビジネスロジック層
│   │   ├── Configuration/  # 設定管理
│   │   └── Services/       # チャット、クリップボード、システムリソース監視
│   └── OtakAgent.App/      # プレゼンテーション層
│       ├── Forms/          # WinForms UI
│       └── Resources/      # アセット（GIF、PNG、WAVファイル）
│
├── installer/              # MSIインストーラー定義
│   ├── OtakAgent.wxs       # WiX v5定義ファイル
│   └── license.rtf        # インストーラー用ライセンス
│
├── OtakAgent.Package/      # MSIX/Storeパッケージング
│   ├── Package.appxmanifest    # MSIXマニフェスト
│   ├── Images/                 # Storeアセット
│   ├── create-certificate.ps1  # 証明書生成スクリプト
│   └── generate-assets.ps1     # アセット生成スクリプト
│
├── docs/                   # GitHub Pagesドキュメント
│   ├── index.md            # トップページ（日本語）
│   ├── privacy.md          # プライバシーポリシー（日本語）
│   └── _config.yml         # Jekyll設定
│
└── publish/                # ビルド出力（.gitignore対象）
    ├── OtakAgent-Portable.zip  # ポータブル版
    ├── OtakAgent.msi           # MSIインストーラー
    └── portable/               # ポータブル版作業ディレクトリ
```

## アーキテクチャ概要

### ソリューション構造
これはレガシーAgentTalkアシスタントを最新化した.NET 10 WinFormsアプリケーションです。ソリューションは関心の分離によるクリーンアーキテクチャに従っています：

- **OtakAgent.Core** (クラスライブラリ): ビジネスロジック、サービス、モデル
  - 設定管理（設定、INI移行）
  - OpenAI互換API統合のチャットサービス
  - Clippy/Kairuペルソナ用のパーソナリティプロンプトビルダー
  - クリップボードホットキー監視サービス

- **OtakAgent.App** (WinFormsアプリ): プレゼンテーション層
  - Microsoft.Extensions.DependencyInjectionによる依存性注入セットアップ
  - ボーダーレスデザインのメインフローティングウィンドウ
  - 設定管理用の設定フォーム
  - リソースアセット（GIF、WAV、PNG）

### 主要サービス依存関係
アプリケーションはコンストラクタ依存性注入で以下のコアサービスを使用：
- `ChatService`: OpenAI互換エンドポイントとのAPI通信を処理
- `SettingsService`: JSON設定の永続化を管理
- `ClipboardHotkeyService`: ダブルCtrl+Cジェスチャーを監視
- `PersonalityPromptBuilder`: ペルソナ固有のシステムプロンプトを構築

### 設定フロー
1. 実行ファイルの隣に`agenttalk.settings.json`として設定を保存
2. 初回起動時にレガシー`agenttalk.ini`と`SystemPrompt.ini`を自動インポート
3. オプションで`%AppData%/AgentTalk/history.json`に履歴を永続化

### UI動作パターン
- ボーダーレス、常に最前面のフローティングウィンドウ
- マウスダウン/移動イベントによるドラッグで移動
- ダブルクリックで表示切り替え
- コンテキストメニュー付きシステムトレイアイコン
- Ctrl+Enterで送信、Ctrl+Backspaceでリセット
- 会話継続モードでの文脈保持

## 重要なコンテキスト

### プロジェクト構成
単一の統合ビルドスクリプト`build-packages.ps1`で全パッケージング処理を管理。不要なスクリプトは削除済み。

### 重要なファイル
- **build-packages.ps1**: 統合パッケージングスクリプト（Portable/MSI/MSIX対応）
- **installer/OtakAgent.wxs**: WiX v5用MSI定義ファイル
- **OtakAgent.Package/**: MSIX関連ファイル
  - `Package.appxmanifest`: MSIXマニフェスト
  - `create-certificate.ps1`: 署名証明書生成
  - `generate-assets.ps1`: Storeアセット生成

### アセット管理
GIF/PNG/WAVリソースは現在`src/OtakAgent.App/Resources/`にあり、ビルド時に出力にコピーされます。透過処理にはマゼンタカラーキー（RGB 255,0,255）を使用。

### Microsoft Storeアセット
StoreアセットはMSIXパッケージング用に`OtakAgent.Package/Images/`に配置：
- **Square150x150Logo.png** - 中タイル (150x150)
- **Square44x44Logo.png** - 小アプリアイコン (44x44)
- **Square44x44Logo.targetsize-24_altform-unplated.png** - タスクバーアイコン (24x24)
- **Wide310x150Logo.png** - ワイドタイル (310x150)
- **SmallTile.png** - 小タイル (71x71)
- **LargeTile.png** - 大タイル (310x310)
- **StoreLogo.png** - ストアリスティングロゴ (50x50)
- **SplashScreen.png** - アプリスプラッシュ画面 (620x300)

ベースアイコンからこれらのアセットを再生成するには`generate-assets.ps1`スクリプトを使用。

### MSIXパッケージングとMicrosoft Store配布
`OtakAgent.Package/`ディレクトリにはMSIXパッケージ作成用のWindows Application Packaging Projectが含まれます：
- **OtakAgent.Package.wapproj** - Visual Studio用パッケージングプロジェクトファイル
- **Package.appxmanifest** - アプリID、機能、ビジュアル要素を定義するアプリケーションマニフェスト
- **create-certificate.ps1** - パッケージ署名用の自己署名証明書生成スクリプト
- **generate-assets.ps1** - ベースイメージからStoreアイコンを生成するスクリプト

#### MSIXパッケージのビルド方法

##### 方法1: Windows SDK makeappxツール使用（推奨）
```powershell
# 1. ポータブル版をビルド
dotnet publish src/OtakAgent.App -c Release -r win-x64 --self-contained false -o ./publish/portable

# 2. AppxManifestを配置
Copy-Item OtakAgent.Package\Package.appxmanifest publish\portable\AppxManifest.xml

# 3. MSIXパッケージ作成（Windows SDK必須）
& "C:\Program Files (x86)\Windows Kits\10\bin\10.0.18362.0\x64\makeappx.exe" pack /d publish\portable /p publish\OtakAgent.msix /nv
```

##### 方法2: build-packages.ps1スクリプト使用
```powershell
powershell -ExecutionPolicy Bypass -File build-packages.ps1 -MSIX
```

##### 方法3: Visual Studio 2022使用（.NET 10 SDK対応後）
```powershell
msbuild OtakAgent.Package\OtakAgent.Package.wapproj /p:Configuration=Release /p:Platform=x64
```

#### Microsoft Store提出用
- 生成されたMSIXパッケージをパートナーセンターにアップロード
- パッケージはx64アーキテクチャ対応
- Windows 11バージョン22621.0以上が必要
- 署名は必須（Store署名またはテスト用自己署名証明書）

### 日本語サポート
アプリケーションはバイリンガルペルソナ（日本語/英語）をサポートし、ロケール検出にSystem.Globalizationを使用。UI文字列は現在ハードコードされていますが、ローカライゼーション対応準備済み。

### Windows固有機能
- クリップボード監視とウィンドウ管理にP/Invokeを使用
- デスクトップ開発ツール付きWindows 11が必要
- ターゲットフレームワークは`net10.0-windows`