namespace Games180_FinalProject_EddyZhang;

public class RoomDefinition
{
    public string Id;
    public string Name;
    public RoomType Type;
    public string MapBase;
    public string[] MapPool;
    public int EncounterNumber;

    public RoomDefinition(string id, string name, RoomType type, string mapBase, int encounterNumber)
    {
        Id = id;
        Name = name;
        Type = type;
        MapBase = mapBase;
        MapPool = null;
        EncounterNumber = encounterNumber;
    }

    public RoomDefinition(string id, string name, RoomType type, string[] mapPool, int encounterNumber)
    {
        Id = id;
        Name = name;
        Type = type;
        MapBase = string.Empty;
        MapPool = mapPool;
        EncounterNumber = encounterNumber;
    }
}
