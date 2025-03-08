using System;

namespace ENGINE_NAMESPACE_Minimax_V4 {

public class Eval {
    private Random rn = new Random();
    private NewMoveGenerator mg = new NewMoveGenerator();

    private float value_multiplier = 1f;
    private PieceMaps pieceMaps;

    public Eval() { 
        pieceMaps = new PieceMaps(value_multiplier);
    }


    public double EvaluateBoard(Board board) {
        double eval_bias;

        bool draw_state = mg.isDraw(board);
        int checkmate_state = mg.isCheckmate(board);

        if (checkmate_state == 0 && !draw_state) {
            eval_bias = PieceSum(board);
        
            bool is_check = mg.isCheck(board);
            if (is_check) {
                eval_bias += board.turn ? -2f : +2f;
            }
        
        }
        else if (checkmate_state == 1 ) {
            eval_bias = +900;
        }
        else if (checkmate_state == -1) {
            eval_bias = -900;
        }
        else {
            eval_bias = 0;
        }

        // eval_bias += (rn.NextDouble() * 0.1f) - 0.1f;
        return Math.Round(eval_bias, 2);
    }

    public double PieceSum(Board board) {
        double eval_bias = 0;

        for (int row=0; row<8; row++) {
            for (int col=0; col<8; col++) {
                eval_bias += GetSqVal(board.b, row, col);
            }
        }

        return eval_bias;
    }

    public double GetSqVal(int[,] board, int row, int col) {
        int piece = board[row, col];

        if (piece == 0) return 0;


        if (piece == 1) {
            return 1 + (pieceMaps.white_pawn_map_early[row, col] * value_multiplier);
        }
        if (piece == 8) {
            return -(1 + (pieceMaps.black_pawn_map_early[row, col] * value_multiplier));
        }


        if (piece == 2) {
            return 5 + (pieceMaps.white_rook_map_early[row, col] * value_multiplier);
        }
        if (piece == 9) {
            return -(5 + (pieceMaps.black_rook_map_early[row, col] * value_multiplier));
        }


        if (piece == 3) {
            return 3.5 + (pieceMaps.white_knight_map_early[row, col] * value_multiplier);
        }
        if (piece == 10) {
            return -(3.5 + (pieceMaps.black_knight_map_early[row, col] * value_multiplier));
        }


        if (piece == 4) {
            return 3.3 + (pieceMaps.white_bishop_map_early[row, col] * value_multiplier);
        }
        if (piece == 11) {
            return -(3.3 + (pieceMaps.black_bishop_map_early[row, col] * value_multiplier));
        }


        if (piece == 5) {
            return 9 + (pieceMaps.white_queen_map_early[row, col] * value_multiplier);
        }
        if (piece == 12) {
            return -(9 + (pieceMaps.black_queen_map_early[row, col] * value_multiplier));
        }


        if (piece == 6) {
            return pieceMaps.white_king_map_early[row, col] * value_multiplier;
        }
        if (piece == 13) {
            return -(pieceMaps.black_king_map_early[row, col] * value_multiplier);
        }


        return 0;
    }
}




public class PieceMaps {
    public float multiplier;

    public PieceMaps (float out_multipler) {
        multiplier = out_multipler;
    }

    readonly public float[,] white_pawn_map_early = new float [8,8] { 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f},
        {+0.0f, -0.0f, -0.3f, -0.4f, -0.4f, -0.3f, +0.0f, +0.0f},
        {-0.2f, +0.1f, +0.4f, +0.2f, +0.2f, +0.4f, +0.1f, -0.2f},
        {-0.4f, +0.0f, +0.1f, +0.6f, +0.6f, +0.1f, +0.0f, -0.4f},
        {-0.3f, -0.2f, +0.1f, +0.3f, +0.3f, +0.1f, -0.2f, -0.3f},
        {-0.3f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, -0.3f},
        {+1.2f, +1.0f, +0.8f, +0.6f, +0.6f, +0.8f, +1.0f, +1.2f},
        {+5.0f, +5.0f, +5.0f, +5.0f, +5.0f, +5.0f, +5.0f, +5.0f}
    };

