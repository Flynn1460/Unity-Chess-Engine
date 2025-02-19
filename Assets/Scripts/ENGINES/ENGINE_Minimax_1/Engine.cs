using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ENGINE_NAMESPACE_Minimax_1 {
public class MinimaxEngine {
    
    private MoveGenerator move_generator = new MoveGenerator();
    private Eval eval_cl = new Eval();


    public Move Get_Move(Board board, int movetime) {
        List<Move> legal_moves = move_generator.GenerateLegalMoves(board);
        // UnityEngine.Debug.Log(String.Join(" ", legal_moves));
        int highest_eval = board.turn ? -999 : 999;
        Move best_move = legal_moves[0];

        foreach(Move move in legal_moves) {
            board.move(move);

            int eval = eval_cl.EvaluateBoard(board);

            if((eval > highest_eval && !board.turn) || (eval < highest_eval && board.turn)) {
                highest_eval = eval;
                best_move = move;
            }

            board.undo_move();
        }

        return best_move;
    }
}
}
