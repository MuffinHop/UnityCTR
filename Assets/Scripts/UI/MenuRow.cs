namespace OGData.UI
{
    public class MenuRow 
    {
        // can have values above 0xFF,
        // such as 0x155 for "Controller 1C",
        // sometimes the top bit 0x8000 is used,
        // like VS 2P,3P,4P in main menu, to
        // determine if the row is "locked"

        public ushort StringIndex;
        public byte RowOnPressUp;
        public byte RowOnPressDown;
        public byte RowOnPressLeft;
        public byte RowOnPressRight;
    }
}