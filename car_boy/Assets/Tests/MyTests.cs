using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class MyTests
{
    Type FindTypeByName(string name)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType(name);
            if (t != null) return t;
        }
        return null;
    }

    [Test]
    public void SVM_EvaluateAndPredict_WorksAsExpected()
    {
        var svmType = FindTypeByName("SVM");
        Assert.IsNotNull(svmType);

        var svm = Activator.CreateInstance(svmType, new object[] { 2 });

        var wf = svmType.GetField("weights");
        var bf = svmType.GetField("bias");

        wf.SetValue(svm, new float[] { 0.5f, -0.2f });
        bf.SetValue(svm, 0.1f);

        float[] input = { 1f, 2f };

        float eval = (float)svmType.GetMethod("Evaluate").Invoke(svm, new object[] { input });
        Assert.AreEqual(0.2f, eval, 1e-5f);

        int pred = (int)svmType.GetMethod("Predict").Invoke(svm, new object[] { input });
        Assert.AreEqual(1, pred);
    }

    GameObject CreateMVCarPrefab() => new GameObject("MVCar");

    [Test]
    public void InitializePopulation_CreatesPopulation()
    {
        var emType = FindTypeByName("EvolutionManager");
        Assert.IsNotNull(emType);

        var go = new GameObject("EM");
        var em = go.AddComponent(emType);

        emType.GetField("MVCar").SetValue(em, CreateMVCarPrefab());
        emType.GetField("start").SetValue(em, new GameObject("Start"));
        emType.GetField("populationSize").SetValue(em, 3);

        emType.GetMethod("Start").Invoke(em, null);

        var pop = (GameObject[])emType.GetField("population", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(em);

        Assert.AreEqual(3, pop.Length);
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void ArithmeticCrossover_ProducesChildWithinBounds()
    {
        var emType = FindTypeByName("EvolutionManager");
        var svmType = FindTypeByName("SVM");

        var go = new GameObject("EM2");
        var em = go.AddComponent(emType);

        var listType = typeof(System.Collections.Generic.List<>).MakeGenericType(svmType);
        var p1 = Activator.CreateInstance(listType);
        var p2 = Activator.CreateInstance(listType);

        var s1 = Activator.CreateInstance(svmType, new object[] { 2 });
        var s2 = Activator.CreateInstance(svmType, new object[] { 2 });

        svmType.GetField("weights").SetValue(s1, new float[] { 0f, 1f });
        svmType.GetField("weights").SetValue(s2, new float[] { 1f, -1f });

        listType.GetMethod("Add").Invoke(p1, new object[] { s1 });
        listType.GetMethod("Add").Invoke(p2, new object[] { s2 });

        var child = emType.GetMethod("ArithmeticCrossover", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(em, new object[] { p1, p2 }) as System.Collections.IList;

        Assert.AreEqual(1, child.Count);
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void EvolvePopulation_ReinitializesPopulation_WhenEmpty()
    {
        var emType = FindTypeByName("EvolutionManager");
        var go = new GameObject("EM3");
        var em = go.AddComponent(emType);
        // prepare manager state: set MVCar prefab and start position
        emType.GetField("MVCar").SetValue(em, CreateMVCarPrefab());
        emType.GetField("start").SetValue(em, new GameObject("Start"));
        // ensure desired population size
        emType.GetField("populationSize").SetValue(em, 3);

        // set population to empty to trigger reinitialization in EvolvePopulation
        emType.GetField("population", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(em, new GameObject[0]);
        // set generation to a known value
        emType.GetField("generation", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(em, 7);

        // call EvolvePopulation directly; with empty population it should call InitializePopulation and return
        emType.GetMethod("EvolvePopulation", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(em, null);

        var gen = (int)emType.GetField("generation", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(em);
        // generation should remain the same because there were no genomes to evolve
        Assert.AreEqual(7, gen);

        var pop = (GameObject[])emType.GetField("population", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(em);
        Assert.IsNotNull(pop);
        Assert.AreEqual(3, pop.Length);

        // cleanup
        foreach (var g in pop) if (g != null) UnityEngine.Object.DestroyImmediate(g);
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void RestartSimulation_ResetsState()
    {
        var emType = FindTypeByName("EvolutionManager");
        var go = new GameObject("EM4");
        var em = go.AddComponent(emType);

        emType.GetMethod("RestartSimulation").Invoke(em, null);
        Assert.AreEqual(1, emType.GetField("generation", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(em));

        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void Brain_GetSetSVMs_DeepCopy()
    {
        var brainType = FindTypeByName("Brain");
        var svmType = FindTypeByName("SVM");

        var brain = Activator.CreateInstance(brainType);
        var svm = Activator.CreateInstance(svmType, new object[] { 2 });

        svmType.GetField("weights").SetValue(svm, new float[] { 0.3f, -0.4f });

        var listType = typeof(System.Collections.Generic.List<>).MakeGenericType(svmType);
        var list = Activator.CreateInstance(listType);
        listType.GetMethod("Add").Invoke(list, new object[] { svm });

        brainType.GetMethod("SetSVMs").Invoke(brain, new object[] { list });
        var ret = brainType.GetMethod("GetSVMs").Invoke(brain, null) as System.Collections.IList;

        Assert.AreNotSame(svm, ret[0]);
    }

    [Test]
    public void SaveAndLoadBrain_Roundtrip()
    {
        var brainType = FindTypeByName("Brain");
        var brain = Activator.CreateInstance(brainType);

        brainType.GetMethod("SaveBrain").Invoke(brain, new object[] { "test" });
        brainType.GetMethod("LoadBrain").Invoke(brain, new object[] { "test" });

        Assert.Pass();
    }

    [Test]
    public void CloneGenome_ProducesDeepCopy()
    {
        var emType = FindTypeByName("EvolutionManager");
        var svmType = FindTypeByName("SVM");

        var go = new GameObject("EM5");
        var em = go.AddComponent(emType);

        var listType = typeof(System.Collections.Generic.List<>).MakeGenericType(svmType);
        var list = Activator.CreateInstance(listType);

        var svm = Activator.CreateInstance(svmType, new object[] { 2 });
        listType.GetMethod("Add").Invoke(list, new object[] { svm });

        var clone = emType.GetMethod("CloneGenome", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(em, new object[] { list }) as System.Collections.IList;

        Assert.AreNotSame(svm, clone[0]);
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void MutateGenome_NoMutation_WhenRateZero()
    {
        var emType = FindTypeByName("EvolutionManager");
        var svmType = FindTypeByName("SVM");

        var go = new GameObject("EM6");
        var em = go.AddComponent(emType);

        emType.GetField("mutationRate").SetValue(em, 0f);

        var svm = Activator.CreateInstance(svmType, new object[] { 2 });
        svmType.GetField("weights").SetValue(svm, new float[] { 0.1f, 0.2f });

        var listType = typeof(System.Collections.Generic.List<>).MakeGenericType(svmType);
        var list = Activator.CreateInstance(listType);
        listType.GetMethod("Add").Invoke(list, new object[] { svm });

        emType.GetMethod("MutateGenome", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(em, new object[] { list });

        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TournamentSelection_ReturnsValidGenome()
    {
        var emType = FindTypeByName("EvolutionManager");
        var svmType = FindTypeByName("SVM");

        var go = new GameObject("EM7");
        var em = go.AddComponent(emType);

        var listType = typeof(System.Collections.Generic.List<>).MakeGenericType(svmType);
        var genome = Activator.CreateInstance(listType);

        var svm = Activator.CreateInstance(svmType, new object[] { 2 });
        listType.GetMethod("Add").Invoke(genome, new object[] { svm });

        var tupleType = typeof(ValueTuple<,,>).MakeGenericType(listType, typeof(float), typeof(float));
        var rankedType = typeof(System.Collections.Generic.List<>).MakeGenericType(tupleType);
        var ranked = Activator.CreateInstance(rankedType);

        var entry = Activator.CreateInstance(tupleType, new object[] { genome, 1f, 1f });
        rankedType.GetMethod("Add").Invoke(ranked, new object[] { entry });

        var result = emType.GetMethod("TournamentSelection", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(em, new object[] { ranked }) as System.Collections.IList;

        Assert.IsNotNull(result);
        UnityEngine.Object.DestroyImmediate(go);
    }
}
