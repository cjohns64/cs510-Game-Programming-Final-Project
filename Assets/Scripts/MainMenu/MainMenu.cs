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

    public void LoadZoneByIndex(int index)
    {
        ShowLoadingScreen();
        scenesToLoad.Clear();
        scenesToLoad.Add(SceneManager.LoadSceneAsync("Zone " + index.ToString()));
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
