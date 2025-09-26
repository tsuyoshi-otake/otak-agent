# Create GitHub Release for otak-agent v1.5.2
# This script creates a new release on GitHub with the portable and MSI packages

$version = "v1.5.2"
$releaseTitle = "otak-agent v1.5.2 - Windows自動起動対応"

Write-Host "Creating GitHub release for $version..." -ForegroundColor Cyan

# Check if gh CLI is installed
$ghPath = Get-Command gh -ErrorAction SilentlyContinue
if (-not $ghPath) {
    Write-Host "GitHub CLI (gh) is not installed. Please install it from https://cli.github.com/" -ForegroundColor Red
    Write-Host "Or create the release manually at: https://github.com/tsuyoshi-otake/otak-agent/releases/new?tag=$version" -ForegroundColor Yellow
    exit 1
}

# Check if files exist
$portableZip = "publish\otak-agent-portable.zip"
$msiInstaller = "publish\otak-agent.msi"

if (-not (Test-Path $portableZip)) {
    Write-Host "Portable package not found: $portableZip" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $msiInstaller)) {
    Write-Host "MSI installer not found: $msiInstaller" -ForegroundColor Red
    exit 1
}

# Create release with gh CLI
Write-Host "Creating release on GitHub..." -ForegroundColor Yellow

$releaseNotes = @"
## 🎉 新機能

### Windows自動起動対応
- キャラクター右クリックメニューに「Windows起動時に自動起動」オプションを追加
- MSIインストーラーでWindows起動時の自動起動を自動設定

### Microsoft Store対応
- Microsoft Store提出用MSIXパッケージのサポート

## 🔧 改善点
- 二重起動防止機能の強化
- MSIアップグレード時の設定保持
- インストール完了時の自動起動オプション

## 📦 ダウンロード

| ファイル | サイズ | 説明 |
|---------|--------|------|
| otak-agent-portable.zip | 49.1 MB | ポータブル版（インストール不要） |
| otak-agent.msi | 49.1 MB | Windowsインストーラー（自動起動設定付き） |

## システム要件
- Windows 11（Windows 10でも動作可能）
- .NET 10 RC1ランタイム
"@

# Create the release
gh release create $version `
    --title $releaseTitle `
    --notes $releaseNotes `
    --draft `
    $portableZip `
    $msiInstaller

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n✅ Draft release created successfully!" -ForegroundColor Green
    Write-Host "Please review and publish the release at:" -ForegroundColor Yellow
    Write-Host "https://github.com/tsuyoshi-otake/otak-agent/releases" -ForegroundColor Cyan
} else {
    Write-Host "`n❌ Failed to create release" -ForegroundColor Red
    Write-Host "Please create the release manually at:" -ForegroundColor Yellow
    Write-Host "https://github.com/tsuyoshi-otake/otak-agent/releases/new?tag=$version" -ForegroundColor Cyan
}