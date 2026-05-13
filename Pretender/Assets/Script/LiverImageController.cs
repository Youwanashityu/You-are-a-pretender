using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 配信者の立ち絵を管理するクラス。
/// CSVのimage_typeに応じてスプライトを切り替えます。
/// </summary>
public class LiverImageController : MonoBehaviour
{
    // -------------------------------------------------------
    // インスペクター設定
    // -------------------------------------------------------

    [Header("UI")]
    [Tooltip("立ち絵を表示するImageコンポーネント")]
    [SerializeField] private Image _liverImage;

    [Header("画像設定")]
    [Tooltip("立ち絵画像の読み込みパス（Resources以下）例：StreamerImages/")]
    [SerializeField] private string _imageFolderPath = "StreamerImages/";

    // -------------------------------------------------------
    // 公開メソッド
    // -------------------------------------------------------

    /// <summary>
    /// 立ち絵を切り替えます。
    /// ScenarioManagerのGetImageType()で取得した値を渡してください。
    /// </summary>
    /// <param name="imageType">画像ファイル名（拡張子なし）例："smile"</param>
    public void SetImage(string imageType)
    {
        if (string.IsNullOrEmpty(imageType)) return;
        if (_liverImage == null)
        {
            Debug.LogWarning("[LiverImageController] LiverImageが未設定です");
            return;
        }

        var sprite = Resources.Load<Sprite>(_imageFolderPath + imageType);
        if (sprite == null)
        {
            Debug.LogWarning($"[LiverImageController] 画像が見つかりません: {_imageFolderPath}{imageType}");
            return;
        }

        _liverImage.sprite = sprite;
        Debug.Log($"[LiverImageController] 立ち絵切り替え: {imageType}");
    }
}