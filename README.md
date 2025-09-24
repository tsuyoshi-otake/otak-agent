# otak-agent

クラシックなAgentTalkフローティングアシスタントを、ネイティブ依存関係のない単一の.NET 10 WinFormsアプリケーションに最新化しました。

## 主な特徴
- .NET 10 RC1の最新機能とパフォーマンス改善を活用する`net10.0-windows`をターゲットとしています
- AgentTalkの特徴であったClippy/Kairuペルソナ、フローティングウィンドウ、ダブルCtrl+Cキャプチャを維持しています
- 1つの実行ファイル、隣接するJSON設定、オプションの履歴（`%AppData%/AgentTalk`）にデプロイメントを簡素化しました
- より長い入力のための5倍垂直拡張機能を備えた拡張可能なテキストエリア
- 右クリックコンテキストメニューとダブルクリックバブル切り替えによるインタラクティブUI
- システムリソース（CPU/メモリ）使用率のリアルタイム監視
- システムプロンプトプリセット機能（組み込みおよびカスタム）
- GPT-5およびGPT-5 Codexモデルをサポート

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
  - `otak-agent.msi` - Windowsインストーラー（推奨）
  - `otak-agent-portable.zip` - インストール/アンインストールスクリプト付きポータブル版

#### Microsoft Storeからインストール
- Microsoft Storeで近日公開予定

#### MSIインストール
1. リリースから`otak-agent.msi`をダウンロード
2. ダブルクリックしてインストール
3. インストールウィザードに従う

#### ポータブルインストール（ZIP）
1. リリースから`otak-agent-portable.zip`をダウンロード
2. ZIPファイルを解凍
3. システム全体のインストールには管理者として`Install.bat`を実行
4. またはポータブル使用のために`OtakAgent.App.exe`を直接実行

アンインストールするには: 管理者として`Uninstall.bat`を実行（インストール版の場合）

### ソースからのビルド

#### Microsoft Store用MSIXパッケージの作成

##### 前提条件
- Windows SDKがインストールされている（makeappx.exeを含む）
  - 次のコマンドでインストール: `winget install --id Microsoft.WindowsSDK.10.0.18362`

##### 手順

1. **リリース版をビルド**
   ```bash
   dotnet publish src/OtakAgent.App -c Release -r win-x64 --self-contained false -o ./publish
   ```

2. **Storeアセットを生成**（まだ作成されていない場合）
   ```powershell
   cd OtakAgent.Package
   powershell -ExecutionPolicy Bypass -File generate-assets.ps1
   cd ..
   ```

3. **MSIX構造を作成**
   ```powershell
   powershell -ExecutionPolicy Bypass -File create-simple-msix.ps1
   ```

4. **MSIXパッケージをビルド**
   ```powershell
   powershell -ExecutionPolicy Bypass -File build-msix.ps1
   ```

   または手動で:
   ```powershell
   & "C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\makeappx.exe" pack /d OtakAgent_MSIX /p OtakAgent.msix /nv /o
   ```

5. **インストールをテスト**（開発者モードが必要）
   ```powershell
   Add-AppxPackage -Path OtakAgent.msix -AllowUnsigned
   ```

#### ポータブルインストーラー（ZIP）の作成
1. **ビルドとインストーラーの作成**
   ```powershell
   dotnet publish src/OtakAgent.App -c Release -r win-x64 --self-contained false -o ./publish
   powershell -ExecutionPolicy Bypass -File create-portable-installer.ps1
   ```

2. **出力**: 以下を含む`otak-agent-portable.zip`
   - アプリケーションファイル
   - `Install.bat` - インストーラースクリプト
   - `Uninstall.bat` - アンインストーラースクリプト
   - `README.txt` - 説明書

#### MSIインストーラーの作成
1. **WiX v6のインストール**（.NETツール）
   ```bash
   dotnet tool install -g wix
   ```

2. **MSIをビルドして作成**
   ```bash
   dotnet publish src/OtakAgent.App -c Release -r win-x64 --self-contained false -o ./publish
   wix build simple-installer.wxs -o otak-agent.msi
   ```

#### Microsoft Storeへの提出
1. [パートナーセンター](https://partner.microsoft.com/dashboard)に`OtakAgent.msix`をアップロード
2. Microsoftが自動的にパッケージに署名します
3. Storeリスト情報を入力
4. 認定のために提出

## 設定
- 設定は実行ファイルの隣に`agenttalk.settings.json`として存在し、`OtakAgent.Core`の`SettingsService`によって管理されます
- 初回起動時、レガシーの`agenttalk.ini`と`SystemPrompt.ini`が検出されると`IniSettingsImporter`が移行します
- 会話履歴はデフォルトでメモリに残り、オプションで`%AppData%/AgentTalk/history.json`に永続化できます

## デフォルト設定
- **言語**: 日本語UIがデフォルト
- **モデル**: GPT-5 Codexがデフォルト
- **キャラクター人格**: 有効がデフォルト
- **会話履歴**: 保持がデフォルト

## サポートされているAIモデル
- GPT-5シリーズ: GPT-5、GPT-5 Codex
- GPT-4シリーズ: GPT-4.1
- その他のOpenAI互換モデル

## ペルソナとホットキー
- `PersonalityPromptBuilder`は、人格が有効な場合にClippyとKairuのプロンプトを再構築し、カスタムペルソナテキストを許可します
- `ClipboardHotkeyService`はダブルCtrl+Cジェスチャーをリッスンし、設定可能なデバウンスを適用し、クリップボードの内容をチャットに転送します
- UIショートカットは元のAgentTalk体験を反映しています（Ctrl+Enterで送信、Ctrl+Backspaceでリセット、ダブルクリックで表示切り替え）

## プロジェクト構造
```
otak-agent/
  CLAUDE.md
  README.md
  docs/
    modernization-architecture.md
    modernization-roadmap.md
  src/
    OtakAgent.Core/
    OtakAgent.App/
  agent-talk-main/
  OtakAgent.Package/
```

- `OtakAgent.Core`: 設定、チャット、人格、ホットキーサービスをホスト
- `OtakAgent.App`: WinForms UI、依存性注入ブートストラップ、リソースを提供
- `agent-talk-main/`: 動作パリティチェック用のレガシーコードベースを保持
- `OtakAgent.Package/`: Microsoft Store用のパッケージング資産

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

## システム要件
- **OS**: Windows 11
- **アーキテクチャ**: x64、x86、ARM64
- **ランタイム**: .NET 10ランタイム（self-containedビルドの場合は不要）
- **メモリ**: 最小512MB RAM
- **ストレージ**: 約50MBの空き容量

## レガシー参照
元のAgentTalk実装（C++ブリッジを使用した.NET Framework 3.5をターゲット）は`agent-talk-main/`に残っており、最新化中にUIや人格動作を比較するために使用できます。