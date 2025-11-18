using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class EndingVideoController : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private bool autoReturnToMenu = true;

    private void Start()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer != null && autoReturnToMenu)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("[EndingVideoController] Video finished, returning to main menu...");
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Optional: Skip video with ESC key
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}