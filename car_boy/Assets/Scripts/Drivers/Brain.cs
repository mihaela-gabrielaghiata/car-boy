using UnityEngine;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public class Brain
{
    [SerializeField]
    private List<SVM> svms;

    public Brain(List<SVM> svms)
    {
        this.svms = new List<SVM>();
        foreach (var svm in svms)
        {
            this.svms.Add(new SVM(svm));
        }
    }

    public Brain()
    {
        svms = new List<SVM>();
    }

    public List<SVM> GetSVMs()
    {
        List<SVM> clonedSvms = new List<SVM>();
        foreach (var svm in svms)
        {
            clonedSvms.Add(new SVM(svm));
        }
        return clonedSvms;
    }

    public void SetSVMs(List<SVM> newSvms)
    {
        svms.Clear();
        foreach (var svm in newSvms)
        {
            svms.Add(new SVM(svm));
        }
    }

    public void SaveBrain(string prefix)
    {
        Debug.Log("count: " + svms.Count);
        string dir = Path.Combine(Application.dataPath, "Brains");
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        string path = Path.Combine(dir, prefix + "_brain.json");

        string json = JsonUtility.ToJson(this, true);
        File.WriteAllText(path, json);

        Debug.Log("Brain saved to: " + path);
    }

    public void LoadBrain(string prefix)
    {
        string path = Path.Combine(
            Application.dataPath,
            "Brains",
            prefix + "_brain.json"
        );

        if (!File.Exists(path))
        {
            Debug.LogWarning("Brain file not found: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        Brain loaded = JsonUtility.FromJson<Brain>(json);

        SetSVMs(loaded.svms);

        Debug.Log("Brain loaded from: " + path);
    }
}
