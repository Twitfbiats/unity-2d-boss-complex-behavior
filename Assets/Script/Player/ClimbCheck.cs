using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbCheck : MonoBehaviour
{
    private bool isClimbing;
    // Just a fancy way of writing get and set
    public bool IsClimbing { get => isClimbing; set => isClimbing = value; }

    // This is called when object collide with something
    private void OnCollisionEnter2D(Collision2D other) 
    {
        if (other.gameObject.tag == "Climb") isClimbing = true;
    }

    // This is called when object quit colliding with something
    private void OnCollisionExit2D(Collision2D other) 
    {
        if (other.gameObject.tag == "Climb") isClimbing = false;
    }
}
