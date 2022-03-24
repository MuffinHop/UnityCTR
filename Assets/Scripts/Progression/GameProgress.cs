namespace OGData.Progression
{
    public class Unlocks
    {
        public uint CharactersTracks;
        public uint Scrapbook;
    };
    public class GameProgress
    {
        public uint Unknown;
        public Unlocks Unlocks;
        public HighScoreTrack[] HighScoreTracks = new HighScoreTrack[18];
    }
}
