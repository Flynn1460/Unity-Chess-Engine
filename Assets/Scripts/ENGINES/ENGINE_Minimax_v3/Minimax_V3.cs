using System;

namespace ENGINE_NAMESPACE_Minimax_V3 {
public class ENGINE_Minimax_V3 {
    /* 
    Name       :    ENGINE_Minimax_V3
    Engine ID  :    5
    */
    private MinimaxEngine minimax = new MinimaxEngine();


    public String GET_MOVE(Board cpy_board, int movetime) {
        // Move move5 = minimax.Get_SetMove(cpy_board, 1);
        // Move move4 = minimax.Get_SetMove(cpy_board, 2);
        // Move move3 = minimax.Get_SetMove(cpy_board, 3);
        // Move move2 = minimax.Get_SetMove(cpy_board, 4);
        // Move move1 = minimax.Get_SetMove(cpy_board, 5);
        // Move move = minimax.Get_SetMove(cpy_board, 6);
        Move move = minimax.Get_TimeMove(cpy_board, movetime);

        // 1ply : 9ms
        // 2ply : 10ms
        // 3ply : 85ms
        // 4ply : 739ms
        // 5ply : 4,750ms
        // 6ply : 66,675ms

        // 1ply : 8ms
        // 2ply : 7ms
        // 3ply : 56ms
        // 4ply : 532ms
        // 5ply : 3,109ms
        // 6ply : 56,158ms

        return move.str_uci();
    }
}
}