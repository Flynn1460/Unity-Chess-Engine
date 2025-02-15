using System;

namespace ENGINE_NAMESPACE_Minimax_1 {
public class ENGINE_Minimax_1 {
    /* 
    Name       :    ENGINE_Minimax_1
    Engine ID  :    3
    */
    private MinimaxEngine minimax = new MinimaxEngine();


    public String GET_MOVE(Board cpy_board, int movetime) {
        Move move = minimax.Get_Move(cpy_board, movetime);

        return move.str_uci();
    }
}
}