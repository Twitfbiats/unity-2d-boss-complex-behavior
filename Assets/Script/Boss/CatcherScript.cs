using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatcherScript : MonoBehaviour
{
    public bool catched = false;
    private void OnCollisionEnter2D(Collision2D other) 
    {
        if (other.collider.gameObject.name.Equals("Scythe")) catched = true;
    }
}
