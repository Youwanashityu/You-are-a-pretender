using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;

/// <summary>
/// ゲーム全体のBGM・SEを管理するシングルトンクラス。
/// Resourcesフォルダに置いたプレハブから自動生成されるため、
/// どのシーンから起動してもサウンドが正しく再生されます。
/// 音量調整はVolumeControllerに委譲しています。
/// </summary>
public class SoundManager : MonoBehaviour
{
    // -------------------------------------------------------
    // シングルトン
    // -------------------------------------------------------

    /// <summary>唯一のインスタンス（読み取り専用）</summary>
    public static SoundManager Instance { get; private set; }

    // -------------------------------------------------------
    // インスペクター設定
    // -------------------------------------------------------

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("BGM用 AudioSource")]
    [SerializeField] private AudioSource bgmSource;

    [Header("SE用 AudioSource")]
    [SerializeField] private AudioSource seSource;

    // -------------------------------------------------------
    // SE データ定義
    // -------------------------------------------------------

    /// <summary>
    /// SE 1件分のデータ。インスペクターで設定します。
    /// </summary>
    [System.Serializable]
    public class SoundData
    {
        [Header("サウンド名（PlaySE呼び出し時のキー）")]
        public string name;

        [Header("AudioClip")]
        public AudioClip clip;

        [Header("音量")]
        [Range(0f, 1f)]
        public float volume = 1f;
    }

    [Header("SE 一覧")]
    [SerializeField] private List<SoundData> seList = new();

    /// <summary>名前をキーにした SE の高速検索用辞書</summary>
    private Dictionary<string, SoundData> seDict;

    // -------------------------------------------------------
    // 自動初期化（どのシーンから起動しても動作する）
    // -------------------------------------------------------

    /// <summary>
    /// シーンロード前に呼ばれ、SoundManager プレハブを自動生成します。
    /// プレハブは Assets/Resources/SoundManager.prefab に配置してください。
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInitialize()
    {
        if (Instance != null) return;

        var prefab = Resources.Load<GameObject>("SoundManager");
        if (prefab != null)
        {
            Instantiate(prefab);
        }
        else
        {
            Debug.LogWarning("[SoundManager] Resources/SoundManager.prefab が見つかりません。");
        }
    }

    // -------------------------------------------------------
    // ライフサイクル
    // -------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // SE 辞書を構築
        seDict = new Dictionary<string, SoundData>();
        foreach (var s in seList)
        {
            if (!string.IsNullOrEmpty(s.name) && !seDict.ContainsKey(s.name))
                seDict.Add(s.name, s);
        }
    }

    // -------------------------------------------------------
    // BGM
    // -------------------------------------------------------

    /// <summary>
    /// BGM を再生します。すでに同じクリップが再生中の場合はスキップします。
    /// </summary>
    /// <param name="clip">再生する AudioClip</param>
    /// <param name="volume">音量（0〜1）</param>
    public void PlayBGM(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.volume = Mathf.Clamp01(volume);
        bgmSource.loop = true; // ループ追加
        bgmSource.Play();
    }




    /// <summary>BGM を停止します。</summary>
    public void StopBGM() => bgmSource.Stop();

    // -------------------------------------------------------
    // SE
    // -------------------------------------------------------

    /// <summary>
    /// 名前を指定して SE を再生します。
    /// 同じ SE がすでに再生中の場合は重複再生しません。
    /// </summary>
    /// <param name="name">SoundData に設定したサウンド名</param>
    public void PlaySE(string name)
    {
        if (!seDict.TryGetValue(name, out var data))
        {
            Debug.LogWarning($"[SoundManager] SE '{name}' が見つかりません。");
            return;
        }

        if (seSource.isPlaying && seSource.clip == data.clip) return;

        seSource.clip = data.clip;
        seSource.volume = data.volume;
        Debug.Log($"[SoundManager] Play直前 volume:{seSource.volume} clip:{seSource.clip?.name} output:{seSource.outputAudioMixerGroup}");
        seSource.Play();
        Debug.Log($"[SoundManager] Play直後 isPlaying:{seSource.isPlaying}");
    }
}