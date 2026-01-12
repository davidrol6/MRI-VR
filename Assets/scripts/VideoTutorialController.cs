/* * SCRIPT SUMMARY:
 * This script manages a simple video playlist system for UI-based tutorials.
 * * * How it works:
 * - It holds an array of 'VideoClips' (e.g., Tutorial 1, Tutorial 2).
 * - On Start, it automatically loads and plays the first video in the list.
 * - It provides public methods meant to be linked to UI Buttons:
 * 1. 'SelectVideo(index)': Swaps the current clip.
 * 2. 'TogglePlayPause()': Pauses or Resumes the video.
 */

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoTutorialController : MonoBehaviour
{
    [Header("Components")]
    [Tooltip("The Video Player component attached to the RawImage")]
    public VideoPlayer videoPlayer; 

    [Header("Content")]
    [Tooltip("Drag your video clips here in order (Element 0 = First Video)")]
    public VideoClip[] videoClips;

    // Internal tracking of which video is currently active
    private int currentVideoIndex = 0;

    void Start()
    {
        // Validation to ensure clips are assigned to avoid errors
        if (videoClips.Length > 0)
        {
            // Requirement: Default to Video 1 (Index 0) immediately upon opening the menu
            PlayVideo(0);
        }
    }

    /// <summary>
    /// Call this method from the Video Selector buttons.
    /// Usage: In the Inspector's OnClick(), type 0 for Video 1, 1 for Video 2, etc.
    /// </summary>
    public void SelectVideo(int index)
    {
        if (index >= 0 && index < videoClips.Length)
        {
            currentVideoIndex = index;
            PlayVideo(currentVideoIndex);
        }
        else
        {
            Debug.LogWarning("Video index out of range.");
        }
    }

    /// <summary>
    /// Call this method from the Play/Pause UI button.
    /// Checks the current state and flips it.
    /// </summary>
    public void TogglePlayPause()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
        }
        else
        {
            videoPlayer.Play();
        }
    }

    // Helper method to handle the actual loading and playing of the clip
    private void PlayVideo(int index)
    {
        videoPlayer.clip = videoClips[index];
        videoPlayer.Play();
    }
}