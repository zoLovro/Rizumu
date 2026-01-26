namespace BetterRyn.Gameplay;

public struct NoteData
{
    public float Time;
    
    public int Lane;
    
    public NoteType Type;
    
    // Only for hold notes
    public float Duration;

    public float ScrollSpeed;
}

public enum NoteType
{
    Tap,
    Hold
}