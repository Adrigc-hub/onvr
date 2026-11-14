using Unity.Netcode;
using UnityEngine;

public class BotController : NetworkBehaviour
{
    private BotDifficulty difficultyLevel;
    private float reactionDelay = 1.0f;
    private float aimPrecision = 0.5f; // 0 = errático, 1 = perfecto
    private float attackCooldown = 2.0f;
    private float currentTimer = 0f;

    public void InitializeBot(BotDifficulty difficulty)
    {
        difficultyLevel = difficulty;

        // Configurar parámetros según el nivel de dificultad seleccionado
        switch (difficulty)
        {
            case BotDifficulty.Noob:
                reactionDelay = 1.5f;
                aimPrecision = 0.2f;
                attackCooldown = 3.0f;
                break;
            case BotDifficulty.Normal:
                reactionDelay = 1.0f;
                aimPrecision = 0.4f;
                attackCooldown = 2.0f;
                break;
            case BotDifficulty.Medium:
                reactionDelay = 0.7f;
                aimPrecision = 0.6f;
                attackCooldown = 1.5f;
                break;
            case BotDifficulty.Pro:
                reactionDelay = 0.3f;
                aimPrecision = 0.85f;
                attackCooldown = 0.8f;
                break;
            case BotDifficulty.Extremo:
                reactionDelay = 0.1f;
                aimPrecision = 0.98f;
                attackCooldown = 0.4f;
                break;
        }

        Debug.Log($"Bot inicializado en modo: {difficultyLevel} (Reacción: {reactionDelay}s, Precisión: {aimPrecision})");
    }

    private void Update()
    {
        if (!IsServer) return; // La IA corre netamente en el servidor

        currentTimer += Time.deltaTime;
        if (currentTimer >= attackCooldown)
        {
            currentTimer = 0f;
            ExecuteBotAction();
        }
    }

    private void ExecuteBotAction()
    {
        // Lógica de combate simulada del bot basada en su dificultad
        Debug.Log($"[Bot - {difficultyLevel}] Buscando objetivo para disparar con precisión {aimPrecision * 100}%...");
        
        // Aquí el bot puede buscar jugadores cercanos usando Physics.OverlapSphere y aplicar daño reduciéndolo por la 'aimPrecision'
    }
}
 