using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KunaiScript : MonoBehaviour
{
    private float xDiff;
    private float yDiff;
    private float xMoveAmount;
    private float yMoveAmount;
    private float moveAmount;
    private float movedAmount = 0;
    private float distance;
    public float travelSpeed;
    bool travel = false;
    public float stopTimeAfterReachTarget = 1;
    public GameObject target;
    public float _angle;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Fire(float speed)
    {
        this.travelSpeed = speed;
        xDiff = target.transform.position.x - transform.position.x;
        yDiff = target.transform.position.y - transform.position.y;
        xMoveAmount = (xDiff < 0 ? -1 : 1) * Time.fixedDeltaTime * travelSpeed;
        yMoveAmount = yDiff / (xDiff == 0 ? XDiffZero() : xDiff) * xMoveAmount;
        moveAmount = (float)Math.Sqrt(Math.Pow(xMoveAmount, 2) + Math.Pow(yMoveAmount, 2));
        movedAmount = 0;
        distance = Vector2.Distance(transform.position, target.transform.position);
        CalculateAngle();
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, (float)_angle));

        StartCoroutine(Travel());
    }

    public void CalculateAngle()
    {
        Vector2 vector2 = new Vector2(xDiff, yDiff);
        _angle = Vector2.SignedAngle(Vector2.right, vector2);
    }

    public float XDiffZero() {return 1/(Time.fixedDeltaTime * travelSpeed);}

    public IEnumerator Travel()
    {
        travel = true;
        while (travel)
        {
            yield return new WaitForSeconds(Time.fixedDeltaTime);

            transform.position += new Vector3(xMoveAmount, yMoveAmount);
            movedAmount += moveAmount;
            if (movedAmount >= distance) StartCoroutine(StopTravel());
        }
    }

    public IEnumerator StopTravel()
    {
        yield return new WaitForSeconds(stopTimeAfterReachTarget);

        travel = false;
        gameObject.SetActive(false);
    }
}
