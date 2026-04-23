using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 配信者の発言を配信画面エリアに字幕風で表示するクラス。
/// テキストがフェードイン→一定時間表示→フェードアウトします。
/// </summary>
public class StreamerDialogueController : MonoBehaviour
{
    // -------------------------------------------------------
    // インスペクター設定
    // -------------------------------------------------------

    [Header("UI")]
    [Tooltip("配信者の発言を表示するテキスト")]
    [SerializeField] private TMP_Text _dialogueText;

    [Header("タイミング設定")]
    [Tooltip("タイピング完了後、発言が表示されるまでの待機時間（秒）")]
    [SerializeField] private float _delayBeforeShow  = 0.8f;
    [Tooltip("テキストがじわっと表示されるフェードイン時間（秒）")]
    [SerializeField] private float _fadeInDuration   = 0.5f;
    [Tooltip("テキストが表示され続ける時間（秒）")]
    [SerializeField] private float _displayDuration  = 2.5f;
    [Tooltip("テキストがじわっと消えるフェードアウト時間（秒）")]
    [SerializeField] private float _fadeOutDuration  = 0.5f;

    // -------------------------------------------------------
    // 内部データ
    // -------------------------------------------------------

    private Coroutine _currentCoroutine;

    // -------------------------------------------------------
    // Unity ライフサイクル
    // -------------------------------------------------------

    private void Awake()
    {
        // 最初は非表示
        if (_dialogueText != null)
            SetAlpha(0f);
    }

    // -------------------------------------------------------
    // 公開メソッド
    // -------------------------------------------------------

    /// <summary>
    /// 配信者の発言を表示します。
    /// ScenarioManagerのGetStreamerReaction()で取得した文字列を渡してください。
    /// </summary>
    public void Show(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        // 表示中のものがあればキャンセルして上書き
        if (_currentCoroutine != null)
            StopCoroutine(_currentCoroutine);

        _currentCoroutine = StartCoroutine(ShowCoroutine(text));
    }

    /// <summary>
    /// 表示中の発言を即座に非表示にします。
    /// </summary>
    public void Hide()
    {
        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
            _currentCoroutine = null;
        }

        SetAlpha(0f);
    }

    // -------------------------------------------------------
    // 内部処理
    // -------------------------------------------------------

    private IEnumerator ShowCoroutine(string text)
    {
        // 前の表示が残っていたらフェードアウトしてからリセット
        SetAlpha(0f);

        // テキストをセット
        if (_dialogueText != null)
            _dialogueText.text = text;

        // タイピング完了後の間を置く
        yield return new WaitForSeconds(_delayBeforeShow);

        // フェードイン
        yield return StartCoroutine(FadeCoroutine(0f, 1f, _fadeInDuration));

        // 表示維持
        yield return new WaitForSeconds(_displayDuration);

        // フェードアウト
        yield return StartCoroutine(FadeCoroutine(1f, 0f, _fadeOutDuration));

        _currentCoroutine = null;
    }

    /// <summary>
    /// テキストのアルファ値を補間するコルーチン。
    /// </summary>
    private IEnumerator FadeCoroutine(float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var alpha = Mathf.Lerp(from, to, elapsed / duration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(to);
    }

    /// <summary>
    /// テキストのアルファ値をセットします。
    /// </summary>
    private void SetAlpha(float alpha)
    {
        if (_dialogueText == null) return;

        var color = _dialogueText.color;
        color.a = alpha;
        _dialogueText.color = color;
    }
}