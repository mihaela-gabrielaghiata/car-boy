using UnityEngine;
using System.Collections.Generic;

public class MaxVerstappenDriver : ICarDriver
{
    public GameObject sensorObject;

    public SVM svmLeft;
    public SVM svmRight;
    public SVM svmForward;

    Vector3 lastPos;
    float distanceTravelled;

    private DistanceSensor sensor;

    void Start()
    {
        lastPos = transform.position;
        distanceTravelled = 0f;

        sensor = sensorObject.GetComponent<DistanceSensor>();

        int sizeN = sensor.GetNumberOfRays();

        Debug.Log("Number of sensor rays: " + sizeN);

        svmLeft = new SVM(sizeN);
        svmRight = new SVM(sizeN);
        svmForward = new SVM(sizeN);
    }

    // Update is called once per frame
    void Update()
    {
        if (!colided)
        {
            float[] distances = sensor.GetDistances();

            if(distances.Length != svmLeft.weights.Length)
            {
                Debug.LogError("d: " + distances.Length + "\nsvm: " + svmLeft.weights.Length + "\n");
                return;
            }

            float leftScore = svmLeft.Evaluate(distances);
            float rightScore = svmRight.Evaluate(distances);
            float forwardScore = svmForward.Evaluate(distances);

            selectedDirection[FORWARD] = true;
            selectedDirection[REVERSE] = false;

            if (leftScore > rightScore && leftScore > forwardScore)
            {
                selectedDirection[LEFT] = true;
                selectedDirection[RIGHT] = false;
            }
            else if (rightScore > leftScore && rightScore > forwardScore)
            {
                selectedDirection[LEFT] = false;
                selectedDirection[RIGHT] = true;
            }
            else
            {
                selectedDirection[LEFT] = false;
                selectedDirection[RIGHT] = false;
            }
            
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

    public List<SVM> GetSVM()
    {
        if(svmLeft == null || svmRight == null || svmForward == null)
        {
            Debug.LogError("SVMs are not initialized!");
            return null;
        }
        List<SVM> svms = new List<SVM>();
        svms.Add(new SVM(svmLeft));
        svms.Add(new SVM(svmRight));
        svms.Add(new SVM(svmForward));
        return svms;
    }

    public void SetSVM(List<SVM> svms)
    {
        if(svms == null || svms.Count != 3)
        {
            Debug.LogError("Invalid SVM list provided!");
            return;
        }
        svmLeft = new SVM(svms[0]);
        svmRight = new SVM(svms[1]);
        svmForward = new SVM(svms[2]);
    }

    public float GetDistanceTravelled()
    {
        return distanceTravelled;
    }
}
