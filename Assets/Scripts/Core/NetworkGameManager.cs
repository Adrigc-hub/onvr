using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public enum BotDifficulty { Noob, Normal, Medium, Pro, Extremo }

public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager Instance;

    [Header("Configuración Global")]
    public float maxPlayerHealth = 200f; // 200 HP para todos por igual
    public Transform spawnPoint;         // Posición del Spawn del mapa

    [Header("Seguridad del Servidor (Admins, Mods, Devs, Owners)")]
    [SerializeField] private List<string> authorizedAdminIds = new List<string> 
    { 
        "TuIDDeCreador123", 
        "Admin_Test_ID" 
    };

    [Header("Configuración de Bots")]
    public GameObject botPrefab;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool IsUserAdminServerSide(string userId)
    {
        if (!IsServer) return false;
        return authorizedAdminIds.Contains(userId);
    }

    public void SpawnPracticeBot(BotDifficulty difficulty)
    {
        if (!IsServer) return;

        if (botPrefab != null && spawnPoint != null)
        {
            GameObject botInstance = Instantiate(botPrefab, spawnPoint.position + transform.right * 2f, spawnPoint.rotation);
            if (botInstance.TryGetComponent(out BotController botComp))
            {
                botComp.InitializeBot(difficulty);
            }
            botInstance.GetComponent<NetworkObject>().Spawn();
            Debug.Log($"[Servidor] Bot de dificultad {difficulty} creado en el campo de práctica.");
        }
    }
}
