#pragma warning disable CS0219

using System.Collections.Generic;
using UnityEngine;

public class BoardManager
{
    // Classes
    public Board board = new Board();
    public MoveGenerator move_generator = new MoveGenerator();

    private CONTROLLER_SquareHighlight boardHighlighter = new CONTROLLER_SquareHighlight();

    // Textures
    private Sprite queen_texture = Resources.Load<Sprite>("Chess/wq");


    public void Highlight_Piece_Moves(Square piece_square) {
        if (board.turn_id == 0) {
            List<Move> piece_legal_moves = move_generator.GenerateLegalMoves(board, piece_square);

            boardHighlighter.Highlight_Tiles(piece_legal_moves);
        }
    }

    public void Push(Move piece_move, int pawn_promote_piece=-1){
        List<int> old_position = piece_move.start_square.sq;
        List<int> new_position = piece_move.end_square.sq;
        // If replace piece is set to something make piece otherwise use what was at the position
        int piece = pawn_promote_piece == -1 ? board.b[old_position[0], old_position[1]] : pawn_promote_piece;

        if (piece == 1 && new_position[0] == 7) board.b[new_position[0], new_position[1]] = 5; // WHITE QUEEN PROMO
        if (piece == 8 && new_position[0] == 0) board.b[new_position[0], new_position[1]] = 12; // BLACK QUEEN PROMO

        // If move is pushed then highlighting isn't needed
        boardHighlighter.Reset_Tiles();

        board.move(piece_move);
    } 
}
 