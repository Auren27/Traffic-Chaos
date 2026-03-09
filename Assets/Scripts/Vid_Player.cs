using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class Vid_Player : MonoBehaviour
{
    [SerializeField] private string VideoFileName;

    private void Start()
    {
        PlayVideo();
    }

    public void PlayVideo()
    {
        VideoPlayer videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer)
        {
            string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, VideoFileName);
            Debug.Log(videoPath);

            if (System.IO.File.Exists(videoPath))
            {
                videoPlayer.url = videoPath;
                videoPlayer.Play();
            }
            else
            {
                Debug.LogError("Файл не найден: " + videoPath);
            }
        }
    }
}
