using System;
using System.Diagnostics;
using System.IO;

namespace BetterRyn.Diagnostics;

// NEEDS ffmpeg.exe in Diagnostics to work!!!!!
public static class ConvertAudio
{
    public static void ConvertAudioToWav(string inputSongPath, string outputWavPath)
    {
        var process = new Process();
        process.StartInfo.FileName = OperatingSystem.IsWindows()
            ? Path.Combine(AppContext.BaseDirectory, "Diagnostics", "ffmpeg.exe")
            : "ffmpeg";

        process.StartInfo.Arguments =
            $"-y -i \"{inputSongPath}\" -vn -ac 2 -ar 44100 -c:a pcm_s16le \"{outputWavPath}\"";

        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        process.Start();
        process.WaitForExit();
    }
}