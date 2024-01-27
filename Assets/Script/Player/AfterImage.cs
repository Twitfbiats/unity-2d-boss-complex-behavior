using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImage : MonoBehaviour
{
    /* This will be called after the
    animation end, see AfterImage
    prefab animation tab for more
    detail */
    public void AfterFade()
    {
        gameObject.SetActive(false);
    }
}
