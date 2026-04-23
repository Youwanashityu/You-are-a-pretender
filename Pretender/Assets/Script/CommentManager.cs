using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// コメント欄全体を管理するクラス。
/// 主人公・ダミーコメントをコメント欄に流します。
/// 配信者の発言はStreamerDialogueControllerが担当します。
/// </summary>
public class CommentManager : MonoBehaviour
{
    // -------------------------------------------------------
    // 定数
    // -------------------------------------------------------

    private const string DummyCsvName = "DummyCommentData";

    // -------------------------------------------------------
    // インスペクター設定
    // -------------------------------------------------------

    [Header("コメント欄UI")]
    [Tooltip("コメントを縦に並べる親オブジェクト（ScrollViewのContentなど）")]
    [SerializeField] private Transform _commentContainer;
    [Tooltip("コメント1行分のPrefab")]
    [SerializeField] private GameObject _commentPrefab;
    [Tooltip("コメント欄に表示する最大件数（超えたら古いものから削除）")]
    [SerializeField] private int _maxCommentCount = 20;

    [Header("ダミーコメント設定")]
    [Tooltip("ダミーコメントが流れる間隔（秒）")]
    [SerializeField] private float _dummyCommentInterval = 3f;
    [Tooltip("ONにすると起動時に自動でダミーコメントを流し始める")]
    [SerializeField] private bool _autoPlayDummy = true;

    // -------------------------------------------------------
    // 内部データ
    // -------------------------------------------------------

    /// <summary>
    /// ダミーコメント1件分のデータ
    /// </summary>
    private class DummyCommentRow
    {
        public string CommentId;
        public string Username;
        public string Comment;
    }

    private List<DummyCommentRow> _dummyComments = new List<DummyCommentRow>();
    private List<GameObject> _activeComments = new List<GameObject>();
    private int _dummyIndex = 0;
    private Coroutine _dummyCoroutine;

    // -------------------------------------------------------
    // コメントの種類
    // -------------------------------------------------------

    public enum CommentType
    {
        Player,     // 主人公（プレイヤー）のコメント
        Dummy,      // ダミー視聴者のコメント
    }

    // -------------------------------------------------------
    // Unity ライフサイクル
    // -------------------------------------------------------

    private void Awake()
    {
        LoadDummyComments();
    }

    private void Start()
    {
        if (_autoPlayDummy)
            StartDummyComments();
    }

    private void OnDestroy()
    {
        StopDummyComments();
    }

    // -------------------------------------------------------
    // 初期化
    // -------------------------------------------------------

    /// <summary>
    /// ダミーコメントCSVを読み込みます。
    /// </summary>
    private void LoadDummyComments()
    {
        _dummyComments.Clear();

        var rows = CSVLoader.Load(DummyCsvName);

        foreach (var row in rows)
        {
            _dummyComments.Add(new DummyCommentRow
            {
                CommentId = row["comment_id"],
                Username  = row["username"],
                Comment   = row["comment"],
            });
        }

        // ランダムな順番で流すためシャッフル
        Shuffle(_dummyComments);

        Debug.Log($"[CommentManager] ダミーコメント読み込み完了: {_dummyComments.Count}件");
    }

    // -------------------------------------------------------
    // 公開メソッド
    // -------------------------------------------------------

    /// <summary>
    /// 主人公のコメントをコメント欄に追加します。
    /// タイピング完了時に呼んでください。
    /// </summary>
    /// <param name="username">主人公のユーザー名</param>
    /// <param name="comment">コメントテキスト</param>
    /// <param name="superchatColor">スパチャ背景色（スパチャなしならnull）</param>
    public void AddPlayerComment(string username, string comment, Color? superchatColor = null)
    {
        AddComment(username, comment, CommentType.Player, superchatColor);
    }

    /// <summary>
    /// ダミーコメントの自動再生を開始します。
    /// </summary>
    public void StartDummyComments()
    {
        if (_dummyCoroutine != null)
            StopCoroutine(_dummyCoroutine);

        _dummyCoroutine = StartCoroutine(DummyCommentCoroutine());
    }

    /// <summary>
    /// ダミーコメントの自動再生を停止します。
    /// </summary>
    public void StopDummyComments()
    {
        if (_dummyCoroutine != null)
        {
            StopCoroutine(_dummyCoroutine);
            _dummyCoroutine = null;
        }
    }

    /// <summary>
    /// コメント欄を全件削除します。
    /// </summary>
    public void ClearComments()
    {
        foreach (var comment in _activeComments)
        {
            if (comment != null)
                Destroy(comment);
        }
        _activeComments.Clear();
    }

    // -------------------------------------------------------
    // 内部処理
    // -------------------------------------------------------

    /// <summary>
    /// コメントをコメント欄に追加します。
    /// </summary>
    private void AddComment(string username, string comment, CommentType type, Color? superchatColor = null)
    {
        if (_commentPrefab == null || _commentContainer == null)
        {
            Debug.LogWarning("[CommentManager] CommentPrefabまたはCommentContainerが未設定です");
            return;
        }

        // Prefabを生成してコメント欄に追加
        var obj = Instantiate(_commentPrefab, _commentContainer);
        _activeComments.Add(obj);

        // CommentItemコンポーネントに表示内容をセット
        var item = obj.GetComponent<CommentItem>();
        if (item != null)
            item.Setup(username, comment, type, superchatColor);

        // 最大件数を超えたら古いコメントを削除
        while (_activeComments.Count > _maxCommentCount)
        {
            var oldest = _activeComments[0];
            _activeComments.RemoveAt(0);
            if (oldest != null)
                Destroy(oldest);
        }
    }

    /// <summary>
    /// 一定間隔でダミーコメントを流すコルーチン。
    /// ダミーコメントを全件流したらシャッフルして最初から繰り返します。
    /// </summary>
    private IEnumerator DummyCommentCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_dummyCommentInterval);

            if (_dummyComments.Count == 0) yield break;

            var row = _dummyComments[_dummyIndex];
            AddComment(row.Username, row.Comment, CommentType.Dummy);

            _dummyIndex++;
            if (_dummyIndex >= _dummyComments.Count)
            {
                _dummyIndex = 0;
                Shuffle(_dummyComments);
            }
        }
    }

    /// <summary>
    /// リストをFisher-Yatesアルゴリズムでシャッフルします。
    /// </summary>
    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}