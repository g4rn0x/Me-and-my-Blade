using UnityEngine;

public class ColliderProxy : MonoBehaviour
{
    private MeleeWeapon weapon;

    private void Awake()
    {
        weapon = GetComponentInParent<MeleeWeapon>();
    }

    private void OnTriggerEnter(Collider other)
    {
        weapon.OnTriggerEnter(other);
    }
}