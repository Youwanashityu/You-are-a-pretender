using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// プレイヤーがBGM・SEの音量をスライダーで調整し、
/// ボタンでミュートできるUIコントローラー。
/// AudioMixerを直接操作します。
/// </summary>
public class VolumeController : MonoBehaviour
{
    // -------------------------------------------------------
    // インスペクター設定
    // -------------------------------------------------------

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("BGM")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Button bgmMuteButton;

    [Header("SE")]
    [SerializeField] private Slider seSlider;
    [SerializeField] private Button seMuteButton;

    [Header("BGMミュートボタン画像")]
    [SerializeField] private Image bgmMuteButtonImage;
    [SerializeField] private Sprite bgmMuteOffSprite;  // 通常時の画像
    [SerializeField] private Sprite bgmMuteOnSprite;   // ミュート時の画像

    [Header("SEミュートボタン画像")]
    [SerializeField] private Image seMuteButtonImage;
    [SerializeField] private Sprite seMuteOffSprite;   // 通常時の画像
    [SerializeField] private Sprite seMuteOnSprite;    // ミュート時の画像

    // -------------------------------------------------------
    // 内部状態
    // -------------------------------------------------------

    /// <summary>ミュート前のBGM音量を保持する</summary>
    private float lastBGMVolume = 1f;

    /// <summary>ミュート前のSE音量を保持する</summary>
    private float lastSEVolume = 1f;

    /// <summary>BGMがミュート中かどうか</summary>
    private bool isBGMMuted = false;

    /// <summary>SEがミュート中かどうか</summary>
    private bool isSEMuted = false;

    // -------------------------------------------------------
    // ライフサイクル
    // -------------------------------------------------------

    private void Start()
    {
        // スライダーの範囲を設定
        bgmSlider.minValue = 0.1f;
        bgmSlider.maxValue = 1f;
        bgmSlider.value = 1f;

        seSlider.minValue = 0.1f;
        seSlider.maxValue = 1f;
        seSlider.value = 1f;

        // リスナー登録
        bgmSlider.onValueChanged.AddListener(OnBGMSliderChanged);
        seSlider.onValueChanged.AddListener(OnSESliderChanged);
        bgmMuteButton.onClick.AddListener(ToggleBGMMute);
        seMuteButton.onClick.AddListener(ToggleSEMute);
    }

    // -------------------------------------------------------
    // スライダー
    // -------------------------------------------------------

    /// <summary>
    /// BGMスライダーの値が変わったときに呼ばれます。
    /// </summary>
    private void OnBGMSliderChanged(float value)
    {
        // ミュート中にスライダーを動かしたらミュート解除
        if (isBGMMuted) isBGMMuted = false;

        lastBGMVolume = value;
        ApplyBGMVolume(value);
    }

    /// <summary>
    /// SEスライダーの値が変わったときに呼ばれます。
    /// </summary>
    private void OnSESliderChanged(float value)
    {
        if (isSEMuted) isSEMuted = false;

        lastSEVolume = value;
        ApplySEVolume(value);
    }

    // -------------------------------------------------------
    // ミュートボタン
    // -------------------------------------------------------

    /// <summary>
    /// BGMのミュートをトグルします。
    /// </summary>
    private void ToggleBGMMute()
    {
        isBGMMuted = !isBGMMuted;

        if (isBGMMuted)
        {
            ApplyBGMVolume(0f);
            bgmMuteButtonImage.sprite = bgmMuteOnSprite;
        }
        else
        {
            bgmSlider.value = lastBGMVolume;
            ApplyBGMVolume(lastBGMVolume);
            bgmMuteButtonImage.sprite = bgmMuteOffSprite;
        }
    }
    /// <summary>
    /// SEのミュートをトグルします。
    /// </summary>
    private void ToggleSEMute()
    {
        isSEMuted = !isSEMuted;

        if (isSEMuted)
        {
            ApplySEVolume(0f);
            seMuteButtonImage.sprite = seMuteOnSprite;
        }
        else
        {
            seSlider.value = lastSEVolume;
            ApplySEVolume(lastSEVolume);
            seMuteButtonImage.sprite = seMuteOffSprite;
        }
    }

    // -------------------------------------------------------
    // AudioMixer操作
    // -------------------------------------------------------

    /// <summary>
    /// BGMの音量をAudioMixerに反映します。
    /// </summary>
    /// <param name="volume">音量（0〜1）</param>
    private void ApplyBGMVolume(float volume)
    {
        float dB = volume <= 0.0001f ? -80f : Mathf.Clamp(Mathf.Log10(volume) * 20f, -80f, 0f);
        audioMixer.SetFloat("BGMVolume", dB);
    }

    /// <summary>
    /// SEの音量をAudioMixerに反映します。
    /// </summary>
    /// <param name="volume">音量（0〜1）</param>
    private void ApplySEVolume(float volume)
    {
        float dB = volume <= 0.0001f ? -80f : Mathf.Clamp(Mathf.Log10(volume) * 20f, -80f, 0f);
        audioMixer.SetFloat("SEVolume", dB);
    }
}