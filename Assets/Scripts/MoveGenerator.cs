#pragma warning disable CS0219

using System.Collections.Generic;
using UnityEngine;

public class MoveGenerator
{
    private static TOOLS tools = new TOOLS();
    private static (int row, int col)? enPassantTarget = null;
    private static bool whiteKingMoved = false;
    private static bool blackKingMoved = false;
    private static bool whiteRookA1Moved = false;
    private static bool whiteRookH1Moved = false;
    private static bool blackRookA8Moved = false;
    private static bool blackRookH8Moved = false;

    public List<string> GenerateLegalMoves(int[,] board, bool isWhiteTurn)
    {
        if (!isWhiteTurn)
        {
            board = FlipBoard(board);
            enPassantTarget = FlipPosition(enPassantTarget);
        }

        List<string> legalMoves = new List<string>();

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                int piece = board[row, col];
                if (piece == 0) continue; // Empty square

                bool isWhitePiece = piece >= 1 && piece <= 7;
                if (!isWhitePiece) continue; // Skip opponent pieces (black after flipping)

                switch (piece % 7) // Normalize piece type for movement logic
                {
                    case 1: // Pawn
                        AddPawnMoves(board, row, col, true, legalMoves);
                        break;
                    case 2: // Rook
                        AddRookMoves(board, row, col, true, legalMoves);
                        break;
                    case 3: // Knight
                        AddKnightMoves(board, row, col, true, legalMoves);
                        break;
                    case 4: // Bishop
                        AddBishopMoves(board, row, col, true, legalMoves);
                        break;
                    case 5: // Queen
                        AddQueenMoves(board, row, col, true, legalMoves);
                        break;
                    case 6: // King
                        AddKingMoves(board, row, col, true, legalMoves);
                        break;
                }
            }
        }

        if (!isWhiteTurn)
        {
            legalMoves = FlipMoves(legalMoves);
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

    private static void AddPawnMoves(int[,] board, int row, int col, bool isWhite, List<string> moves)
{
    int direction = isWhite ? 1 : -1; // Forward for white is +1, for black is -1
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

    private static void AddKingMoves(int[,] board, int row, int col, bool isWhite, List<string> moves, bool non_recurion=false)
{
    foreach (var delta in new[] { (1, 0), (-1, 0), (0, 1), (0, -1), (1, 1), (-1, -1), (1, -1), (-1, 1) })
    {
        int newRow = row + delta.Item1;
        int newCol = col + delta.Item2;

        // Ensure the move is within bounds and not a friendly piece
        if (IsInBounds(newRow, newCol) && !IsFriendlyPiece(board[newRow, newCol], isWhite))
        {
            // Simulate the move on a copy of the board
            int[,] futureBoard = (int[,])board.Clone();
            futureBoard[newRow, newCol] = futureBoard[row, col]; // Move king
            futureBoard[row, col] = 0; // Empty original square

            // Check if the king would be in check after the move
            if (!non_recurion){
                if (!IsKingInCheck(futureBoard, isWhite))
                {
                    moves.Add(FormatMove(row, col, newRow, newCol));
                }
            }
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

    private static bool IsFriendlyPiece(int piece, bool isWhite)
    {
        return isWhite ? (piece >= 1 && piece <= 7) : (piece >= 8 && piece <= 13);
    }

    private static bool IsOpponentPiece(int piece, bool isWhite)
    {
        return isWhite ? (piece >= 8 && piece <= 13) : (piece >= 1 && piece <= 7);
    }

    private static string FormatMove(int startRow, int startCol, int endRow, int endCol) =>
        $"{(char)(startCol + 'a')}{1 + startRow}{(char)(endCol + 'a')}{1 + endRow}";

    private static int[,] FlipBoard(int[,] board)
    {
        int[,] flipped = new int[8, 8];
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                flipped[7 - row, 7 - col] = board[row, col];
            }
        }
        return flipped;
    }

    private static (int, int)? FlipPosition((int, int)? position)
    {
        if (!position.HasValue) return null;
        return (7 - position.Value.Item1, 7 - position.Value.Item2);
    }

    private static List<string> FlipMoves(List<string> moves)
    {
        List<string> flippedMoves = new List<string>();
        foreach (var move in moves)
        {
            int startCol = move[0] - 'a';
            int startRow = move[1] - '1';
            int endCol = move[2] - 'a';
            int endRow = move[3] - '1';

            flippedMoves.Add(FormatMove(7 - startRow, 7 - startCol, 7 - endRow, 7 - endCol));
        }
        return flippedMoves;
    }

    private static bool IsKingInCheck(int[,] board, bool isWhite)
{
    // Find the king's position
    int kingPiece = isWhite ? 6 : 12;
    (int kingRow, int kingCol) = (-1, -1);

    for (int r = 0; r < 8; r++)
    {
        for (int c = 0; c < 8; c++)
        {
            if (board[r, c] == kingPiece)
            {
                kingRow = r;
                kingCol = c;
                break;
            }
        }
    }

    // Generate opponent's moves
    List<string> opponentMoves = new List<string>();

    for (int row = 0; row < 8; row++)
    {
        for (int col = 0; col < 8; col++)
        {
            int piece = board[row, col];
            if (piece == 0) continue; // Empty square

            bool isOpponentPiece = isWhite ? (piece >= 8 && piece <= 13) : (piece >= 1 && piece <= 7);
            if (!isOpponentPiece) continue; // Skip friendly pieces

            switch (piece % 7) // Normalize piece type for movement logic
            {
                case 1: // Pawn
                    AddPawnMoves(board, row, col, !isWhite, opponentMoves);
                    break;
                case 2: // Rook
                    AddRookMoves(board, row, col, !isWhite, opponentMoves);
                    break;
                case 3: // Knight
                    AddKnightMoves(board, row, col, !isWhite, opponentMoves);
                    break;
                case 4: // Bishop
                    AddBishopMoves(board, row, col, !isWhite, opponentMoves);
                    break;
                case 5: // Queen
                    AddQueenMoves(board, row, col, !isWhite, opponentMoves);
                    break;
                case 6: // King
                    AddKingMoves(board, row, col, !isWhite, opponentMoves, non_recurion: true);
                    break;
            }
        }
    }

    // Check if the king's position is targeted
    string kingPosition = FormatMove(kingRow, kingCol, kingRow, kingCol).Substring(0, 2);

    foreach (string move in opponentMoves)
    {
        if (move.Contains(kingPosition))
        {
            return true;
        }
    }

    return false;
}
}
