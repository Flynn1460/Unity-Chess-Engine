#pragma warning disable CS0219

using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : TOOLS
{
    // Classes
    public Board board = new Board();
    public MoveGenerator move_generator = new MoveGenerator();

    private CONTROLLER_SquareHighlight boardHighlighter = new CONTROLLER_SquareHighlight();

    // Textures
    private Sprite queen_texture = Resources.Load<Sprite>("Chess/wq");


    public List<String> GenerateLegalMoves(Square filter_square=null) {
        if (board.turn_id == 0) {
            List<int> new_filter_square = new List<int>() {filter_square.col, filter_square.row};
            return move_generator.GenerateLegalMoves(board, filter_square:new_filter_square);
        }
        else {
            return new List<String>();
        }
    }


    public void Highlight_Piece_Moves(Square piece_square) {
        if (board.turn_id == 0) {
            List<String> piece_legal_moves = move_generator.GenerateLegalMoves(board, piece_square.sq);
            List<String> stripped_moves = strip_moves(piece_legal_moves, false, include_promotion:false); // Get list of moves, ignore promotion data

            boardHighlighter.Highlight_Tiles(stripped_moves);
        }
    }

    public void Push(Move piece_move, int pawn_promote_piece=-1){
        List<int> old_position = piece_move.start_square.sq;
        List<int> new_position = piece_move.end_square.sq;
        // If replace piece is set to something make piece otherwise use what was at the position
        int piece = pawn_promote_piece == -1 ? board.b[old_position[0], old_position[1]] : pawn_promote_piece;

        board.move(piece_move);

        if (piece == 1 && new_position[0] == 7) board.b[new_position[0], new_position[1]] = 5; // WHITE QUEEN PROMO
        if (piece == 8 && new_position[0] == 0) board.b[new_position[0], new_position[1]] = 12; // BLACK QUEEN PROMO

        
        // If move is pushed then highlighting isn't needed
        boardHighlighter.Reset_Tiles();

        // CHECKMATE
        if (move_generator.isCheckmate(board)) {
            board.is_checkmate = true;
        }
    } 
}
