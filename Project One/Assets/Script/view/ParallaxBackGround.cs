using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//背景控制脚本
public class ParallaxBackGround : MonoBehaviour
{
    private GameObject cam;
    [SerializeField] private float parallaxEffect;

    private float xPosition;
    private float length;

    void Start()
    {
        cam =GameObject.Find("Main Camera");
        length = GetComponent<SpriteRenderer>().bounds.size.x;
        Debug.Log(length);
        xPosition = transform.position.x;
        
    }

    
    void Update()
    {
        float distanceMove = cam.transform.position.x*(1-parallaxEffect);
     //   Debug.Log(distanceMove);
        float distanceToMove = cam.transform.position.x*parallaxEffect;
        transform.position = new Vector3(xPosition + distanceToMove,transform.position.y);

        if(distanceMove >xPosition+length)
        {     
        xPosition = xPosition+length;
        }
        else if (distanceMove < xPosition - length)
        {    
        xPosition = xPosition -length; 
      }

    }
}
