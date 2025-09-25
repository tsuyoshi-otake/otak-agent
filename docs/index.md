---
layout: default
title: otak-agent - AI搭載デスクトップアシスタント
---

# otak-agent

クラシックなAgentTalkフローティングアシスタントを、ネイティブ依存関係のない単一の.NET 10 WinFormsアプリケーションに最新化しました。

![otak-agent Logo](https://raw.githubusercontent.com/tsuyoshi-otake/otak-agent/main/OtakAgent.Package/Images/StoreLogo.png)

## 機能

- **クラシックキャラクター**: 懐かしいClippy/Kairuペルソナがデスクトップに個性を追加
- **AI搭載チャット**: OpenAI互換APIとの統合による知的な会話
- **フローティングウィンドウ**: 常にアクセス可能な最前面アシスタント
- **拡張可能な入力**: より長いテキスト入力のための5倍垂直拡張
- **会話継続モード**: 文脈を保持したまま会話を継続
- **プライバシー優先**: すべてのデータはローカル保存、外部サーバーなし

## ダウンロード

[**最新リリースをダウンロード**](https://github.com/tsuyoshi-otake/otak-agent/releases/latest)

利用可能なパッケージ:
- **OtakAgent-Portable.zip** - ポータブル版（推奨、インストール不要）
- **OtakAgent.msix** - Microsoft Store形式パッケージ
- **OtakAgent.msi** - Windowsインストーラー（WiXツールセット必要）

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

## オープンソース

otak-agentはMITライセンスの下でライセンスされたオープンソースソフトウェアです。貢献を歓迎します！

---

© 2025 Tsuyoshi Otake. All rights reserved.