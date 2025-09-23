# Microsoft Store配布ガイド

## 必要な手順

### 1. 開発者証明書の作成
```powershell
cd OtakAgent.Package
powershell -ExecutionPolicy Bypass -File create-certificate.ps1
```

### 2. MSIXパッケージのビルド

#### Visual Studio 2022を使用する場合:
1. `otak-agent.sln`を開く
2. `OtakAgent.Package`プロジェクトを右クリック
3. 「発行」→「パッケージの作成」を選択
4. Microsoft Storeを選択
5. 証明書を選択（作成した.pfxファイル）

#### コマンドラインを使用する場合:
```powershell
# MSBuildを使用
msbuild OtakAgent.Package\OtakAgent.Package.wapproj /p:Configuration=Release /p:Platform=x64 /p:UapAppxPackageBuildMode=StoreUpload
```

### 3. Microsoft Partner Centerでの申請

1. [Partner Center](https://partner.microsoft.com/dashboard)にログイン
2. 新しいアプリの作成
3. パッケージをアップロード
4. ストア情報を記入:
   - アプリ名: Otak Agent
   - 説明: AI-powered desktop assistant
   - カテゴリ: 仕事効率化
   - 価格: 無料

### 4. テスト用インストール

開発環境でテストする場合:
```powershell
# 証明書をインストール
certutil -addstore TrustedPeople OtakAgent.Package\OtakAgent.cer

# パッケージをインストール
Add-AppxPackage -Path OtakAgent.Package\AppPackages\*.msix
```

## プロジェクト構造

```
OtakAgent.Package/
├── Package.appxmanifest    # マニフェストファイル
├── Images/                 # ストア用アイコン
│   ├── Square150x150Logo.png
│   ├── Square44x44Logo.png
│   ├── Wide310x150Logo.png
│   ├── StoreLogo.png
│   └── SplashScreen.png
├── OtakAgent.pfx          # 署名証明書（gitignore推奨）
└── OtakAgent.Package.wapproj # パッケージプロジェクト
```

## 注意事項

- .NET 10.0 ランタイムが必要
- Windows 11 22H2以降が対象
- 初回申請は審査に数日かかる場合があります
- 証明書(.pfx)はGitにコミットしないでください