namespace BetterRyn.Screens;
using System.Collections.Generic;

public class MapsetGroup
{
    public string SongTitle { get; set; }
    public string Artist { get; set; }
    public string Folder { get; set; }
    public List<MapMetadata> Difficulties { get; set; } = new List<MapMetadata>();
    public bool IsExpanded { get; set; } = false;
}