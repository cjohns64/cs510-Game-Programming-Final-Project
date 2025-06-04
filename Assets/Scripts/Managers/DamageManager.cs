using UnityEngine;

public class DamageManager : MonoBehaviour
{
    private StatManager statManager;
    [SerializeField] private float danger_radius;
    [SerializeField] private float danger_max_damage = 100f;
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
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Asteroid")
        {
            statManager.Damage(25f, DamageType.Impact);
            Destroy(other.gameObject);
        }
        else if (other.gameObject.tag == "Star")
        {
            // lots of damge for colliding with a star
            statManager.Damage(5 * danger_max_damage, DamageType.Raw);
        }
    }
}
