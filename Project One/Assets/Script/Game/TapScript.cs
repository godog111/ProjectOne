using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapScript : MonoBehaviour
{
    float myTime = -1;
    bool add=true,remove=true;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        myTime+=Time.deltaTime;
        if(add&&myTime>-0.06)
        {
            DataTransfer.tapjudgeList.Add(this);
            add =false;
        }
        else if(remove&&myTime >0.06)
        {
            DataTransfer.tapjudgeList.Remove(this);
            remove= false;
            Miss();
        }
    }

    public bool JudgeNote(float xPosition)
    {
        float x =System.Math.Abs(transform.position.x -xPosition);
        if(x<1)
        {
                DataTransfer.tapjudgeList.Remove(this);
                Destroy(gameObject);
                return true;
        }
        return false;
    }

    void Miss()
    {
        DataTransfer.tapjudgeList.Remove(this);
                Destroy(gameObject);
    }
}
