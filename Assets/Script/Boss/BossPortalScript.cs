using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPortalScript : MonoBehaviour
{
    private Animator animator;
    public BossSummonScript bossSummonScript;
    public GameObject target;

    public void StartPortal()
    {
        transform.position = target.transform.position;
        FadeIn();
    }

    public void Fade(bool fade_in)
    {
        animator.SetBool("FadeIn", fade_in);
    }

    public void FadeIn() {Fade(true);}
    public void FadeOut() {Fade(false);}

    public void Summon()
    {
        bossSummonScript.FadeIn();
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
