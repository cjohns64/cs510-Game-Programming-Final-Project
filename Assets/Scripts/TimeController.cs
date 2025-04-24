using UnityEngine;
using System;

public class TimeController : MonoBehaviour
{
    // Singleton instance
    private static TimeController _instance;
    public static TimeController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<TimeController>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("TimeController");
                    _instance = obj.AddComponent<TimeController>();
                }
            }
            return _instance;
        }
    }

    [Header("Settings")]
    [SerializeField, Range(0f, 100f)]
    private float _timeScale = 1f;

    public float TimeScale
    {
        get => _timeScale;
        set
        {
            _timeScale = Mathf.Clamp(value, 0f, 100f);
            UpdateTimeScale();
            OnTimeScaleChanged?.Invoke(_timeScale);
        }
    }

    public static event Action<float> OnTimeScaleChanged;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(this.gameObject); // Optional: persist across scenes
        UpdateTimeScale();
    }

    private void UpdateTimeScale()
    {
        Time.timeScale = _timeScale;
        Time.fixedDeltaTime = 0.02f * _timeScale;
    }

    public void Pause() => TimeScale = 0f;
    public void Resume() => TimeScale = 1f;
    public void SetTimeScale(float scale) => TimeScale = scale;
}