using System.Collections.Generic;

namespace BetterRyn.Gameplay;

public class Beatmap
{
    // Metadata
    public string Title { get; set; }
    public string Artist { get; set; }
    public string AudioFileName { get; set; }
    public float PreviewTime { get; set; } // At what time the song starts when previewing
    
    // Timing & difficulty 
    public float BPM { get; set; }
    public float Offset { get; set; } 
}