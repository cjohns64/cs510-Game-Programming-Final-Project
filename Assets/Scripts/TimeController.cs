using UnityEngine;

public class TimeController : MonoBehaviour
{
    private readonly float[] timeScales = { 1f, 5f, 10f, 50f, 100f };
    private int currentIndex = 0;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Period) || Input.GetKeyDown(KeyCode.Greater)) // > or .
        {
            IncreaseTimeScale();
        }
        else if (Input.GetKeyDown(KeyCode.Comma) || Input.GetKeyDown(KeyCode.Less)) // < or ,
        {
            DecreaseTimeScale();
        }
    }

    public void IncreaseTimeScale()
    {
        currentIndex = Mathf.Min(currentIndex + 1, timeScales.Length - 1);
        UpdateTimeScale();
    }

    public void DecreaseTimeScale()
    {
        currentIndex = Mathf.Max(currentIndex - 1, 0);
        UpdateTimeScale();
    }

    void UpdateTimeScale()
    {
        Time.timeScale = timeScales[currentIndex];
        Debug.Log($"Time scale set to: {Time.timeScale}x");
    }

    public void SetTimeScale(int index = 0) 
    {
        currentIndex = index;
        UpdateTimeScale();
    }

    void OnDestroy()
    {
        // Reset time scale when this component is destroyed
        Time.timeScale = 1f;
    }
}