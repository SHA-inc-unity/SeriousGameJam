using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum MusicUsage
{
    None = 0,
    Overworld = 1 << 0,
    Battle = 1 << 1
}

[Serializable]
public struct MusicEntry
{
    public AudioClip clip;
    public MusicUsage usage;
}

[CreateAssetMenu(fileName = "MusicLibrary", menuName = "Audio/MusicLibrary")]
public class MusicLibrary : ScriptableObject
{
    [SerializeField]
    private List<MusicEntry> tracks;

    public List<AudioClip> GetClipsFor(MusicUsage mode)
    {
        var result = new List<AudioClip>();
        foreach (var entry in tracks)
            if (entry.clip != null && (entry.usage & mode) != 0)
                result.Add(entry.clip);
        return result;
    }
}