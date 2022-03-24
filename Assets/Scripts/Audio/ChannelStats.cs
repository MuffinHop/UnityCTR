namespace OGData.Audio
{
    public class ChannelStats
    {
        public ChannelStats Next;
        public ChannelStats Prev;
        public char Flags;

        public char ChannelID;

        // ??? set in "noteon"
        public char Unk1;

        // Type (0=engineFX,1=otherFX,2=music)
        public char Type;

        // ??? set in "noteon"
        public char Unk2;
        public char ShortSampleIndex;
        public char Echo;
        public char Vol;
        public char Distort;
        public char LeftRight;
        public char[] Unk6 = new char[0x4];
        public short TimeLeft;

        // bitshifted top 2 bytes are "CountSounds"
        public int SoundID;
        public int StartFrame;
    }
}