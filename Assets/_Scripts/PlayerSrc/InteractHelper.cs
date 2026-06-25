using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class InteractHelper : MonoBehaviour
{
    public static InteractHelper Instance { get; private set; }

    [SerializeField]
    private GameObject visualPart;

    private Vector3 posToHelp;
    private int sortingOrder;
    private float forwardHelperDistance;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if(posToHelp != Vector3.zero)
        {
            if(!visualPart.activeSelf)
                visualPart.SetActive(true);

            transform.position = posToHelp;
            visualPart.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 25;
            visualPart.transform.localPosition = -visualPart.transform.forward * forwardHelperDistance;
        }
        else
            visualPart.SetActive(false);
    }

    public void SetObj(ObjectInteract obj)
    {
        if (obj == null)
            posToHelp = Vector3.zero;
        else
        {
            posToHelp = obj.gameObject.transform.position;
            sortingOrder = obj.GetComponent<DepthSorter>().GetSO();
            forwardHelperDistance = obj.ForwardHelperDistance;
        }

        //posToHelp.y = 0;
    }
}
