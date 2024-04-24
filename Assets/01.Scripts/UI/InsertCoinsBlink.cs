using UnityEngine;
using TMPro;

public class InsertCoinsBlink : MonoBehaviour {

    private TextMeshProUGUI insertCoins;

    void Start()
    {
        insertCoins = GetComponent<TextMeshProUGUI>();
    }

    void HideShow()
    {
        insertCoins.enabled = !insertCoins.enabled;
    }

    void OnDestroy()
    {

    }

	void Update () {

	
	}
}
