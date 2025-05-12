using System.Numerics;
using NUnit.Framework;
using UnityEngine;

public enum ItemType
{
    SolarSails,
    ChemicalEngine,
    IonEngine,
    NuclearThermalEngine,
    WarpEngine,
    WormholeEngine,
    HullBrace,
    HullExtenderM1,
    OuterStabilizers,
    EngineArmSmall,
    HullExtenderM2,
    InnerStabilizers,
    EngineArmLarge,
    ArmorModule,
    ShieldingModule,
    CargoModule,
    Metals,
    HeavyMetals,
    Ions,
    SemiConductors,
    Fuel,
    Conductors,
    EnergyCells,
    IonSource,
    RadiationShielding,
    FuelRods,
    Processors,
    LaserArray,
    ComputerCores,
    PhaseInverter,
    FissionReactor,
    TachyonSource,
    WarpStabilizer,
    WarpDrive,
    MassConverter,
    AntiGravGenerator,
    WormholeStabilizer,
    FusionReactor,
    WormholeDrive
}

public abstract class ItemObject : ScriptableObject
{
    public Sprite icon;
    public ItemType type;
    public float item_value = 100.0f;
    public int item_size = 1;
    public string item_name;
    [TextArea(15,20)]
    public string description;
}
