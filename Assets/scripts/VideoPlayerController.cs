using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class VideoPlayerToggleController : MonoBehaviour
{
    [Header("Video Components")]
    public VideoPlayer videoPlayer;
    public Slider progressSlider;
    public Image playPauseIcon;
    public Sprite playSprite;
    public Sprite pauseSprite;

    [Header("Time Labels (optional)")]
    public TextMeshProUGUI currentTimeLabel;
    public TextMeshProUGUI totalTimeLabel;

    private bool isDragging = false;
    private bool isPlaying = false;

    void Start()
    {
        // Make sure the VideoPlayer is prepared to know total frame count
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.Prepare();

        if (progressSlider)
            progressSlider.onValueChanged.AddListener(OnSliderValueChanged);

        UpdateIcon();
    }

    private void OnVideoPrepared(VideoPlayer source)
    {
        if (totalTimeLabel)
            totalTimeLabel.text = FormatTime((float)videoPlayer.length);
    }

    void Update()
    {
        if (videoPlayer.isPrepared && videoPlayer.frameCount > 0 && !isDragging)
        {
            float progress = (float)(videoPlayer.frame / (double)videoPlayer.frameCount);
            if (progressSlider)
                progressSlider.SetValueWithoutNotify(progress);

            if (currentTimeLabel)
                currentTimeLabel.text = FormatTime((float)videoPlayer.time);
        }
    }

    public void TogglePlayPause()
    {
        if (isPlaying)
        {
            videoPlayer.Pause();
            isPlaying = false;
        }
        else
        {
            // If video ended, restart from beginning
            if (Mathf.Abs((float)(videoPlayer.time - videoPlayer.length)) < 0.05f)
                videoPlayer.time = 0f;

            videoPlayer.Play();
            isPlaying = true;
        }

        UpdateIcon();
    }

    private void OnSliderValueChanged(float value)
    {
        if (videoPlayer.frameCount > 0)
        {
            isDragging = true;
            videoPlayer.time = (float)(value * videoPlayer.length);
            isDragging = false;
        }
    }

    private void UpdateIcon()
    {
        if (playPauseIcon)
            playPauseIcon.sprite = isPlaying ? pauseSprite : playSprite;
    }

    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return $"{minutes}:{secs:00}";
    }
}
