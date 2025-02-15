

namespace ENGINE_NAMESPACE_Minimax_1 {
public class Eval {
    public int EvaluateBoard(Board board) {
        int eval_bias = 0;

        for (int row=0; row<8; row++) {
            for (int col=0; col<8; col++) {
                int piece = board.b[row, col];

                switch (piece) {
                    case 1 : eval_bias += 1; break;
                    case 2 : eval_bias += 5; break;
                    case 3 : eval_bias += 3; break;
                    case 4 : eval_bias += 3; break;
                    case 5 : eval_bias += 9; break;
                    case 6 : eval_bias += 100; break;

                    case 8 : eval_bias -= 1; break;
                    case 9 : eval_bias -= 5; break;
                    case 10: eval_bias -= 3; break;
                    case 11: eval_bias -= 3; break;
                    case 12: eval_bias -= 9; break;
                    case 13: eval_bias -= 100; break;
                }
            }
        }

        return eval_bias;
    }
}
}