using UnityEngine;
using UnityEngine.Video;

public class BackgroundVideoLooper : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    public VideoClip[] videoClips;

    private int currentVideoIndex = 0;

    void Start()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        videoPlayer.isLooping = false;

        videoPlayer.loopPointReached += CheckOver;

        PlayVideo(currentVideoIndex);
    }

    void CheckOver(UnityEngine.Video.VideoPlayer vp)
    {
        currentVideoIndex++;

        if (currentVideoIndex >= videoClips.Length)
        {
            currentVideoIndex = 0;
        }

        PlayVideo(currentVideoIndex);
    }

    void PlayVideo(int index)
    {
        if (videoClips.Length > 0)
        {
            videoPlayer.clip = videoClips[index];
            videoPlayer.Play();
        }
        else
        {
          
        }
    }
}