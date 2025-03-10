using System.Collections.Generic;
using System.Diagnostics;

namespace ENGINE_NAMESPACE_Minimax_V1 {
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
            (Move returned_mv, double eval) = minimax_dr(board, 1, set_depth);
            UnityEngine.Debug.Log($"E1: Max Depth of {set_depth} reached in: {stopwatch.ElapsedMilliseconds}ms");
            return returned_mv;
        }

        Move best_move = new Move(board, "a1h4");
        int max_depth = 1;

        while (true) {
            (Move returned_mv, double eval) = minimax_dr(board, 1, max_depth);

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

    public (Move, double) minimax_dr(Board board, int depth, int max_depth) {
        List<Move> moves = move_generator.GenerateLegalMoves(board);

        double highest_eval = board.turn ? -1000 : +1000;
        double eval;

        Move highest_mv = new Move(board, "a1h3");

        foreach(Move move in moves) {
            board.move(move); // Flip turn

            if (move_generator.isGM(board) || depth == max_depth) {
                eval = eval_cl.EvaluateBoard(board);
            }
            else {
                (Move x, double y) = minimax_dr(board, depth+1, max_depth);
                eval = y;
            }

            board.undo_move(); // Flip Turn back


            if (stopwatch.ElapsedMilliseconds > allocated_movetime) {
                return (new Move(board, "a1h2"), -1001);
            }
            
            if ((eval > highest_eval && board.turn)  || (eval < highest_eval && !board.turn)) {
                highest_eval = eval;
                highest_mv = move.copy();
            }
        }

        return (highest_mv, highest_eval);
    }
}
}
