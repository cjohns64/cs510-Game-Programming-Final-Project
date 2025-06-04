using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject loadingScreen;
    private Slider loading_bar;

    [Header("Save Managers")]
    public GlobalSaveManager global_save_manager;
    public LocalSceneSaveManager[] local_save_managers;

    private void Start()
    {
        global_save_manager.tutorial_enabled = false;
        loading_bar = loadingScreen.GetComponentInChildren<Slider>();
    }
    public void LoadZoneByIndex(int index)
    {
        loadingScreen.SetActive(true);
        InvalidateSaves();
        StartCoroutine(LoadAsync("Zone " + index.ToString()));
    }

    private void InvalidateSaves()
    {
        global_save_manager.contains_saved_data = false;
        foreach (LocalSceneSaveManager manager in local_save_managers)
        {
            manager.contains_local_saved_data = false;
        }
    }

    IEnumerator LoadAsync(string level)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(level);
        while (!operation.isDone)
        {
            float progress = operation.progress;
            loading_bar.value = progress;
            yield return null;
        }
    }

    public void PlayTutorial()
    {
        global_save_manager.tutorial_enabled = true;
        LoadZoneByIndex(1);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
