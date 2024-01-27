using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainScript : MonoBehaviour
{
    public VortexMidPointScript vortexMidPointScript;
    private SpriteRenderer spriteRenderer;
    public float chainLength;
    public float distancePlusChainLength = 0;
    private float xChange = 0;
    private float yChange = 0;
    private GameObject target;
    private float xMoveAmount = 0;
    private float yMoveAmount = 0;
    public bool forward = false;
    private bool handleForward = false;
    private float forwardAmount = 0;
    private float forwardedAmount = 0;
    private float forwardDistance = 0;
    public float forwardTime = 0.5f;
    private float accelerationTimeLine = 0.15f;
    private float decelerateValue = 3;
    private bool backward = false;
    public GameObject catchPoint;
    private PolygonCollider2D polygonCollider2D;
    public bool catched = false;
    public delegate void CatchAtTheEnd();
    public static CatchAtTheEnd catchAtTheEnd;
    public bool handleCatchBool = false;
    private void Awake() 
    {
        target = vortexMidPointScript.target;
        spriteRenderer = GetComponent<SpriteRenderer>();
        chainLength = spriteRenderer.size.x * transform.localScale.x;
        polygonCollider2D = GetComponent<PolygonCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate() 
    {
        Forward();
        HandleForward();
        Backward();
    }

    public void Begin()
    {
        distancePlusChainLength = vortexMidPointScript.distance + chainLength;

        xChange = distancePlusChainLength * vortexMidPointScript.absXDiff / vortexMidPointScript.distance;
        yChange = distancePlusChainLength * vortexMidPointScript.absYDiff / vortexMidPointScript.distance;

        
        transform.SetPositionAndRotation
        (
            target.transform.position - 
            new Vector3((float)vortexMidPointScript.xDirection.direction * xChange , (float)vortexMidPointScript.yDirection.direction * yChange, 0)
            , Quaternion.Euler(new Vector3(0, 0, (float)vortexMidPointScript._angle))
        );

        forward = true;
    }

    private void Forward()
    {
        if (forward)
        {
            forward = false;
            xMoveAmount = (vortexMidPointScript.absXDiff * Time.fixedDeltaTime / (forwardTime * decelerateValue)) * (float)vortexMidPointScript.xDirection.direction;
            yMoveAmount = (vortexMidPointScript.absYDiff / vortexMidPointScript.absXDiff) * Math.Abs(xMoveAmount) * (float)vortexMidPointScript.yDirection.direction;
            forwardAmount = (float)Math.Sqrt(Math.Pow(xMoveAmount, 2) + Math.Pow(yMoveAmount, 2));
            forwardDistance = vortexMidPointScript.distance;
            forwardedAmount = 0;
            polygonCollider2D.enabled = true;
            catched = false;

            handleForward = true;
            StartCoroutine(WaitAccelerate());
        }
    }

    private void HandleForward()
    {
        if (handleForward)
        {
            if (forwardedAmount < forwardDistance)
            {
                forwardedAmount += forwardAmount;
                transform.position += new Vector3(xMoveAmount, yMoveAmount);
            }
            else
            {
                handleForward = false;
                backward = true;
            }
        }
    }

    public IEnumerator WaitAccelerate()
    {
        yield return new WaitForSeconds(accelerationTimeLine);

        xMoveAmount *= decelerateValue;
        yMoveAmount *= decelerateValue;
        forwardAmount *= decelerateValue;
    }

    private void Backward()
    {
        if (backward)
        {
            if (forwardedAmount > 0)
            {
                forwardedAmount -= forwardAmount;
                transform.position -= new Vector3(xMoveAmount, yMoveAmount);
            }
            else
            {
                polygonCollider2D.enabled = false;
                handleCatchBool = false;
                backward = false;
                if (catched) catchAtTheEnd?.Invoke();
            }
        }
    }

    public IEnumerator HandleCatch(GameObject player)
    {
        handleCatchBool = true;
        while (handleCatchBool)
        {
            yield return new WaitForSeconds(Time.fixedDeltaTime);
            player.transform.position = catchPoint.transform.position;
        }
    }

    public IEnumerator StopCatch()
    {
        yield return new WaitForSeconds(0.5f);

        handleCatchBool = false;
    }

    private void OnCollisionEnter2D(Collision2D other) 
    {
        catched = true;
        StartCoroutine(HandleCatch(other.gameObject));
    }
}
