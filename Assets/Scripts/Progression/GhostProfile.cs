namespace OGData.Progression
{
    public class GhostProfile
    {
        public char[] ProfileName = new char[0x15];
        public char[] OskName = new char[0x11]; // On-Screen-Keyboard EnterYourName
        public short AlwaysOne;
        public short TrackID;
        public short CharacterID;
        public int MemcardProfileIndex;
        public int TrackTime;
    }
}