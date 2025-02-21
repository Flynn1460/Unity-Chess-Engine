using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace ENGINE_NAMESPACE_Minimax_V1 {
public class MinimaxEngine {
    
    private MoveGenerator move_generator = new MoveGenerator();
    private Eval eval_cl = new Eval();

    private Stopwatch stopwatch = new Stopwatch();
    private int allocated_movetime = 0;

    public Move Get_Move(Board board, int movetime) {
        int allocated_movetime = movetime;

        stopwatch = new Stopwatch();
        stopwatch.Start();

        Move best_move = new Move();

        int max_depth = 1;

        while (true) {
            (Move returned_mv, double eval) = minimax_dr(board, 1, max_depth);

            if (eval == -1001) {
                return best_move;
            }

            best_move = returned_mv.copy();
            max_depth++;
        }
    }

    public (Move, double) minimax_dr(Board board, int depth, int max_depth) {
        List<Move> moves = move_generator.GenerateLegalMoves(board);

        Move highest_mv = new Move();
        double highest_eval = board.turn ? -999 : 999;
        double eval;
        

        foreach(Move move in moves) {
            board.move(move); // Flip turn

            if (depth == max_depth) {
                eval = eval_cl.EvaluateBoard(board);
            }
            else {
                (Move x, double y) = minimax_dr(board, depth+1, max_depth);
                eval = y;
            }

            board.undo_move(); // Flip Turn back


            if (stopwatch.ElapsedMilliseconds > allocated_movetime) {
                return (new Move(), -1001);
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
