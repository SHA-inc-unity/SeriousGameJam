using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoScenePlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string videoFileName; // e.g. "intro.mp4" — file must be in Assets/StreamingAssets/
    public string nextSceneName;

    void Start()
    {
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);
        videoPlayer.loopPointReached += OnVideoEnd;
        videoPlayer.Play();
    }

    private void Update()
    {
        if (Keyboard.current[Key.P].wasPressedThisFrame)
            SceneManager.LoadScene(nextSceneName);
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        SceneManager.LoadScene(nextSceneName);
    }
}