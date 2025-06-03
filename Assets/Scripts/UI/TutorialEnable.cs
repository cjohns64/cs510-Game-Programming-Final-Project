using UnityEngine;

public class TutorialEnable : MonoBehaviour
{
    [SerializeField] private GlobalSaveManager global_save_manager;
    [SerializeField] private GameObject tutorial_menu;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (global_save_manager.tutorial_enabled)
        {
            tutorial_menu.SetActive(true);
        }
    }
}
