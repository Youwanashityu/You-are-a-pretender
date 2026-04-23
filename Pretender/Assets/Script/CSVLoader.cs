using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 汎用CSVローダー
/// Resources フォルダ以下に置いた CSV ファイルを
/// ヘッダー列名をキーにした Dictionary のリストとして返します。
/// 構造が異なる複数のCSVに対して同じローダーを使い回せます。
/// </summary>
public static class CSVLoader
{
    /// <summary>
    /// CSVを読み込み、行データのリストを返します。
    /// </summary>
    /// <param name="fileName">Resourcesフォルダ内のファイル名（拡張子なし）</param>
    /// <returns>ヘッダー名をキーにしたDictionaryのリスト。読み込み失敗時は空リスト。</returns>
    public static List<Dictionary<string, string>> Load(string fileName)
    {
        var result = new List<Dictionary<string, string>>();

        // Resourcesフォルダからテキストアセットを読み込む
        var textAsset = Resources.Load<TextAsset>(fileName);
        if (textAsset == null)
        {
            Debug.LogWarning($"[CSVLoader] ファイルが見つかりません: {fileName}");
            return result;
        }

        var lines = SplitLines(textAsset.text);
        if (lines.Count < 2)
        {
            Debug.LogWarning($"[CSVLoader] データが不足しています（ヘッダーのみ or 空）: {fileName}");
            return result;
        }

        // 1行目をヘッダーとして取得
        var headers = ParseLine(lines[0]);

        // 2行目以降をデータとして取得
        for (int i = 1; i < lines.Count; i++)
        {
            var line = lines[i].Trim();

            // 空行はスキップ
            if (string.IsNullOrEmpty(line)) continue;

            var values = ParseLine(line);
            var row = new Dictionary<string, string>();

            for (int j = 0; j < headers.Count; j++)
            {
                var key = headers[j].Trim();
                var value = j < values.Count ? values[j].Trim() : string.Empty;
                row[key] = value;
            }

            result.Add(row);
        }

        Debug.Log($"[CSVLoader] {fileName} を読み込みました（{result.Count}行）");
        return result;
    }

    /// <summary>
    /// テキストを改行で分割してリストとして返します。
    /// Windows/Mac/Unix の改行コードに対応します。
    /// </summary>
    private static List<string> SplitLines(string text)
    {
        // 改行コードを統一してから分割
        var normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
        var lines = new List<string>(normalized.Split('\n'));
        return lines;
    }

    /// <summary>
    /// CSV の1行をカンマで分割してリストとして返します。
    /// ダブルクォートで囲まれたフィールド内のカンマは無視します。
    /// </summary>
    private static List<string> ParseLine(string line)
    {
        var fields = new List<string>();
        var current = new System.Text.StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                // エスケープされたダブルクォート（""）の処理
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++; // 次の " をスキップ
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        // 最後のフィールドを追加
        fields.Add(current.ToString());
        return fields;
    }
}