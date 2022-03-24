namespace OGData.Audio
{
    public class XNF
    {
        private const int CDSYS_XA_TYPE_MUSIC = 4;
        private const int CDSYS_XA_TYPE_EXTRA = 4;
        private const int CDSYS_XA_TYPE_GAME = 4;
        private const int CDSYS_XA_NUM_TYPES = 4;
        public int Magic;
        public int Version;
        public int NumTypes;
        public int NumXAsTotal;
        public int NumAudioTracksTotal;
        public int[] numXA = new int[CDSYS_XA_NUM_TYPES];
        public int[] firstXaIndex = new int[CDSYS_XA_NUM_TYPES];
        public int[] numSongs = new int[CDSYS_XA_NUM_TYPES];

        public int[] firstSongIndex = new int[CDSYS_XA_NUM_TYPES];

        // total number of XA files
        public int[] XaCdPos = new int[1];
    }
}