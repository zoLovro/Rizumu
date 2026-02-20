using System;
using System.Diagnostics;
using System.IO;

namespace BetterRyn.Diagnostics;

// NEEDS ffmpeg.exe in Diagnostics to work!!!!!
public static class ConvertAudio
{
    public static void ConvertAudioToWav(string inputSongPath, string outputWavPath)
    {
        Process process = new Process();
        
        process.StartInfo.FileName = Path.Combine(AppContext.BaseDirectory, "Diagnostics", "ffmpeg.exe");
    
        // overwrite (-y), input file (-i), output file
        process.StartInfo.Arguments = $"-y -i \"{inputSongPath}\" \"{outputWavPath}\""; 
        
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        
        process.Start();
        process.WaitForExit(); 
    }
}