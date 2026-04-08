using UnityEngine;

public class FireController : MonoBehaviour
{
    public WeaponController weapon;

    void Awake()
    {
        if (weapon == null) weapon = GetComponent<WeaponController>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            weapon.TryFire();
    }
}