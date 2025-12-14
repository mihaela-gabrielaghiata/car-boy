using UnityEngine;

public class EvolutionManager : MonoBehaviour
{
    public GameObject start;
    public GameObject carPrefab;
    public GameObject MVCar;

    [Space(20)]
    public int populationSize = 10;

    private Camera mainCam;
    private GameObject[] population;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        mainCam = Camera.main;

        InitializePopulation();
    }

    // Update is called once per frame
    void Update()
    {
        if (allCarsCrashed())
        {
            // TODO: Aici trebuie sa scoti apelul functiei de mai jos si sa faci algoritmul evolutiv
            // =================================================================================================================
            InitializePopulation();
            // =================================================================================================================
        }
        GameObject bestCar = getBestCar();
        mainCam.GetComponent<CameraFollow>().target = bestCar.transform;

    }

    void InitializePopulation()
    {
        Vector3 spawnPos = start.transform.position;

        if (population != null)
        {
            foreach (GameObject car in population)
            {
                Destroy(car);
            }
        }

        population = new GameObject[populationSize];
        for (int i = 0; i < populationSize; i++)
        {
            population[i] = Instantiate(MVCar, spawnPos, Quaternion.identity);
        }
    }

    bool allCarsCrashed()
    {
        foreach (GameObject car in population)
        {
            ICarDriver driver = car.GetComponent<ICarDriver>();
            if (!driver.HasCollided())
            {
                return false;
            }
        }
        return true;
    }

    GameObject getBestCar()
    {
        GameObject bestCar = null;
        float maxDistance = -1f;
        foreach (GameObject car in population)
        {
            MaxVerstappenDriver driver = car.GetComponent<MaxVerstappenDriver>();
            float distance = driver.GetDistanceTravelled();
            if (distance > maxDistance)
            {
                maxDistance = distance;
                bestCar = car;
            }
        }
        return bestCar;
    }

}
