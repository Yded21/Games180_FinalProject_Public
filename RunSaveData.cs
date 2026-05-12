using System.Collections.Generic;

namespace Games180_FinalProject_EddyZhang;

public class RunSaveData
{
    public int CurrentHp { get; set; }
    public int MaxHp { get; set; }
    public int BaseDamage { get; set; }
    public PlayerForm CurrentForm { get; set; }
    public bool HasForm { get; set; }
    public int RoomIndex { get; set; }
    public int Seed { get; set; }
    public List<string> AcquiredUpgrades { get; set; }
    public List<string> RoomMapBases { get; set; }

    public RunSaveData()
    {
        AcquiredUpgrades = new List<string>();
        RoomMapBases = new List<string>();
    }
}
