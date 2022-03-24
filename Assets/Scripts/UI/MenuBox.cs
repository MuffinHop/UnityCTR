using System.Collections.Generic;
using UnityEngine;

namespace OGData.UI
{
    public enum BoxState
    {
        CenterY = 1,
        CenterX = 2,
        DrawOnlyTitle = 4,
        DrawNextMenuBoxInHierarchy = 0x10,
        TinyTextInRows = 0x80,
        InvisibleMenuBox = 0x1000,
        BigTextInTitle = 0x4000,
        MuteSoundOfCursorMoving = 0x800000
    } 
    public class MenuBox
    {
        public ushort Index1; // string index of title (null, with no row)

        // position for current frame
        public ushort PosXcurr; // X position
        public ushort PosYcurr; // Y position

        public ushort Unk1;

        // This is an int, see FUN_800469dc
        // & 1, centers Y
        // & 2, centers X
        // & 4, draw only title bar
        // & 0x10, draw ptrNextMenuBox_InHierarchy
        // & 0x80, tiny text in rows
        // & 0x1000, invisible MenuBox
        // & 0x4000, big text in title
        // & 0x800000, mute sound of moving cursor
        public BoxState State;

        public List<MenuRow> Rows;

        public Event ExecuteOnSelect; // Do this event when selected

        // text color, box color, etc
        // one-byte variable with
        // two-byte alignment
        public short DrawStyle;

        // position for previous frame
        public short PosXprev;
        public short PosYprev;

        public char RowSelected;
        public char Unk3;

        public char Unk4;
        public char Unk5;

        // not hierarchy level,
        // used several times in code
        public char Unk6;
        public char Unk7;

        public short Width;
        public short Height;

        public MenuBox NextMenuBox_InHierarchy;
        public MenuBox PrevMenuBox_InHierarchy;
    }
}