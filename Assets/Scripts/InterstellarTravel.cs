using System;
using UnityEngine;

public class InterstellarTravel : MonoBehaviour
{
    private SaveManagerInterface saveManager;
    [Header("Default settings")]
    [SerializeField] private string global_scripts = "GlobalScripts";
    private void Start()
    {
        saveManager = GameObject.Find(global_scripts).GetComponent<SaveManagerInterface>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ship")) return;

        string[] words = gameObject.name.Split(' ');
        string systemName = words.Length > 1
            ? string.Join(" ", words, 0, words.Length - 1)
            : gameObject.name;
        Debug.Log($"Entering interstellar tunnel to {systemName}");
        // load other scene
        int index = 0;
        if (Int32.TryParse(words[1], out index))
        {
            // index should be the number of the zone, like 3 in the case of name="Zone 3".
            saveManager.SceneTransitionByIndex(index);
        }
    }
}
