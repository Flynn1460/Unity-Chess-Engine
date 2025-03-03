using System.Collections.Generic;

public class BoardManager
{
    // Classes
    public Board board = new Board();
    public NewMoveGenerator move_generator = new NewMoveGenerator();

    public SquareHighlight boardHighlighter = new SquareHighlight();

    public BoardManager() {      boardHighlighter.Reset_Tiles(remove_prev_mv:true);      }

    public void Highlight_Piece_Moves(Square piece_square) {
        if (board.turn_id == 0) {
            List<Move> piece_legal_moves = move_generator.GenerateLegalMoves(board, piece_square);

            boardHighlighter.Highlight_Tiles(piece_legal_moves);
        }
    }

    public void Push(Move piece_move){
        boardHighlighter.Reset_Tiles(remove_prev_mv:true);
        boardHighlighter.Highlight_Previous_Move(piece_move);
        board.advanced_move(piece_move);
    } 
}
