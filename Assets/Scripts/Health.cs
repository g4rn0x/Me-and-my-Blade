using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [Tooltip("Необязательно: Image типа Filled для полоски HP")]
    [SerializeField] private Image healthBarFill;

    public UnityEvent onDamaged = new UnityEvent();
    public UnityEvent onDied = new UnityEvent();

    public float CurrentHealth { get; private set; }
    public bool IsDead { get { return CurrentHealth <= 0f; } }

    private void Awake()
    {
        CurrentHealth = maxHealth;
        UpdateBar();
    }

    public void TakeDamage(float amount)
    {
        if (IsDead)
        {
            return;
        }

        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0f);
        Debug.Log($"{name}: -{amount} HP, осталось {CurrentHealth}/{maxHealth}");
        UpdateBar();

        if (IsDead)
        {
            onDied.Invoke();
        }
        else
        {
            onDamaged.Invoke();
        }
    }

    private void UpdateBar()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = CurrentHealth / maxHealth;
        }
    }
}