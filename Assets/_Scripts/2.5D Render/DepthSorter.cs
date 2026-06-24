using System.Collections.Generic;
using UnityEngine;

public class DepthSorter : MonoBehaviour
{
    [SerializeField]
    private List<SpriteRenderer> sr;
    [SerializeField]
    private float precision = 100f;

    void Update()
    {
        foreach (var spriteRenderer in sr)
        {
            spriteRenderer.sortingOrder = -Mathf.RoundToInt(transform.position.z * precision);
        }
    }
}
