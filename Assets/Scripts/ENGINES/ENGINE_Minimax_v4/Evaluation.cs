using System;

namespace ENGINE_NAMESPACE_Minimax_V4 {

public class Eval {
    private Random rn = new Random();
    private MoveGenerator mg = new MoveGenerator();

    private float value_multiplier = 0.02f;
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

        eval_bias += (rn.NextDouble() * 0.02f) - 0.02f;
        return Math.Round(eval_bias, 2);
    }

    public double PieceSum(Board board) {
        double eval_bias = 0;

        for (int row=0; row<8; row++) {
            for (int col=0; col<8; col++) {
                eval_bias += GetSqVal(board.b, row, col, 1, 1);
            }
        }

        return eval_bias;
    }

    public double GetSqVal(int[,] board, int row, int col, float white_early_late_bias, float black_early_late_bias) {
        int piece = board[row, col];

        if (piece == 0) return 0;


        if (piece == 1) {
            return 1 + (pieceMaps.white_pawn_map_early[row, col] * value_multiplier * white_early_late_bias);
        }
        if (piece == 8) {
            return -(1 + (pieceMaps.black_pawn_map_early[row, col] * value_multiplier * black_early_late_bias));
        }


        if (piece == 2) {
            return 5 + (pieceMaps.white_rook_map_early[row, col] * value_multiplier * white_early_late_bias);
        }
        if (piece == 9) {
            return -(5 + (pieceMaps.black_rook_map_early[row, col] * value_multiplier * black_early_late_bias));
        }


        if (piece == 3) {
            return 3.5 + (pieceMaps.white_knight_map_early[row, col] * value_multiplier * white_early_late_bias);
        }
        if (piece == 10) {
            return -(3.5 + (pieceMaps.black_knight_map_early[row, col] * value_multiplier * black_early_late_bias));
        }


        if (piece == 4) {
            return 3.3 + (pieceMaps.white_bishop_map_early[row, col] * value_multiplier * white_early_late_bias);
        }
        if (piece == 11) {
            return -(3.3 + (pieceMaps.black_bishop_map_early[row, col] * value_multiplier * black_early_late_bias));
        }


        if (piece == 5) {
            return 9 + (pieceMaps.white_queen_map_early[row, col] * value_multiplier * white_early_late_bias);
        }
        if (piece == 12) {
            return -(9 + (pieceMaps.black_queen_map_early[row, col] * value_multiplier * black_early_late_bias));
        }


        if (piece == 6) {
            return (pieceMaps.white_king_map_early[row, col] * value_multiplier * white_early_late_bias);
        }
        if (piece == 13) {
            return -((pieceMaps.black_king_map_early[row, col] * value_multiplier * black_early_late_bias));
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
            {0,  0,  0,  0,  0,  0,  0,  0},
            {50, 50, 50, 50, 50, 50, 50, 50},
            {10, 10, 20, 30, 30, 20, 10, 10},
            {5,  5, 10, 25, 25, 10,  5,  5},
            {0,  0,  0, 20, 20,  0,  0,  0},
            {5, -5,-10,  0,  0,-10, -5,  5},
            {5, 10, 10,-20,-20, 10, 10,  5},
            {0,  0,  0,  0,  0,  0,  0,  0}
    };

    readonly public float[,] white_bishop_map_early = new float [8,8] { 
            {-20,-10,-10,-10,-10,-10,-10,-20},
            {-10,  0,  0,  0,  0,  0,  0,-10},
            {-10,  0,  5, 10, 10,  5,  0,-10},
            {-10,  5,  5, 10, 10,  5,  5,-10},
            {-10,  0, 10, 10, 10, 10,  0,-10},
            {-10, 10, 10, 10, 10, 10, 10,-10},
            {-10,  5,  0,  0,  0,  0,  5,-10},
            {-20,-10,-10,-10,-10,-10,-10,-20},
    };

    readonly public float[,] white_queen_map_early = new float [8,8] { 
            {-20,-10,-10, -5, -5,-10,-10,-20},
            {-10,  0,  0,  0,  0,  0,  0,-10},
            {-10,  0,  5,  5,  5,  5,  0,-10},
            {-5,  0,  5,  5,  5,  5,  0, -5},
            {+0,  0,  5,  5,  5,  5,  0, -5},
            {-10,  5,  5,  5,  5,  5,  0,-10},
            {-10,  0,  5,  0,  0,  0,  0,-10},
            {-20,-10,-10, -5, -5,-10,-10,-20}
    };

    readonly public float[,] white_knight_map_early = new float [8,8] { 
            {-50,-40,-30,-30,-30,-30,-40,-50},
            {-40,-20,  0,  0,  0,  0,-20,-40},
            {-30,  0, 10, 15, 15, 10,  0,-30},
            {-30,  5, 15, 20, 20, 15,  5,-30},
            {-30,  0, 15, 20, 20, 15,  0,-30},
            {-30,  5, 10, 15, 15, 10,  5,-30},
            {-40,-20,  0,  5,  5,  0,-20,-40},
            {-50,-40,-30,-30,-30,-30,-40,-50}
    };

    readonly public float[,] white_king_map_early = new float [8,8] { 
            {-30,-40,-40,-50,-50,-40,-40,-30},
            {-30,-40,-40,-50,-50,-40,-40,-30},
            {-30,-40,-40,-50,-50,-40,-40,-30},
            {-30,-40,-40,-50,-50,-40,-40,-30},
            {-20,-30,-30,-40,-40,-30,-30,-20},
            {-10,-20,-20,-20,-20,-20,-20,-10},
            {20, 20,  0,  0,  0,  0, 20, 20},
            {20, 30, 10,  0,  0, 10, 30, 20}
    };

    readonly public float[,] white_rook_map_early = new float [8,8] { 
            {0, -5,  0,  0,  0,  0,  -5, 0},
            {5, 10, 10, 10, 10, 10, 10,  5},
            {-5,  0,  0,  0,  0,  0,  0, -5},
            {-5,  0,  0,  0,  0,  0,  0, -5},
            {-5,  0,  0,  0,  0,  0,  0, -5},
            {-5,  0,  0,  0,  0,  0,  0, -5},
            {-5,  0,  0,  0,  0,  0,  0, -5},
            {0,  0,  0,  5,  5,  0,  0,  0}
    };


        readonly public float[,] black_pawn_map_early = new float [8,8] { 
            {0,  0,  0,  0,  0,  0,  0,  0},
            {5, 10, 10,-20,-20, 10, 10,  5},
            {5, -5,-10,  0,  0,-10, -5,  5},
            {0,  0,  0, 20, 20,  0,  0,  0},
            {5,  5, 10, 25, 25, 10,  5,  5},
            {10, 10, 20, 30, 30, 20, 10, 10},
            {50, 50, 50, 50, 50, 50, 50, 50},
            {0,  0,  0,  0,  0,  0,  0,  0},
    };

    readonly public float[,] black_bishop_map_early = new float [8,8] { 
            {-20,-10,-10,-10,-10,-10,-10,-20},
            {-10,  5,  0,  0,  0,  0,  5,-10},
            {-10, 10, 10, 10, 10, 10, 10,-10},
            {-10,  0, 10, 10, 10, 10,  0,-10},
            {-10,  5,  5, 10, 10,  5,  5,-10},
            {-10,  0,  5, 10, 10,  5,  0,-10},
            {-10,  0,  0,  0,  0,  0,  0,-10},
            {-20,-10,-10,-10,-10,-10,-10,-20}
    };

    readonly public float[,] black_queen_map_early = new float [8,8] { 
            {-20,-10,-10, -5, -5,-10,-10,-20},
            {-10,  0,  5,  0,  0,  0,  0,-10},
            {-10,  5,  5,  5,  5,  5,  0,-10},
            {+0,  0,  5,  5,  5,  5,  0, -5},
            {-5,  0,  5,  5,  5,  5,  0, -5},
            {-10,  0,  5,  5,  5,  5,  0,-10},
            {-10,  0,  0,  0,  0,  0,  0,-10},
            {-20,-10,-10, -5, -5,-10,-10,-20}
    };

    readonly public float[,] black_knight_map_early = new float [8,8] { 
            {-50,-40,-30,-30,-30,-30,-40,-50},
            {-40,-20,  0,  5,  5,  0,-20,-40},
            {-30,  5, 10, 15, 15, 10,  5,-30},
            {-30,  0, 15, 20, 20, 15,  0,-30},
            {-30,  5, 15, 20, 20, 15,  5,-30},
            {-30,  0, 10, 15, 15, 10,  0,-30},
            {-40,-20,  0,  0,  0,  0,-20,-40},
            {-50,-40,-30,-30,-30,-30,-40,-50}
    };

    readonly public float[,] black_king_map_early = new float [8,8] { 
            {20, 30, 10,  0,  0, 10, 30, 20},
            {20, 20,  0,  0,  0,  0, 20, 20},
            {-10,-20,-20,-20,-20,-20,-20,-10},
            {-20,-30,-30,-40,-40,-30,-30,-20},
            {-30,-40,-40,-50,-50,-40,-40,-30},
            {-30,-40,-40,-50,-50,-40,-40,-30},
            {-30,-40,-40,-50,-50,-40,-40,-30},
            {-30,-40,-40,-50,-50,-40,-40,-30}
    };

    readonly public float[,] black_rook_map_early = new float [8,8] { 
            {0,  0,  0,  5,  5,  0,  0,  0},
            {-5,  0,  0,  0,  0,  0,  0, -5},
            {-5,  0,  0,  0,  0,  0,  0, -5},
            {-5,  0,  0,  0,  0,  0,  0, -5},
            {-5,  0,  0,  0,  0,  0,  0, -5},
            {-5,  0,  0,  0,  0,  0,  0, -5},
            {5, 10, 10, 10, 10, 10, 10,  5},
            {0,  -5,  0,  0,  0,  0,  -5,  0}
    };
}   
}
