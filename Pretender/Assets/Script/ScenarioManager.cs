using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// シナリオの進行を管理するクラス。
/// CSVLoaderでシナリオデータを読み込み、
/// 感情選択・タイピング完了・シーン進行を管理します。
/// </summary>
public class ScenarioManager : MonoBehaviour
{
    // -------------------------------------------------------
    // 定数
    // -------------------------------------------------------

    // ResourcesフォルダのCSVファイル名（拡張子なし）
    private const string ScenarioCsvName = "ScenarioData";

    // -------------------------------------------------------
    // 内部データ
    // -------------------------------------------------------

    /// <summary>
    /// 1行分のシナリオデータ
    /// </summary>
    private class ScenarioRow
    {
        public string SceneId;           // シーンID
        public string Emotion;           // 感情ラベル（好意・揶揄う・応援・挨拶）
        public string DisplayText;       // 画面に表示する日本語テキスト
        public string TypingText;        // タイピング判定に使うひらがなテキスト
        public string StreamerReaction;  // 配信者のリアクション
    }

    // シーンIDをキーにした選択肢リスト
    // 同じシーンIDに複数の感情が紐づく
    private Dictionary<string, List<ScenarioRow>> _scenarioData
        = new Dictionary<string, List<ScenarioRow>>();

    // 全シーンIDを順番に保持するリスト（一本道の進行に使う）
    private List<string> _sceneOrder = new List<string>();

    // 現在のシーンインデックス
    private int _currentSceneIndex = 0;

    // 現在選択中の行データ
    private ScenarioRow _selectedRow = null;

    // -------------------------------------------------------
    // 公開プロパティ
    // -------------------------------------------------------

    /// <summary>現在のシーンID</summary>
    public string CurrentSceneId =>
        _sceneOrder.Count > 0 ? _sceneOrder[_currentSceneIndex] : null;

    /// <summary>最後のシーンかどうか</summary>
    public bool IsLastScene =>
        _currentSceneIndex >= _sceneOrder.Count - 1;

    // -------------------------------------------------------
    // Unity ライフサイクル
    // -------------------------------------------------------

    private void Awake()
    {
        LoadScenario();
    }

    // -------------------------------------------------------
    // 初期化
    // -------------------------------------------------------

    /// <summary>
    /// CSVを読み込んでシナリオデータを構築します。
    /// </summary>
    private void LoadScenario()
    {
        _scenarioData.Clear();
        _sceneOrder.Clear();

        var rows = CSVLoader.Load(ScenarioCsvName);

        foreach (var row in rows)
        {
            var sceneId         = row["scene_id"];
            var emotion         = row["emotion"];
            var displayText     = row["display_text"];
            var typingText      = row["typing_text"];
            var streamerReaction = row["streamer_reaction"];

            var scenarioRow = new ScenarioRow
            {
                SceneId          = sceneId,
                Emotion          = emotion,
                DisplayText      = displayText,
                TypingText       = typingText,
                StreamerReaction = streamerReaction,
            };

            // シーンIDのリストに追加（順番を保持）
            if (!_scenarioData.ContainsKey(sceneId))
            {
                _scenarioData[sceneId] = new List<ScenarioRow>();
                _sceneOrder.Add(sceneId);
            }

            // 配信者のみ喋るシーン（emotion・displayText・typingTextが空）も登録する
            _scenarioData[sceneId].Add(scenarioRow);
        }

        Debug.Log($"[ScenarioManager] シナリオ読み込み完了: {_sceneOrder.Count}シーン");
    }

    // -------------------------------------------------------
    // 公開メソッド
    // -------------------------------------------------------

    /// <summary>
    /// 現在のシーンの選択肢（感情ラベルのリスト）を返します。
    /// 配信者のみ喋るシーンの場合は空リストを返します。
    /// </summary>
    public List<string> GetCurrentEmotions()
    {
        if (!_scenarioData.TryGetValue(CurrentSceneId, out var rows))
            return new List<string>();

        // emotionが空のシーン＝配信者のみ喋るシーン
        return rows
            .Where(r => !string.IsNullOrEmpty(r.Emotion))
            .Select(r => r.Emotion)
            .ToList();
    }

    /// <summary>
    /// 感情を選択したときに呼びます。
    /// 選択した感情に対応するDisplayTextを返します。
    /// </summary>
    /// <param name="emotion">選択した感情ラベル</param>
    /// <returns>表示用テキスト。見つからない場合はnull。</returns>
    public string SelectEmotion(string emotion)
    {
        if (!_scenarioData.TryGetValue(CurrentSceneId, out var rows))
        {
            Debug.LogWarning($"[ScenarioManager] シーンが見つかりません: {CurrentSceneId}");
            return null;
        }

        _selectedRow = rows.FirstOrDefault(r => r.Emotion == emotion);

        if (_selectedRow == null)
        {
            Debug.LogWarning($"[ScenarioManager] 感情が見つかりません: {emotion}");
            return null;
        }

        return _selectedRow.DisplayText;
    }

    /// <summary>
    /// タイピング判定に使うひらがなテキストを返します。
    /// SelectEmotionの後に呼んでください。
    /// </summary>
    public string GetTypingText()
    {
        return _selectedRow?.TypingText;
    }

    /// <summary>
    /// タイピング完了時に呼びます。
    /// 配信者のリアクションテキストを返します。
    /// </summary>
    public string GetStreamerReaction()
    {
        // 配信者のみ喋るシーンの場合
        if (_selectedRow == null)
        {
            if (_scenarioData.TryGetValue(CurrentSceneId, out var rows))
                return rows.FirstOrDefault()?.StreamerReaction;
        }

        return _selectedRow?.StreamerReaction;
    }

    /// <summary>
    /// 次のシーンに進みます。
    /// 最後のシーンの場合は進まずfalseを返します。
    /// </summary>
    /// <returns>次のシーンに進めた場合はtrue</returns>
    public bool NextScene()
    {
        if (IsLastScene)
        {
            Debug.Log("[ScenarioManager] 最後のシーンです");
            return false;
        }

        _currentSceneIndex++;
        _selectedRow = null;
        Debug.Log($"[ScenarioManager] シーン進行: {CurrentSceneId}");
        return true;
    }

    /// <summary>
    /// 現在のシーンが配信者のみ喋るシーンかどうかを返します。
    /// </summary>
    public bool IsStreamerOnlyScene()
    {
        if (!_scenarioData.TryGetValue(CurrentSceneId, out var rows))
            return false;

        return rows.All(r => string.IsNullOrEmpty(r.Emotion));
    }
}