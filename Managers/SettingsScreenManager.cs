using System;
using System.IO;

namespace BetterRyn.Managers;

public class SettingsScreenManager
{
    static string _appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private string _gameFolder = Path.Combine(_appDataPath, "BetterRyn");
    
    public string GameFolder => _gameFolder;
    
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
    public string[] Resolutions { get; } = new[] { "1280x720", "1360x768", "1600x900", "1920x1080" };
    public bool Fullscreen { get; set; } = false;
    public float Volume { get; set; } = 100f;
    public float Offset { get; set; } = 120f;
    
    
    public SettingsScreenManager()
    {
        
    }

    public void LoadContent()
    {
        
    }

    public string CreateSettingsFileIfExists()
    {
        if (!Directory.Exists(_gameFolder))
        {
            Directory.CreateDirectory(_gameFolder);
        }
    
        return Path.Combine(_gameFolder, "settings.txt");
    }
    
    public void ApplySettings(int mainIndex, int subIntex)
    {
        switch (mainIndex)
        {
            case 0: // KEYBINDS
                break;
            case 1: // OFFSET
                break;
            case 2: // RESOLUTION
                break;
            case 3: // FULLSCREEN
                break;
            case 4: // VOLUME
                break;
            case 5: // SAVE
                break;
            case 6: // DISCARD
                break;
        }
    }

    public void SaveSettings()
    {
        
    }
    

    public void ChangeKeybind(int index, string keybind)
    {
        Keybinds[index] = keybind;
    }
    
    public void ChangeOffset()
    {
        
    }

    public void ChamgeResolution()
    {
        
    }

    public void ToggleFullscreen()
    {
        Fullscreen = !Fullscreen;
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
        
    }

    public void Discard()
    {
        
    }
    
    
}