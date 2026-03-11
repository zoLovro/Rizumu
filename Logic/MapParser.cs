
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BetterRyn.Diagnostics;

namespace BetterRyn.Logic;

public static class MapParser
{
    public static List<MapMetadata> LoadAllMaps(string mapsDir)
    {
        if (!Directory.Exists(mapsDir))
            Directory.CreateDirectory(mapsDir);
        var mapList = new List<MapMetadata>();

        foreach (var folder in Directory.GetDirectories(mapsDir))
        {
            string[] allMapFiles = Directory.GetFiles(folder, "*.osu");

            foreach (var mapFile in allMapFiles)
            {
                if (mapFile == null) continue;
                
                Console.WriteLine(folder);
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
                    DifficultyName = version,
                    DifficultyRating = CalculateDifficulty(mapFile),
                    BPM = CalculateBPM(mapFile),
                    Folder = folder
                });
            }
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

    static float CalculateDifficulty(string filepath)
    {
        if (!File.Exists(filepath)) return 0f;

        string[] lines = File.ReadAllLines(filepath);
        bool hitObjectsStarted = false;
        int noteCount = 0;
        float firstNoteTime = -1f;
        float lastNoteTime = 0f;

        foreach (var line in lines)
        {
            if (line.StartsWith("[HitObjects]"))
            {
                hitObjectsStarted = true;
                continue;
            }

            if (hitObjectsStarted && !string.IsNullOrWhiteSpace(line))
            {
                string[] data = line.Split(',');
                if (data.Length > 2 && float.TryParse(data[2], out float timeInMs))
                {
                    noteCount++;
                    lastNoteTime = timeInMs;
                    if (firstNoteTime == -1f) firstNoteTime = timeInMs;
                }
            }
        }
        if (noteCount == 0 || firstNoteTime == lastNoteTime) return 0f;
        float playableDurationSeconds = (lastNoteTime - firstNoteTime) / 1000f;
        float nps = noteCount / playableDurationSeconds;
        return (float)Math.Round(nps, 2);
    }

    static float CalculateBPM(string filepath)
    {
        if (!File.Exists(filepath)) return 0;
        bool inTimingPoints = false;
        
        string[] lines = File.ReadAllLines(filepath);
        foreach (var line in lines)
        {
            var temp = line.Split(":");
            if (temp[0].Equals("[TimingPoints]") && inTimingPoints == false)
            {
                inTimingPoints = true;
                continue;
            }
            if (temp[0].Equals("[HitObjects]")) break;
            if (inTimingPoints)
            {
                var parts = line.Split(",");
                if (parts.Length < 8 || parts[6] == "0") continue;
                else
                {
                    if (float.TryParse(parts[1], System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out float beatLength))
                    {
                        if (beatLength > 0)
                        {
                            float bpm = 60000f / beatLength;
                            return (float)Math.Round(bpm, 0);
                        }
                    }
                }
            }
        }
        return 0f;
    }
}