#pragma warning disable CS0219

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

class Square
{
    public int row;
    public int col;
    public List<int> rcList;

    public bool isWhite;
    public int piece;
    public int piece_type;

    public Square(int row, int col, int piece) {
        this.row = row;
        this.col = col;

        this.rcList = new List<int>() {row, col};
        
        this.piece = piece;
        this.piece_type = piece % 7;

        this.isWhite = (piece == piece_type);
    }

    public override string ToString() {
        return "("+row+","+col+") "+piece_type+ ", "+isWhite;
    }
}

public class NewMoveGenerator
{
    private TOOLS tools = new TOOLS();

    public List<String> GenerateLegalMoves(int[,] board, bool isWhite, List<String> board_move_list, List<int> filter_square=null) {
        List<String> legal_moves = new List<String>();

        List<Square> piece_squares_for_turn = GetSquares(board, colour:tools.bool_num(isWhite));


        foreach(Square piece_square in piece_squares_for_turn) {
            if (filter_square != null && !filter_square.SequenceEqual(piece_square.rcList)) continue;

            switch(piece_square.piece_type) {
                case 1: legal_moves.AddRange(GetPawnMoves(board, piece_square, board_move_list)); break;
                case 2: legal_moves.AddRange(GetRookMoves(board, piece_square)); break;
                case 3: legal_moves.AddRange(GetKnightMoves(board, piece_square)); break;
                case 4: legal_moves.AddRange(GetBishopMoves(board, piece_square)); break;
                case 5: legal_moves.AddRange(GetQueenMoves(board, piece_square)); break;
                case 6: legal_moves.AddRange(GetKingMoves(board, piece_square)); break;
            }
        }

        return legal_moves;
    }

    public bool isCheckmate(int[,] board_, bool isWhite, List<String> board_move_list) {
        /*
        Check for all moves for whoevers turn it is can white checkmate you
        */
        int[,] board = tools.cpy_board(board_);

        List<String> playable_moves = GenerateLegalMoves(board, isWhite, board_move_list);

        foreach(String playable_move in playable_moves) {
            List<int> move = tools.uci_converter(playable_move);
            
            Debug.Log("# "+playable_move);

            board[move[3], move[2]] = board[move[1], move[0]];
            board[move[1], move[0]] = 0;

            isCheck(board, isWhite, board_move_list);
        }

        return false;
    }

    public bool isCheck(int[,] board, bool isWhite, List<String> board_move_list) {
        List<String> playable_moves = GenerateLegalMoves(board, !isWhite, board_move_list);

        // King Pos Calc        
        int king_piece = isWhite ? 6 : 13;
        List<int> king_location = tools.find_value(board, king_piece);


        foreach(String playable_move in playable_moves) {
            Debug.Log("= "+playable_move + "," + tools.uci_converter(king_location));
        }


        return false;
    }

    private List<Square> GetSquares(int[,] board, int colour=0) {
        List<Square> enemy_squares = new List<Square>();

        // If colour is 0 then returns all white squares
        // If colour is 1 then returns all black squares
        // If colour is 2 then returns all squares

        bool white_squares = (colour == 0 || colour == 2);
        bool black_squares = (colour == 1 || colour == 2);

        // Get enemy squares
        for (int r=0; r<8; r++) {
            for (int c=0; c<8; c++) {

                int square = board[r,c];

                if ((white_squares && square >= 7)  ||  (black_squares && square > 0 && square < 7)) {

                    Square piece_square = new Square(r, c, square);

                    enemy_squares.Add(piece_square);
                }
            }
        }

        return enemy_squares;
    }


    // ======================================================================
    // ========================= PIECE MOVING ===============================
    // ======================================================================


