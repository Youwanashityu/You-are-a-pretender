# CSVLoader

Unity向け 汎用CSVローダーライブラリ

CSVの列構造が変わっても**ローダー側を一切触らずに使い回せる**ことを目的に設計されています。
WebGL（unityroom）でも動作確認済みです。

---

## 動作環境

- Unity 2021.3 以降
- TextMeshPro 不要
- 外部ライブラリ 不要

---

## セットアップ

### 1. スクリプトを配置する

`CSVLoader.cs` をプロジェクトの任意のフォルダに入れてください。

```
Assets/
└── Scripts/
    └── Lib/
        └── CSVLoader.cs   ← ここに置く（場所はどこでもOK）
```

### 2. CSVファイルを配置する

CSVファイルは `Assets/Resources/` フォルダ以下に置いてください。

```
Assets/
└── Resources/
    └── ScenarioData.csv   ← ここに置く
```

サブフォルダに入れる場合はパスを指定します。

```
Assets/
└── Resources/
    └── CSV/
        └── ScenarioData.csv   ← サブフォルダに置く場合
```

### 3. CSVのフォーマット

- 1行目はヘッダー行（列名）にしてください
- 文字コードは **UTF-8** で保存してください
- カンマを含むテキストは `"` で囲んでください

```
scene_id, emotion, display_text, typing_text, streamer_reaction
1, 好意, 頑張ってるね, がんばってるね, 「ありがとう！」
1, 揶揄う, "どうせ, 失敗するでしょ", どうせしっぱいするでしょ, 「ひどっw」
```

---

## 使い方

### 基本的な読み込み

```csharp
// CSVLoader.Load("ファイル名") でCSVを読み込む（拡張子なし）
var rows = CSVLoader.Load("ScenarioData");
```

サブフォルダに置いた場合はパスを指定します。

```csharp
var rows = CSVLoader.Load("CSV/ScenarioData");
```

### 列名で値を取得する

```csharp
var rows = CSVLoader.Load("ScenarioData");

foreach (var row in rows)
{
    var sceneId     = row["scene_id"];
    var emotion     = row["emotion"];
    var displayText = row["display_text"];
    var typingText  = row["typing_text"];
}
```

### 値が空かどうか確認する

```csharp
foreach (var row in rows)
{
    // emotionが空のとき（配信者のみのシーンなど）はスキップ
    if (string.IsNullOrEmpty(row["emotion"]))
        continue;
}
```

### 別のCSVも同じように読める

```csharp
// シナリオCSV
var scenarioRows = CSVLoader.Load("ScenarioData");

// ダミーコメントCSV（列が違っても同じ書き方でOK）
var dummyRows = CSVLoader.Load("DummyCommentData");

foreach (var row in dummyRows)
{
    var username = row["username"];
    var comment  = row["comment"];
}
```

---

## 対応している機能

| 機能 | 対応 |
|------|------|
| ヘッダー行あり CSV | ✅ |
| カンマを含むフィールド（`"` で囲む） | ✅ |
| ダブルクォートのエスケープ（`""` → `"`） | ✅ |
| 空行のスキップ | ✅ |
| Windows / Mac / Unix 改行コード | ✅ |
| WebGL（unityroom）での動作 | ✅ |
| 存在しないファイルの警告ログ | ✅ |

---

## 注意事項

- CSVファイルは必ず `Assets/Resources/` フォルダ以下に置いてください
- ファイル名の指定は**拡張子なし**です（`"ScenarioData"` ← `.csv` は不要）
- 列名は**完全一致**で取得します（スペースに注意してください）
- 存在しない列名を指定するとエラーになります。事前に `row.ContainsKey("列名")` で確認してください

---

## ライセンス

MIT License  
自由に改変・再配布できます。
