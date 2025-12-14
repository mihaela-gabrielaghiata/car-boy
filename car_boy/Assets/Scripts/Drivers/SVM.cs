using UnityEngine;

[System.Serializable]
public class SVM
{
    public float[] weights;
    public float bias;

    public SVM(int inputSize)
    {
        weights = new float[inputSize];
        Randomize();
    }

    public void Randomize()
    {
        for (int i = 0; i < weights.Length; i++)
            weights[i] = Random.Range(-1f, 1f);

        bias = Random.Range(-1f, 1f);
    }

    public float Evaluate(float[] x)
    {
        float sum = bias;
        for (int i = 0; i < x.Length; i++)
            sum += weights[i] * x[i];

        return sum;
    }

    public int Predict(float[] x)
    {
        return Evaluate(x) >= 0 ? 1 : -1;
    }
}

