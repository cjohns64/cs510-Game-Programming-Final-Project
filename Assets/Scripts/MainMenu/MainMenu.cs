using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu;
    private List<AsyncOperation> scenesToLoad = new();
    private float loading_progress = 0f;

    [Header("Save Managers")]
    public GlobalSaveManager global_save_manager;
    public LocalSceneSaveManager[] local_save_managers;

    public void LoadZoneByIndex(int index)
    {
        InvalidateSaves();
        ShowLoadingScreen();
        scenesToLoad.Clear();
        scenesToLoad.Add(SceneManager.LoadSceneAsync("Zone " + index.ToString()));
    }

    private void InvalidateSaves()
    {
        global_save_manager.contains_saved_data = false;
        foreach (LocalSceneSaveManager manager in local_save_managers)
        {
            manager.contains_local_saved_data = false;
        }
    }

    private void ShowLoadingScreen()
    {

    }

    public void PlayTutorial()
    {
        LoadZoneByIndex(0);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
