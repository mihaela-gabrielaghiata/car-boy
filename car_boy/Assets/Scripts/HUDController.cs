using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Buttons")]
    public Button StartPauseButton;
    public Button RestartButton;

    [Header("Speed Buttons:")]
    public Button Speed1xButton;
    public Button Speed2xButton;
    public Button Speed3xButton;
    public Button SpeedMaxButton;
    public float MaxSpeed = 20f;

    [Header("Population Size")]
    public TMP_InputField populationSizeInput;

    private bool isPaused = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Time.timeScale = 0f;
    }

    public void TogglePause()
    {
        SetPauseState(!isPaused);
        StartPauseButton.GetComponentInChildren<TMP_Text>().text = isPaused ? "Start" : "Pause";
    }

    public void SetSpeed1x()
    {
        SetSimulationSpeed(1f);
    }

    public void SetSpeed2x()
    {
        SetSimulationSpeed(2f);
    }

    public void SetSpeed3x()
    {
        SetSimulationSpeed(3f);
    }

    public void SetSpeedMax()
    {
        SetSimulationSpeed(MaxSpeed);
    }

    public int GetPopulationSize()
    {
        int size = 10;
        if(int.TryParse(populationSizeInput.text, out size))
        {
            return size;
        }
        return 10;
    }


    // ---------------

    private void SetPauseState(bool paused)
    {
        if(paused != isPaused)
        {
            isPaused = paused;
            Time.timeScale = isPaused ? 0f : 1f;
        }
    }

    private void SetSimulationSpeed(float speed)
    {
        if (!isPaused)
        {
            Time.timeScale = speed;
        }
    }
}
