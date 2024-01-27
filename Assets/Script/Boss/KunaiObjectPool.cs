using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KunaiObjectPool : MonoBehaviour
{
    List<GameObject> gameObjects = new List<GameObject>(20);
    public GameObject Kunai;
    public GameObject target;
    public bool on = false;
    public bool fire;
    public float fireTime = 1;
    void Start()
    {
        for (int i=0;i<gameObjects.Capacity;i++)
        {
            gameObjects.Add(Instantiate(Kunai));
            gameObjects[i].SetActive(false);
        }
    }

    public void FireOne(float speed = 5)
    {
        for (int i=0;i<gameObjects.Capacity;i++)
        {
            if (!gameObjects[i].activeSelf)
            {
                gameObjects[i].transform.position = transform.position;
                gameObjects[i].SetActive(true);
                gameObjects[i].GetComponent<KunaiScript>().target = target;
                gameObjects[i].GetComponent<KunaiScript>().Fire(speed);
                break;
            }
        }
    }
}
