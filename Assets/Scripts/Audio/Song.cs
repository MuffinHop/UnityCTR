namespace OGData.Audio
{
    public class Song
    {
        // 0x0
        // & 1 = Playing
        // & 2 = Paused (can be &3 in menus)
        // & 4 = needs to stop
        public short Flags;

        // 0x2
        public short Id;

        // 0x4
        public int Unk4;

        // 0x8
        public short Unk8;

        // 0xA
        public short UnkA;

        // 0xC
        public int Tempo;

        // 0x10
        public int Unk10;

        // 0x14
        // time spent playing
        public int TimeSpentPlaying;

        // used for changing music volume
        // over the course of a second
        // 0x18 = vol_Curr
        // 0x19 = vol_Next
        // 0x1a = another volume?
        public char[] VolumeFadeSettings = new char[3];

        // 0x1b
        public char NumSequences;

        // 0x1c array of all cseq sequences in song
        public SongSeq[] CseqSequences = new SongSeq[0x18];
    }
}

