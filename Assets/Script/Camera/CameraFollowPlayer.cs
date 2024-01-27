using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private Vector3 playerPosition;

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate() 
    {
        playerPosition = player.transform.position; 
        transform.position = new Vector3(playerPosition.x, playerPosition.y, transform.position.z);    
    }
}