    private List<String> GetPawnMoves(int[,] board, Square piece_square, List<String> MOVE_LIST) {
        List<String> legal_move_list = new List<String>();

        int direction = piece_square.isWhite ? 1 : -1; // White moves up (+1), Black moves down (-1)
        int startRow = piece_square.isWhite ? 1 : 6;   // Starting row for double move

        int[][] diagonalOffsets = new[] { new[] { direction, -1 }, new[] { direction, 1 } };

        // Normal forward move
        int oneStepRow = piece_square.row + direction;

        if (IsInBounds(oneStepRow, piece_square.col) && board[oneStepRow, piece_square.col] == 0) {
            if (oneStepRow%7 == 0) legal_move_list.Add(FormatMove(piece_square.row, piece_square.col, oneStepRow, piece_square.col)+"=Q");
            else legal_move_list.Add(FormatMove(piece_square.row, piece_square.col, oneStepRow, piece_square.col));

            // Double move from the starting position
            int twoStepRow = piece_square.row + 2 * direction;

            if (piece_square.row == startRow && board[twoStepRow, piece_square.col] == 0) {
                legal_move_list.Add(FormatMove(piece_square.row, piece_square.col, twoStepRow, piece_square.col));
            }
        }

        // Diagonal captures
        foreach (var offset in diagonalOffsets) {
            int newRow = piece_square.row + offset[0];
            int newCol = piece_square.col + offset[1];

            if (IsInBounds(newRow, newCol)) {
                int targetPiece = board[newRow, newCol];

                if (targetPiece != 0 && !IsFriendlyPiece(targetPiece, piece_square.isWhite)) {
                    if (newRow%7 == 0) legal_move_list.Add(FormatMove(piece_square.row, piece_square.col, newRow, newCol)+"=Q");
                    else legal_move_list.Add(FormatMove(piece_square.row, piece_square.col, newRow, newCol));
                }
            }
        }

        // En Passant
        legal_move_list.AddRange(GetEnPassantMoves(board, piece_square, MOVE_LIST));

        return legal_move_list;
    }

    private List<String> GetEnPassantMoves(int[,] board, Square piece_square, List<String> MOVE_LIST) {
        List<String> legal_move_list = new List<String>();
        if (MOVE_LIST.Count == 0) return legal_move_list;

        // Last move
        String lastMove = MOVE_LIST[MOVE_LIST.Count - 1];
        List<int> lastMoveCoords = tools.uci_converter(lastMove);
        int lastMoveStartRow = lastMoveCoords[1];
        int lastMoveEndRow = lastMoveCoords[3];
        int lastMoveCol = lastMoveCoords[0];

        // Check if the last move was a two-step pawn move
        if (Math.Abs(lastMoveStartRow - lastMoveEndRow) == 2 && 
            board[lastMoveEndRow, lastMoveCol] % 7 == 1) // Ensure it's a pawn
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

    private List<String> GetRookMoves(int[,] board, Square piece_square) {
        List<String> legal_move_list = new List<String>();

        (int, int)[] directions = {(1, 0), (-1, 0), (0, 1), (0, -1)};
        legal_move_list.AddRange( AddSlidingMoves(board, piece_square, directions) );

        return legal_move_list;
    }

    private List<String> GetKnightMoves(int[,] board, Square piece_square) {
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

            if (IsInBounds(newRow, newCol) && !IsFriendlyPiece(board[newRow, newCol], piece_square.isWhite))
            {
                legal_move_list.Add(FormatMove(piece_square.row, piece_square.col, newRow, newCol));
            }
        }
        
        return legal_move_list;
    }
    
    private List<String> GetBishopMoves(int[,] board, Square piece_square) {
        List<String> legal_move_list = new List<String>();

        (int, int)[] directions = {(1, 1), (1, -1), (-1, 1), (-1, -1)};
        legal_move_list.AddRange( AddSlidingMoves(board, piece_square, directions) );

        return legal_move_list;
    }

    private List<String> GetQueenMoves(int[,] board, Square piece_square) {
        List<String> legal_move_list = new List<String>();

        (int, int)[] directions = {(1, 1), (1, 0), (1, -1), (0, -1), (-1, -1), (-1, 0), (-1, 1), (0, 1)};
        legal_move_list.AddRange( AddSlidingMoves(board, piece_square, directions) );

        return legal_move_list;
    }

    private List<String> GetKingMoves(int[,] board, Square piece_square) {
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

            if (IsInBounds(newRow, newCol) && !IsFriendlyPiece(board[newRow, newCol], piece_square.isWhite))
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
 