using System.Collections.Generic;

public class BoardManager
{
    // Classes
    public Board board = new Board();
    public MoveGenerator move_generator = new MoveGenerator();

    private CONTROLLER_SquareHighlight boardHighlighter = new CONTROLLER_SquareHighlight();


    public void Highlight_Piece_Moves(Square piece_square) {
        if (board.turn_id == 0) {
            List<Move> piece_legal_moves = move_generator.GenerateLegalMoves(board, piece_square);

            boardHighlighter.Highlight_Tiles(piece_legal_moves);
        }
    }

    public void Push(Move piece_move){
        boardHighlighter.Reset_Tiles();
        board.move(piece_move);
    } 
}
 