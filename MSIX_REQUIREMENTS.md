# MSIX パッケージ作成に必要なツール

現在、MSIXパッケージを作成するために必要なツールがインストールされていません。
以下のいずれかをインストールしてください：

## オプション 1: Visual Studio 2022
Visual Studio 2022 Community Edition以上をインストール
- 「.NET デスクトップ開発」ワークロード
- 「Windows アプリ SDK」コンポーネント
- 「Windows App Packaging Project」テンプレート

## オプション 2: Windows SDK
Windows SDK (10.0.22621 以降) をインストール
- [ダウンロードリンク](https://developer.microsoft.com/windows/downloads/windows-sdk/)
- makeappx.exe と signtool.exe が含まれています

## オプション 3: .NET SDK with MSIX Extension
```powershell
# MSIX拡張をインストール
dotnet tool install --global dotnet-msix
```

## 現在の状況
- ✅ リリースビルド完了 (`publish/` フォルダ)
- ✅ MSIXマニフェストファイル作成済み
- ✅ Storeアイコン生成済み
- ❌ MSIXパッケージツールが未インストール

## 次のステップ
1. 上記のツールのいずれかをインストール
2. 以下のコマンドでパッケージを作成：

### Visual Studio使用時
```
msbuild OtakAgent.Package\OtakAgent.Package.wapproj /p:Configuration=Release
```

### Windows SDK使用時
```powershell
# AppxManifestをpublishフォルダにコピー
copy OtakAgent.Package\Package.appxmanifest publish\AppxManifest.xml

# MSIXパッケージを作成
makeappx pack /d publish /p OtakAgent.msix

# 署名（オプション）
signtool sign /a /fd SHA256 /f OtakAgent.pfx /p TempPassword123! OtakAgent.msix
```

### dotnet-msix使用時
```powershell
dotnet msix pack -p OtakAgent.msix -m OtakAgent.Package\Package.appxmanifest
```