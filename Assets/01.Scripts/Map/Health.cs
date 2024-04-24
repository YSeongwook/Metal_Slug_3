using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100;
    public float health;
    public bool immortal;
    private bool isPlayer;

    public delegate void OnDamageEvent(float damage);
    public OnDamageEvent OnDead;

    void Start()
    {
        isPlayer = GetComponent<PlayerController>() != null;
        health = maxHealth;
    }

    public bool IsAlive()
    {
        return health > 0;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public float GetHealth()
    {
        return health;
    }

    public void increaseHealth()
    {
        health += maxHealth * 0.2f;
        if (health > maxHealth) health = maxHealth;
        // UIManager.UpdateHealthUI(health, maxHealth);
    }
}