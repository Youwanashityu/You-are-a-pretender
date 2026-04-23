using UnityEngine;

/// <summary>
/// ボタンやイベントに合わせてSEを再生するコンポーネント。
/// インスペクターでSE名を指定し、PlaySE()を呼び出すだけで再生できます。
/// </summary>
public class SEChange : MonoBehaviour
{
    // -------------------------------------------------------
    // インスペクター設定
    // -------------------------------------------------------

    [Header("再生するSE名（SoundManagerのSE一覧に登録した名前）")]
    [SerializeField] private string seName;

    // -------------------------------------------------------
    // SE再生
    // -------------------------------------------------------

    /// <summary>
    /// インスペクターで指定したSEをSoundManager経由で再生します。
    /// ボタンのOnClickやAnimationEventから呼び出せます。
    /// </summary>
    public void PlaySE()
    {
        if (SoundManager.Instance == null)
        {
            Debug.LogWarning("[SEChange] SoundManager が見つかりません。");
            return;
        }

        Debug.Log($"[SEChange] PlaySE呼び出し: {seName}");
        SoundManager.Instance.PlaySE(seName);
    }
}