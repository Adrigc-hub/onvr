using Unity.Netcode;
using UnityEngine;

public enum OnlineWeaponType { Pistol, Shotgun, Sniper }

public class NetworkWeaponBase : NetworkBehaviour
{
    [Header("Estadísticas del Arma")]
    public OnlineWeaponType weaponType;
    public float baseDamage = 45f;
    public float fireRate = 0.25f;
    private float nextFireTime = 0f;

    [Header("Puntos de Referencia")]
    public Transform muzzlePoint;

    public void FireWeapon()
    {
        if (!IsOwner) return;
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + fireRate;
        RequestFireServerRpc(muzzlePoint.position, muzzlePoint.forward);
    }

    [ServerRpc]
    private void RequestFireServerRpc(Vector3 origin, Vector3 direction)
    {
        if (weaponType == OnlineWeaponType.Shotgun)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector3 spreadDir = direction + new Vector3(
                    Random.Range(-0.03f, 0.03f),
                    Random.Range(-0.03f, 0.03f),
                    Random.Range(-0.03f, 0.03f)
                );
                ProcessServerRaycast(origin, spreadDir.normalized, baseDamage / 4f);
            }
        }
        else if (weaponType == OnlineWeaponType.Sniper)
        {
            ProcessServerRaycast(origin, direction, 120f);
        }
        else
        {
            ProcessServerRaycast(origin, direction, baseDamage);
        }
    }

    private void ProcessServerRaycast(Vector3 origin, Vector3 direction, float damage)
    {
        if (Physics.Raycast(origin, direction, out RaycastHit hit, 500f))
        {
            if (hit.collider.TryGetComponent(out NetworkPlayerHealth targetHealth))
            {
                targetHealth.TakeDamageServer(damage);
            }
        }
    }
}
