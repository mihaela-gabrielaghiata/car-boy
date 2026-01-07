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
    private float bestRecordLapTime = -1f;
    private float currentGenBestDistance = 0f;

    private Brain bestBrain;
    private string mode = "evolving";

    private Camera mainCam;
    private GameObject[] population;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        mainCam = Camera.main;
        bestBrain = new Brain();

        InitializePopulation();
    }

    // Update is called once per frame
    void Update()
    {
        if (allCarsCrashed())
        {
            if (mode == "evolving")
            {
                EvolvePopulation();
            }
            else if (mode == "best_loaded")
            {
                InitializePopulation(new List<List<SVM>> { bestBrain.GetSVMs() });
            }
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

        GUI.Box(new Rect(10, 10, 300, 130), "");
        GUI.Label(new Rect(20, 20, 280, 30), $"Generation: {generation}", style);
        GUI.Label(new Rect(20, 50, 280, 30), $"Current Best Dist: {currentGenBestDistance:F2}", style);
        GUI.Label(new Rect(20, 80, 280, 30), $"All-Time Record: {bestRecordDistance:F2}", style);
        GUI.Label(new Rect(20, 110, 280, 30), $"All-Time Lap Time Record: {bestRecordLapTime:F2}", style);
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
        bestRecordLapTime = -1f;
        mode = "evolving";
        if (!Application.isPlaying) return;
        InitializePopulation();
    }

    public void SaveBestGenome()
    {
        bestBrain.SaveBrain("best");
    }

    public void LoadBestGenome()
    {
        bestBrain.LoadBrain("best");
        InitializePopulation(new List<List<SVM>> { bestBrain.GetSVMs() });
        mode = "best_loaded";
    }

    void InitializePopulation(List<List<SVM>> genomes = null)
    {
        Debug.Log("Genomes count: " + (genomes != null ? genomes.Count : 0));
        Vector3 spawnPos = start.transform.position;

        if (population != null)
        {
            foreach (GameObject car in population)
            {
                if (car != null) Destroy(car);
            }
        }

        int currentSize = populationSize;
        if (genomes != null)
        {
            currentSize = genomes.Count;
        }

        population = new GameObject[currentSize];
        for (int i = 0; i < currentSize; i++)
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
            MaxVerstappenDriver driver = car.GetComponent<MaxVerstappenDriver>();
            if (!driver.HasCollided() && !driver.HasLapEnded())
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
        List<(List<SVM> genome, float fitness, float lapTime)> rankedPopulation = new List<(List<SVM>, float, float)>();

        if (population != null)
        {
            foreach (GameObject car in population)
            {
                if (car == null) continue;
                MaxVerstappenDriver driver = car.GetComponent<MaxVerstappenDriver>();
                if (driver != null)
                {
                    rankedPopulation.Add((driver.GetSVM(), driver.GetDistanceTravelled(), driver.GetLapTime()));
                }
            }
        }

        rankedPopulation = rankedPopulation.OrderByDescending(x => x.fitness).ToList();

        // If there are no genomes to evolve, reinitialize population and do not increment generation
        if (rankedPopulation.Count == 0)
        {
            InitializePopulation();
            Debug.Log("EvolvePopulation: no ranked genomes, reinitialized population");
            return;
        }

        generation++;

        if (rankedPopulation.Count > 0)
        {
            float genBest = rankedPopulation[0].fitness;
            if (genBest > bestRecordDistance)
            {
                bestRecordDistance = genBest;
                bestRecordLapTime = rankedPopulation[0].lapTime;
                bestBrain = new Brain(rankedPopulation[0].genome);
            }
        }

        List<List<SVM>> nextGenGenomes = new List<List<SVM>>();

        for (int i = 0; i < elitismCount && i < rankedPopulation.Count; i++)
        {
            nextGenGenomes.Add(CloneGenome(rankedPopulation[i].genome));
        }
        Debug.Log($"Next generation init: {nextGenGenomes.Count}");

        while (nextGenGenomes.Count < populationSize)
        {
            List<SVM> parent1 = TournamentSelection(rankedPopulation);
            List<SVM> parent2 = TournamentSelection(rankedPopulation);

            List<SVM> child;

            if (Random.value < crossoverRate && parent1.Count == parent2.Count && parent1.Count > 0)
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

    List<SVM> TournamentSelection(List<(List<SVM> genome, float fitness, float lapTime)> rankedPop)
    {
        var result = new List<SVM>();
        if (rankedPop == null || rankedPop.Count == 0) return result;

        int effectiveTournament = Mathf.Min(tournamentSize, rankedPop.Count);

        int idx = (rankedPop.Count > 0) ? UnityEngine.Random.Range(0, rankedPop.Count) : 0;
        if (idx < 0 || idx >= rankedPop.Count) idx = 0;
        var best = rankedPop[idx];
        for (int i = 1; i < effectiveTournament; i++)
        {
            int cidx = (rankedPop.Count > 0) ? UnityEngine.Random.Range(0, rankedPop.Count) : 0;
            if (cidx < 0 || cidx >= rankedPop.Count) cidx = 0;
            var candidate = rankedPop[cidx];
            if (candidate.fitness > best.fitness)
            {
                best = candidate;
            }
        }

        if (best.genome != null)
            return CloneGenome(best.genome);

        return result;
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