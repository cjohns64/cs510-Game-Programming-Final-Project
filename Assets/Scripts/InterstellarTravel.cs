using UnityEngine;

public class InterstellarTravel : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ship")) return;

        string[] words = gameObject.name.Split(' ');
        string systemName = words.Length > 1
            ? string.Join(" ", words, 0, words.Length - 1)
            : gameObject.name;
        Debug.Log($"Entering interstellar tunnel to {systemName}");
    }
}
