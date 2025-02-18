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

    public void Push(Move piece_move, int pawn_promote_piece=-1){
        (int old_row, int old_col) = piece_move.start_square.sq;
        (int new_row, int new_col) = piece_move.end_square.sq;
        
        // If replace piece is set to something make piece otherwise use what was at the position
        int piece = pawn_promote_piece == -1 ? board.b[old_row, old_col] : pawn_promote_piece;

        if (piece == 1 && new_row == 7) board.b[new_row, new_col] = 5; // WHITE QUEEN PROMO
        if (piece == 8 && new_row == 0) board.b[new_row, new_col] = 12; // BLACK QUEEN PROMO

        // If move is pushed then highlighting isn't needed
        boardHighlighter.Reset_Tiles();
        
        board.move(piece_move);
    } 
}
 