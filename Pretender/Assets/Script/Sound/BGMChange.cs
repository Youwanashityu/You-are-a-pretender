using System.Collections;
using UnityEngine;

/// <summary>
/// シーンに置くだけでそのシーンのBGMを自動再生するコンポーネント。
/// SoundManagerの生成を1フレーム待ってから再生します。
/// </summary>
public class BGMChange : MonoBehaviour
{
    // -------------------------------------------------------
    // インスペクター設定
    // -------------------------------------------------------

    [Header("再生するBGM")]
    [SerializeField] private AudioClip bgmClip;

    // -------------------------------------------------------
    // ライフサイクル
    // -------------------------------------------------------

    /// <summary>
    /// 1フレーム待ってからBGMを再生します。
    /// （RuntimeInitializeOnLoadMethodで生成されるSoundManagerの初期化を待つため）
    /// </summary>
    private IEnumerator Start()
    {
        yield return null;
        SoundManager.Instance?.PlayBGM(bgmClip);
    }
}