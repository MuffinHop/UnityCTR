namespace OGData.UI
{
    public class MainMenuLevelRow
    {
        // 0 - dingo canyon
        // 3 - crash cove
        // etc
        public short LevID;

        // texture that shows before video plays
        public short VideoThumbnail;

        // which black+white map draws
        public short MapTextureID;

        // 0xFFFF for unlock by default
        // otherwise has a flag for what is needed,
        // 0xFFFE means "only show in 1P mode" (oxide station)
        public short Unlock;

        // changes which video of level plays
        public int VideoID;

        // how long video plays before looping
        public int VideoLength;
    }
}