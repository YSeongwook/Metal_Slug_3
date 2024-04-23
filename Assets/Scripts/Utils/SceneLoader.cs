using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : Singleton<SceneLoader>
{
    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Mission1")
        {
            PlayerMissionStart();
        }
    }

    private void PlayerMissionStart()
    {
        SoundManager.Instance.PlayBGM();
        SoundManager.Instance.PlayLevelStartAudio();
    }
}
