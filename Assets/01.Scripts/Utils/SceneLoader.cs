using _01.Scripts.Sound;
using UnityEngine.SceneManagement;

namespace _01.Scripts.Utils
{
    public class SceneLoader : Singleton<SceneLoader>
    {
        private void Start()
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
}
