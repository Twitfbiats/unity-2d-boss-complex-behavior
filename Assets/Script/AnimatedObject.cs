using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimatedObject
{
    public float GetAnimationClipLength(Animator animator, String clip)
    {
        return animator.runtimeAnimatorController.animationClips.First((aC) => aC.name.Equals(clip)).length;
    }
}
