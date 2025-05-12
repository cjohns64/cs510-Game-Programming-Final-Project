using UnityEngine;

public class TradeEventNotifications : MonoBehaviour
{
    public void NoSpaceEvent()
    {
        Debug.Log("Not enough cargo space!");
    }
    public void NoFundsEvent()
    {
        Debug.Log("Not enough credits!");
    }
}
