using UnityEngine;

public class DamageManager : MonoBehaviour
{
    private StatManager statManager;
    [SerializeField] private float danger_radius;
    [SerializeField] private float danger_max_damage = 100f;
    [SerializeField] private AudioSource sun_damage_audio;
    [SerializeField] private AudioSource asteriod_impact_audio;
    [SerializeField] private AudioSource shield_impact_audio;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        statManager = GameObject.Find("GlobalScripts").GetComponent<StatManager>();
    }

    // Update is called once per frame
    void Update()
    {
        float dot = Vector3.Dot(transform.position, transform.position);
        if ( dot < danger_radius)
        {
            // central bodies are at the origin
            statManager.Damage(danger_max_damage / ( dot + 0.1f), DamageType.Energy); // more damage at lower radii
            if (!sun_damage_audio.isPlaying )
            {
                sun_damage_audio.Play();
                sun_damage_audio.volume = (danger_radius / (dot + 0.1f)) * 0.1f;
            }
        }
        else if (sun_damage_audio.isPlaying)
        {
            sun_damage_audio.Stop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Asteroid")
        {
            statManager.Damage(Random.Range(10f, 50f), DamageType.Impact);
            Destroy(other.gameObject);
            asteriod_impact_audio.Play();
            if (statManager.IsShielded())
                shield_impact_audio.Play();
        }
        else if (other.gameObject.tag == "Star")
        {
            // lots of damage for colliding with a star
            statManager.Damage(5 * danger_max_damage, DamageType.Raw);
        }
    }
}
