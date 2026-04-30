using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ゲーム全体の進行を統括するクラス。
/// 各マネージャーの橋渡しをしてゲームの状態を管理します。
/// </summary>
public class GameManager : MonoBehaviour
{
    // -------------------------------------------------------
    // ゲームの状態
    // -------------------------------------------------------

    public enum GameState
    {
        EmotionSelect,  // 感情選択中
        Typing,         // タイピング中
        Reaction,       // 配信者リアクション表示中
        Finished,       // ゲーム終了
    }

    // -------------------------------------------------------
    // インスペクター設定
    // -------------------------------------------------------

    [Header("マネージャー参照")]
    [Tooltip("シナリオ進行を管理するScenarioManager")]
    [SerializeField] private ScenarioManager _scenarioManager;

    [Tooltip("タイピング判定を管理するTypingManager")]
    [SerializeField] private TypingManager _typingManager;

    [Tooltip("コメント欄を管理するCommentManager")]
    [SerializeField] private CommentManager _commentManager;

    [Tooltip("配信者の発言を表示するStreamerDialogueController")]
    [SerializeField] private StreamerDialogueController _streamerDialogueController;

    [Header("プレイヤー設定")]
    [Tooltip("コメント欄に表示される主人公のユーザー名")]
    [SerializeField] private string _playerUsername = "名無し";

    [Header("リアクション設定")]
    [Tooltip("リアクション表示後、次のシーンへ進むまでの待機時間（秒）")]
    [SerializeField] private float _nextSceneDelay = 4f;

    [Header("UI参照")]
    [Tooltip("感情・スパチャボタンが並ぶパネル")]
    [SerializeField] private GameObject _selectionPanel;

    [Tooltip("お題と入力が表示されるパネル")]
    [SerializeField] private GameObject _typingPanel;

    [Tooltip("感情ボタンを並べる親オブジェクト（Horizontal Layout Group付き）")]
    [SerializeField] private Transform _emotionButtonsParent;

    [Tooltip("感情ボタン1個分のPrefab")]
    [SerializeField] private GameObject _emotionButtonPrefab;

    [Tooltip("お題テキスト（display_textを表示）")]
    [SerializeField] private TMP_Text _displayText;

    [Tooltip("プレイヤーの入力を表示するテキスト")]
    [SerializeField] private TMP_Text _inputText;

    // -------------------------------------------------------
    // 公開プロパティ
    // -------------------------------------------------------

    /// <summary>現在のゲーム状態</summary>
    public GameState CurrentState { get; private set; } = GameState.EmotionSelect;

    // -------------------------------------------------------
    // 内部データ
    // -------------------------------------------------------

    private Color? _currentSuperchatColor = null;
    private List<GameObject> _emotionButtons = new List<GameObject>();

    // -------------------------------------------------------
    // Unity ライフサイクル
    // -------------------------------------------------------

    private void Start()
    {
        _typingManager.OnComplete += OnTypingComplete;
        _typingManager.OnProgress += OnTypingProgress;

        StartCurrentScene();
    }

    private void Update()
    {
        if (CurrentState == GameState.Typing)
            _typingManager.Input(Input.inputString);
    }

    private void OnDestroy()
    {
        if (_typingManager != null)
        {
            _typingManager.OnComplete -= OnTypingComplete;
            _typingManager.OnProgress -= OnTypingProgress;
        }
    }

    // -------------------------------------------------------
    // 公開メソッド（UIから呼ぶ）
    // -------------------------------------------------------

    /// <summary>
    /// 感情ボタンが押されたときに呼びます。
    /// </summary>
    public void OnEmotionSelected(string emotion)
    {
        if (CurrentState != GameState.EmotionSelect) return;

        var displayText = _scenarioManager.SelectEmotion(emotion);
        if (displayText == null) return;

        if (_displayText != null)
            _displayText.text = displayText;

        if (_inputText != null)
            _inputText.text = string.Empty;

        var typingText = _scenarioManager.GetTypingText();
        _typingManager.SetText(typingText);

        ShowTypingPanel();
        ChangeState(GameState.Typing);
    }

    /// <summary>
    /// スパチャ額ボタンが押されたときに呼びます。
    /// </summary>
    public void OnSuperchatSelected(Color color)
    {
        _currentSuperchatColor = color;
    }

    /// <summary>
    /// スパチャなしが選択されたときに呼びます。
    /// </summary>
    public void OnSuperchatDeselected()
    {
        _currentSuperchatColor = null;
    }

    // -------------------------------------------------------
    // 内部処理
    // -------------------------------------------------------

    private void StartCurrentScene()
    {
        if (_scenarioManager.IsStreamerOnlyScene())
        {
            var reaction = _scenarioManager.GetStreamerReaction();
            _streamerDialogueController.Show(reaction);
            ChangeState(GameState.Reaction);
            Invoke(nameof(GoToNextScene), _nextSceneDelay);
            return;
        }

        GenerateEmotionButtons();
        ShowSelectionPanel();
        ChangeState(GameState.EmotionSelect);
    }

    /// <summary>
    /// CSVの感情リストからボタンを動的に生成します。
    /// </summary>
    private void GenerateEmotionButtons()
    {
        foreach (var btn in _emotionButtons)
        {
            if (btn != null) Destroy(btn);
        }
        _emotionButtons.Clear();

        if (_emotionButtonPrefab == null || _emotionButtonsParent == null) return;

        var emotions = _scenarioManager.GetCurrentEmotions();

        foreach (var emotion in emotions)
        {
            var obj = Instantiate(_emotionButtonPrefab, _emotionButtonsParent);
            _emotionButtons.Add(obj);

            var label = obj.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = emotion;

            var button = obj.GetComponent<Button>();
            if (button != null)
            {
                var capturedEmotion = emotion;
                button.onClick.AddListener(() => OnEmotionSelected(capturedEmotion));
            }
        }
    }

    private void OnTypingProgress(float progress)
    {
        if (_inputText != null)
            _inputText.text = _typingManager.GetInputtedRomaji();
    }

    private void OnTypingComplete()
    {
        var displayText = _scenarioManager.GetDisplayText();
        _commentManager.AddPlayerComment(_playerUsername, displayText, _currentSuperchatColor);

        _currentSuperchatColor = null;

        var reaction = _scenarioManager.GetStreamerReaction();
        _streamerDialogueController.Show(reaction);

        ChangeState(GameState.Reaction);
        Invoke(nameof(GoToNextScene), _nextSceneDelay);
    }

    private void GoToNextScene()
    {
        if (_scenarioManager.IsLastScene)
        {
            ChangeState(GameState.Finished);
            OnGameFinished();
            return;
        }

        _scenarioManager.NextScene();
        StartCurrentScene();
    }

    private void OnGameFinished()
    {
        Debug.Log("[GameManager] ゲームクリア！");
        // TODO: エンディング演出やシーン遷移をここに追加
    }

    private void ShowSelectionPanel()
    {
        if (_selectionPanel != null) _selectionPanel.SetActive(true);
        if (_typingPanel != null)    _typingPanel.SetActive(false);
    }

    private void ShowTypingPanel()
    {
        if (_selectionPanel != null) _selectionPanel.SetActive(false);
        if (_typingPanel != null)    _typingPanel.SetActive(true);
    }

    private void ChangeState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"[GameManager] 状態変更: {newState}");
    }
}
