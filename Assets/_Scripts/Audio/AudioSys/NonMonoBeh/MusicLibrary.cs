using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum MusicUsage
{
    None = 0,
    Overworld = 1 << 0,
    Battle = 1 << 1,
    Menu = 1 << 2
}

[Serializable]
public struct MusicEntry
{
    public AudioClip clip;
    public MusicUsage usage;
    public float loopMarker; // Ivy - maker
    [Header("Optional if MusicUsage includes Battle")]
    public List<string> enemyNames;
}

[CreateAssetMenu(fileName = "MusicLibrary", menuName = "Audio/MusicLibrary")]
public class MusicLibrary : ScriptableObject
{
    [SerializeField]
    private List<MusicEntry> tracks;

    public List<MusicEntry> GetEntriesFor(MusicUsage mode)
    {
        var result = new List<MusicEntry>();
        foreach (var entry in tracks)
            if (entry.clip != null && (entry.usage & mode) != 0)
                result.Add(entry);
        return result;
    }

    public bool TryGetEntryForEnemy(string enemyName, out MusicEntry found)
    {
        foreach (var entry in tracks)
        {
            if (entry.clip == null || entry.enemyNames == null) continue;

            foreach (var name in entry.enemyNames)
            {
                if (name == enemyName)
                {
                    found = entry;
                    return true;
                }
            }
        }

        found = default;
        return false;
    }
}