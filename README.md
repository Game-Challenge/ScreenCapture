# Screen Capture
[Booth](https://lesspeer-waya.booth.pm/items/4349439) | [GitHub](https://github.com/waya0125/ScreenCapture) | [WebSite](https://waya0125.com/vcc/ScreenCapture) | [Twitter](https://twitter.com/wayamoti2015) | [Misskey](https://misskey.waya0125.com/@waya)

## 概要

**キャラクターをキレイに撮影したい！その一心で作りました。**  
※ このソフトウェアは、Unity Editor上でのキャラクターおよびシーン撮影を目的としています。  

## 特徴

- **専用EditorWindow**: プレビュー画面を見ながら直感的に撮影可能。
- **撮影対象カメラの自由選択**: `MainCamera` だけでなく、シーン内の任意のカメラを指定して撮影可能。
- **軽量な高解像度撮影**: 普段のGameビューは軽量なまま、撮影時のみ指定解像度（FHD / WQHD / 4K / 8K / 2倍・4倍スーパーサンプル / カスタム）でレンダリング。
- **カスタム解像度の登録・履歴管理**: 任意のカスタム解像度を保存・削除可能。最新5件の撮影解像度履歴をクイック選択可能。
- **背景透過トグル**: カメラ設定を変更せずにワンタップで背景を透過（アルファ抜き）して撮影。
- **多言語対応 (Multi-Language)**: 日本語・英語・韓国語・中国語（簡体字）の表示切替に対応。
- **フォルダ展開**: 撮影後、保存先フォルダー（`Screenshot`）をワンクリックで開くボタンを搭載。
- **ビルドエラー対策済み**: アセンブリおよびスクリプトをエディター専用に保護し、Playerビルド時のコンパイルエラーを解消。

## 導入方法

VRChat Userの場合は `vcc://vpm/addRepo?url=https://waya0125.com/vpm` を踏んでリポジトリを追加した上でVCCを上でインポートしてください。
[こちらから](https://waya0125.com/vcc/ScreenCapture) も導入方法を確認できます。

その他の場合は、以下の手順でインポートしてください。

1. このリポジトリのMasterをZIPファイルでダウンロードして解凍します。
2. 解凍したフォルダをUnityのプロジェクトのAssetsフォルダにコピーすれば完了です。

もしくは Release にある unitypackage をインポートしてください。

## 使い方

1. メニューから「Tools」→「Screen Capture」を選択し、ウィンドウを開きます。
2. 必要に応じて言語切替、撮影対象カメラ、背景透過、解像度プリセットを設定します。
3. 「Capture Screenshot（スクリーンショット撮影）」ボタンを押すと指定解像度で画像が保存されます。
4. 「Open Screenshot Folder（フォルダを開く）」ボタンで保存先フォルダーを確認できます。

## Release

### Version 1.0.3

リリース日: 2026年08月24日

- **ビルドエラー修正**: `asmdef` のエディタープラットフォーム制約設定およびスクリプトの `#if UNITY_EDITOR` 保護を追加し、Playerビルド失敗問題を修正。
- **UIの大幅刷新**: 専用EditorWindowおよびリアルタイム・アスペクト比連動プレビュー画面を実装。
- **撮影カメラ選択機能**: `Camera.main` 以外の任意カメラの選択に対応。
- **解像度プリセット・カスタム登録・履歴管理**: FHD / 4K / 8K / スーパーサンプル / カスタム解像度の登録・削除、および最大5件の撮影履歴ドロップダウンを追加。
- **背景透過トグル**: カメラ設定を変えずに背景アルファ抜き撮影ができるトグルを追加。
- **多言語対応**: 日本語・英語・韓国語・中国語のUI切替に対応。
- **保存フォルダー展開機能**: 保存先 `Screenshot` フォルダーを開くボタンを追加。

### Version 1.0.1

リリース日: 2023年07月22日

- VCCでの導入方法を追記
- README.md, package.jsonの内容を変更

### Version 1.0.0

リリース日: 2022年11月27日

- 初版リリース

# 参考文献
## ～コードのベース～
[Unity]Unity Editor上でスクリーンショットを撮る方法 - [https://nobushiueshi.com/unityunity-editor上でスクリーンショットを撮る方法/](https://nobushiueshi.com/unityunity-editor%E4%B8%8A%E3%81%A7%E3%82%B9%E3%82%AF%E3%83%AA%E3%83%BC%E3%83%B3%E3%82%B7%E3%83%A7%E3%83%83%E3%83%88%E3%82%92%E6%92%AE%E3%82%8B%E6%96%B9%E6%B3%95/)  

## ～プラットフォーム判定～
Unity プラットフォーム判別 - https://qiita.com/Ubermensch/items/75072ef89249cb3b30e7  
RuntimePlatform - https://docs.unity3d.com/ScriptReference/RuntimePlatform.html  

## ～ファイラーで表示する方法～
【Unity】 セーブファイルのパスを開くメニューを作る - https://www.urablog.xyz/entry/2021/11/05/070000  
How to Open a File or Folder in Terminal on Mac - https://www.switchingtomac.com/tutorials/how-to-open-a-file-or-folder-in-terminal-on-mac/  
【 xdg-open 】コマンド――ファイルをデフォルトアプリケーションで開く - https://atmarkit.itmedia.co.jp/ait/articles/1906/06/news007.html  

## ～ディレクトリの有無判定～
Unity フォルダやファイルの有無を確認し無ければ生成する - https://hakase0274.hatenablog.com/entry/2019/07/27/223840  
[unity]指定されたファイルがどのパスにあるか調べる - https://www.create-forever.games/unity-directory-getfiles/  
