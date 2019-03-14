using System;

namespace YDock
{
    [Flags]
    internal enum DragManagerFlags
    {
        None = 0x0,
        Left = 0x1,
        Top = Left << 1,
        Right = Top << 1,
        Bottom = Right << 1,
        Center = Bottom << 1,
        Tab = Center << 1,
        Head = Tab << 1,
        Split = Head << 1,
        Active = Split << 1
    }
}