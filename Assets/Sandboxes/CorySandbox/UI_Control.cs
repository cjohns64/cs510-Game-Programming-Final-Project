using UnityEngine;

public class UI_Control : MonoBehaviour
{
    private bool keydown_T = false;
    private float key_timer = 0.0f;
    public float wait_time = 0.1f;
    public GameObject menu;
    public GameObject prompt;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        if (keydown_T && key_timer > wait_time) {
            menu.SetActive(!menu.activeSelf);
            prompt.SetActive(!prompt.activeSelf);
            keydown_T = false;
            key_timer = 0.0f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        key_timer += Time.deltaTime;

        if (Input.GetKeyDown("t"))
        {
            keydown_T = true;
        }
        
    }
}
