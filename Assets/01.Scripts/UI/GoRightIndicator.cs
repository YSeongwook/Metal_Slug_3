using _01.Scripts.Utils;
using UnityEngine;

public class GoRightIndicator : MonoBehaviour
{
    private new AudioSource audio;

    private void Awake()
    {
        audio = GetComponent<AudioSource>();
        gameObject.SetActive(false);
        EventManager<GlobalEvents>.StartListening(GlobalEvents.WaveEventEnd, () => SetActive(true));
        EventManager<PlayerEvents>.StartListening(PlayerEvents.PlayerInactive, () => SetActive(true));
    }

    public void PlaySound()
    {
        audio.Play();
    }

    private void SetActive(bool active)
    {
        // 게임 오버 상태가 아닌 경우에만 활성화 가능
        if(!GameManager.Instance.IsGameOver()) gameObject.SetActive(active);
    }
}
