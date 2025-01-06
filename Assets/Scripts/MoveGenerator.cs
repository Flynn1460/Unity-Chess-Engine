#pragma warning disable CS0219

using System.Collections.Generic;
using UnityEngine;

public class MoveGenerator
{
    private static (int row, int col)? enPassantTarget = null;
    private static bool whiteKingMoved = false;
    private static bool blackKingMoved = false;
    private static bool whiteRookA1Moved = false;
    private static bool whiteRookH1Moved = false;
    private static bool blackRookA8Moved = false;
    private static bool blackRookH8Moved = false;


    // Method to generate all legal moves for a given board position
    public List<string> GenerateLegalMoves(int[,] board, bool isWhiteTurn)
    {
        List<string> legalMoves = new List<string>();

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                int piece = board[row, col];
                if (piece == 0) continue; // Empty square

                bool isWhitePiece = piece >= 1 && piece <= 7;
                if (isWhitePiece != isWhiteTurn) continue; // Skip opponent pieces

                switch (piece % 7) // Normalize piece type for movement logic
                {
                    case 1: // Pawn
                        AddPawnMoves(board, row, col, isWhiteTurn, legalMoves);
                        break;
                    case 2: // Rook
                        AddRookMoves(board, row, col, isWhiteTurn, legalMoves);
                        break;
                    case 3: // Knight
                        AddKnightMoves(board, row, col, isWhiteTurn, legalMoves);
                        break;
                    case 4: // Bishop
                        AddBishopMoves(board, row, col, isWhiteTurn, legalMoves);
                        break;
                    case 5: // Queen
                        AddQueenMoves(board, row, col, isWhiteTurn, legalMoves);
                        break;
                    case 6: // King
                        AddKingMoves(board, row, col, isWhiteTurn, legalMoves);
                        break;
                }
            }
        }

        return legalMoves;
    }

    public List<string> GenerateLegalMoves_ForPiece(int[,] board, bool isWhiteTurn, List<int> piece_square)
    {
        List<string> legalMoves = new List<string>();

        int piece = board[piece_square[0], piece_square[1]];
        bool isWhitePiece = piece >= 1 && piece <= 7;
        if (isWhitePiece != isWhiteTurn) return legalMoves; // Skip opponent pieces

        switch (piece % 7) // Normalize piece type for movement logic
        {
            case 1: // Pawn
                AddPawnMoves(board, piece_square[0], piece_square[1], isWhiteTurn, legalMoves);
                return legalMoves;
            case 2: // Rook
                AddRookMoves(board, piece_square[0], piece_square[1], isWhiteTurn, legalMoves);
                return legalMoves;
            case 3: // Knight
                AddKnightMoves(board, piece_square[0], piece_square[1], isWhiteTurn, legalMoves);
                return legalMoves;
            case 4: // Bishop
                AddBishopMoves(board, piece_square[0], piece_square[1], isWhiteTurn, legalMoves);
                return legalMoves;
            case 5: // Queen
                AddQueenMoves(board, piece_square[0], piece_square[1], isWhiteTurn, legalMoves);
                return legalMoves;
            case 6: // King
                AddKingMoves(board, piece_square[0], piece_square[1], isWhiteTurn, legalMoves);
                return legalMoves;
        }
        return legalMoves;
    }

    // MOVES
    private static void AddPawnMoves(int[,] board, int row, int col, bool isWhite, List<string> moves)
    {
        int direction = isWhite ? 1 : -1;
        int startRow = isWhite ? 1 : 6;
        int promotionRow = isWhite ? 7 : 0;

        // Forward move
        if (IsInBounds(row + direction, col) && board[row + direction, col] == 0)
        {
            if (row + direction == promotionRow)
            {
                AddPawnPromotionMoves(row, col, row + direction, col, moves);
            }
            else
            {
                moves.Add(FormatMove(row, col, row + direction, col));
            }

            // Double forward move
            if (row == startRow && board[row + 2 * direction, col] == 0)
            {
                moves.Add(FormatMove(row, col, row + 2 * direction, col));
                enPassantTarget = (row + direction, col);
            }
        }

        // Captures
        foreach (int dc in new[] { -1, 1 })
        {
            int targetRow = row + direction;
            int targetCol = col + dc;
            if (IsInBounds(targetRow, targetCol))
            {
                if (IsOpponentPiece(board[targetRow, targetCol], isWhite))
                {
                    if (targetRow == promotionRow)
                    {
                        AddPawnPromotionMoves(row, col, targetRow, targetCol, moves);
                    }
                    else
                    {
                        moves.Add(FormatMove(row, col, targetRow, targetCol));
                    }
                }

                // En passant
                if (enPassantTarget.HasValue && enPassantTarget.Value == (targetRow, targetCol))
                {
                    moves.Add(FormatMove(row, col, targetRow, targetCol) + " e.p.");
                }
            }
        }
    }

    private static void AddPawnPromotionMoves(int startRow, int startCol, int endRow, int endCol, List<string> moves)
    {
        foreach (char promotionPiece in new[] { 'Q', 'R', 'B', 'N' })
        {
            moves.Add(FormatMove(startRow, startCol, endRow, endCol) + "=" + promotionPiece);
        }
    }

    private static void AddRookMoves(int[,] board, int row, int col, bool isWhite, List<string> moves)
    {
        AddSlidingMoves(board, row, col, isWhite, moves, new[] { (1, 0), (-1, 0), (0, 1), (0, -1) });

        // Update rook movement tracking for castling
        if (isWhite && row == 0 && col == 0) whiteRookA1Moved = true;
        if (isWhite && row == 0 && col == 7) whiteRookH1Moved = true;
        if (!isWhite && row == 7 && col == 0) blackRookA8Moved = true;
        if (!isWhite && row == 7 && col == 7) blackRookH8Moved = true;
    }

    private static void AddKnightMoves(int[,] board, int row, int col, bool isWhite, List<string> moves)
    {
        int[][] deltas = new[]
        {
            new[] { -2, -1 }, new[] { -2, 1 },
            new[] { -1, -2 }, new[] { -1, 2 },
            new[] { 1, -2 }, new[] { 1, 2 },
            new[] { 2, -1 }, new[] { 2, 1 }
        };

        foreach (var delta in deltas)
        {
            int newRow = row + delta[0];
            int newCol = col + delta[1];
            if (IsInBounds(newRow, newCol) && !IsFriendlyPiece(board[newRow, newCol], isWhite))
            {
                moves.Add(FormatMove(row, col, newRow, newCol));
            }
        }
    }

    private static void AddBishopMoves(int[,] board, int row, int col, bool isWhite, List<string> moves)
    {
        AddSlidingMoves(board, row, col, isWhite, moves, new[] { (1, 1), (-1, -1), (1, -1), (-1, 1) });
    }

    private static void AddQueenMoves(int[,] board, int row, int col, bool isWhite, List<string> moves)
    {
        AddSlidingMoves(board, row, col, isWhite, moves, new[] { (1, 0), (-1, 0), (0, 1), (0, -1), (1, 1), (-1, -1), (1, -1), (-1, 1) });
    }

    private static void AddKingMoves(int[,] board, int row, int col, bool isWhite, List<string> moves)
    {
        foreach (var delta in new[] { (1, 0), (-1, 0), (0, 1), (0, -1), (1, 1), (-1, -1), (1, -1), (-1, 1) })
        {
            int newRow = row + delta.Item1;
            int newCol = col + delta.Item2;
            if (IsInBounds(newRow, newCol) && !IsFriendlyPiece(board[newRow, newCol], isWhite))
            {
                moves.Add(FormatMove(row, col, newRow, newCol));
            }
        }

        // Castling
        if (isWhite && !whiteKingMoved && row == 0 && col == 4)
        {
            if (!whiteRookA1Moved && board[0, 1] == 0 && board[0, 2] == 0 && board[0, 3] == 0)
            {
                moves.Add("e1c1");
            }
            if (!whiteRookH1Moved && board[0, 5] == 0 && board[0, 6] == 0)
            {
                moves.Add("e1g1");
            }
        }
        else if (!isWhite && !blackKingMoved && row == 7 && col == 4)
        {
            if (!blackRookA8Moved && board[7, 1] == 0 && board[7, 2] == 0 && board[7, 3] == 0)
            {
                moves.Add("e8c8");
            }
            if (!blackRookH8Moved && board[7, 5] == 0 && board[7, 6] == 0)
            {
                moves.Add("e8g8");
            }
        }
    }

    private static void AddSlidingMoves(int[,] board, int row, int col, bool isWhite, List<string> moves, (int, int)[] directions)
    {
        foreach (var (dRow, dCol) in directions)
        {
            int newRow = row + dRow;
            int newCol = col + dCol;
            while (IsInBounds(newRow, newCol) && !IsFriendlyPiece(board[newRow, newCol], isWhite))
            {
                moves.Add(FormatMove(row, col, newRow, newCol));
                if (board[newRow, newCol] != 0) break; // Stop at capture
                newRow += dRow;
                newCol += dCol;
            }
        }
    }

    private static bool IsInBounds(int row, int col) => row >= 0 && row < 8 && col >= 0 && col < 8;

    private static bool IsFriendlyPiece(int piece, bool isWhite) => (isWhite && piece >= 1 && piece <= 7) || (!isWhite && piece >= 8 && piece <= 13);

    private static bool IsOpponentPiece(int piece, bool isWhite) => (isWhite && piece >= 8 && piece <= 13) || (!isWhite && piece >= 1 && piece <= 7);

    private static string FormatMove(int startRow, int startCol, int endRow, int endCol) => $"{(char)(startCol + 'a')}{1 + startRow}{(char)(endCol + 'a')}{1 + endRow}";
}
