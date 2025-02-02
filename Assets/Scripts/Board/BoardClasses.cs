
using System.Collections.Generic;

class Square
{
    public int col;
    public int row;
    public List<int> sq;

    public bool isWhite;
    public int piece;
    public int piece_type;

    public Square(Board board, int col, int row) {
        this.row = row;
        this.col = col;

        this.sq = new List<int>() {col, row};
        
        this.piece = board.b[row, col];
        this.piece_type = piece % 7;

        this.isWhite = (piece == piece_type);
    }

    public override string ToString() {
        return "("+row+","+col+") "+piece_type+ ", "+isWhite;
    }
}



class Move
{
    public Square start_square;
    public Square end_square;

    public Move(Board board, int col1, int row1, int col2, int row2) {
        
        start_square = new Square( board, col1, row1 );
        end_square = new Square( board, col2, row2 );
    }

    public override string ToString()
    {
        return (char)('a' + start_square.col) + (start_square.row+1).ToString() + (char)('a' + end_square.col) + (end_square.row+1).ToString();
    }
}