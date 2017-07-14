namespace CollabEdit.DocumentModel
{
    public struct EditState
    {
        public Position Cursor;
        public Range Selection;
    }

    public struct Position
    {
        public static Position Default = new Position(0, 0);

        public Position(int row, int column)
        {
            Row = row;
            Column = column;
        }
        public int Row;
        public int Column;
    }

    public struct Range
    {
        public static Range Empty = new Range(Position.Default, Position.Default);
        public Range(Position start, Position end)
        {
            Start = start;
            End = end;
        }
        public Position Start;
        public Position End;
    }
}