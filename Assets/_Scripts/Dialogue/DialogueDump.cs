using System.Collections.Generic;
using UnityEngine;

public class DialogueDump : MonoBehaviour
{
    void Start()
    {
        Dictionary<string, string> map = new Dictionary<string, string>();
        List<DialogueHolder> all = new List<DialogueHolder>(Resources.LoadAll<DialogueHolder>("Dialogue"));

        foreach (var dialogue in all)
        {
            foreach (var line in dialogue.GetDialogueLines())
            {
                map[line.who] = line.who;
                map[line.text] = line.text;
                for (int i = 0; i < line.answers.Count; i++)
                {
                    map[line.answers[i].text] = line.answers[i].text;
                }
            }
        }

        string output = "";
        foreach (var kvp in map)
        {
            output += "  - Key: " + kvp.Key + "\n    Value: " + kvp.Value + "\n";
        }
        Debug.Log(output);
    }
}
