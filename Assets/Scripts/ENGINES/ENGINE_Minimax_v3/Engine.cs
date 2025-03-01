using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ENGINE_NAMESPACE_Minimax_V3 {
public class MinimaxEngine {
    
    private MoveGenerator move_generator = new MoveGenerator();
    private Eval eval_cl = new Eval();

    private Stopwatch stopwatch = new Stopwatch();
    private int allocated_movetime = 2000000000;


    public Move Get_SetMove(Board board, int set_depth) {
        stopwatch = new Stopwatch();
        stopwatch.Start();

        (Move returned_mv, double eval, List<Move> ordered_moves_) = minimax(board, set_depth-1, -999, +999, is_top_depth:true);

        UnityEngine.Debug.Log($"E3: Max Depth of {set_depth} reached in: {stopwatch.ElapsedMilliseconds}ms");
        return returned_mv;
    }

    public Move Get_TimeMove(Board board, int movetime) {
        List<Move> ordered_moves = null;
        allocated_movetime = movetime;

        stopwatch = new Stopwatch();
        stopwatch.Start();

        Move best_move = new Move();

        int max_depth = 1;
        double prev_eval = 0;


        while (true) {
            (Move returned_mv, double eval, List<Move> returned_ordered_moves) = minimax(board, (max_depth-1), -999, +999, is_top_depth:true, last_eval_best_moves:ordered_moves);

            if (eval == -1001) {
                // UnityEngine.Debug.Log($"3.1  :  Depth: {max_depth-1}    {best_move}    {prev_eval}");
                return best_move;
            }
            else if (eval == 900 || eval == -900) {
                return returned_mv;
            }

            ordered_moves = returned_ordered_moves;

            best_move = returned_mv.copy();
            prev_eval = eval;
            max_depth++;
        }
    }



          
    public (Move, double, List<Move>) minimax(Board board, int depth, double alpha, double beta, bool is_top_depth=false, List<Move> last_eval_best_moves=null) {        
                
        
        List<Move> moves = move_generator.GenerateLegalMoves(board);
        if (last_eval_best_moves != null) {
            moves = last_eval_best_moves.Intersect(moves).Concat(moves.Except(last_eval_best_moves)).ToList();
        }
        else if (depth > 1){
            moves = OrderMoves(moves, board.turn); 
        }


        List<(Move move, double eval)> moveEvals = new List<(Move, double)>();

        double highest_eval = board.turn ? -1000 : +1000;
        double eval;

        Move highest_mv = new Move();


        foreach(Move move in moves) {
            board.move(move);

            if (move_generator.isGM(board) || depth == 0) {
                eval = eval_cl.EvaluateBoard(board);
            }
            else {
                (Move x, double y, List<Move> z) = minimax(board, depth-1, alpha, beta);
                eval = y;
            }

            board.undo_move();



            if ((eval > highest_eval && board.turn) || (eval < highest_eval && !board.turn)) {
                highest_eval = eval;
                highest_mv = move;

                if (board.turn)  {  alpha = Math.Max(alpha, eval);  }
                else  {  beta = Math.Min(beta, eval);  }
                
                if (beta <= alpha) break;
            }

            if (is_top_depth) moveEvals.Add((move, eval));
            
            if (stopwatch.ElapsedMilliseconds > allocated_movetime) {
                return (null, -1001, null);
            }
        }

        if (is_top_depth){
            if (board.turn) moveEvals.Sort((a, b) => b.eval.CompareTo(a.eval));
            if (!board.turn) moveEvals.Sort((a, b) => a.eval.CompareTo(b.eval));
            return (highest_mv, highest_eval, moveEvals.Select(x => x.move).ToList());
        }  


        return (highest_mv, highest_eval, null);
    }


    
    public List<Move> OrderMoves(List<Move> moves, bool turn) {
        List<(Move mv, int diff)> ordered_moves = new List<(Move mv, int diff)>();

        foreach(Move move in moves) {
            ordered_moves.Add((move, move.end_square.piece_type));
        }

        if  (turn)  ordered_moves = ordered_moves.OrderBy(x => x.diff).ToList();
        if (!turn)  ordered_moves = ordered_moves.OrderByDescending(x => x.diff).ToList();
        

        return ordered_moves.Select(x => x.mv).ToList();
    }
}
}
