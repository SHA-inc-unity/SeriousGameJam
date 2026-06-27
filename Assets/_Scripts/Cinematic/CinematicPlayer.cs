using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class VideoScenePlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nextSceneName;

    void Start()
    {
        //videoPlayer.loopPointReached += OnVideoEnd;
        //videoPlayer.Play();

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, "IntroCutscene2.mp4");

        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.loopPointReached += OnVideoEnd;
        videoPlayer.Prepare();
    }

    void OnPrepared(VideoPlayer vp) => vp.Play();

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