    readonly public float[,] white_bishop_map_early = new float [8,8] { 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f},
        {+0.2f, +0.2f, -0.3f, -0.4f, -0.4f, -0.3f, +0.2f, +0.2f},
        {+0.3f, +0.4f, +0.1f, +0.2f, +0.2f, +0.1f, +0.4f, +0.3f},
        {+0.0f, +0.0f, +0.5f, +0.3f, +0.3f, +0.5f, +0.0f, +0.0f},
        {-0.3f, -0.2f, +0.1f, +0.3f, +0.3f, +0.1f, -0.2f, -0.3f},
        {-0.3f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, -0.3f},
        {+0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f},
        {+0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f}
    };

    readonly public float[,] white_queen_map_early = new float [8,8] { 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f},
        {+0.2f, +0.2f, -0.3f, -0.4f, -0.4f, -0.3f, +0.2f, +0.2f},
        {+0.3f, +0.4f, +0.1f, +0.2f, +0.2f, +0.1f, +0.4f, +0.3f},
        {+0.0f, +0.0f, +0.5f, +0.3f, +0.3f, +0.5f, +0.0f, +0.0f},
        {-0.3f, -0.2f, +0.1f, +0.3f, +0.3f, +0.1f, -0.2f, -0.3f},
        {-0.3f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, -0.3f},
        {+0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f},
        {+0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f}
    };

    readonly public float[,] white_knight_map_early = new float [8,8] { 
        {+0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f},
        {+0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f},
        {-0.4f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, -0.4f},
        {-0.4f, +0.2f, +0.1f, +0.3f, +0.3f, +0.1f, +0.2f, -0.4f},
        {-0.6f, +0.0f, +0.5f, +0.3f, +0.3f, +0.5f, +0.0f, -0.6f},
        {-0.6f, +0.4f, +0.5f, +0.2f, +0.2f, +0.5f, +0.4f, -0.6f},
        {-0.2f, -0.2f, -0.3f, -0.4f, -0.4f, -0.3f, -0.2f, -0.2f},
        {-1.0f, -1.6f, -1.0f, -1.0f, -1.0f, -1.0f, -1.6f, -1.0f}
    };

    readonly public float[,] white_king_map_early = new float [8,8] { 
        {+0.8f, +0.3f, -1.0f, -1.0f, -1.0f, -1.0f, +0.3f, +0.8f }, 
        {+1.0f, +0.8f, +0.8f, -0.5f, -0.3f, -0.5f, +0.8f, +1.0f },
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f },
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }, 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }, 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }, 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }, 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }
    };

    readonly public float[,] white_rook_map_early = new float [8,8] { 
        {+1.0f, -0.3f, +0.8f, +0.5f, +0.3f, +0.5f, -0.3f, +1.0f },
        {+0.1f, +0.1f, +0.0f, +0.0f, +0.0f, +0.0f, +0.1f, +0.1f }, 
        {-0.2f, -0.2f, -0.1f, +0.0f, +0.0f, -0.1f, -0.2f, -0.2f }, 
        {-0.5f, -0.5f, -0.3f, -0.2f, -0.2f, -0.3f, -0.5f, -0.5f }, 
        {-0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f },
        {-0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f }, 
        {-0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f }, 
        {-0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f }, 
    };


    readonly public float[,] black_pawn_map_early = new float [8,8] { 
        {+5.0f, +5.0f, +5.0f, +5.0f, +5.0f, +5.0f, +5.0f, +5.0f},
        {+1.2f, +1.0f, +0.8f, +0.6f, +0.6f, +0.8f, +1.0f, +1.2f},
        {-0.3f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, -0.3f},
        {-0.3f, -0.2f, +0.1f, +0.3f, +0.3f, +0.1f, -0.2f, -0.3f},
        {-0.4f, +0.0f, +0.1f, +0.6f, +0.6f, +0.1f, +0.0f, -0.4f},
        {-0.2f, +0.1f, +0.4f, +0.2f, +0.2f, +0.4f, +0.1f, -0.2f},
        {+0.0f, -0.0f, -0.3f, -0.4f, -0.4f, -0.3f, +0.0f, +0.0f},
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f}
    };

    readonly public float[,] black_bishop_map_early = new float [8,8] { 
        {+0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f},
        {+0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f},
        {-0.3f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, -0.3f},
        {-0.3f, -0.2f, +0.1f, +0.3f, +0.3f, +0.1f, -0.2f, -0.3f},
        {+0.0f, +0.0f, +0.5f, +0.3f, +0.3f, +0.5f, +0.0f, +0.0f},
        {+0.3f, +0.4f, +0.1f, +0.2f, +0.2f, +0.1f, +0.4f, +0.3f},
        {+0.2f, +0.2f, -0.3f, -0.4f, -0.4f, -0.3f, +0.2f, +0.2f},
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f}
    };

    readonly public float[,] black_queen_map_early = new float [8,8] { 
        {+0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f},
        {+0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f},
        {-0.3f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, -0.3f},
        {-0.3f, -0.2f, +0.1f, +0.3f, +0.3f, +0.1f, -0.2f, -0.3f},
        {+0.0f, +0.0f, +0.5f, +0.3f, +0.3f, +0.5f, +0.0f, +0.0f},
        {+0.3f, +0.4f, +0.1f, +0.2f, +0.2f, +0.1f, +0.4f, +0.3f},
        {+0.2f, +0.2f, -0.3f, -0.4f, -0.4f, -0.3f, +0.2f, +0.2f},
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f}
    };

    readonly public float[,] black_knight_map_early = new float [8,8] { 
        {+0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f},
        {+0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f},
        {-0.4f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, -0.4f},
        {-0.4f, +0.2f, +0.1f, +0.3f, +0.3f, +0.1f, +0.2f, -0.4f},
        {-0.6f, +0.0f, +0.5f, +0.3f, +0.3f, +0.5f, +0.0f, -0.6f},
        {-0.6f, +0.4f, +0.5f, +0.2f, +0.2f, +0.5f, +0.4f, -0.6f},
        {-0.2f, -0.2f, -0.3f, -0.4f, -0.4f, -0.3f, -0.2f, -0.2f},
        {-1.0f, -1.6f, -1.0f, -1.0f, -1.0f, -1.0f, -1.6f, -1.0f}
    };

    readonly public float[,] black_king_map_early = new float [8,8] { 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f },
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }, 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }, 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }, 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }, 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }, 
        {+0.8f, +0.3f, -1.0f, -1.0f, -1.0f, -1.0f, +0.3f, +0.8f }, 
        {+1.0f, +0.8f, +0.8f, -0.5f, -0.3f, -0.5f, +0.8f, +1.0f } 
    };

    readonly public float[,] black_rook_map_early = new float [8,8] { 
        {-0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f },
        {-0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f }, 
        {-0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f }, 
        {-0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f }, 
        {-0.5f, -0.5f, -0.3f, -0.2f, -0.2f, -0.3f, -0.5f, -0.5f }, 
        {-0.2f, -0.2f, -0.1f, +0.0f, +0.0f, -0.1f, -0.2f, -0.2f }, 
        {+0.1f, +0.1f, +0.0f, +0.0f, +0.0f, +0.0f, +0.1f, +0.1f }, 
        {+1.0f, -0.3f, +0.8f, +0.5f, +0.3f, +0.5f, -0.3f, +1.0f }
    };
}   
}
