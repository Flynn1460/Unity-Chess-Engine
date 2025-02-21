using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace ENGINE_NAMESPACE_Minimax_1 {
public class MinimaxEngine {
    
    private MoveGenerator move_generator = new MoveGenerator();
    private Eval eval_cl = new Eval();

    private Stopwatch stopwatch;

    private int allocated_movetime = 0;

    public Move Get_Move(Board board, int movetime) {
        stopwatch = new Stopwatch();
        stopwatch.Start();
        allocated_movetime = movetime;

        Move best_move = new Move();
        int max_depth = 0;

        while (true) {
            max_depth++;

            (Move new_depth_move, double move_eval) = minimax(board, 1, max_depth);

            if (move_eval == -1000) { // Time Escape Code
                return best_move;
            }
            else {
                // UnityEngine.Debug.Log("Completed Depth: "+max_depth+", "+stopwatch.ElapsedMilliseconds);
                best_move = new_depth_move.copy();
            }
        }
    }

    public (Move, double) minimax(Board board, int depth, int max_depth) {
        double eval;
        double highest_eval = board.turn ? -999 : 999;

        List<Move> legal_moves = move_generator.GenerateLegalMoves(board);

        if (legal_moves.Count == 0) {
            int cm_state = move_generator.isCheckmate(board);
            if (cm_state != 0) {
                return (new Move(), cm_state * 998); // Assign win/loss value
            }
            return (new Move(), 0); // Return draw score
        }

        Move best_move = legal_moves[0]; // Now it's safe
        
        foreach(Move move in legal_moves) {
            board.move(move);
            // UnityEngine.Debug.Log(move + " - " + depth + ", " + max_depth + ", "+stopwatch.ElapsedMilliseconds+", "+allocated_movetime);

            if (stopwatch.ElapsedMilliseconds > allocated_movetime) {
                board.undo_move();
                return (new Move(), -1000);
            }

            bool draw_state = move_generator.isDraw(board);
            int cm_state = move_generator.isCheckmate(board); // -1 for white checkmated: +1 for black checkmated


            if (cm_state != 0) {
                eval = cm_state * 998;
            }
            else if (draw_state) {
                eval = 0;
            }
            else if (depth == max_depth) {
                eval = eval_cl.EvaluateBoard(board);
            }
            else {
                (Move best_mv, double x) = minimax(board, depth+1, max_depth);

                if (stopwatch.ElapsedMilliseconds > allocated_movetime) {
                    board.undo_move();
                    return (new Move(), -1000);
                }

                eval = x;
            }


            if((eval > highest_eval && !board.turn) || (eval < highest_eval && board.turn)) {
                highest_eval = eval;
                best_move = move;
            }

            board.undo_move();
        }

        return (best_move, highest_eval);
    }




    public Move Get_Move2(Board board, int movetime) {
        (Move returned_mv, double eval) = minimax_d2(board, 1, 1);

        Thread.Sleep(movetime);

        return returned_mv;
    }

    public (Move, double) minimax_d2(Board board, int depth, int max_depth) {
        List<Move> moves = move_generator.GenerateLegalMoves(board);

        Move highest_mv1 = new Move();
        double highest_eval1 = board.turn ? -999 : 999;
        

        foreach(Move move in moves) {
            board.move(move); // Flip Turn

            List<Move> moves2 = move_generator.GenerateLegalMoves(board);

            Move highest_mv2 = new Move();
            double highest_eval2 = board.turn ? -999 : 999;


            foreach(Move move2 in moves2) {
                board.move(move2);

                double eval2 = eval_cl.EvaluateBoard(board);

                board.undo_move();


                if ((eval2 < highest_eval2 && !board.turn) || (eval2 > highest_eval2 && board.turn)) {
                    // UnityEngine.Debug.Log($"2.({board.turn})  {highest_mv2} {highest_eval2} => {move2} {eval2}");
                    highest_eval2 = eval2;
                    highest_mv2 = move2.copy();
                }

            }


            board.undo_move(); // Flip Turn back


            if ((highest_eval2 > highest_eval1 && board.turn)  || (highest_eval2 < highest_eval1 && !board.turn)) {
                // UnityEngine.Debug.Log($"1.({board.turn})  {highest_mv1} {highest_eval1} => {move} {highest_eval2}");
                highest_eval1 = highest_eval2;
                highest_mv1 = move;
            }
        }

        return (highest_mv1, highest_eval1);
    }
}
}
