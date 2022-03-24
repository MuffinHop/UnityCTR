namespace OGData.Progression
{
    public struct RacingWheelData // this is never used. OG has a racing wheel support that was removed.
    {
        public short ControllerCenter;
        public short DeadZone;
        public short Range;
    };

    public class GameOptions
    {
        public short VolFx;
        public short VolMusic;
        public short VolVoice;
        public RacingWheelData[] Rwd = new RacingWheelData[4];
        // backup of gameMode flag
        public int GameMode;
    }
}