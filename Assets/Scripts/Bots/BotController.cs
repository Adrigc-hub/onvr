using Unity.Netcode;
using UnityEngine;

public class BotController : NetworkBehaviour
{
    private BotDifficulty difficultyLevel;
    private float reactionDelay = 1.0f;
    private float aimPrecision = 0.5f; 
    private float attackCooldown = 2.0f;
    private float currentTimer = 0f;

    public void InitializeBot(BotDifficulty difficulty)
    {
        difficultyLevel = difficulty;

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

        Debug.Log($"Bot configurado en modo: {difficultyLevel}");
    }

    private void Update()
    {
        if (!IsServer) return;

        currentTimer += Time.deltaTime;
        if (currentTimer >= attackCooldown)
        {
            currentTimer = 0f;
            ExecuteBotAction();
        }
    }

    private void ExecuteBotAction()
    {
        Debug.Log($"[Bot - {difficultyLevel}] Ejecutando acción de combate con precisión del {aimPrecision * 100}%.");
    }
}
