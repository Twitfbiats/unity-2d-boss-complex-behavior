using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ScytheScript : MonoBehaviour
{
    public Transform controlPoint;
    public bool triggered = false;
    public bool travelBezier = false;
    public float t = 0;
    public bool travelLine = false;
    public float lineA;
    public float lineB;
    public Vector2 startPoint;
    public Vector2 endPoint;
    public float moveAmount = 0;
    public float absMoveAmount;
    public float t1 = 0;
    public enum TravelStateEnum {Halt, TravelLine, TravelBezier}
    public TravelStateEnum travelState = TravelStateEnum.Halt;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate() 
    {
        Triggered();
        TriggerTravelInBezierCurve();
        TriggerTravelLine();
    }

    public void Triggered()
    {
        if (triggered)
        {
            startPoint = GameObject.Find("Boss").transform.position;
            endPoint = GameObject.Find("X").transform.position;
            controlPoint.position = new Vector3((startPoint.x + endPoint.x)/2, (endPoint.y - startPoint.y)/2 - 10);
            triggered = false;
            travelBezier = true;
            travelState = TravelStateEnum.TravelBezier;
        }
    }

    public void TriggerTravelInBezierCurve()
    {
        if (travelBezier) TravelInBezierCurve(t += Time.fixedDeltaTime);
    }

    public void TravelInBezierCurve(float t)
    {
        if (t < 1)
        {
            var x = Math.Pow((1-t), 2) * startPoint.x + 2 * t * (1-t) * controlPoint.position.x + Math.Pow(t, 2) * endPoint.x;
            var y = Math.Pow((1-t), 2) * startPoint.y + 2 * t * (1-t) * controlPoint.position.y + Math.Pow(t, 2) * endPoint.y;
            transform.position = new Vector3((float) x, (float) y);
        }
        else
        {
            travelBezier = false;
            this.t = 0;

            travelLine = true;
            travelState = TravelStateEnum.TravelLine;
            lineA = (endPoint.y - startPoint.y)/(endPoint.x - startPoint.x);
            lineB = (endPoint.y * startPoint.x - startPoint.y * endPoint.x)/(startPoint.x - endPoint.x);
            moveAmount = (endPoint.x - startPoint.x) * Time.fixedDeltaTime;
            t1 = endPoint.x;
        }
    }

    public void TriggerTravelLine()
    {
        if (travelLine) TravelLine(t1 -= moveAmount);
    }

    public void TravelLine(float t)
    {
        transform.position = new Vector3(t, lineA * t + lineB);
    }

    public void Finish()
    {
        travelLine = false;
        gameObject.SetActive(false);
        travelState = TravelStateEnum.Halt;
    }
}
