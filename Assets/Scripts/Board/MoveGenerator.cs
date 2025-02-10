#pragma warning disable CS0219

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class MoveGenerator
{
    public List<Move> GenerateLegalMoves(Board board, Square filter_square=null, bool check_search=false) {
        List<Move> legal_moves = new List<Move>();
        List<Square> piece_squares_for_turn = GetFriendlySquares(board);

        foreach(Square piece_square in piece_squares_for_turn) {
            // Check if you are only searching for a singular piece
            if (filter_square != null && !filter_square.sq.SequenceEqual(piece_square.sq)) continue;

            switch(piece_square.piece_type) {
                case 1: legal_moves.AddRange(GetPawnMoves(board, piece_square)); break;
                case 2: legal_moves.AddRange(GetRookMoves(board, piece_square)); break;
                case 3: legal_moves.AddRange(GetKnightMoves(board, piece_square)); break;
                case 4: legal_moves.AddRange(GetBishopMoves(board, piece_square)); break;
                case 5: legal_moves.AddRange(GetQueenMoves(board, piece_square)); break;
                case 6: legal_moves.AddRange(GetKingMoves(board, piece_square)); break;
            }
        }

        //List<Move> discarded_legal_moves = getDiscardedMoves(board, legal_moves, check_search);
        //legal_moves.Except(discarded_legal_moves).ToList();

        legal_moves = getDiscardedMoves(board, legal_moves, check_search);

        return legal_moves;
    }

    // CHECK AND CHECKMATE

    public bool isCheckmate(Board board_) {
        // Check for all moves for whoevers turn it is can white checkmate you

        Board board = board_.copy();
        List<Move> playable_moves = GenerateLegalMoves(board);

        int number_of_safe_moves = 0;

        foreach(Move playable_move in playable_moves) {
            board.move(playable_move);

            if (!isCheck(board)) {
                number_of_safe_moves += 1;
            }

            board = board_.copy();
        }

        if (number_of_safe_moves == 0) {
            return true;
        }

        return false;
    }

    public bool isCheck(Board board) {
        List<Move> playable_moves = GenerateLegalMoves(board, check_search:true);

        bool is_in_check = false;
        int king_piece = board.turn ? 13 : 6;
        Square king_location = board.find_piece_location(king_piece);

        foreach(Move playable_move in playable_moves) {
            if (playable_move.end_square.str_uci() == king_location.str_uci()) {
                is_in_check = true;
            }
        }
        return is_in_check;
    }

    private List<Move> getDiscardedMoves(Board board_, List<Move> legal_moves, bool check_search) {
        List<Move> removed_moves = new List<Move>();

        if (!check_search) {
            Board board = board_.copy();

            foreach (Move move_ in legal_moves) {
                board.move(move_);

                if (isCheck(board)) {
                    removed_moves.Add(move_);
                }

                board = board_.copy();
            }

        }
        return legal_moves.Except(removed_moves).ToList();
    }




    private List<Square> GetFriendlySquares(Board board) {
        List<Square> friendly_squares = new List<Square>();

        // Get enemy squares
        for (int r=0; r<8; r++) {
            for (int c=0; c<8; c++) {
                Square piece_square = new Square(board, c, r);

                //  If piece is opposite colour to turn
                if ((!board.turn && piece_square.piece >= 7)  ||  (board.turn && piece_square.piece > 0 && piece_square.piece < 7)) {
                    friendly_squares.Add(piece_square);
                }
            }
        }

        return friendly_squares;
    }


    // ======================================================================
    // ========================= PIECE MOVING ===============================
    // ======================================================================


    private List<Move> GetPawnMoves(Board board, Square piece_square) {
        List<Move> legal_move_list = new List<Move>();

        int direction = piece_square.isWhite ? 1 : -1; // White moves up (+1), Black moves down (-1)

        // 1 Space forward and 2 Space forward squares
        Square forward_sq = new Square(board, piece_square.col, piece_square.row + direction);
        Square double_forward_sq = new Square(board, piece_square.col, piece_square.row + (2*direction));

        Move forward_move = new Move(board, piece_square, forward_sq);
        Move double_forward_move = new Move(board, piece_square, double_forward_sq);


        // Diagonal Moves
        Square l_diagonal_sq = new Square(board, piece_square.col-1, piece_square.row+direction);
        Square r_diagonal_sq = new Square(board, piece_square.col+1, piece_square.row+direction);

        Move l_diagonal_move = new Move(board, piece_square, l_diagonal_sq);
        Move r_diagonal_move = new Move(board, piece_square, r_diagonal_sq);


        if (IsInBounds(forward_move) && forward_move.end_square.piece == 0) {

            // Add forward move and set it as promotion square if needed
            if (forward_move.end_square.row%7 == 0) {
                forward_move.promote = 2; // Promote to Rook
                legal_move_list.Add(forward_move);

                forward_move.promote = 3; // Promote to Bishop
                legal_move_list.Add(forward_move);

                forward_move.promote = 4; // Promote to Knight
                legal_move_list.Add(forward_move);

                forward_move.promote = 5; // Promote to Queen
                legal_move_list.Add(forward_move);
            }
            else {
                legal_move_list.Add(forward_move); // Normal 1 space forward move
            }


            // DOUBLE MOVE
            bool is_on_starting_square = double_forward_move.start_square.row == double_forward_move.start_square.start_row;

            if (is_on_starting_square && double_forward_move.end_square.piece == 0) {
                legal_move_list.Add(double_forward_move);
            }
        }

        // DIAGONAL MOVES
        foreach(Move diagonal_move in new Move[] {l_diagonal_move, r_diagonal_move}) {

            if (IsInBounds(diagonal_move)) {
                int captured_piece = diagonal_move.end_square.piece;

                if (captured_piece != 0 && !IsFriendlyPiece(captured_piece, piece_square.isWhite)) {

                    // Add end square and promotion if needed
                    if (diagonal_move.end_square.row%7 == 0) {
                        diagonal_move.promote = 2; // Promote to Rook
                        legal_move_list.Add(diagonal_move);

                        diagonal_move.promote = 3; // Promote to Bishop
                        legal_move_list.Add(diagonal_move);

                        diagonal_move.promote = 4; // Promote to Knight
                        legal_move_list.Add(diagonal_move);

                        diagonal_move.promote = 5; // Promote to Queen
                        legal_move_list.Add(diagonal_move);
                    }
                    else {
                        legal_move_list.Add(diagonal_move);
                    }

                }
            }

        }

        // EN PASSANT
        legal_move_list.AddRange(GetEnPassantMoves(board, piece_square));

        return legal_move_list;
    }


    private List<Move> GetEnPassantMoves(Board board, Square piece_square) {
        List<Move> legal_move_list = new List<Move>();

        if (board.move_list.Count == 0) return legal_move_list;

        // Last move
        Move lastMove = board.move_list[board.move_list.Count - 1];

        bool is_double_move = Math.Abs(lastMove.start_square.row - lastMove.end_square.row) == 2;

        // Check if the last move was a two-step pawn move
        if (lastMove.end_square.piece_type == 1) // Ensure it's a pawn
        {
            // Check if en passant is possible
            if (Math.Abs(piece_square.col - lastMove.end_square.col) == 1 && piece_square.row == lastMove.end_square.row)
            {
                int pawn_fwd_dir = piece_square.isWhite ? 1 : -1;

                Move en_passant_move = new Move(board, piece_square, lastMove.end_square.row + pawn_fwd_dir, lastMove.end_square.col);

                legal_move_list.Add(en_passant_move);
            }
        }

        return legal_move_list;
    }


    private List<Move> GetRookMoves(Board board, Square piece_square) {
        List<Move> legal_move_list = new List<Move>();

        (int, int)[] directions = {(1, 0), (-1, 0), (0, 1), (0, -1)};
        legal_move_list.AddRange(AddSlidingMoves(board, piece_square, directions) );

        return legal_move_list;
    }


    private List<Move> GetKnightMoves(Board board, Square piece_square) {
        List<Move> legal_move_list = new List<Move>();

        int[][] move_pattern = new[] {
            new[] { -2, -1 }, new[] { -2, 1 },
            new[] { -1, -2 }, new[] { -1, 2 },
            new[] { 1, -2 }, new[] { 1, 2 },
            new[] { 2, -1 }, new[] { 2, 1 }
        };

        foreach (int[] move in move_pattern) {
            int newRow = piece_square.row + move[0];
            int newCol = piece_square.col + move[1];

            Move knight_move = new Move(board, piece_square, newCol, newRow);

            if (IsInBounds(knight_move) && !IsFriendlyPiece(knight_move.end_square.piece, piece_square.isWhite))
            {
                legal_move_list.Add(knight_move);
            }
        }
        
        return legal_move_list;
    }
    

    private List<Move> GetBishopMoves(Board board, Square piece_square) {
        List<Move> legal_move_list = new List<Move>();

        (int, int)[] directions = {(1, 1), (1, -1), (-1, 1), (-1, -1)};
        legal_move_list.AddRange( AddSlidingMoves(board, piece_square, directions) );

        return legal_move_list;
    }


    private List<Move> GetQueenMoves(Board board, Square piece_square) {
        List<Move> legal_move_list = new List<Move>();

        (int, int)[] directions = {(1, 1), (1, 0), (1, -1), (0, -1), (-1, -1), (-1, 0), (-1, 1), (0, 1)};
        legal_move_list.AddRange( AddSlidingMoves(board, piece_square, directions) );

        return legal_move_list;
    }


    private List<Move> GetKingMoves(Board board, Square piece_square) {
        List<Move> legal_move_list = new List<Move>();

        int[][] move_pattern = new[] {
            new[] { -1, -1 }, new[] { -1, 0 }, new[] { -1, 1},
            new[] {  0, -1 },                  new[] {  0, 1},
            new[] {  1, -1 }, new[] {  1, 0 }, new[] {  1, 1},
        };

        foreach (int[] move in move_pattern) {
            int newRow = piece_square.row + move[0];
            int newCol = piece_square.col + move[1];

            Move king_move = new Move(board, piece_square, newCol, newRow);

            if (IsInBounds(king_move) && !IsFriendlyPiece(king_move.end_square.piece, piece_square.isWhite))
            {
                legal_move_list.Add(king_move);
            }
        }

        if (!board.is_h1_rook_moved && !board.is_wking_moved && board.turn && board.b[0, 5] == 0 && board.b[0, 6] == 0) {
            Move mv = new Move(board, "e1g1");
            mv.is_castle_white_short = true;
            legal_move_list.Add(mv);
        }
        if (!board.is_a1_rook_moved && !board.is_wking_moved && board.turn && board.b[0, 3] == 0 && board.b[0, 2] == 0) {
            Move mv = new Move(board, "e1c1");
            mv.is_castle_white_long = true;
            legal_move_list.Add(mv);
        }
        if (!board.is_h8_rook_moved && !board.is_bking_moved && !board.turn && board.b[7, 5] == 0 && board.b[7, 6] == 0) {
            Move mv = new Move(board, "e8g8");
            mv.is_castle_black_short = true;
            legal_move_list.Add(mv);
        }
        if (!board.is_a8_rook_moved && !board.is_bking_moved && !board.turn && board.b[7, 3] == 0 && board.b[7, 2] == 0) {
            Move mv = new Move(board, "e8c8");
            mv.is_castle_black_long = true;
            legal_move_list.Add(mv);
        }
        
        return legal_move_list;
    }


    // ======================================================================
    // ====================== PIECE MOVING HELPER ===========================
    // ======================================================================


    private List<Move> AddSlidingMoves(Board board_, Square piece_square, (int, int)[] directions)
    {
        int[,] board = board_.b;
        List<Move> legal_moves = new List<Move>();

        foreach (var (dRow, dCol) in directions)
        {
            int newRow = piece_square.row + dRow;
            int newCol = piece_square.col + dCol;
            while (IsInBounds(newRow, newCol) && !IsFriendlyPiece(board[newRow, newCol], piece_square.isWhite))
            {
                Move mv = new Move(board_, piece_square.col, piece_square.row, newCol, newRow);
                legal_moves.Add(mv);
                if (board[newRow, newCol] != 0) break; // Stop at capture
                newRow += dRow;
                newCol += dCol;
            }
        }

        return legal_moves;
    }

    private bool IsFriendlyPiece(int piece, bool isWhite) { return isWhite ? (piece > 0 && piece < 7) : (piece >= 7);}
    private bool IsInBounds(Move sq) => sq.end_square.row >= 0 && sq.end_square.row < 8 && sq.end_square.col >= 0 && sq.end_square.col < 8;
    private bool IsInBounds(int row, int col) => row >= 0 && row < 8 && col >= 0 && col < 8;
}
