using UnityEngine;


public class DropdownController : MonoBehaviour
{
    public DropdownSelector type;
    private UpgradeManager upgradeManager;
    [SerializeField] private string global_scripts = "GlobalScripts";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // lookup upgrade manager
        upgradeManager = GameObject.Find(global_scripts).GetComponent<UpgradeManager>();
    }

    public bool GetInteractableState (int index)
    {
        return upgradeManager.GetInteractableState(index, type);
    }
}
