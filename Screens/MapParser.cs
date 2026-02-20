
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
            string audioFile = Directory.GetFiles(folder, "audio.wav").FirstOrDefault();
            if (audioFile == null)
            {
                string alternativeAudioFile = Directory.GetFiles(folder)
                    .FirstOrDefault(f =>
                        f.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)
                        || f.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase));

                if (alternativeAudioFile != null)
                {
                    string outputPath = Path.Combine(folder, "audio.wav");
                    ConvertAudio.ConvertAudioToWav(alternativeAudioFile, outputPath);
                    audioFile = outputPath;
                }
            }
            string backgroundFile = Directory.GetFiles(folder).FirstOrDefault(f =>
                f.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                || f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase));

            if (mapFile != null && audioFile != null && backgroundFile != null)
            {
                string[] otherMetadata = GetMetadata(mapFile);
                mapList.Add(new MapMetadata
                {
                    Artist = otherMetadata[1],
                    SongTitle = otherMetadata[0],
                    MapPath = mapFile,
                    AudioPath = audioFile,
                    BackgroundPath = backgroundFile,
                    Difficulty = otherMetadata[2],
                    BPM = 1
                });
            }
        }
        return mapList;
    }

    static string[] GetMetadata(string filepath)
    {
        string[] metadata = new string[3];
        if (File.Exists(filepath))
        {
            string[] lines = File.ReadAllLines(filepath);
            string[] temp;
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
                }
            }
        }

        return metadata;
    }
}