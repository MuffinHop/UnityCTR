namespace OGData.Progression
{
    public class HighScoreTrack
    {
        // Time Trial Best Lap
        // Time Trial Best Race (5)
        // Relic Race Best Lap -- unused
        // Relic Race Best Race (5)
        public HighScoreEntry[] ScoreEntry = new HighScoreEntry[12];

        // 1 - N Tropy Open
        // 2 - N Tropy Beaten, Oxide Open
        // 4 - Oxide Beaten
        public uint TimeTrialFlags;
    }
}