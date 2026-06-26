using UnityEngine;

public class otterBoss : MonoBehaviour
{
    [SerializeField] private bool isAngry;
    [SerializeField] private bool isDefeated;
    [SerializeField] private Animator anim;

    private void Start()
    {
        isAngry = false;
        isDefeated = false;

    }

    private void Update()
    {
        if (isAngry == true)  {anim.SetBool("isAngry", true);}
        else if (isAngry == false) { anim.SetBool("isAngry",false);}

        if (isDefeated == true) { anim.SetBool("isDefeated",true);}
    }

}
