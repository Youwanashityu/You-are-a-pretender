using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// タイピング判定を管理するクラス。
/// ひらがなをローマ字に変換し、ゆらぎを考慮しながら入力を判定します。
/// </summary>
public class TypingManager : MonoBehaviour
{
    // -------------------------------------------------------
    // イベント
    // -------------------------------------------------------

    /// <summary>1文字正しく打てたときに発火。引数は進捗（0.0〜1.0）</summary>
    public event Action<float> OnProgress;

    /// <summary>全文字打ち終わったときに発火</summary>
    public event Action OnComplete;

    /// <summary>ミスタイプしたときに発火</summary>
    public event Action OnMiss;

    // -------------------------------------------------------
    // 内部データ
    // -------------------------------------------------------

    // 現在のお題（ひらがな）
    private string _hiragana = string.Empty;

    // ひらがなをローマ字候補リストに変換したもの
    // 例：「し」→ ["si", "shi"]
    private List<List<string>> _romanCandidates = new List<List<string>>();

    // 現在何文字目のひらがなを打っているか
    private int _hiraganaIndex = 0;

    // 現在のひらがなに対して何文字ローマ字を打ったか
    private int _romanProgress = 0;

    // 現在のひらがなで有効な候補を絞り込んだリスト
    private List<string> _activeCandidates = new List<string>();

    // 確定した文字の実際の入力を記録するリスト（shiで打ったらshiと記録）
    private List<string> _confirmedInputs = new List<string>();

    // -------------------------------------------------------
    // 公開プロパティ
    // -------------------------------------------------------

    /// <summary>現在のお題（ひらがな）</summary>
    public string Hiragana => _hiragana;

    /// <summary>完了しているかどうか</summary>
    public bool IsCompleted => _hiraganaIndex >= _romanCandidates.Count;

    /// <summary>進捗（0.0〜1.0）</summary>
    public float Progress => _romanCandidates.Count == 0 ? 0f
        : (float)_hiraganaIndex / _romanCandidates.Count;

    /// <summary>
    /// これまでに入力されたローマ字文字列を返します。
    /// InputTextの表示に使ってください。
    /// </summary>
    public string GetInputtedRomaji()
    {
        var sb = new System.Text.StringBuilder();

        // 確定した文字は実際の入力を使う（shiで打ったらshi）
        foreach (var confirmed in _confirmedInputs)
            sb.Append(confirmed);

        // 現在入力中の文字の途中まで
        if (_hiraganaIndex < _romanCandidates.Count && _romanProgress > 0)
            sb.Append(_activeCandidates[0].Substring(0, _romanProgress));

        return sb.ToString();
    }

    /// <summary>
    /// 打った部分・未打ち部分を色分けしたリッチテキストを返します。
    /// TMP_TextのInputTextに使ってください。
    /// </summary>
    /// <param name="typedColor">打った部分の色（例："#FFFFFF"）</param>
    /// <param name="remainingColor">未打ち部分の色（例："#888888"）</param>
    public string GetColoredRomaji(string typedColor = "#FFFFFF", string remainingColor = "#888888")
    {
        var sb = new System.Text.StringBuilder();

        // 確定した文字は実際の入力を使う（shiで打ったらshi）
        foreach (var confirmed in _confirmedInputs)
            sb.Append($"<color={typedColor}>{confirmed}</color>");

        if (_hiraganaIndex < _romanCandidates.Count)
        {
            // 現在打っている文字の打ち済み部分（白）
            if (_romanProgress > 0)
            {
                var inProgress = _activeCandidates[0].Substring(0, _romanProgress);
                sb.Append($"<color={typedColor}>{inProgress}</color>");
            }

            // 現在打っている文字の残り部分（グレー）
            var remaining = _activeCandidates[0].Substring(_romanProgress);
            sb.Append($"<color={remainingColor}>{remaining}</color>");

            // まだ打っていない文字（グレー）
            for (int i = _hiraganaIndex + 1; i < _romanCandidates.Count; i++)
            {
                sb.Append($"<color={remainingColor}>{_romanCandidates[i][0]}</color>");
            }
        }

        return sb.ToString();
    }

    // -------------------------------------------------------
    // 公開メソッド
    // -------------------------------------------------------

