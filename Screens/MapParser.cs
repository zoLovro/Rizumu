
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BetterRyn.Diagnostics;

namespace BetterRyn.Screens;

public static class MapParser
{
    public static List<MapMetadata> LoadAllMaps(string mapsDir)
    {
        var mapList = new List<MapMetadata>();

        foreach (var folder in Directory.GetDirectories(mapsDir))
        {
            string mapFile = Directory.GetFiles(folder, "*.osu").FirstOrDefault();
            if (mapFile == null) continue;
            
            string[] meta = GetMetadata(mapFile);
            string title = meta[0];
            string artist = meta[1];
            string version = meta[2];
            string audioName = meta[3];
            
            string backgroundFile = Directory.GetFiles(folder).FirstOrDefault(f =>
                f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase));
            
            string audioFile = Path.Combine(folder, "audio.wav");
            if (!File.Exists(audioFile))
            {
                string sourceAudio = null;

                if (!string.IsNullOrWhiteSpace(audioName))
                {
                    string wanted = Path.Combine(folder, audioName);
                    if (File.Exists(wanted))
                        sourceAudio = wanted;
                }
                
                if (sourceAudio == null)
                {
                    sourceAudio = Directory.GetFiles(folder).FirstOrDefault(f =>
                        f.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) ||
                        f.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase) ||
                        f.EndsWith(".wav", StringComparison.OrdinalIgnoreCase));
                }

                if (sourceAudio != null)
                    ConvertAudio.ConvertAudioToWav(sourceAudio, audioFile);
            }

            if (!File.Exists(audioFile)) continue;
            if (backgroundFile == null) continue;

            mapList.Add(new MapMetadata
            {
                Artist = artist,
                SongTitle = title,
                MapPath = mapFile,
                AudioPath = audioFile,
                BackgroundPath = backgroundFile,
                Difficulty = version,
                BPM = 1
            });
        }
        return mapList;
    }

    static string[] GetMetadata(string filepath)
    {
        string[] metadata = new string[4];
        if (File.Exists(filepath))
        {
            string[] lines = File.ReadAllLines(filepath);
            foreach (var line in lines)
            {
                var parts = line.Split(':', 2);
                if (parts.Length < 2) continue;

                string key = parts[0].Trim();
                string value = parts[1].Trim();
                switch (key)
                {
                    case "Title":
                        metadata[0] = value;
                        break;
                    case "Artist":
                        metadata[1] = value;
                        break;
                    case "Version":
                        metadata[2] = value;
                        break;
                    case "AudioFilename":
                        metadata[3] = value;
                        break;
                }
            }
        }

        return metadata;
    }
}