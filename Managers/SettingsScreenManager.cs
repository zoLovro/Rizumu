using System;
using System.Globalization;
using System.IO;
using System.Linq;
using BetterRyn.Screens;

namespace BetterRyn.Managers;

public class SettingsScreenManager
{
    static string _appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private string _gameFolder = Path.Combine(_appDataPath, "BetterRyn");
    
    public string GameFolder => _gameFolder;
    public int CurrentResolutionIndex { get; set; } = 3;
    
    public string[] TextFileLines { get; set; } = new[]
    {
        "keybinds=D, F, J, K",
        "offset=120",
        "resolution=1920x1080",
        "fullscreen=NO",
        "volume=100"
    };
    
    public string[] MenuItems { get; } =  new[]
        { "KEYBINDS", "OFFSET", "RESOLUTION", "FULLSCREEN", "VOLUME", "SAVE", "DISCARD" };

    public string[] Keybinds { get; set; } = new[] { "D", "F", "J", "K" };
    public static string[] Resolutions { get; } = new[] { "1280x720", "1360x768", "1600x900", "1920x1080", "32x32", "1x1" };
    public bool Fullscreen { get; set; } = false;
    public float Volume { get; set; } = 100f;
    public float Offset { get; set; } = 120f;
    
    
    
    
    public SettingsScreenManager()
    {
        Directory.CreateDirectory(_gameFolder);
    }

    public void LoadContent()
    {
        string path = Path.Combine(_gameFolder, "settings.txt");
        if (File.Exists(path))
            Discard();
    }

    public string CreateSettingsFileIfExists()
    {
        if (!Directory.Exists(_gameFolder))
        {
            Directory.CreateDirectory(_gameFolder);
        }
    
        return Path.Combine(_gameFolder, "settings.txt");
    }
    
    public void ApplySettings(int mainIndex, int subIndex)
    {
        switch (mainIndex)
        {
            case 0: // KEYBINDS
                break;
            case 1: // OFFSET
                ChangeOffset(subIndex);
                break;
            case 2: // RESOLUTION
                ChangeResolution(subIndex);
                break;
            case 3: // FULLSCREEN
                ToggleFullscreen(subIndex);
                break;
            case 4: // VOLUME
                ChangeVolume(subIndex);
                break;
            case 5: // SAVE
                Save();
                break;
            case 6: // DISCARD
                Discard();
                break;
        }
    }

    public void ChangeKeybind(int index, string keybind)
    {
        Keybinds[index] = keybind;
    }
    
    public void ChangeOffset(int num)
    {
        switch (num)
        {
            case 0:
                Offset--;
                break;
            case 1:
                Offset++;
                break;
        }
    }

    public void ChangeResolution(int index)
    {
        CurrentResolutionIndex = index;
    }

    public void ToggleFullscreen(int subIndex)
    {
        Fullscreen = subIndex == 1;
    }
    
    public void ChangeVolume(int num)
    {
        if (num == 1 && Volume < 100f)
        {
            Volume++;
        }

        if (num == 0 && Volume > 0f)
        {
            Volume--;
        }
    }

    public void Save()
    {
        if (!Directory.Exists(_gameFolder))
            Directory.CreateDirectory(_gameFolder);

        if (CurrentResolutionIndex < 0 || CurrentResolutionIndex >= Resolutions.Length)
            CurrentResolutionIndex = 3;

        string[] lines =
        {
            $"keybinds={string.Join(", ", Keybinds)}",
            $"offset={Offset.ToString(CultureInfo.InvariantCulture)}",
            $"resolution={Resolutions[CurrentResolutionIndex]}",
            $"fullscreen={(Fullscreen ? "YES" : "NO")}",
            $"volume={Volume.ToString(CultureInfo.InvariantCulture)}"
        };

        File.WriteAllLines(Path.Combine(_gameFolder, "settings.txt"), lines);

        string[] res = Resolutions[CurrentResolutionIndex].Split('x');
        RynGame.Instance.ApplyResolution(int.Parse(res[0]), int.Parse(res[1]));
    }

    public void Discard()
    {
        string path = Path.Combine(_gameFolder, "settings.txt");
        if (!File.Exists(path)) return;

        foreach (string line in File.ReadAllLines(path))
        {
            string[] parts = line.Split('=');
            if (parts.Length != 2) continue;

            switch (parts[0].Trim())
            {
                case "keybinds":
                    Keybinds = parts[1].Split(',')
                        .Select(k => k.Trim())
                        .ToArray();
                    break;
                case "offset":
                    // InvariantCulture ensures "120.5" parses correctly on any system locale
                    if (float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float offset))
                        Offset = offset;
                    break;
                case "resolution":
                    int resIndex = Array.IndexOf(Resolutions, parts[1].Trim());
                    if (resIndex >= 0) // ignore unknown resolutions instead of storing -1
                        CurrentResolutionIndex = resIndex;
                    break;
                case "fullscreen":
                    Fullscreen = parts[1].Trim() == "YES";
                    break;
                case "volume":
                    // InvariantCulture ensures "100.0" parses correctly on any system locale
                    if (float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float volume))
                        Volume = volume;
                    break;
            }
        }
    }
    
    
}