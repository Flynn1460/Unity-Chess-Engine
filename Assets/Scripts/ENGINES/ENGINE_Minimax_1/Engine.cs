using System.Collections.Generic;
using UnityEngine;

namespace ENGINE_NAMESPACE_Minimax_1 {

public class MinimaxEngine {
    
    private MoveGenerator move_generator = new MoveGenerator();
    private System.Random random = new System.Random();
    private Eval eval_cl = new Eval();


    public Move Get_Move(Board board, int movetime) {
        List<Move> legal_moves = move_generator.GenerateLegalMoves(board);
        int highest_eval = board.turn ? -999 : 999;
        Move best_move = legal_moves[0];

        foreach(Move move in legal_moves) {
            board.move(move, flip_turn:false);

            int eval = eval_cl.EvaluateBoard(board);
            Debug.Log(move + ", " + eval + ", " + highest_eval);

            if((eval > highest_eval && board.turn) || (eval < highest_eval && !board.turn)) {
                highest_eval = eval;
                best_move = move;
            }

            board.undo_move(flip_turn:false);
        }

        Debug.Log(best_move + " : " + highest_eval);

        return best_move;
    }
}
}
