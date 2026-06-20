using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowHand : MonoBehaviour
{
    public bool SpinAllow;

    public Transform clockHandTransform;
    public float spinSpeed;
    public float currentSpinSpeed;
    public float spinStopSpeed;


    // Start is called before the first frame update
    void Start()
    {
        clockHandTransform = this.transform;
        //SpinAllow = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (SpinAllow == true) //normal spin speed
        {
            currentSpinSpeed = spinSpeed;
            clockHandTransform.eulerAngles -= new Vector3(0, 0, (currentSpinSpeed / 10));
        }

        else if (SpinAllow == false && currentSpinSpeed > 0) //slows down a little bit 
        {
            clockHandTransform.eulerAngles -= new Vector3(0, 0, (currentSpinSpeed / 10));
            currentSpinSpeed -= spinStopSpeed;
        }

        else if (currentSpinSpeed <= 0) //when it comes to a complete stop
        {
            registerStop();
        }
    }

    public void stopRotate() //referenced by button on scene
    {
        SpinAllow = false;
        
    }

    public void registerStop()
    {
        float rot = transform.eulerAngles.z; //angular maths things
        //because the way that ive coded this, each segment is oppsoite, starting at 12 going clockwise its 5,4,3,2,1
        print(rot);
        if (rot > 0 && rot <= 72)
        {
            print("1");
        }
        else if (rot > 72 && rot <= 144)
        {
            print("2");
        }
        else if (rot > 144 && rot <= 216)
        {
            print("3");
        }
        else if (rot > 216 && rot <= 288)
        {
            print("4");
        }
        else if (rot > 288 && rot <= 360)
        {
            print("5");
        }
    }
}
