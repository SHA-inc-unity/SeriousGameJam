using UnityEngine;

public class DogGuard : MonoBehaviour
{
    [SerializeField] private BattleTrigger[] battles;

    private void Update()
    {
        foreach (var battle in battles)
            if (!battle.NPCInteract.isDefeated) return;

        gameObject.SetActive(false);
    }
}