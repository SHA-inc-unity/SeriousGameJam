using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class NewGameManager : MonoBehaviour
{
    [SerializeField]
    private List<CombatantData> combatantDatas;
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {

        foreach(CombatantData combatantData in combatantDatas)
        {
            combatantData.isDefeated = false;
        }
    }

}
