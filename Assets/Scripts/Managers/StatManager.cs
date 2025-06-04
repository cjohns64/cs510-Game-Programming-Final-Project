using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public enum DamageType
{
    Impact,
    Energy,
    Raw
}
public class StatManager : MonoBehaviour
{
    [SerializeField] private Slider hull_slider;
    private TMP_Text hull_text;
    private TMP_Text armor_text;
    [SerializeField] private Slider shield_slider;
    private TMP_Text shield_text;

    private float armor = 0f;
    private float shields = 0f;
    private float hull = 100f;
    public UnityEvent ShipDestroyed;
    private float max_shields;
    private float min_hull = 100f;
    private float hull_bonus = 0f;
    [SerializeField] private float shield_recharge_delay = 5f;
    [SerializeField] private float shield_recharge_speed = 0.1f;
    private float shield_timer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // lookup health and shields text
        hull_text = hull_slider.gameObject.transform.Find("current").gameObject.GetComponent<TMP_Text>();
        armor_text = hull_slider.gameObject.transform.Find("armor").gameObject.GetComponent<TMP_Text>();
        shield_text = shield_slider.gameObject.transform.Find("current").gameObject.GetComponent<TMP_Text>();
        SetSliderValues();
        ShipDestroyed.AddListener(OnShipDestroyed);
    }

    // Update is called once per frame
    void Update()
    {
        shield_timer += Time.deltaTime;
        if (shield_timer > shield_recharge_delay )
        {
            shields += shield_recharge_speed;
            if (shields > max_shields)
            {
                shields = max_shields;
            }
            SetSliderValues();
        }
    }

    public void OnShipDestroyed()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }

    private void SetSliderValues()
    {
        hull_slider.value = hull;
        hull_text.text = hull.ToString("0.##") + "/" + (min_hull + hull_bonus).ToString("0.##");
        armor_text.text = armor.ToString() + "AR";
        shield_slider.value = shields;
        shield_text.text = shields.ToString("0.##") + "/" + max_shields.ToString("0.##");
    }

    public void SetMaxHull(float hull)
    {
        this.hull_bonus = hull;
        hull_slider.maxValue = hull;
    }

    public bool IsShielded()
    {
        return shields > 0;
    }

    public void SetMaxShields(float shields)
    {
        this.max_shields = shields;
        shield_slider.maxValue = shields;
    }

    public void SetArmor(float armor)
    {
        this.armor = armor;
    }

    public void RepairHull()
    {
        hull = this.hull_bonus + min_hull;
        SetSliderValues();
    }

    public void Damage(float damage, DamageType type)
    {
        if (type == DamageType.Raw)
        {
            // direct damage
            hull -= damage;
            if (hull < 0)
            {
                ShipDestroyed?.Invoke();
            }
            SetSliderValues();
        }
        else if (type == DamageType.Impact)
        {
            shields -= damage;
            if (shields < 0)
            {
                float reduced_damage = (-shields) - armor; //shields is negative
                if (reduced_damage < 0)
                {
                    reduced_damage = 0f;
                }
                hull -= reduced_damage; // subtract the difference
                if (hull < 0)
                {
                    ShipDestroyed?.Invoke();
                }
                shields = 0;
            }
            // start recharge delay
            shield_timer = 0f;
            SetSliderValues();
        }
        else
        {
            // energy damage ignores shields
            float reduced_damage = damage - armor;
            if (reduced_damage < 0)
            {
                reduced_damage = 0f;
            }
            hull -= reduced_damage;
            if (hull < 0)
            {
                ShipDestroyed?.Invoke();
            }
            SetSliderValues();
        }
    }
}
