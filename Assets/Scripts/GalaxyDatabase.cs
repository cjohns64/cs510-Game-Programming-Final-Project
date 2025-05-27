using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Galaxy/Galaxy Database")]
public class GalaxyDatabase : ScriptableObject
{
    public List<SolarSystemData> allSystems;

    public SolarSystemData GetSystemByName(string name)
    {
        return allSystems.Find(s => s.systemName == name);
    }
}
