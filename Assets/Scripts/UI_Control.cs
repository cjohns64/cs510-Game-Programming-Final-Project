using UnityEngine;
using TMPro;

public class UI_Control : MonoBehaviour
{
    private bool keydown_T = false;
    private bool keydown_U = false;
    private float key_timer_T = 0.0f;
    private float key_timer_U = 0.0f;
    public float wait_time = 0.1f;
    public GameObject tradeMenu;
    public GameObject upgradeMenu;
    public GameObject prompt;
    public Animator shipAnimatior;
    private bool docked = false;

    void FixedUpdate()
    {
        if (keydown_U && key_timer_U > wait_time) {
            if (tradeMenu.activeSelf)
            {
                // trade menu is active
                // shutdown tradeMenu and activate upgradeMenu
                ToggleMenus(false, true);
                ResetUpgradeTimer();
            }
            else if (upgradeMenu.activeSelf)
            {
                // upgrade menu is active
                // shutdown upgradeMenu and activate prompt
                ToggleMenus(false, false);
                ResetUpgradeTimer();
            }
            else
            {
                // no menu is active
                // activate upgradeMenu, shutdown prompt
                ToggleMenus(false, true);
                ResetUpgradeTimer();
            }
        }
        else if (keydown_T && key_timer_T > wait_time) {
            if (tradeMenu.activeSelf)
            {
                // trade menu is active
                // shutdown tradeMenu and activate prompt
                ToggleMenus(false, false);
                ResetTradeTimer();
            }
            else if (upgradeMenu.activeSelf)
            {
                // upgrade menu is active
                // shutdown upgradeMenu and activate tradeMenu
                ToggleMenus(true, false);
                ResetTradeTimer();
            }
            else
            {
                // no menu is active
                // activate tradeMenu, shutdown prompt
                ToggleMenus(true, false);
                ResetTradeTimer();
            }
        }
    }

    void ToggleMenus(bool trade, bool upgrade)
    {
        // only activate trade menu if it is up and upgrade is down
        tradeMenu.SetActive(trade && !upgrade);
        // only activate upgrade menu if it is up and trade is down
        upgradeMenu.SetActive(upgrade && !trade);
        // activate prompt if both menus are up or both are down
        prompt.SetActive(!(trade || upgrade) || (trade && upgrade));
    }

    void ResetTradeTimer()
    {
        keydown_T = false;
        key_timer_T = 0.0f;
    }

    void ResetUpgradeTimer()
    {
        keydown_U = false;
        key_timer_U = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        key_timer_T += Time.deltaTime;
        key_timer_U += Time.deltaTime;
        if (Input.GetKeyDown("y") && key_timer_T > wait_time)
        {
            key_timer_T = 0.0f;
            shipAnimatior.SetBool("isDocked", !docked);
        }
        if (Input.GetKeyDown("t"))
        {
            keydown_T = true;
        }
        if (Input.GetKeyDown("u"))
        {
            keydown_U = true;
        }
    }
}
