using System;

namespace ENGINE_NAMESPACE_Minimax_V3 {
public class ENGINE_Minimax_V3 {
    /* 
    Name       :    ENGINE_Minimax_V3
    Engine ID  :    5
    */
    private MinimaxEngine minimax = new MinimaxEngine();


    public String GET_MOVE(Board cpy_board, int movetime) {
        Move move = minimax.Get_SetMove(cpy_board, 4);

        return move.str_uci();
    }
}
}