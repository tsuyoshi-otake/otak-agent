---
layout: default
title: otak-agent - AI搭載デスクトップアシスタント
---

# otak-agent

懐かしのMicrosoft Officeアシスタントを再現したパロディソフトウェアです。.NET 10 WinFormsで実装された現代版のデスクトップマスコット/アシスタントです。

> **注記**: これはMicrosoft Officeアシスタント（Clippy、Kairu）を愛情を込めて再現したパロディソフトウェアです。キャラクターアセットおよびアイコンはMicrosoft Corporation の知的財産を模倣したものであり、エンターテイメント目的でのみ使用されています。

![otak-agent Logo](https://raw.githubusercontent.com/tsuyoshi-otake/otak-agent/main/OtakAgent.Package/Images/StoreLogo.png)

## 機能

- **クラシックキャラクター**: 懐かしいClippy/Kairuペルソナがデスクトップに個性を追加
- **AI搭載チャット**: OpenAI互換APIとの統合による知的な会話
- **フローティングウィンドウ**: 常にアクセス可能な最前面アシスタント
- **拡張可能な入力**: より長いテキスト入力のための5倍垂直拡張
- **会話継続モード**: 文脈を保持したまま会話を継続
- **Windows自動起動対応**: 右クリックメニューから簡単設定、MSIインストーラーで自動設定
- **二重起動防止**: Mutexによる多重起動防止機能搭載
- **プライバシー優先**: すべてのデータはローカル保存、外部サーバーなし

## ダウンロード

[**最新リリース v1.5.2 をダウンロード**](https://github.com/tsuyoshi-otake/otak-agent/releases/latest)

利用可能なパッケージ:
- **otak-agent-portable.zip** - ポータブル版（推奨、インストール不要）
- **otak-agent.msix** - Microsoft Store形式パッケージ（開発者モード必要）
- **otak-agent.msi** - Windowsインストーラー（Windows自動起動設定付き）

## システム要件

### 最小要件
- Windows 11（Windows 10でも動作可能）
- .NET 10ランタイム
- キーボードとマウス

### 推奨環境
- Windows 11（最新バージョン）

## はじめに

1. **アプリケーションのインストール**
   - GitHubリリースからダウンロード
   - ZIPファイルを解凍またはインストーラーを実行

2. **API設定の構成**
   - キャラクターアイコンを右クリック
   - 「設定」を選択
   - OpenAI互換APIキーを入力
   - お好みのエンドポイントを選択
   - モデル（デフォルト：GPT-5 Codex）を選択

3. **チャット開始**
   - キャラクターをダブルクリックでチャットバブルの表示/非表示
   - メッセージを入力してCtrl+Enterで送信
   - Ctrl+Backspaceで会話をクリア

## キーボードショートカット

- **Ctrl+Enter**:
  - 入力モード時：メッセージ送信
  - 応答表示時：会話継続モードで新規入力
- **Ctrl+Backspace**: 会話履歴をリセット
- **キャラクターをダブルクリック**: バブル表示切り替え
- **拡張ボタン（▼/▲）**: テキストエリアの5倍拡張

## 会話継続モード

応答が表示されている状態で「入力」ボタンをクリックまたはCtrl+Enterを押すと：
- 会話履歴を保持したまま新規入力が可能
- プレースホルダーテキストは表示されない
- 「リセット」ボタンが常に表示される
- プロンプトが「会話を続けてください...」に変わる

## プライバシー

あなたのプライバシーは重要です。すべてのデータはデバイスにローカル保存されます。[プライバシーポリシー全文を読む](privacy.html)。

## サポート

ヘルプが必要ですか？バグを見つけましたか？機能リクエストがありますか？

- [GitHubで問題を報告](https://github.com/tsuyoshi-otake/otak-agent/issues)
- [ドキュメントを表示](https://github.com/tsuyoshi-otake/otak-agent/wiki)
- [ソースコード](https://github.com/tsuyoshi-otake/otak-agent)

## 最新アップデート (v1.5.2)

### 新機能
- **Windows自動起動**: 右クリックメニューから「Windows起動時に自動起動」を選択可能
- **MSIアップグレード改善**: 既存インストールを自動検出してアップグレード
- **設定の保持**: MSIアップグレード時も設定（APIキー、プリセット等）を維持
- **MSIX対応**: Microsoft Store配布用MSIXパッケージのサポート

### 改善点
- 二重起動防止機能の強化
- インストール完了時の自動起動オプション
- パッケージ名の統一（otak-agent-*.zip/msi/msix）

## オープンソース

otak-agentはMITライセンスの下でライセンスされたオープンソースソフトウェアです。貢献を歓迎します！

---

© 2025 Tsuyoshi Otake. All rights reserved.