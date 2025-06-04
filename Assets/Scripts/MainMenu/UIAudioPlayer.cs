using UnityEngine;

public class UIAudioPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource ui_click;

    public void OnClickEvent()
    {
        ui_click.Play();
    }
}
