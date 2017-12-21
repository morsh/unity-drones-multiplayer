using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instantiation : MonoBehaviour {

    private List<Rigidbody> objects = new List<Rigidbody>();

    public Transform brick;
    void Start()
    {
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                Transform newObject = Instantiate(brick, new Vector3(x, y, 0), Quaternion.identity);
                objects.Add(newObject.GetComponent<Rigidbody>());
            }
        }
    }

    private void Update()
    {
        objects.ForEach(obj =>
        {
            obj.AddRelativeForce(new Vector3(Random.Range(-100, 100) / 100f, Random.Range(-100, 100) / 100f, Random.Range(-100, 100) / 100f));
        });
    }
}