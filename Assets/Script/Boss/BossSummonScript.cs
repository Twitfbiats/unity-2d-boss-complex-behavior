using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSummonScript : MonoBehaviour
{
    public GameObject target;
    private Animator animator;
    public BossPortalScript bossPortalScript;

    public void Fade(bool fade_in)
    {
        animator.SetBool("FadeIn", fade_in);
    }

    public void Attack(bool attack)
    {
        animator.SetBool("Attack", attack);
    }

    public void FadeIn()
    {
        Fade(true);
    }

    public void FadeOut()
    {
        Fade(false);
    }

    public void EndPortal() {bossPortalScript.FadeOut();}

    public void StartAttack()
    {
        transform.localScale = new 
        Vector3(transform.position.x < target.transform.position.x ? -1 : 1, 1);
        FadeOut();
        Attack(true);
    }

    public void StopAttack()
    {
        Attack(false);
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
