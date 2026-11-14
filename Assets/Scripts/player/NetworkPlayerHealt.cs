using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerHealth : NetworkBehaviour
{
    public NetworkVariable<float> currentHealth = new NetworkVariable<float>(
        200f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> isInvulnerable = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Anti-Cheat Config (Servidor)")]
    [SerializeField] private float maxAllowedSpeed = 15f; // Velocidad máxima lógica corriendo/saltando
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

        // Anti-Cheat: Verificar velocidad de movimiento cada cierto intervalo
        antiCheatTimer += Time.deltaTime;
        if (antiCheatTimer >= 0.5f) // Chequeo cada medio segundo
        {
            antiCheatTimer = 0f;
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);
            float calculatedSpeed = distanceMoved / 0.5f;

            // Si la velocidad supera el límite sin una habilidad oficial de salto/dash
            if (calculatedSpeed > maxAllowedSpeed && !isInvulnerable.Value)
            {
                TriggerAntiCheatPunishment();
            }

            lastPosition = transform.position;
        }
    }

    private void TriggerAntiCheatPunishment()
    {
        Debug.LogWarning($"[Anti-Cheat] Movimiento anómalo detectado en Cliente {OwnerClientId}. Posible hack de vuelo/teletransporte.");

        // 1. Teletransportar al Spawn
        if (NetworkGameManager.Instance.spawnPoint != null)
        {
            transform.position = NetworkGameManager.Instance.spawnPoint.position;
        }

        // 2. Quitar 50 de vida
        currentHealth.Value = Mathf.Max(0, currentHealth.Value - 50f);
        Debug.Log($"[Anti-Cheat] Castigado: Teletransportado al spawn y -50 HP. Vida actual: {currentHealth.Value}");

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
 