namespace OGData.Progression
{
    public class AdvProgress
    {
        public uint[] rewards = new uint[6];
        public string Name; // char array 18 long
        public ushort CharacterID;
        public ushort Unk;

        public short HubLevYouSavedOn;

        // Count up to 10 times player lost
        // Including Crystal Challenge
        public char[] TimesLostRacePerLev = new char[0x12];
        public char[] TimesLostCupRace = new char[5];
        public char[] TimesLostBossRace = new char[5];
        public int Unk_8FBF0;
    }
}