using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class VortexMidPointScript : MonoBehaviour
{
    public GameObject target;
    private bool flag = false;
    public float xDiff = 0;
    public float yDiff = 0;
    public DirectionInfo xDirection;
    public DirectionInfo yDirection;
    public float absXDiff = 0;
    public float absYDiff = 0;
    public double _angle = 0;
    public float distance = 0;
    public enum Direction {LEFT = -1, RIGHT = 1, UP = 1, DOWN = -1, NONE = 0}
    public enum Condition {TRUE = 1, FALSE = -1, NONE = 0}
    public GameObject chainMask;
    public ChainScript chainScript;
    public class DirectionInfo
    {
        public Direction direction;
        public Condition condition;

        public DirectionInfo(Direction direction, Condition condition)
        {
            this.direction = direction;
            this.condition = condition;
        }
    }

    public DirectionInfo[] directionInfos = new DirectionInfo[5];

    // Start is called before the first frame update
    void Start()
    {
        directionInfos[0] = new DirectionInfo(Direction.RIGHT, Condition.TRUE);
        directionInfos[1] = new DirectionInfo(Direction.LEFT, Condition.FALSE);
        directionInfos[2] = new DirectionInfo(Direction.UP, Condition.TRUE);
        directionInfos[3] = new DirectionInfo(Direction.DOWN, Condition.FALSE);
        directionInfos[4] = new DirectionInfo(Direction.NONE, Condition.NONE);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Begin()
    {
        _angle = getAngleSolution1();
        chainMask.transform.rotation = Quaternion.Euler(new Vector3(0, 0, (float)_angle));
        calDistance();

        chainScript.Begin();
    }

    public double getAngleSolution1()
    {
        flag = false;
        xDiff = target.transform.position.x - transform.position.x;
        yDiff = target.transform.position.y - transform.position.y;
        xDirection = xDiff > 0 ? directionInfos[0] : (xDiff < 0 ? directionInfos[1] : directionInfos[4]);
        yDirection = yDiff > 0 ? directionInfos[2] : (yDiff < 0 ? directionInfos[3] : directionInfos[4]);
        absXDiff = Math.Abs(xDiff);
        absYDiff = Math.Abs(yDiff);
        double angle = Mathf.Atan2(absYDiff, absXDiff) * 360 / (2* Math.PI);
        
        if (xDiff * yDiff < 0)
        {
            angle = 180 - angle;
            flag = true;
        }
        
        if (yDirection.condition == Condition.FALSE) angle += 180;
        else if (xDirection.condition == Condition.FALSE && !flag)
        {
            angle += 180;
        }

        return angle;
    }

    public double getAngleSolution2()
    {
        xDiff = target.transform.position.x - transform.position.x;
        yDiff = target.transform.position.y - transform.position.y;

        Vector2 vector2 = new Vector2(xDiff, yDiff);
        double angle = Vector2.SignedAngle(Vector2.right, vector2);

        return angle;
    }

    public void calDistance()
    {
        distance = Vector2.Distance(transform.position, target.transform.position);
        distance = distance == 0 ? 0.0001f : distance;
    }
}
