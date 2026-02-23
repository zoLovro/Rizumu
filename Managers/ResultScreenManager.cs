using BetterRyn.Gameplay;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BetterRyn.Managers;

public class ResultScreenManager
{
    private ContentManager _content;
    private Texture2D _rankXplus;
    private Texture2D _rankX;
    private Texture2D _rankSplus;
    private Texture2D _rankS;
    private Texture2D _rankA;
    private Texture2D _rankB;
    private Texture2D _rankC;
    private Texture2D _rankD;

    public ResultScreenManager() {}

    public void LoadContent(ContentManager content)
    {
        _content = content;
        _rankXplus = _content.Load<Texture2D>("ranking-xh@2x");
        _rankX = _content.Load<Texture2D>("ranking-x@2x");
        _rankSplus = _content.Load<Texture2D>("ranking-sh@2x");
        _rankS = _content.Load<Texture2D>("ranking-S@2x");
        _rankA = _content.Load<Texture2D>("ranking-A@2x");
        _rankB = _content.Load<Texture2D>("ranking-B@2x");
        _rankC = _content.Load<Texture2D>("ranking-C@2x");
        _rankD = _content.Load<Texture2D>("ranking-D@2x");
    }


    public Texture2D WhatRankingIsIt(NoteManager noteManager)
    {
         /*
         SS,100% Accuracy (Only MAX and 300s)
         S,Over 95% Accuracy
         A,Over 90% Accuracy
         B,Over 80% Accuracy
         C,Over 70% Accuracy
         D,Anything below 70%
         */
         var accuracy = noteManager.Accuracy * 100;
         Texture2D selectedRank;
         
         if (accuracy >= 100.00) selectedRank = _rankX;
         else if (accuracy >= 95.00) selectedRank = _rankS;
         else if (accuracy >= 90.00) selectedRank = _rankA;
         else if (accuracy >= 80.00) selectedRank = _rankB;
         else if (accuracy >= 70.00) selectedRank = _rankC;
         else selectedRank = _rankD;
         
         return selectedRank ?? _rankD;
    }
}