using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// コメント1行分の表示を管理するコンポーネント。
/// コメント欄のPrefabにアタッチして使います。
/// </summary>
public class CommentItem : MonoBehaviour
{
    // -------------------------------------------------------
    // インスペクター設定
    // -------------------------------------------------------

    [Tooltip("ユーザー名を表示するテキスト")]
    [SerializeField] private TMP_Text _usernameText;
    [Tooltip("コメント本文を表示するテキスト")]
    [SerializeField] private TMP_Text _commentText;
    [Tooltip("スパチャ時の背景色に使うImageコンポーネント")]
    [SerializeField] private Image _backgroundImage;

    // -------------------------------------------------------
    // コメントタイプ別の色設定（インスペクターから変更可能）
    // -------------------------------------------------------

    [Header("ユーザー名の色")]
    [Tooltip("主人公（プレイヤー）のユーザー名の色")]
    [SerializeField] private Color _playerNameColor   = new Color(1f, 0.85f, 0f);
    [Tooltip("ダミー視聴者のユーザー名の色")]
    [SerializeField] private Color _dummyNameColor    = new Color(0.7f, 0.7f, 0.7f);

    [Header("背景色")]
    [Tooltip("スパチャなし時の背景色（デフォルトは透明）")]
    [SerializeField] private Color _defaultBgColor    = new Color(0f, 0f, 0f, 0f);

    // -------------------------------------------------------
    // 公開メソッド
    // -------------------------------------------------------

    /// <summary>
    /// コメントの表示内容をセットします。
    /// CommentManagerから呼ばれます。
    /// </summary>
    /// <param name="username">ユーザー名</param>
    /// <param name="comment">コメントテキスト</param>
    /// <param name="type">コメントの種類</param>
    /// <param name="superchatColor">スパチャ背景色（スパチャなしならnull）</param>
    public void Setup(string username, string comment, CommentManager.CommentType type, Color? superchatColor = null)
    {
        if (_usernameText != null)
        {
            _usernameText.text  = username;
            _usernameText.color = GetNameColor(type);
        }

        if (_commentText != null)
            _commentText.text = comment;

        if (_backgroundImage != null)
        {
            // スパチャ色が指定されていればその色、なければ透明
            _backgroundImage.color = superchatColor ?? _defaultBgColor;
        }
    }

    // -------------------------------------------------------
    // 内部処理
    // -------------------------------------------------------

    private Color GetNameColor(CommentManager.CommentType type)
    {
        return type switch
        {
            CommentManager.CommentType.Player => _playerNameColor,
            _                                 => _dummyNameColor,
        };
    }
}