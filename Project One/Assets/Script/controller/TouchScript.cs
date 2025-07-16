using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchScript : MonoBehaviour
{
   List<float> tap = new List<float>();

    // Update is called once per frame
    void Update()
    {
        tap.Clear();
        foreach(Touch finger in Input.touches)
        {
            Ray ray =Camera.main.ScreenPointToRay(finger.position);
            RaycastHit hit;
            if(Physics.Raycast(ray,out hit))
            {
                if(finger.phase == TouchPhase.Began)
                {
                    tap.Add(hit.point.x);
                }
            }
        }

        for(int i = 0;i<DataTransfer.tapjudgeList.Count;i++)
        {
            for(int n= 0;n<tap.Count;n++)
            {
                if(DataTransfer.tapjudgeList[i].JudgeNote(tap[n]))
                {
                    tap.Remove(tap[n]);
                }
            }
        }
    }
}