    /// <summary>
    /// タイピングするひらがなをセットします。
    /// ScenarioManagerからGetTypingText()で取得した値を渡してください。
    /// </summary>
    public void SetText(string hiragana)
    {
        _hiragana         = hiragana;
        _hiraganaIndex    = 0;
        _romanProgress    = 0;
        _confirmedInputs  = new List<string>();
        _romanCandidates  = BuildRomanCandidates(hiragana);
        _activeCandidates = _hiraganaIndex < _romanCandidates.Count
            ? new List<string>(_romanCandidates[_hiraganaIndex])
            : new List<string>();

        Debug.Log($"[TypingManager] セット: {hiragana}（{_romanCandidates.Count}文字）");
    }

    /// <summary>
    /// キー入力を受け取って判定します。
    /// Update()内でInput.inputStringを渡してください。
    /// </summary>
    public void Input(string inputString)
    {
        if (IsCompleted) return;

        foreach (char c in inputString)
        {
            if (IsCompleted) break;
            ProcessChar(c);
        }
    }

    // -------------------------------------------------------
    // 入力処理
    // -------------------------------------------------------

    private void ProcessChar(char c)
    {
        var input = c.ToString().ToLower();

        // 現在の候補のうち、次の1文字が一致するものに絞り込む
        var matched = new List<string>();
        foreach (var candidate in _activeCandidates)
        {
            if (_romanProgress < candidate.Length && candidate[_romanProgress].ToString() == input)
                matched.Add(candidate);
        }

        if (matched.Count == 0)
        {
            // ミスタイプ
            OnMiss?.Invoke();
            return;
        }

        _romanProgress++;
        _activeCandidates = matched;

        // いずれかの候補を打ち切ったら次のひらがなへ
        bool finished = false;
        foreach (var candidate in _activeCandidates)
        {
            if (_romanProgress >= candidate.Length)
            {
                finished = true;
                break;
            }
        }

        if (finished)
        {
            // 実際に打ったローマ字を記録
            _confirmedInputs.Add(_activeCandidates[0].Substring(0, _romanProgress));

            _hiraganaIndex++;
            _romanProgress = 0;
            _activeCandidates = _hiraganaIndex < _romanCandidates.Count
                ? new List<string>(_romanCandidates[_hiraganaIndex])
                : new List<string>();
        }

        OnProgress?.Invoke(Progress);

        if (IsCompleted)
            OnComplete?.Invoke();
    }

    // -------------------------------------------------------
    // ひらがな→ローマ字変換
    // -------------------------------------------------------

    /// <summary>
    /// ひらがな文字列を1文字ずつローマ字候補リストに変換します。
    /// </summary>
    private List<List<string>> BuildRomanCandidates(string hiragana)
    {
        var result = new List<List<string>>();
        int i = 0;

        while (i < hiragana.Length)
        {
            // 2文字で1セット（きゃ、しゃ など）を先にチェック
            if (i + 1 < hiragana.Length)
            {
                var two = hiragana.Substring(i, 2);
                if (TwoCharMap.TryGetValue(two, out var twoCandidates))
                {
                    result.Add(new List<string>(twoCandidates));
                    i += 2;
                    continue;
                }
            }

            // っ（促音）は次の子音を2回打つパターンも追加
            if (hiragana[i] == 'っ' && i + 1 < hiragana.Length)
            {
                var next = hiragana[i + 1].ToString();
                var candidates = new List<string> { "ltu", "xtu", "ltsu" };

                // 次の文字の子音を2回打つパターンを追加
                if (OneCharMap.TryGetValue(next, out var nextCandidates))
                {
                    foreach (var nc in nextCandidates)
                    {
                        if (nc.Length > 0)
                            candidates.Add(nc[0].ToString() + nc[0].ToString());
                    }
                }

                result.Add(candidates);
                i++;
                continue;
            }

            // 1文字変換
            var one = hiragana[i].ToString();
            if (OneCharMap.TryGetValue(one, out var oneCandidates))
            {
                result.Add(new List<string>(oneCandidates));
            }
            else
            {
                // 変換できない文字はそのまま（英数字など）
                result.Add(new List<string> { one });
            }

            i++;
        }

        return result;
    }

    // -------------------------------------------------------
    // 変換テーブル
    // -------------------------------------------------------

