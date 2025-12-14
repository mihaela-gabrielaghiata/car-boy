using UnityEngine;

public class KeyboardDriver : ICarDriver
{
    Vector3 lastPos;
    float distanceTravelled;

    void Start()
    {
        lastPos = transform.position;
        distanceTravelled = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!colided)
        {
            selectedDirection[FORWARD] = Input.GetKey(KeyCode.W);
            selectedDirection[REVERSE] = Input.GetKey(KeyCode.S);
            selectedDirection[LEFT] = Input.GetKey(KeyCode.A);
            selectedDirection[RIGHT] = Input.GetKey(KeyCode.D);
            distanceTravelled += Vector3.Distance(transform.position, lastPos);
            lastPos = transform.position;
        }
        else
        {
            selectedDirection[FORWARD] = false;
            selectedDirection[REVERSE] = false;
            selectedDirection[LEFT] = false;
            selectedDirection[RIGHT] = false;
        }

        
    }

    public float GetDistanceTravelled()
    {
        return distanceTravelled;
    }
}
