using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BetterRyn.Gameplay;

public class NoteManager
{
    private Texture2D _tapTexture;
    private List<Note> _activeNotes = new List<Note>();
    public Queue<NoteData> noteQueue = new Queue<NoteData>();
    public float ScrollSpeed = 100f;
    private readonly int _hitLine = 50;
    private readonly int _spawnWindow = 1000;
    private readonly int _despawnTime = 100;
    private readonly float[] _laneX = { 100f, 400f, 700f, 1000f };
    
    private const float PerfectWindow = 50f;
    private const float GoodWindow = 100f;
    private const float MissWindow = 150f;
    private int _hitNotes = 0;
    private int _hitGoodNotes = 0;
    private int _missedNotes = 0;
    private int _combo = 0;
    private int _highestCombo;
    private int _score;
    private float _accuracy;

    public void LoadContent(ContentManager content)
    {
        _tapTexture = content.Load<Texture2D>("mania-note1");
    }

    public void LoadMap(string filepath)
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
                        noteQueue.Enqueue(new NoteData { Time = time, Lane = lane, Type = NoteType.Tap});
                        break;
                    case "128": 
                        float endTime = float.Parse(parts[5].Split(':')[0]);
                        noteQueue.Enqueue(new NoteData { Time = time, Lane = lane, Duration = endTime - time, Type = NoteType.Hold});
                        break;
                }
            }
        }
    }

    public void SpawnNotes(float songTime)
    {
        while (noteQueue.Count > 0 && noteQueue.Peek().Time - songTime <= _spawnWindow)
        {
            NoteData nextNote = noteQueue.Dequeue();
            float xPos = _laneX[nextNote.Lane];
            switch (nextNote.Type)
            {
                case NoteType.Tap:
                    _activeNotes.Add(new TapNote(nextNote.Time, nextNote.Lane, ScrollSpeed, _tapTexture, xPos));
                    break;
                case NoteType.Hold:
                    _activeNotes.Add(new HoldNote(nextNote.Time, nextNote.Duration, nextNote.Lane, ScrollSpeed, _tapTexture, xPos));
                    break;
            }
        }
    }

    public void UpdateActiveNotes(float songTime, GameTime gameTime)
    {
        for (int i = _activeNotes.Count - 1; i >= 0; i--)
        {
            Note note = _activeNotes[i];
            note.Update(gameTime, songTime, _hitLine);
            
            if (songTime > note.HitTime + _despawnTime)
            {
                _activeNotes.RemoveAt(i);
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (Note note in _activeNotes)
        {
            note.Draw(spriteBatch);
        }
    }

    public void CheckHit(float songTime, int laneIndex)
    {
        Note closestNote = null;
        bool first = true;
        
        foreach (Note note in _activeNotes)
        {
            if (first && !note.IsHit)
            {
                closestNote = note;
                first = false;
                continue;
            }
            if (note.HitTime < closestNote.HitTime && !note.IsHit)
            {
                closestNote = note;
            }
        }

        float timeDifference = Math.Abs(closestNote.HitTime - songTime);
        if (timeDifference <= PerfectWindow)
        {
            closestNote.IsHit = true;
            _hitNotes++;
            _combo++;
            _score += 300 * _combo;
            _activeNotes.Remove(closestNote);
        }
        else if (timeDifference <= GoodWindow)
        {
            closestNote.IsHit = true;
            _hitGoodNotes++;
            _combo++;
            _score += 100 * _combo;
            _activeNotes.Remove(closestNote);
        }
        else if (timeDifference <= MissWindow)
        {
            closestNote.IsHit = true;
            _missedNotes++;
            _highestCombo = _combo;
            _combo = 0;
            _activeNotes.Remove(closestNote);
        }
        
        
        UpdateAccuracy();
    }

    private void UpdateAccuracy()
    {
        int allNotes = _hitNotes + _hitGoodNotes + _missedNotes;
        
    }
}