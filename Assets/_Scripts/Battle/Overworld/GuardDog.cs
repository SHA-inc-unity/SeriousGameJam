using UnityEngine;

public class DogGuard : MonoBehaviour
{
    [SerializeField] private BattleTrigger[] battles;

    private void Update()
    {
        foreach (var battle in battles)
            if (!battle.defeated) return;

        gameObject.SetActive(false);
    }
}