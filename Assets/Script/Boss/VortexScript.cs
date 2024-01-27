using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VortexScript : MonoBehaviour
{
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Active(bool active)
    {
        animator.SetBool("Active", active);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
