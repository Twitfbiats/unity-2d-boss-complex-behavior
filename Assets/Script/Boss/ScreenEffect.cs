using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenEffect : MonoBehaviour
{
    public Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
    }

    public void Darken()
    {
        animator.SetBool("Darken", true);
    }

    public void Lighten()
    {
        animator.SetBool("Darken", false);
    }
}
