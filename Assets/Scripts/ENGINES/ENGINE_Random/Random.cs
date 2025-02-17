using System;

namespace ENGINE_NAMESPACE_Random {
public class ENGINE_Random {
    /* 
    Name       :    ENGINE_Random
    Engine ID  :    2
    */
    private Random_Engine random_Engine = new Random_Engine();


    public String GET_MOVE(Board cpy_board, int movetime) {
        Move move = random_Engine.Get_Random_Move(cpy_board, movetime);
        return move.str_uci();
    }
}
}