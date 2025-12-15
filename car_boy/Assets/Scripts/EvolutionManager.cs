using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EvolutionManager : MonoBehaviour
{
    public HUDController hudController;

    public GameObject start;
    public GameObject carPrefab;
    public GameObject MVCar;

    [Header("Evolutionary Algorithm Parameters")]
    [Space(20)]
    public int populationSize = 10;
    public float mutationRate = 0.1f;
    public float crossoverRate = 0.9f;
    public int tournamentSize = 3;
    public int elitismCount = 1;

    private int generation = 1;
    private float bestRecordDistance = 0f;
    private float currentGenBestDistance = 0f;

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
            EvolvePopulation();
        }
        GameObject bestCar = getBestCar();
        if (bestCar != null)
        {
            mainCam.GetComponent<CameraFollow>().target = bestCar.transform;

            MaxVerstappenDriver driver = bestCar.GetComponent<MaxVerstappenDriver>();
            if (driver != null)
            {
                currentGenBestDistance = driver.GetDistanceTravelled();
            }
        }
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;

        GUI.Box(new Rect(10, 10, 300, 100), "");
        GUI.Label(new Rect(20, 20, 280, 30), $"Generation: {generation}", style);
        GUI.Label(new Rect(20, 50, 280, 30), $"Current Best Dist: {currentGenBestDistance:F2}", style);
        GUI.Label(new Rect(20, 80, 280, 30), $"All-Time Record: {bestRecordDistance:F2}", style);
    }

    public void PopulationSizeChanged()
    {
        int newSize = hudController.GetPopulationSize();
        populationSize = newSize;
        InitializePopulation();
    }

    public void RestartSimulation()
    {
        generation = 1;
        bestRecordDistance = 0f;
        InitializePopulation();
    }

    void InitializePopulation(List<List<SVM>> genomes = null)
    {
        Vector3 spawnPos = start.transform.position;

        if (population != null)
        {
            foreach (GameObject car in population)
            {
                if (car != null) Destroy(car);
            }
        }

        population = new GameObject[populationSize];
        for (int i = 0; i < populationSize; i++)
        {
            population[i] = Instantiate(MVCar, spawnPos, Quaternion.identity);

            if (genomes != null && i < genomes.Count)
            {
                MaxVerstappenDriver driver = population[i].GetComponent<MaxVerstappenDriver>();
                driver.SetSVM(genomes[i]);
            }
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

    void EvolvePopulation()
    {
        generation++;

        List<(List<SVM> genome, float fitness)> rankedPopulation = new List<(List<SVM>, float)>();

        foreach (GameObject car in population)
        {
            MaxVerstappenDriver driver = car.GetComponent<MaxVerstappenDriver>();
            if (driver != null)
            {
                rankedPopulation.Add((driver.GetSVM(), driver.GetDistanceTravelled()));
            }
        }

        rankedPopulation = rankedPopulation.OrderByDescending(x => x.fitness).ToList();

        if (rankedPopulation.Count > 0)
        {
            float genBest = rankedPopulation[0].fitness;
            if (genBest > bestRecordDistance)
            {
                bestRecordDistance = genBest;
            }
        }

        List<List<SVM>> nextGenGenomes = new List<List<SVM>>();

        for (int i = 0; i < elitismCount && i < rankedPopulation.Count; i++)
        {
            nextGenGenomes.Add(CloneGenome(rankedPopulation[i].genome));
        }

        while (nextGenGenomes.Count < populationSize)
        {
            List<SVM> parent1 = TournamentSelection(rankedPopulation);
            List<SVM> parent2 = TournamentSelection(rankedPopulation);

            List<SVM> child;

            if (Random.value < crossoverRate)
            {
                child = ArithmeticCrossover(parent1, parent2);
            }
            else
            {
                child = CloneGenome(parent1);
            }

            MutateGenome(child);

            nextGenGenomes.Add(child);
        }

        InitializePopulation(nextGenGenomes);
    }

    List<SVM> TournamentSelection(List<(List<SVM> genome, float fitness)> rankedPop)
    {
        var best = rankedPop[Random.Range(0, rankedPop.Count)];

        for (int i = 1; i < tournamentSize; i++)
        {
            var candidate = rankedPop[Random.Range(0, rankedPop.Count)];
            if (candidate.fitness > best.fitness)
            {
                best = candidate;
            }
        }
        return CloneGenome(best.genome);
    }

    List<SVM> ArithmeticCrossover(List<SVM> p1, List<SVM> p2)
    {
        List<SVM> child = new List<SVM>();
        float alpha = Random.Range(0f, 1f);

        for (int i = 0; i < p1.Count; i++)
        {
            SVM svmChild = new SVM(p1[i].weights.Length);

            svmChild.bias = alpha * p1[i].bias + (1f - alpha) * p2[i].bias;

            for (int w = 0; w < p1[i].weights.Length; w++)
            {
                svmChild.weights[w] = alpha * p1[i].weights[w] + (1f - alpha) * p2[i].weights[w];
            }
            child.Add(svmChild);
        }
        return child;
    }

    void MutateGenome(List<SVM> genome)
    {
        foreach (SVM svm in genome)
        {
            if (Random.value < mutationRate)
            {
                svm.bias += Random.Range(-0.1f, 0.1f);
            }

            for (int w = 0; w < svm.weights.Length; w++)
            {
                if (Random.value < mutationRate)
                {
                    svm.weights[w] += Random.Range(-0.2f, 0.2f);
                }
            }
        }
    }

    List<SVM> CloneGenome(List<SVM> original)
    {
        List<SVM> clone = new List<SVM>();
        foreach (SVM svm in original)
        {
            clone.Add(new SVM(svm));
        }
        return clone;
    }
}