    // 1文字変換テーブル（ひらがな → ローマ字候補リスト）
    private static readonly Dictionary<string, List<string>> OneCharMap
        = new Dictionary<string, List<string>>
    {
        // あ行
        { "あ", new List<string> { "a" } },
        { "い", new List<string> { "i", "yi" } },
        { "う", new List<string> { "u", "wu", "whu" } },
        { "え", new List<string> { "e" } },
        { "お", new List<string> { "o" } },
        // か行
        { "か", new List<string> { "ka", "ca" } },
        { "き", new List<string> { "ki" } },
        { "く", new List<string> { "ku", "cu", "qu" } },
        { "け", new List<string> { "ke" } },
        { "こ", new List<string> { "ko", "co" } },
        // さ行
        { "さ", new List<string> { "sa" } },
        { "し", new List<string> { "si", "shi", "ci" } },
        { "す", new List<string> { "su" } },
        { "せ", new List<string> { "se", "ce" } },
        { "そ", new List<string> { "so" } },
        // た行
        { "た", new List<string> { "ta" } },
        { "ち", new List<string> { "ti", "chi" } },
        { "つ", new List<string> { "tu", "tsu" } },
        { "て", new List<string> { "te" } },
        { "と", new List<string> { "to" } },
        // な行
        { "な", new List<string> { "na" } },
        { "に", new List<string> { "ni" } },
        { "ぬ", new List<string> { "nu" } },
        { "ね", new List<string> { "ne" } },
        { "の", new List<string> { "no" } },
        // は行
        { "は", new List<string> { "ha" } },
        { "ひ", new List<string> { "hi" } },
        { "ふ", new List<string> { "fu", "hu" } },
        { "へ", new List<string> { "he" } },
        { "ほ", new List<string> { "ho" } },
        // ま行
        { "ま", new List<string> { "ma" } },
        { "み", new List<string> { "mi" } },
        { "む", new List<string> { "mu" } },
        { "め", new List<string> { "me" } },
        { "も", new List<string> { "mo" } },
        // や行
        { "や", new List<string> { "ya" } },
        { "ゆ", new List<string> { "yu" } },
        { "よ", new List<string> { "yo" } },
        // ら行
        { "ら", new List<string> { "ra" } },
        { "り", new List<string> { "ri" } },
        { "る", new List<string> { "ru" } },
        { "れ", new List<string> { "re" } },
        { "ろ", new List<string> { "ro" } },
        // わ行
        { "わ", new List<string> { "wa" } },
        { "ゐ", new List<string> { "wi" } },
        { "ゑ", new List<string> { "we" } },
        { "を", new List<string> { "wo" } },
        { "ん", new List<string> { "nn", "xn" } },
        // が行
        { "が", new List<string> { "ga" } },
        { "ぎ", new List<string> { "gi" } },
        { "ぐ", new List<string> { "gu" } },
        { "げ", new List<string> { "ge" } },
        { "ご", new List<string> { "go" } },
        // ざ行
        { "ざ", new List<string> { "za" } },
        { "じ", new List<string> { "zi", "ji" } },
        { "ず", new List<string> { "zu" } },
        { "ぜ", new List<string> { "ze" } },
        { "ぞ", new List<string> { "zo" } },
        // だ行
        { "だ", new List<string> { "da" } },
        { "ぢ", new List<string> { "di" } },
        { "づ", new List<string> { "du" } },
        { "で", new List<string> { "de" } },
        { "ど", new List<string> { "do" } },
        // ば行
        { "ば", new List<string> { "ba" } },
        { "び", new List<string> { "bi" } },
        { "ぶ", new List<string> { "bu" } },
        { "べ", new List<string> { "be" } },
        { "ぼ", new List<string> { "bo" } },
        // ぱ行
        { "ぱ", new List<string> { "pa" } },
        { "ぴ", new List<string> { "pi" } },
        { "ぷ", new List<string> { "pu" } },
        { "ぺ", new List<string> { "pe" } },
        { "ぽ", new List<string> { "po" } },
        // 小文字
        { "ぁ", new List<string> { "la", "xa" } },
        { "ぃ", new List<string> { "li", "xi", "lyi", "xyi" } },
        { "ぅ", new List<string> { "lu", "xu" } },
        { "ぇ", new List<string> { "le", "xe", "lye", "xye" } },
        { "ぉ", new List<string> { "lo", "xo" } },
        { "っ", new List<string> { "ltu", "xtu", "ltsu" } },
        { "ゃ", new List<string> { "lya", "xya" } },
        { "ゅ", new List<string> { "lyu", "xyu" } },
        { "ょ", new List<string> { "lyo", "xyo" } },
        // 記号
        { "ー", new List<string> { "-" } },
        { "、", new List<string> { "," } },
        { "。", new List<string> { "." } },
        { "！", new List<string> { "!" } },
        { "？", new List<string> { "?" } },
        { "w",  new List<string> { "w" } },
    };

