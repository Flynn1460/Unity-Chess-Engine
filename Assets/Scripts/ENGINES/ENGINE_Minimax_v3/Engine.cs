using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ENGINE_NAMESPACE_Minimax_V3 {
public class MinimaxEngine {
    
    private MoveGenerator move_generator = new MoveGenerator();
    
    private Transposition trs = new Transposition();
    private Eval eval_cl = new Eval();

    private Stopwatch stopwatch = new Stopwatch();
    private int allocated_movetime = 2000000000;


    public Move Get_SetMove(Board board, int set_depth) {
        stopwatch = new Stopwatch();
        stopwatch.Start();

        trs.CLEAR_HASH_TABLE();

        (Move returned_mv, double eval, List<Move> ordered_moves_) = top_minimax(board, set_depth-1, -999, +999);

        UnityEngine.Debug.Log($"E3: Max Depth of {set_depth} reached in: {stopwatch.ElapsedMilliseconds}ms");
        return returned_mv;
    }

    public Move Get_TimeMove(Board board, int movetime) {
        List<Move> ordered_moves = null;

        stopwatch = new Stopwatch();
        stopwatch.Start();
        
        allocated_movetime = movetime;

        Move best_move = new Move();

        int max_depth = 1;
        double prev_eval = 0;



        while (true) {
            trs.CLEAR_HASH_TABLE();
            
            (Move returned_mv, double eval, List<Move> returned_ordered_moves) = top_minimax(board, max_depth-1, -999, +999, last_eval_best_moves:ordered_moves);

            if (eval == -1001) {
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

          
    public (Move, double, List<Move>) top_minimax(Board board, int depth, double alpha, double beta, List<Move> last_eval_best_moves=null) {        
        List<Move> moves = move_generator.GenerateLegalMoves(board);

        if (last_eval_best_moves != null) {
            moves = last_eval_best_moves.Intersect(moves).Concat(moves.Except(last_eval_best_moves)).ToList();
        }
        else {
            moves = OrderMoves(moves, board.turn); 
        }

        List<(Move move, double eval)> moveEvals = new List<(Move, double)>();

        double highest_eval = board.turn ? -1000 : +1000;
        double eval;

        Move highest_mv = new Move();

        foreach(Move move in moves) {
            board.move(move);

            if (depth == 0) {
                eval = eval_cl.EvaluateBoard(board);
            }
            else if (move_generator.isGM(board)) {
                eval = eval_cl.EvaluateBoard(board);
            }
            else {
                eval = minimax(board, depth-1, alpha, beta);
            }

            board.undo_move();


            if ((eval > highest_eval && board.turn) || (eval < highest_eval && !board.turn)) {
                highest_eval = eval;
                highest_mv = move;

                if (board.turn)  {  alpha = Math.Max(alpha, eval);  }
                else  {  beta = Math.Min(beta, eval);  }
                
                if (beta <= alpha) break;
            }

            moveEvals.Add((move, eval));

            if (stopwatch.ElapsedMilliseconds > allocated_movetime) {
                return (null, -1001, null);
            }
        }

        if (board.turn) moveEvals.Sort((a, b) => b.eval.CompareTo(a.eval));
        if (!board.turn) moveEvals.Sort((a, b) => a.eval.CompareTo(b.eval));

        return (highest_mv, highest_eval, moveEvals.Select(x => x.move).ToList());
    }

    public double minimax(Board board, int depth, double alpha, double beta) {        
        double searched_eval = trs.FIND_HASH(board.copy());

        if (searched_eval != -1002) return searched_eval;

        List<Move> moves = move_generator.GenerateLegalMoves(board);
        if (depth > 1){  moves = OrderMoves(moves, board.turn); }


        double highest_eval = board.turn ? -1000 : +1000;
        double eval;

        if (stopwatch.ElapsedMilliseconds > allocated_movetime) {
            return -1001;
        }

        foreach(Move move in moves) {
            board.move(move);

            if (depth == 0) {
                eval = eval_cl.EvaluateBoard(board);
            }
            else if (move_generator.isGM(board)) {
                eval = eval_cl.EvaluateBoard(board);
            }
            else {
                double y = minimax(board, depth-1, alpha, beta);
                eval = y;
            }

            board.undo_move();


            if ((eval > highest_eval && board.turn) || (eval < highest_eval && !board.turn)) {
                highest_eval = eval;

                if (board.turn)  {  alpha = Math.Max(alpha, eval);  }
                else  {  beta = Math.Min(beta, eval);  }
                
                if (beta <= alpha) break;
            }
        }
        
        trs.MAKE_HASH(board, highest_eval);

        return highest_eval;
    }


    
    public List<Move> OrderMoves(List<Move> moves, bool turn) {
        List<(Move mv, float diff)> ordered_moves = new List<(Move mv, float diff)>();

        foreach(Move move in moves) {
            ordered_moves.Add((move, move.end_square.GetPieceType()-(move.start_square.GetPieceType()/3)));
        }

        if  (turn)  ordered_moves = ordered_moves.OrderBy(x => x.diff).ToList();
        if (!turn)  ordered_moves = ordered_moves.OrderByDescending(x => x.diff).ToList();
        

        return ordered_moves.Select(x => x.mv).ToList();
    }
}
}
