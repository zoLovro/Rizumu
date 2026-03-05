using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BetterRyn.Gameplay;

public class NoteManager
{
    private Texture2D _tapTexture;
    private readonly List<Note> _activeNotes = new List<Note>();
    private readonly Queue<NoteData> _noteQueue = new Queue<NoteData>();
    private float _scrollSpeed = 2f;
    private readonly int _hitLine = 100;
    private readonly int _spawnWindow = 4000;
    private readonly int _despawnTime = 1500;
    private readonly float[] _laneX = { 578.5f, 770.5f, 962.5f, 1154.5f };
    
    private const float PerfectWindow = 100f;
    private const float GoodWindow = 150f;
    private const float MissWindow = 200f;
    private int _hitNotes = 0;
    private int _hitGoodNotes = 0;
    private int _missedNotes = 0;
    private int _combo = 0;
    private int _highestCombo;
    private int _score = 0;
    private double _accuracy = 100f;
    private readonly int _noteTextureWidth = 192;
    
    public double Accuracy => _accuracy;
    public int Score => _score;
    public int NoteTextureWidth => _noteTextureWidth;
    public int HitLine => _hitLine;
    public int Combo => _combo;
    public int HighestCombo => _highestCombo;
    public List<Note> ActiveNotes => _activeNotes;
    public Queue<NoteData> NoteQueue => _noteQueue;
    public int HitNotes => _hitNotes;
    public int HitGoodNotes => _hitGoodNotes;
    public int MissedNotes => _missedNotes;

    public event Action OnMiss;
    public event Action OnHit;
    public event Action OnGoodHit;
    

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
                        float endTime = float.Parse(parts[5].Split(':')[0]);
                        _noteQueue.Enqueue(new NoteData { Time = time, Lane = lane, Duration = endTime - time, Type = NoteType.Hold});
                        break;
                }
            }
        }
    }

    public void SpawnNotes(float songTime)
    {
        while (_noteQueue.Count > 0 && _noteQueue.Peek().Time - songTime <= _spawnWindow)
        {
            NoteData nextNote = _noteQueue.Dequeue();
            float xPos = _laneX[nextNote.Lane];
            switch (nextNote.Type)
            {
                case NoteType.Tap:
                    _activeNotes.Add(new TapNote(nextNote.Time, nextNote.Lane, _scrollSpeed, _tapTexture, xPos));
                    break;
                case NoteType.Hold:
                    _activeNotes.Add(new HoldNote(nextNote.Time, nextNote.Duration, nextNote.Lane, _scrollSpeed, _tapTexture, xPos));
                    break;
            }
        }
    }

    public void UpdateActiveNotes(float songTime, GameTime gameTime)
    {
        for (int i = _activeNotes.Count - 1; i >= 0; i--)
        {
            Note note = _activeNotes[i];
            note.Update(gameTime, songTime, _hitLine, _scrollSpeed);
            
            if (note is HoldNote hold && hold.IsBeingHeld)
            {
                if (songTime >= hold.EndTime)
                {
                    hold.Release();
                    hold.Complete();
                    _activeNotes.RemoveAt(i);
                    continue;
                }
            }
            
            if (!note.IsHit && songTime >= note.EndTime + _despawnTime)
            {
                _missedNotes++;
                OnMiss?.Invoke();
                _highestCombo = _combo;
                _combo = 0;
                _activeNotes.RemoveAt(i);
            }
        }

        UpdateAccuracy();
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
        Note best = null;
        float bestAbs = float.MaxValue;

        foreach (var note in _activeNotes)
        {
            if (note.IsHit) continue;
            if (note.Lane != laneIndex) continue;

            float dt = songTime - note.HitTime; 
            float abs = Math.Abs(dt);
            
            if (abs > MissWindow) continue;

            if (abs < bestAbs)
            {
                bestAbs = abs;
                best = note;
            }
        }

        if (best == null) return;

        if (bestAbs <= PerfectWindow)
        {
            OnHit?.Invoke();
            if (best is TapNote)
            {
                Console.WriteLine("PERFECT");
                best.IsHit = true;
                _hitNotes++;
                _combo++;
                _score += 300 * _combo;
                _activeNotes.Remove(best);
            }
            else if (best is HoldNote holdNote)
            {
                holdNote.StartHold();
                _hitNotes++;
                _combo++;
                _score += 300 * _combo;
            }
        }
        else if (bestAbs <= GoodWindow)
        {
            OnGoodHit?.Invoke();
            if (best is TapNote)
            {
                Console.WriteLine("GOOD");
                best.IsHit = true;
                _hitGoodNotes++;
                _combo++;
                _score += 100 * _combo;
                _activeNotes.Remove(best);
            }
            else if (best is HoldNote holdNote)
            {
                holdNote.StartHold();
                _hitGoodNotes++;
                _combo++;
                _score += 100 * _combo;
            }
        }
        else 
        {
            OnMiss?.Invoke();
            best.IsHit = true;
            _missedNotes++;
            _highestCombo = _combo;
            _combo = 0;
            _activeNotes.Remove(best);
        }

        UpdateAccuracy();
    }
    
    // TODO: Fix the hold notes not registering being held properly
    public void CheckRelease(float songTime, int laneIndex)
    {
        for (int i = _activeNotes.Count - 1; i >= 0; i--)
        {
            if (_activeNotes[i] is HoldNote hold &&
                hold.Lane == laneIndex &&
                hold.IsBeingHeld)
            {
                if (songTime < hold.EndTime)
                {
                    _missedNotes++;
                    _highestCombo = _combo;
                    _combo = 0;

                    hold.Release();
                    _activeNotes.RemoveAt(i);
                }
                else
                {
                    hold.Release();
                    hold.Complete();
                    _activeNotes.RemoveAt(i);
                }

                break;
            }
        }

        UpdateAccuracy();
    }


    private void UpdateAccuracy()
    {
        int allNotes = _hitNotes + _hitGoodNotes + _missedNotes;
        if (allNotes == 0) { _accuracy = 1.0; return; } // 100%
        _accuracy = (300.0 * _hitNotes + 100.0 * _hitGoodNotes) / (300.0 * allNotes);
    }
}