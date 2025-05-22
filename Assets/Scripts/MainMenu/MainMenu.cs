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

    public void StartGame()
    {
        HideMenu();
        ShowLoadingScreen();
        scenesToLoad.Clear();
        scenesToLoad.Add(SceneManager.LoadSceneAsync("Gameplay"));
        scenesToLoad.Add(SceneManager.LoadSceneAsync("Zone1", LoadSceneMode.Additive));
    }

    private void HideMenu()
    {
        
    }

    private void ShowLoadingScreen()
    {

    }

    public void ShowCredits()
    {

    }


    public void ShowZoneSelectMenu()
    {

    }

    public void PlayTutorial()
    {

    }

    public void ExitGame()
    {

    }
}
