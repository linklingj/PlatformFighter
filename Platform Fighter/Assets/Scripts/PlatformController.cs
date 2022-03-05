using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : MonoBehaviour
{
    public Transform playerPos1;
    //public Transform playerPos2;
    private BoxCollider2D collider;
    public float groundBuffer;
    private Vector2 size;
    private float topMost;
    private bool underP1;
    //private bool underP2;
    private bool stopCaculateP1 = false;
    void Start()
    {
        collider = GetComponent<BoxCollider2D>();
        size.x = transform.localScale.x * collider.size.x;
        size.y = transform.localScale.y * collider.size.y;
        topMost = collider.offset.y + size.y / 2 - groundBuffer;
    }

    // Update is called once per frame
    void Update()
    {
        // if (topMost > playerPos1.position.y)
        //     underP1 = false;
        // else
        //     underP1 = true;
        // if (topMost > playerPos2.position.y)
        //     underP2 = false;
        // else
        //     underP2 = true;
        
        // if(underP1 && underP2)
        //     gameObject.layer = 12;
        // else if(underP1 && !underP2)
        //     gameObject.layer = 14;
        // else if(!underP1 && underP2)
        //     gameObject.layer = 13;
        // else
        //     gameObject.layer = 15;
        if (topMost < playerPos1.position.y && !stopCaculateP1)
            underP1 = true;
        else
            underP1 = false;
        if(underP1)
            gameObject.layer = 12;
        else
            gameObject.layer = 13;
        //Debug.Log(gameObject.layer);
    }
    IEnumerator P1Drop()
    {
        stopCaculateP1 = true;
        yield return new WaitForSeconds(0.2f);
        stopCaculateP1 = false;
    }
}
