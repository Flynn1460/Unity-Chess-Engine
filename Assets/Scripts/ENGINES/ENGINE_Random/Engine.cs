
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ENGINE_NAMESPACE_Random {

public class Random_Engine {
    
    private MoveGenerator move_generator = new MoveGenerator();
    private System.Random random = new System.Random();


    public Move Get_Random_Move(Board board, int movetime) {
        List<String> legal_moves = move_generator.GenerateLegalMoves(board);

        Debug.Log(String.Join(" ", legal_moves));

        int random_index = random.Next(legal_moves.Count);
        String random_move = legal_moves[random_index];


        Move move = new Move(board, random_move);

        return move;
    }
}
}