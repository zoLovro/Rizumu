using System;
using System.IO;

namespace BetterRyn.Managers;

public class SettingsScreenManager
{
    private string _appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private string _gameFolder;
    
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
        _gameFolder = Path.Combine(_appDataPath, "BetterRyn");
        if (!Directory.Exists(_gameFolder))
        {
            Directory.CreateDirectory(_gameFolder);
        }
    
        return Path.Combine(_gameFolder, "settings.txt");
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

    public void ChangeOffset()
    {
        
    }
    
}