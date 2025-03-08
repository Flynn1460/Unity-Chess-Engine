using System;

namespace ENGINE_NAMESPACE_Minimax_V4 {
public class ENGINE_Minimax_V4 {
    /* 
    Name       :    ENGINE_Minimax_V4
    Engine ID  :    6
    */
    private MinimaxEngine minimax = new MinimaxEngine();

    public String GET_MOVE(Board cpy_board, int movetime) {
        Move move = minimax.Get_TimeMove(cpy_board, movetime);

        // 15895ms

        return move.str_uci();
    }
}
}