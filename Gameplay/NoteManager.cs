using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    private Queue<NoteData> _noteQueue = new Queue<NoteData>();
    public float ScrollSpeed = 800f;

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
            if (!fillingNotes)
            { var parts = line.Split(":");

            if (parts[0].Trim() == "AudioFileName") map.AudioFileName = parts[1].Trim();
            if (parts[0].Trim() == "PreviewTime") map.PreviewTime = float.Parse(parts[1].Trim());
            if (parts[0].Trim() == "Title") map.Title = parts[1].Trim();
            if (parts[0].Trim() == "Artist") map.Artist = parts[1].Trim();
             // if (parts[0].Trim() == "")
            }

            if (line.Trim() == "[HitObjects]")
            {
                fillingNotes = true;
                continue;
            }

            if (fillingNotes)
            {
                var parts = line.Split(",");
                int lane = int.Parse(parts[0]);
                switch(lane)
                {
                    case 64:
                        lane = 0;
                        break;
                    case 192:
                        lane = 1;
                        break;
                    case 320:
                        lane = 2;
                        break;
                    case 448:
                        lane = 3;
                        break;
                }
                var time = float.Parse(parts[2]);

                switch (parts[3].Trim())
                {
                    case "1":
                        _noteQueue.Enqueue(new NoteData { Time = time, Lane = lane, Type = NoteType.Tap});
                        break;
                    case "128":
                        _noteQueue.Enqueue(new NoteData { Time = time, Lane = lane, Duration = float.Parse(parts[5]) - time, Type = NoteType.Hold});
                        break;
                }
            }
        }
    }
}