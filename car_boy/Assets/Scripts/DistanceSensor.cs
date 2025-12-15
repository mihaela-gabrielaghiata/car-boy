using UnityEngine;
using System.Collections.Generic;

public class DistanceSensor : MonoBehaviour
{
    [Header("Sensor Settings")]
    public float sensorRange = 10f;
    [Range(0, 10)]
    public int numberOfDirectionsPerSide = 3;
    public bool debugMode = true;
    public float lineWidth = 0.05f;
    public LayerMask detectionLayer;

    private Vector3[] directions;
    private float[] distances;
    private List<LineRenderer[]> rays = new List<LineRenderer[]>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BuildDirections();
        distances = new float[directions.Length];
        foreach(var _ in directions)
        {
            LineRenderer hitLine = CreateLine(Color.yellow);
            LineRenderer restLine = CreateLine(Color.black);
            rays.Add(new LineRenderer[] { hitLine, restLine } );
        }
        detectionLayer = LayerMask.GetMask("TrackLimit");
    }

    // Update is called once per frame
    void Update()
    {
        CastRays();
    }

    public float[] GetDistances()
    {
        return distances;
    }

    public int GetNumberOfRays()
    {
        return numberOfDirectionsPerSide * 2 + 1;
    }

    void BuildDirections()
    {
        int totalDirections = numberOfDirectionsPerSide * 2 + 1;
        directions = new Vector3[totalDirections];
        float angleStep = 180f / (totalDirections - 1);
        for(float angle = -90f, i = 0; i < totalDirections; angle += angleStep, i++)
        {
            directions[(int)i] = Quaternion.Euler(0, angle, 0) * transform.forward;
        }
    }


    void CastRays()
    {
        
        for (int i = 0; i < directions.Length; i++)
        {
            Vector3 worldDir = transform.TransformDirection(directions[i]);
            Ray ray = new Ray(transform.position, worldDir);
            if(Physics.Raycast(ray, out RaycastHit hit, sensorRange, detectionLayer))
            {
                distances[i] = hit.distance / sensorRange;


                if (debugMode) { 
                    rays[i][0].SetPosition(0, transform.position);
                    rays[i][0].SetPosition(1, hit.point);

                    rays[i][1].SetPosition(0, hit.point);
                    rays[i][1].SetPosition(1, transform.position + worldDir * sensorRange);
                }
                else
                {
                    rays[i][0].SetPosition(0, transform.position);
                    rays[i][0].SetPosition(1, transform.position);
                    rays[i][1].SetPosition(0, transform.position);
                    rays[i][1].SetPosition(1, transform.position);
                }
            }
            else
            {
                distances[i] = 1f;

                if (debugMode)
                {
                    rays[i][0].SetPosition(0, transform.position);
                    rays[i][0].SetPosition(1, transform.position + worldDir * sensorRange);

                    rays[i][1].SetPosition(0, transform.position + worldDir * sensorRange);
                    rays[i][1].SetPosition(1, transform.position + worldDir * sensorRange);
                }
                else
                {
                    rays[i][0].SetPosition(0, transform.position);
                    rays[i][0].SetPosition(1, transform.position);
                    rays[i][1].SetPosition(0, transform.position);
                    rays[i][1].SetPosition(1, transform.position);
                }
            }
        }

    }

    LineRenderer CreateLine(Color color)
    {
        GameObject go = new GameObject("SensorLine");
        go.transform.parent = transform;

        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.material = new Material(Shader.Find("Unlit/Color"));
        lr.material.color = color;
        lr.useWorldSpace = true;

        return lr;
    }
}
