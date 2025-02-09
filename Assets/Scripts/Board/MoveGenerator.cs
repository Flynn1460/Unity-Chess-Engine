#pragma warning disable CS0219

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class MoveGenerator
{
    private TOOLS tools = new TOOLS();

    public List<String> GenerateLegalMoves(Board board, List<int> filter_square=null, bool check_search=false) {
        List<String> legal_moves = new List<String>();

        List<Square> piece_squares_for_turn = GetSquares(board, colour:tools.bool_num(board.turn));

        foreach(Square piece_square in piece_squares_for_turn) {
            if (filter_square != null && !filter_square.SequenceEqual(piece_square.sq)) continue;

            switch(piece_square.piece_type) {
                case 1: legal_moves.AddRange(GetPawnMoves(board, piece_square)); break;
                case 2: legal_moves.AddRange(GetRookMoves(board, piece_square)); break;
                case 3: legal_moves.AddRange(GetKnightMoves(board, piece_square)); break;
                case 4: legal_moves.AddRange(GetBishopMoves(board, piece_square)); break;
                case 5: legal_moves.AddRange(GetQueenMoves(board, piece_square)); break;
                case 6: legal_moves.AddRange(GetKingMoves(board, piece_square)); break;
            }
        }

        if (!check_search) {
            Board board_ = board.copy();

            List<String> removed_moves = new List<String>();

            foreach (String move_ in legal_moves) {
                Move move = new Move(board_, move_);

                board_.move(move);

                if (isCheck(board_)) {
                    removed_moves.Add(move_);
                }

                board_ = board.copy();
            }

            legal_moves = legal_moves.Except(removed_moves).ToList();
        }

        return legal_moves;
    }

    public bool isCheckmate(Board board_) {
        /*
        Check for all moves for whoevers turn it is can white checkmate you
        */
        Board board = board_.copy();
        List<String> playable_moves = GenerateLegalMoves(board);

        int number_of_total_moves = playable_moves.Count;
        int number_of_safe_moves = 0;

        foreach(String playable_move in playable_moves) {
            Move move_ = new Move(board, playable_move);
            board.move(move_);


            if (!isCheck(board)) {
                number_of_safe_moves += 1;
            }

            board = board_.copy();
        }

        if (number_of_safe_moves == 0) {
            Debug.Log("Checkmate");
            return true;
        }

        return false;
    }

    public bool isCheck(Board board) {
        List<String> playable_moves = GenerateLegalMoves(board, check_search:true);

        // King Pos Calc        
        int king_piece = board.turn ? 13 : 6;
        List<int> king_location = board.find_piece_location(king_piece);

        bool is_in_check = false;

        foreach(String playable_move in playable_moves) {
            if (playable_move.Substring(2,2) == tools.uci_converter(king_location)) {
                is_in_check = true;
            }
        }

        return is_in_check;
    }


    private List<Square> GetSquares(Board board, int colour=0) {
        List<Square> enemy_squares = new List<Square>();

        // If colour is 0 then returns all white squares
        // If colour is 1 then returns all black squares
        // If colour is 2 then returns all squares

        bool white_squares = (colour == 0 || colour == 2);
        bool black_squares = (colour == 1 || colour == 2);

        // Get enemy squares
        for (int r=0; r<8; r++) {
            for (int c=0; c<8; c++) {

                int square = board.b[r,c];

                if ((white_squares && square >= 7)  ||  (black_squares && square > 0 && square < 7)) {

                    Square piece_square = new Square(board, c, r);
                    enemy_squares.Add(piece_square);
                }
            }
        }

        return enemy_squares;
    }


    // ======================================================================
    // ========================= PIECE MOVING ===============================
    // ======================================================================


    private List<String> GetPawnMoves(Board board, Square piece_square) {
        List<String> legal_move_list = new List<String>();

        int direction = piece_square.isWhite ? 1 : -1; // White moves up (+1), Black moves down (-1)
        int startRow = piece_square.isWhite ? 1 : 6;   // Starting row for double move

        int[][] diagonalOffsets = new[] { new[] { direction, -1 }, new[] { direction, 1 } };

        // Normal forward move
        int oneStepRow = piece_square.row + direction;

        if (IsInBounds(oneStepRow, piece_square.col) && board.b[oneStepRow, piece_square.col] == 0) {
            if (oneStepRow%7 == 0) legal_move_list.Add(FormatMove(piece_square.row, piece_square.col, oneStepRow, piece_square.col)+"=Q");
            else legal_move_list.Add(FormatMove(piece_square.row, piece_square.col, oneStepRow, piece_square.col));

            // Double move from the starting position
            int twoStepRow = piece_square.row + 2 * direction;

            if (piece_square.row == startRow && board.b[twoStepRow, piece_square.col] == 0) {
                legal_move_list.Add(FormatMove(piece_square.row, piece_square.col, twoStepRow, piece_square.col));
            }
        }

        // Diagonal captures
        foreach (var offset in diagonalOffsets) {
            int newRow = piece_square.row + offset[0];
            int newCol = piece_square.col + offset[1];

            if (IsInBounds(newRow, newCol)) {
                int targetPiece = board.b[newRow, newCol];

                if (targetPiece != 0 && !IsFriendlyPiece(targetPiece, piece_square.isWhite)) {
                    if (newRow%7 == 0) legal_move_list.Add(FormatMove(piece_square.row, piece_square.col, newRow, newCol)+"=Q");
                    else legal_move_list.Add(FormatMove(piece_square.row, piece_square.col, newRow, newCol));
                }
            }
        }

        // En Passant
        legal_move_list.AddRange(GetEnPassantMoves(board, piece_square));

        return legal_move_list;
    }

    private List<String> GetEnPassantMoves(Board board, Square piece_square) {
        List<String> legal_move_list = new List<String>();
        if (board.move_list.Count == 0) return legal_move_list;

        // Last move
        String lastMove = (board.move_list[board.move_list.Count - 1]).str_uci();
        List<int> lastMoveCoords = tools.uci_converter(lastMove);
        int lastMoveStartRow = lastMoveCoords[1];
        int lastMoveEndRow = lastMoveCoords[3];
        int lastMoveCol = lastMoveCoords[0];

        // Check if the last move was a two-step pawn move
        if (Math.Abs(lastMoveStartRow - lastMoveEndRow) == 2 && 
            board.b[lastMoveEndRow, lastMoveCol] % 7 == 1) // Ensure it's a pawn
        {
            // Check if en passant is possible
            if (Math.Abs(piece_square.col - lastMoveCol) == 1 &&
                piece_square.row == lastMoveEndRow)
            {
                // Use FormatMove to generate the en passant move in UCI notation
                legal_move_list.Add(FormatMove(piece_square.row, piece_square.col, lastMoveEndRow + (piece_square.isWhite ? 1 : -1), lastMoveCol));
            }
        }

        return legal_move_list;
    }

    private List<String> GetRookMoves(Board board, Square piece_square) {
        List<String> legal_move_list = new List<String>();

        (int, int)[] directions = {(1, 0), (-1, 0), (0, 1), (0, -1)};
        legal_move_list.AddRange( AddSlidingMoves(board.b, piece_square, directions) );

        return legal_move_list;
    }

    private List<String> GetKnightMoves(Board board, Square piece_square) {
        List<String> legal_move_list = new List<String>();

        int[][] move_pattern = new[] {

            new[] { -2, -1 }, new[] { -2, 1 },
            new[] { -1, -2 }, new[] { -1, 2 },
            new[] { 1, -2 }, new[] { 1, 2 },
            new[] { 2, -1 }, new[] { 2, 1 }

        };

        foreach (int[] move in move_pattern) {
            int newRow = piece_square.row + move[0];
            int newCol = piece_square.col + move[1];

            if (IsInBounds(newRow, newCol) && !IsFriendlyPiece(board.b[newRow, newCol], piece_square.isWhite))
            {
                legal_move_list.Add(FormatMove(piece_square.row, piece_square.col, newRow, newCol));
            }
        }
        
        return legal_move_list;
    }
    
    private List<String> GetBishopMoves(Board board, Square piece_square) {
        List<String> legal_move_list = new List<String>();

        (int, int)[] directions = {(1, 1), (1, -1), (-1, 1), (-1, -1)};
        legal_move_list.AddRange( AddSlidingMoves(board.b, piece_square, directions) );

        return legal_move_list;
    }

    private List<String> GetQueenMoves(Board board, Square piece_square) {
        List<String> legal_move_list = new List<String>();

        (int, int)[] directions = {(1, 1), (1, 0), (1, -1), (0, -1), (-1, -1), (-1, 0), (-1, 1), (0, 1)};
        legal_move_list.AddRange( AddSlidingMoves(board.b, piece_square, directions) );

        return legal_move_list;
    }

    private List<String> GetKingMoves(Board board, Square piece_square) {
        List<String> legal_move_list = new List<String>();

        int[][] move_pattern = new[] {

            new[] { -1, -1 }, new[] { 0, 1 },
            new[] { -1, 0 }, new[] { 1, -1 },
            new[] { 1, 0 }, new[] { 1, 0 },
            new[] { 0, -1 }, new[] { 1, 1 }

        };

        foreach (int[] move in move_pattern) {
            int newRow = piece_square.row + move[0];
            int newCol = piece_square.col + move[1];

            if (IsInBounds(newRow, newCol) && !IsFriendlyPiece(board.b[newRow, newCol], piece_square.isWhite))
            {
                legal_move_list.Add(FormatMove(piece_square.row, piece_square.col, newRow, newCol));
            }
        }
        

        return legal_move_list;
    }


    // ======================================================================
    // ====================== PIECE MOVING HELPER ===========================
    // ======================================================================

    private List<String> AddSlidingMoves(int[,] board, Square piece_square, (int, int)[] directions)
    {
        List<String> legal_moves = new List<String>();

        foreach (var (dRow, dCol) in directions)
        {
            int newRow = piece_square.row + dRow;
            int newCol = piece_square.col + dCol;
            while (IsInBounds(newRow, newCol) && !IsFriendlyPiece(board[newRow, newCol], piece_square.isWhite))
            {
                legal_moves.Add(FormatMove(piece_square.row, piece_square.col, newRow, newCol));
                if (board[newRow, newCol] != 0) break; // Stop at capture
                newRow += dRow;
                newCol += dCol;
            }
        }

        return legal_moves;
    }

    private string FormatMove(int startRow, int startCol, int endRow, int endCol) =>  $"{(char)(startCol + 'a')}{1 + startRow}{(char)(endCol + 'a')}{1 + endRow}";
    private bool IsFriendlyPiece(int piece, bool isWhite) { return isWhite ? (piece > 0 && piece < 7) : (piece >= 7);}
    private bool IsInBounds(int row, int col) => row >= 0 && row < 8 && col >= 0 && col < 8;
}
