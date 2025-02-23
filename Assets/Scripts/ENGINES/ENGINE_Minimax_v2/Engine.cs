using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ENGINE_NAMESPACE_Minimax_V2 {
public class MinimaxEngine {
    
    private MoveGenerator move_generator = new MoveGenerator();
    private Eval eval_cl = new Eval();

    private Stopwatch stopwatch = new Stopwatch();
    private int allocated_movetime = 0;

    public Move Get_Move(Board board, int movetime, int set_depth=-1) {
        allocated_movetime = movetime;

        stopwatch = new Stopwatch();
        stopwatch.Start();

        if (set_depth != -1) {
            allocated_movetime = 1000000;
            (Move returned_mv, double eval) = minimax(board, (set_depth-1), -999, +999);
            UnityEngine.Debug.Log($"E2: Max Depth of {set_depth} reached in: {stopwatch.ElapsedMilliseconds}ms");
            return returned_mv;
        }

        Move best_move = new Move(board, "a1h4");
        int max_depth = 1;

        while (true) {
            (Move returned_mv, double eval) = minimax(board, (max_depth-1), -999, +999);

            if (eval == -1001) {
                return best_move;
            }
            else if (eval == 900 || eval == -900) {
                return returned_mv;
            }

            best_move = returned_mv.copy();
            max_depth++;
        }
    }

    public (Move, double) minimax(Board board, int depth, double alpha, double beta) {
        List<Move> moves = move_generator.GenerateLegalMoves(board);

        double highest_eval = board.turn ? -1000 : +1000;
        double eval;

        Move highest_mv = new Move(board, "a1h3");

        foreach(Move move in moves) {
            board.move(move);

            if (move_generator.isGM(board) || depth == 0) {
                eval = eval_cl.EvaluateBoard(board);
            }
            else {
                (Move x, double y) = minimax(board, depth-1, alpha, beta);
                eval = y;
            }

            board.undo_move();

            // Timeout
            if (stopwatch.ElapsedMilliseconds > allocated_movetime) {
                return (new Move(board, "a1h2"), -1001);
            }

            if ((eval > highest_eval && board.turn) || (eval < highest_eval && !board.turn)) {
                highest_eval = eval;
                highest_mv = move.copy();

                if (board.turn)  alpha = Math.Max(alpha, eval);
                else  beta = Math.Min(beta, eval);
                
                if (beta <= alpha) break;
            }

        }

        return (highest_mv, highest_eval);
    }
}
}
