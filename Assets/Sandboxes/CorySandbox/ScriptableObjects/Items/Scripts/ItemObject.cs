using UnityEngine;

public enum ItemType
{
    ArmorUpgrade,
    ShieldingUpgrade,
    CargoUpgrade,
    Metals,
    Ions,
    SemiConductors,
    HeavyMetals,
    Fuel,
    SolarSails,
    Conductors,
    EnergyCells,
    IonSource,
    RadiationShielding,
    ChemicalEngine,
    FuelRods,
    Processors,
    LaserArray,
    IonEngine,
    ComputerCores,
    PhaseInverter,
    FissionReactor,
    TachyonSource,
    WarpStabilizer,
    WarpDrive,
    SubspaceNAVComputer,
    NuclearThermalEngine,
    MassConverter,
    HyperspaceComputer,
    SubspaceDrive,
    WarpEngine,
    AntiGravGenerator,
    HyperDrive,
    WormholeStabilizer,
    FusionReactor,
    SubspaceEngine,
    AntimatterContainmentUnit,
    WormholeDrive,
    HyperspaceEngine,
    AntimatterReactor,
    QuantumProjector,
    WormholeEngine,
    TeleportationEngine,
    Default
}

public abstract class ItemObject : ScriptableObject
{
    public GameObject prefab;
    public ItemType type;
    public float item_value = 0.0f;
    public string item_name;
    [TextArea(15,20)]
    public string description;
}
