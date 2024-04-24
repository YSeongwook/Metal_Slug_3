using UnityEngine;
using EventLibrary;

public class GoRightIndicator : MonoBehaviour
{
    private new AudioSource audio;

    void Awake()
    {
        audio = GetComponent<AudioSource>();
        gameObject.SetActive(false);
        EventManager.StartListening(GlobalEvents.WaveEventEnd, () => SetActive(true));
        EventManager.StartListening(GlobalEvents.PlayerInactive, () => SetActive(true));
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
