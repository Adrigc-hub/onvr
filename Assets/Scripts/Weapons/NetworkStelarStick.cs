using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class NetworkStelarStick : NetworkBehaviour
{
    [Header("Identificación de Usuario")]
    public string playerPlatformId = "TuIDDeCreador123";

    [Header("Referencias")]
    public Transform playerBodyTransform;
    public Transform muzzlePoint;
    public TrailRenderer stelarTrail;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            VerifyAdminAccessServerRpc(playerPlatformId);
        }
    }

    [ServerRpc]
    private void VerifyAdminAccessServerRpc(string submittedId)
    {
        // Si no está en la lista de administradores/mods/devs/owners del servidor, el arma se destruye
        if (!NetworkGameManager.Instance.IsUserAdminServerSide(submittedId))
        {
            Debug.LogWarning($"[Servidor] Acceso denegado a ID {submittedId}. El Stelar Stick no se otorga.");
            GetComponent<NetworkObject>().Despawn(true);
        }
        else
        {
            Debug.Log($"[Servidor] Stelar Stick autorizado para el usuario ID: {submittedId}");
        }
    }

    // ==========================================
    // HABILIDAD 1: BOTÓN A (Giro rápido + Estela + Inmunidad por 5 segundos)
    // ==========================================
    public void ActivateButtonAShield()
    {
        if (!IsOwner) return;
        TriggerShieldServerRpc();
    }

    [ServerRpc]
    private void TriggerShieldServerRpc()
    {
        StartCoroutine(ServerShieldRoutine());
        ToggleShieldVisualsClientRpc(true);
    }

    private IEnumerator ServerShieldRoutine()
    {
        if (TryGetComponentInParent(out NetworkPlayerHealth ownerHealth))
        {
            ownerHealth.isInvulnerable.Value = true; // Activa protección contra ataques
        }

        float timer = 0f;
        while (timer < 5f)
        {
            if (playerBodyTransform != null)
            {
                transform.RotateAround(playerBodyTransform.position, Vector3.up, 720f * Time.deltaTime);
            }
            timer += Time.deltaTime;
            yield return null;
        }

        if (TryGetComponentInParent(out NetworkPlayerHealth healthAfter))
        {
            healthAfter.isInvulnerable.Value = false;
        }

        ToggleShieldVisualsClientRpc(false);
    }

    [ClientRpc]
    private void ToggleShieldVisualsClientRpc(bool active)
    {
        if (stelarTrail != null) stelarTrail.emitting = active;
    }

    // ==========================================
    // HABILIDAD 2: BOTÓN DE DISPARO (Vuela recto, 100 de daño + Onda expansiva decreciente)
    // ==========================================
    public void ActivateFireAbility()
    {
        if (!IsOwner) return;
        TriggerFireServerRpc(muzzlePoint.position, muzzlePoint.forward);
    }

    [ServerRpc]
    private void TriggerFireServerRpc(Vector3 origin, Vector3 direction)
    {
        // Disparo recto sin caída ni dispersión
        if (Physics.Raycast(origin, direction, out RaycastHit hit, 400f))
        {
            // Impacto directo: 100 de daño
            if (hit.collider.TryGetComponent(out NetworkPlayerHealth directTarget))
            {
                directTarget.TakeDamageServer(100f);
            }

            // Onda expansiva pequeña con daño decreciente según la distancia
            float shockwaveRadius = 3f;
            Collider[] colliders = Physics.OverlapSphere(hit.point, shockwaveRadius);
            foreach (Collider col in colliders)
            {
                if (col.TryGetComponent(out NetworkPlayerHealth splashTarget) && col.gameObject != hit.collider.gameObject)
                {
                    float dist = Vector3.Distance(hit.point, col.transform.position);
                    float falloff = Mathf.Clamp01(1f - (dist / shockwaveRadius));
                    splashTarget.TakeDamageServer(100f * falloff);
                }
            }
        }
    }

    // ==========================================
    // HABILIDAD 3: BOTÓN DE APUNTAR (Giro en mano + Lanzamiento corto: 50 frente / 200 espalda)
    // ==========================================
    public void ActivateAimMeleeAbility()
    {
        if (!IsOwner) return;
        TriggerAimMeleeServerRpc();
    }

    [ServerRpc]
    private void TriggerAimMeleeServerRpc()
    {
        StartCoroutine(ServerAimMeleeRoutine());
    }

    private IEnumerator ServerAimMeleeRoutine()
    {
        // Fase de giro en la mano
        float spinTime = 0.35f;
        float elapsed = 0f;
        while (elapsed < spinTime)
        {
            transform.Rotate(Vector3.forward, 1080f * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Lanzamiento de pocos centímetros hacia enfrente
        Vector3 strikeCenter = transform.position + transform.forward * 0.3f;
        Collider[] hits = Physics.OverlapSphere(strikeCenter, 0.45f);

        foreach (Collider target in hits)
        {
            if (target.TryGetComponent(out NetworkPlayerHealth targetHealth))
            {
                if (targetHealth.OwnerClientId == OwnerClientId) continue; // No dañarse a sí mismo

                // Calcular ángulo de impacto (frente vs espalda)
                Vector3 dirToTarget = (target.transform.position - transform.position).normalized;
                float angleDot = Vector3.Dot(target.transform.forward, dirToTarget);

                if (angleDot > 0.3f)
                {
                    // Golpe por la espalda: 200 de daño
                    targetHealth.TakeDamageServer(200f);
                }
                else
                {
                    // Golpe de frente: 50 de daño
                    targetHealth.TakeDamageServer(50f);
                }
            }
        }
    }
}
