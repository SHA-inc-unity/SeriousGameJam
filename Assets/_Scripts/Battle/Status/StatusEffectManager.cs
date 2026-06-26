using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectManager : MonoBehaviour
{
    private BattleManager battle;
    private Dictionary<Combatant, List<StatusEffect>> activeStatuses
        = new Dictionary<Combatant, List<StatusEffect>>();

    public void Init(BattleManager bm) => battle = bm;

    public void Register(Combatant combatant)
    {
        if (!activeStatuses.ContainsKey(combatant))
            activeStatuses[combatant] = new List<StatusEffect>();
    }

    // Returns false if the status was rejected (already active, no stacking)
    public bool TryApply(Combatant owner, StatusEffect newStatus)
    {
        List<StatusEffect> list = activeStatuses[owner];

        // No stacking: reject if same type is already active
        foreach (var s in list)
        {
            if (s.GetType() == newStatus.GetType())
            {
                battle.Announce($"{owner.displayName} is already affected by {newStatus.displayName}!");
                return false;
            }
        }

        list.Add(newStatus);
        newStatus.OnApply(owner, battle);
        battle.Announce($"{owner.displayName} is now {newStatus.displayName}!");

        if (newStatus.durationType == StatusDurationType.Seconds)
            StartCoroutine(ExpireAfterSeconds(owner, newStatus));
        else
            newStatus.spinsRemaining = newStatus.durationSpins;

        return true;
    }

    public void NotifySpinCompleted(Combatant owner)
    {
        if (!activeStatuses.ContainsKey(owner)) return;

        List<StatusEffect> list = activeStatuses[owner];
        List<StatusEffect> toExpire = new List<StatusEffect>();

        foreach (var s in list)
        {
            if (s.durationType != StatusDurationType.Spins) continue;
            s.OnSpinCompleted(owner, battle);
            s.spinsRemaining--;
            if (s.spinsRemaining <= 0)
                toExpire.Add(s);
        }

        foreach (var s in toExpire)
            Expire(owner, s);
    }

    public void Remove(Combatant owner, StatusEffect status)
        => Expire(owner, status);

    public bool Has<T>(Combatant owner) where T : StatusEffect
    {
        if (!activeStatuses.ContainsKey(owner)) return false;
        foreach (var s in activeStatuses[owner])
            if (s is T) return true;
        return false;
    }

    public T Get<T>(Combatant owner) where T : StatusEffect
    {
        if (!activeStatuses.ContainsKey(owner)) return null;
        foreach (var s in activeStatuses[owner])
            if (s is T t) return t;
        return null;
    }

    public void ClearAll()
    {
        foreach (var kvp in activeStatuses)
            foreach (var s in kvp.Value)
                s.OnExpire(kvp.Key, battle);
        activeStatuses.Clear();
    }

    private void Expire(Combatant owner, StatusEffect status)
    {
        status.OnExpire(owner, battle);
        activeStatuses[owner].Remove(status);
        battle.Announce($"{owner.displayName}'s {status.displayName} wore off.");
    }

    private IEnumerator ExpireAfterSeconds(Combatant owner, StatusEffect status)
    {
        yield return new WaitForSeconds(status.durationSeconds);
        if (activeStatuses.ContainsKey(owner) && activeStatuses[owner].Contains(status))
            Expire(owner, status);
    }
}