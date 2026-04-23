using UnityEngine;

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

    // -------------------------------------------------------
    // 公開プロパティ
    // -------------------------------------------------------

    /// <summary>現在のゲーム状態</summary>
    public GameState CurrentState { get; private set; } = GameState.EmotionSelect;

    // -------------------------------------------------------
    // 内部データ
    // -------------------------------------------------------

    // 現在選択中のスパチャ色（nullならスパチャなし）
    private Color? _currentSuperchatColor = null;

    // -------------------------------------------------------
    // Unity ライフサイクル
    // -------------------------------------------------------

    private void Start()
    {
        // タイピング完了イベントを登録
        _typingManager.OnComplete += OnTypingComplete;

        // 最初のシーンを開始
        StartCurrentScene();
    }

    private void Update()
    {
        // タイピング中のみ入力を受け付ける
        if (CurrentState == GameState.Typing)
            _typingManager.Input(Input.inputString);
    }

    private void OnDestroy()
    {
        if (_typingManager != null)
            _typingManager.OnComplete -= OnTypingComplete;
    }

    // -------------------------------------------------------
    // 公開メソッド（UIから呼ぶ）
    // -------------------------------------------------------

    /// <summary>
    /// 感情ボタンが押されたときに呼びます。
    /// </summary>
    /// <param name="emotion">選択した感情ラベル（例："好意"）</param>
    public void OnEmotionSelected(string emotion)
    {
        if (CurrentState != GameState.EmotionSelect) return;

        var displayText = _scenarioManager.SelectEmotion(emotion);
        if (displayText == null) return;

        var typingText = _scenarioManager.GetTypingText();
        _typingManager.SetText(typingText);

        ChangeState(GameState.Typing);
    }

    /// <summary>
    /// スパチャ額ボタンが押されたときに呼びます。
    /// 感情選択前に呼んでください。
    /// </summary>
    /// <param name="color">スパチャの背景色</param>
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

    /// <summary>
    /// 現在のシーンを開始します。
    /// </summary>
    private void StartCurrentScene()
    {
        // 配信者のみ喋るシーンの場合
        if (_scenarioManager.IsStreamerOnlyScene())
        {
            var reaction = _scenarioManager.GetStreamerReaction();
            _streamerDialogueController.Show(reaction);
            ChangeState(GameState.Reaction);

            // 一定時間後に次のシーンへ
            Invoke(nameof(GoToNextScene), _nextSceneDelay);
            return;
        }

        // 通常シーン：感情選択待ち
        ChangeState(GameState.EmotionSelect);
    }

    /// <summary>
    /// タイピング完了時に呼ばれます。
    /// </summary>
    private void OnTypingComplete()
    {
        // 打ったコメント（display_text）をコメント欄に追加
        var displayText = _scenarioManager.GetDisplayText();
        _commentManager.AddPlayerComment(
            _playerUsername,
            displayText,
            _currentSuperchatColor
        );

        // スパチャ色をリセット
        _currentSuperchatColor = null;

        // 配信者リアクションを表示
        var reaction = _scenarioManager.GetStreamerReaction();
        _streamerDialogueController.Show(reaction);

        ChangeState(GameState.Reaction);

        // 一定時間後に次のシーンへ
        Invoke(nameof(GoToNextScene), _nextSceneDelay);
    }

    /// <summary>
    /// 次のシーンへ進みます。
    /// </summary>
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

    /// <summary>
    /// ゲーム終了時の処理。
    /// </summary>
    private void OnGameFinished()
    {
        Debug.Log("[GameManager] ゲームクリア！");
        // TODO: エンディング演出やシーン遷移をここに追加
    }

    /// <summary>
    /// ゲームの状態を変更します。
    /// </summary>
    private void ChangeState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"[GameManager] 状態変更: {newState}");
    }
}
