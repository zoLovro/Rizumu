using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BetterRyn.Gameplay;

public class NoteManager
{
    private Texture2D _tapTexture;
    private List<Note> _activeNotes = new List<Note>();
    private List<Note> _queue = new List<Note>();

    public void LoadContent(ContentManager content)
    {
        _tapTexture = content.Load<Texture2D>("mania-note1");
    }

    private void LoadMap(string filepath)
    {
        string[] lines = File.ReadAllLines(filepath);
        Beatmap map = new Beatmap();
        bool fillingNotes = false;

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//")) continue;

            // Filling metadata n stuff
            if (fillingNotes == false)
            { var parts = line.Split(":");

            if (parts[0] == "AudioFileName") map.AudioFileName = parts[1].Trim();
            if (parts[0] == "PreviewTime") map.PreviewTime = float.Parse(parts[1].Trim());
            if (parts[0] == "Title") map.Title = parts[1].Trim();
            if (parts[0] == "Artist") map.Artist = parts[1].Trim();
            }

        if (line.Trim() == "[HitObjects]") fillingNotes = true;
        }
    }
}