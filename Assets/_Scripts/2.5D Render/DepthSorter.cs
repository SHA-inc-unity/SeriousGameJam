using System.Collections.Generic;
using UnityEngine;

public class DepthSorter : MonoBehaviour
{
    [SerializeField]
    private List<SpriteRenderer> sr;
    [SerializeField]
    private float precision = 100f;

    private int so = 0;

    public int GetSO()
    {
        return so;
    }

    private void Update()
    {
        so = -Mathf.RoundToInt(transform.position.z * precision);

        foreach (var spriteRenderer in sr)
        {
            spriteRenderer.sortingOrder = so;
        }
    }
}
