using UnityEngine;
using System.Collections;

public class BlackHoleWooshSimple : MonoBehaviour
{
    public Transform blackHoleTransform;
    public float triggerRadius = 50f;
    public float minInterval = 5f;
    public float maxInterval = 10f;
    public float pitchVariation = 0.1f;

    public AudioSource _audio;
    bool _inRange = false;

    void Awake()
    {
        // _audio = GetComponent<AudioSource>();
        _audio.playOnAwake = false;
        _audio.loop = false;
    }

    void Update()
    {
        float dist = Vector3.Distance(transform.position, blackHoleTransform.position);
        bool nowIn = dist <= triggerRadius;

        if (nowIn && !_inRange)
        {
            _inRange = true;
            StartCoroutine(WooshRoutine());
        }
        else if (!nowIn && _inRange)
        {
            _inRange = false;
            StopAllCoroutines();
        }
    }

    IEnumerator WooshRoutine()
    {
        while (_inRange)
        {
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
            _audio.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
            _audio.Play();
        }
    }
}
