using UnityEngine;
using TMPro;

public class UI_Control : MonoBehaviour
{
    private bool keydown_T = false;
    private float key_timer_T = 0.0f;
    public float wait_time = 0.1f;
    public GameObject menu;
    public GameObject prompt;

    void FixedUpdate()
    {
        if (keydown_T && key_timer_T > wait_time) {
            menu.SetActive(!menu.activeSelf);
            prompt.SetActive(!prompt.activeSelf);
            keydown_T = false;
            key_timer_T = 0.0f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        key_timer_T += Time.deltaTime;

        if (Input.GetKeyDown("t"))
        {
            keydown_T = true;
        }
    }
}