    // 2文字変換テーブル（きゃ・しゃ など）
    private static readonly Dictionary<string, List<string>> TwoCharMap
        = new Dictionary<string, List<string>>
    {
        { "きゃ", new List<string> { "kya" } },
        { "きぃ", new List<string> { "kyi" } },
        { "きゅ", new List<string> { "kyu" } },
        { "きぇ", new List<string> { "kye" } },
        { "きょ", new List<string> { "kyo" } },
        { "しゃ", new List<string> { "sha", "sya" } },
        { "しぃ", new List<string> { "syi" } },
        { "しゅ", new List<string> { "shu", "syu" } },
        { "しぇ", new List<string> { "she", "sye" } },
        { "しょ", new List<string> { "sho", "syo" } },
        { "ちゃ", new List<string> { "cha", "tya", "cya" } },
        { "ちぃ", new List<string> { "tyi", "cyi" } },
        { "ちゅ", new List<string> { "chu", "tyu", "cyu" } },
        { "ちぇ", new List<string> { "che", "tye", "cye" } },
        { "ちょ", new List<string> { "cho", "tyo", "cyo" } },
        { "にゃ", new List<string> { "nya" } },
        { "にぃ", new List<string> { "nyi" } },
        { "にゅ", new List<string> { "nyu" } },
        { "にぇ", new List<string> { "nye" } },
        { "にょ", new List<string> { "nyo" } },
        { "ひゃ", new List<string> { "hya" } },
        { "ひぃ", new List<string> { "hyi" } },
        { "ひゅ", new List<string> { "hyu" } },
        { "ひぇ", new List<string> { "hye" } },
        { "ひょ", new List<string> { "hyo" } },
        { "みゃ", new List<string> { "mya" } },
        { "みぃ", new List<string> { "myi" } },
        { "みゅ", new List<string> { "myu" } },
        { "みぇ", new List<string> { "mye" } },
        { "みょ", new List<string> { "myo" } },
        { "りゃ", new List<string> { "rya" } },
        { "りぃ", new List<string> { "ryi" } },
        { "りゅ", new List<string> { "ryu" } },
        { "りぇ", new List<string> { "rye" } },
        { "りょ", new List<string> { "ryo" } },
        { "ぎゃ", new List<string> { "gya" } },
        { "ぎぃ", new List<string> { "gyi" } },
        { "ぎゅ", new List<string> { "gyu" } },
        { "ぎぇ", new List<string> { "gye" } },
        { "ぎょ", new List<string> { "gyo" } },
        { "じゃ", new List<string> { "ja", "zya", "jya" } },
        { "じぃ", new List<string> { "zyi", "jyi" } },
        { "じゅ", new List<string> { "ju", "zyu", "jyu" } },
        { "じぇ", new List<string> { "je", "zye", "jye" } },
        { "じょ", new List<string> { "jo", "zyo", "jyo" } },
        { "びゃ", new List<string> { "bya" } },
        { "びぃ", new List<string> { "byi" } },
        { "びゅ", new List<string> { "byu" } },
        { "びぇ", new List<string> { "bye" } },
        { "びょ", new List<string> { "byo" } },
        { "ぴゃ", new List<string> { "pya" } },
        { "ぴぃ", new List<string> { "pyi" } },
        { "ぴゅ", new List<string> { "pyu" } },
        { "ぴぇ", new List<string> { "pye" } },
        { "ぴょ", new List<string> { "pyo" } },
        { "ふぁ", new List<string> { "fa" } },
        { "ふぃ", new List<string> { "fi" } },
        { "ふぇ", new List<string> { "fe" } },
        { "ふぉ", new List<string> { "fo" } },
        { "てぃ", new List<string> { "thi" } },
        { "でぃ", new List<string> { "dhi" } },
        { "でゅ", new List<string> { "dyu" } },
        { "うぁ", new List<string> { "wha" } },
        { "うぃ", new List<string> { "whi", "wi" } },
        { "うぇ", new List<string> { "whe", "we" } },
        { "うぉ", new List<string> { "who" } },
    };
}