using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterController : MonoBehaviour
{
    public Animator waterController;

    void Start()
    {
        waterController.SetBool("isInWater", false);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (GameManager.Instance.IsPlayer(collider))
        {
            waterController.SetBool("isInWater", true);
            PlayerController.Instance.waterStep.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (GameManager.Instance.IsPlayer(collider))
        {
            waterController.SetBool("isInWater", false);
            PlayerController.Instance.waterStep.SetActive(false);
        }
    }
}
