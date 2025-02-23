using System;
using System.Collections.Generic;
using UnityEngine;

namespace ENGINE_NAMESPACE_Random {
public class Random_Engine {
    
    private MoveGenerator move_generator = new MoveGenerator();
    private System.Random random = new System.Random();


    public Move Get_Random_Move(Board board, int movetime) {
        List<Move> legal_moves = move_generator.GenerateLegalMoves(board);

        int random_index = random.Next(legal_moves.Count);
        Move random_move = legal_moves[random_index];

        return random_move;
    }
}
}
