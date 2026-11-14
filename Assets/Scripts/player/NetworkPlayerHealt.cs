using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerHealth : NetworkBehaviour
{
    public NetworkVariable<float> currentHealth = new NetworkVariable<float>(
        200f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> isInvulnerable = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Anti-Cheat Config (Servidor)")]
    [SerializeField] private float maxAllowedSpeed = 15f; // Límite de velocidad corriendo/saltando
    private Vector3 lastPosition;
    private float antiCheatTimer = 0f;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = NetworkGameManager.Instance.maxPlayerHealth;
            lastPosition = transform.position;
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        // Anti-Cheat: Verificar velocidad de movimiento cada 0.5s
        antiCheatTimer += Time.deltaTime;
        if (antiCheatTimer >= 0.5f)
        {
            antiCheatTimer = 0f;
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);
            float calculatedSpeed = distanceMoved / 0.5f;

            // Si se mueve más rápido de lo permitido sin inmunidad activa (posible hack de vuelo/teletransporte)
            if (calculatedSpeed > maxAllowedSpeed && !isInvulnerable.Value)
            {
                TriggerAntiCheatPunishment();
            }

            lastPosition = transform.position;
        }
    }

    private void TriggerAntiCheatPunishment()
    {
        Debug.LogWarning($"[Anti-Cheat] Movimiento anómalo en Cliente {OwnerClientId}. Teletransportando al spawn y restando 50 HP.");

        // 1. Teletransportar al Spawn
        if (NetworkGameManager.Instance.spawnPoint != null)
        {
            transform.position = NetworkGameManager.Instance.spawnPoint.position;
        }

        // 2. Quitar 50 de vida
        currentHealth.Value = Mathf.Max(0, currentHealth.Value - 50f);

        if (currentHealth.Value <= 0)
        {
            HandlePlayerDeathClientRpc();
        }
    }

    public void TakeDamageServer(float amount)
    {
        if (!IsServer) return;
        if (isInvulnerable.Value) return; // Escudo del Stelar Stick activo

        currentHealth.Value = Mathf.Max(0, currentHealth.Value - amount);

        if (currentHealth.Value <= 0)
        {
            HandlePlayerDeathClientRpc();
        }
    }

    [ClientRpc]
    private void HandlePlayerDeathClientRpc()
    {
        Debug.Log($"Jugador {OwnerClientId} eliminado.");
    }
}
