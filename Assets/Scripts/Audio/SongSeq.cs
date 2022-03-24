namespace OGData.Audio
{
    public class SongSeq
    {
        // this struct is passed as
        // parameter for every cseq opcode,
        // same struct as SongPool->CseqSequences

        // 0x1 - soundID (from Sound_Play)

        // 0x3 - instruments index
        // 0x4 - reverb
        // 0x5 - volume of sequence

        // 0xb - songPoolIndex

        // 0x18 - pointer to "initData" in opcode5_noteon,
        // first byte of that "initData" is opcodeIndex

        public char[] Data = new char[0x1c];
    }
}