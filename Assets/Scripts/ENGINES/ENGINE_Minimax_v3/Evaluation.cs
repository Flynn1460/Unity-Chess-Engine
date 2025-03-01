using System;
using System.Collections.Generic;

namespace ENGINE_NAMESPACE_Minimax_V3 {

public class Eval {
    private Random rn = new Random();
    private MoveGenerator mg = new MoveGenerator();
    private PieceMaps pieceMaps = new PieceMaps();

    private float value_multiplier = 0.2f;

    Dictionary<int, int> pieceValues = new Dictionary<int, int>
    {
        { 0, 0 },
        { 1, 1 },
        { 2, 5 },
        { 3, 3 },
        { 4, 3 },
        { 5, 9 },
        { 6, 0 },
        { 8, 1 },
        { 9, 5 },
        { 10, 3 },
        { 11, 3 },
        { 12, 9 },
        { 13, 0 }
    };

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

        // eval_bias += (rn.NextDouble() * 0.02f) - 0.02f;
        return Math.Round(eval_bias, 2);
    }

    public double PieceSum(Board board) {
        double eval_bias = 0;

        // int raw_wp_sum = 0;
        // int raw_bp_sum = 0;
        // 
        // int piece = 0;
        // for(int row=0; row<8; row++) {
        //     for (int col=0; col<8; col++) {
        //         piece = board.b[row, col];
        //         if (piece < 7 && piece != 0) raw_wp_sum += pieceValues[piece];
        //         if (piece > 7) raw_bp_sum += pieceValues[piece];
        //     }
        // }

        // float wp_perc = Math.Min((float)raw_wp_sum / 39 * 4, 1);
        // float bp_perc = Math.Min((float)raw_bp_sum / 39 * 4, 1);

        for (int row=0; row<8; row++) {
            for (int col=0; col<8; col++) {
                //eval_bias += GetSqVal(board, row, col, wp_perc, bp_perc);
                eval_bias += GetSqVal(board.b, row, col, 1, 1);
            }
        }

        return eval_bias;
    }

    public double GetSqVal(int[,] board, int row, int col, float white_early_late_bias, float black_early_late_bias) {
        int piece = board[row, col];

        if (piece == 0) return 0;


        if (piece == 1) {
            return 1 + (pieceMaps.white_pawn_map_early[row, col] * value_multiplier * white_early_late_bias) + (pieceMaps.white_pawn_map_late[row, col] * value_multiplier * (1-white_early_late_bias));
        }
        if (piece == 8) {
            return -(1 + (pieceMaps.black_pawn_map_early[row, col] * value_multiplier * black_early_late_bias) + (pieceMaps.black_pawn_map_late[row, col] * value_multiplier * (1-black_early_late_bias)));
        }


        if (piece == 2) {
            return 5 + (pieceMaps.white_rook_map_early[row, col] * value_multiplier * white_early_late_bias) + (0 * value_multiplier * (1-white_early_late_bias));
        }
        if (piece == 9) {
            return -(5 + (pieceMaps.black_rook_map_early[row, col] * value_multiplier * black_early_late_bias) + (0 * value_multiplier * (1-black_early_late_bias)));
        }


        if (piece == 3) {
            return 3 + (pieceMaps.white_knight_map_early[row, col] * value_multiplier * white_early_late_bias) + (0 * value_multiplier * (1-white_early_late_bias));
        }
        if (piece == 10) {
            return -(3 + (pieceMaps.black_knight_map_early[row, col] * value_multiplier * black_early_late_bias) + (0 * value_multiplier * (1-black_early_late_bias)));
        }


        if (piece == 4) {
            return 3 + (pieceMaps.white_bishop_map_early[row, col] * value_multiplier * white_early_late_bias) + (0 * value_multiplier * (1-white_early_late_bias));
        }
        if (piece == 11) {
            return -(3 + (pieceMaps.black_bishop_map_early[row, col] * value_multiplier * black_early_late_bias) + (0 * value_multiplier * (1-black_early_late_bias)));
        }


        if (piece == 5) {
            return 9 + (pieceMaps.white_queen_map_early[row, col] * value_multiplier * white_early_late_bias) + (0 * value_multiplier * (1-white_early_late_bias));
        }
        if (piece == 12) {
            return -(9 + (pieceMaps.black_queen_map_early[row, col] * value_multiplier * black_early_late_bias) + (0 * value_multiplier * (1-black_early_late_bias)));
        }


        if (piece == 6) {
            return (pieceMaps.white_king_map_early[row, col] * value_multiplier * white_early_late_bias) + (0 * value_multiplier * (1-white_early_late_bias));
        }
        if (piece == 13) {
            return -((pieceMaps.black_king_map_early[row, col] * value_multiplier * black_early_late_bias) + (0 * value_multiplier * (1-black_early_late_bias)));
        }


        return 0;
    }
}




public class PieceMaps {
    public void AllEqual() {
        AreEqual(white_pawn_map_early, black_pawn_map_early);
        AreEqual(white_pawn_map_late, black_pawn_map_late);
        AreEqual(white_rook_map_early, black_rook_map_early);
        AreEqual(white_knight_map_early, black_knight_map_early);
        AreEqual(white_bishop_map_early, black_bishop_map_early);
        AreEqual(white_queen_map_early, black_queen_map_early);
        AreEqual(white_king_map_early, black_king_map_early);
    }

    public static bool AreEqual(float[,] array1, float[,] array2)
    {
        if (array1.GetLength(0) != array2.GetLength(0) || array1.GetLength(1) != array2.GetLength(1))
            return false;

        for (int i = 0; i < array1.GetLength(0); i++)
        {
            for (int j = 0; j < array1.GetLength(1); j++)
            {
                if (array1[i, j] != array2[7-i, j])
                    UnityEngine.Debug.Log("ARRAY FAILED  " + i + ", " + j);
            }
        }
        return true;
    }

    // Start at white work down

    readonly public float[,] white_pawn_map_early = new float [8,8] { 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f},
        {+0.0f, -0.0f, -0.3f, -0.4f, -0.4f, -0.3f, +0.0f, +0.0f},
        {-0.2f, +0.1f, +0.1f, +0.2f, +0.2f, +0.1f, +0.1f, -0.2f},
        {-0.4f, +0.0f, +0.4f, +0.6f, +0.6f, +0.4f, +0.0f, -0.4f},
        {-0.3f, -0.2f, +0.1f, +0.3f, +0.3f, +0.1f, -0.2f, -0.3f},
        {-0.3f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, -0.3f},
        {+1.2f, +1.0f, +0.8f, +0.6f, +0.6f, +0.8f, +1.0f, +1.2f},
        {+5.0f, +5.0f, +5.0f, +5.0f, +5.0f, +5.0f, +5.0f, +5.0f}
    };

    readonly public float[,] white_pawn_map_late = new float [8,8] { 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f},
        {-0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f},
        {-0.2f, -0.2f, -0.2f, -0.2f, -0.2f, -0.2f, -0.2f, -0.2f},
        {+0.5f, +0.5f, +0.5f, +0.5f, +0.5f, +0.5f, +0.5f, +0.5f},
        {+1.5f, +1.5f, +1.5f, +1.5f, +1.5f, +1.5f, +1.5f, +1.5f},
        {+2.5f, +2.5f, +2.5f, +2.5f, +2.5f, +2.5f, +2.5f, +2.5f},
        {+4.0f, +4.0f, +4.0f, +4.0f, +4.0f, +4.0f, +4.0f, +4.0f},
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
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f},
        {-0.2f, -0.2f, -0.3f, -0.4f, -0.4f, -0.3f, -0.2f, -0.2f},
        {-0.3f, +0.4f, +0.1f, +0.2f, +0.2f, +0.1f, +0.4f, -0.3f},
        {-0.2f, +0.0f, +0.5f, +0.3f, +0.3f, +0.5f, +0.0f, -0.2f},
        {-0.3f, +0.2f, +0.1f, +0.3f, +0.3f, +0.1f, +0.2f, -0.3f},
        {-0.3f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, -0.3f},
        {+0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f},
        {+0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f}
    };

    readonly public float[,] white_king_map_early = new float [8,8] { 
        {+1.0f, +0.8f, +0.8f, -0.5f, -0.3f, -0.5f, +0.8f, +1.0f }, 
        {+0.1f, +0.1f, +0.0f, +0.0f, +0.0f, +0.0f, +0.1f, +0.1f }, 
        {-0.2f, -0.2f, -0.1f, +0.0f, +0.0f, -0.1f, -0.2f, -0.2f }, 
        {-0.5f, -0.5f, -0.3f, -0.2f, -0.2f, -0.3f, -0.5f, -0.5f }, 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }, 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }, 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }, 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }
    };

    readonly public float[,] white_rook_map_early = new float [8,8] { 
        {+1.0f, +0.8f, +0.8f, +0.5f, +0.3f, +0.5f, +0.8f, +1.0f }, 
        {+0.1f, +0.1f, +0.0f, +0.0f, +0.0f, +0.0f, +0.1f, +0.1f }, 
        {-0.2f, -0.2f, -0.1f, +0.0f, +0.0f, -0.1f, -0.2f, -0.2f }, 
        {-0.5f, -0.5f, -0.3f, -0.2f, -0.2f, -0.3f, -0.5f, -0.5f }, 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }, 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }, 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }, 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }
    };


    readonly public float[,] black_pawn_map_early = new float [8,8] { 
        {+5.0f, +5.0f, +5.0f, +5.0f, +5.0f, +5.0f, +5.0f, +5.0f},
        {+1.2f, +1.0f, +0.8f, +0.6f, +0.6f, +0.8f, +1.0f, +1.2f},
        {-0.3f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, -0.3f},
        {-0.3f, -0.2f, +0.1f, +0.3f, +0.3f, +0.1f, -0.2f, -0.3f},
        {-0.4f, +0.0f, +0.4f, +0.6f, +0.6f, +0.4f, +0.0f, -0.4f},
        {-0.2f, +0.1f, +0.1f, +0.2f, +0.2f, +0.1f, +0.1f, -0.2f},
        {+0.0f, -0.0f, -0.3f, -0.4f, -0.4f, -0.3f, +0.0f, +0.0f},
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f}
    };

    readonly public float[,] black_pawn_map_late = new float [8,8] { 
        {+5.0f, +5.0f, +5.0f, +5.0f, +5.0f, +5.0f, +5.0f, +5.0f},
        {+4.0f, +4.0f, +4.0f, +4.0f, +4.0f, +4.0f, +4.0f, +4.0f},
        {+2.5f, +2.5f, +2.5f, +2.5f, +2.5f, +2.5f, +2.5f, +2.5f},
        {+1.5f, +1.5f, +1.5f, +1.5f, +1.5f, +1.5f, +1.5f, +1.5f},
        {+0.5f, +0.5f, +0.5f, +0.5f, +0.5f, +0.5f, +0.5f, +0.5f},
        {-0.2f, -0.2f, -0.2f, -0.2f, -0.2f, -0.2f, -0.2f, -0.2f},
        {-0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f},
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
        {-0.3f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, -0.3f},
        {-0.3f, +0.2f, +0.1f, +0.3f, +0.3f, +0.1f, +0.2f, -0.3f},
        {-0.2f, +0.0f, +0.5f, +0.3f, +0.3f, +0.5f, +0.0f, -0.2f},
        {-0.3f, +0.4f, +0.1f, +0.2f, +0.2f, +0.1f, +0.4f, -0.3f},
        {-0.2f, -0.2f, -0.3f, -0.4f, -0.4f, -0.3f, -0.2f, -0.2f},
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f}
    };

    readonly public float[,] black_king_map_early = new float [8,8] { 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f },
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }, 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }, 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }, 
        {-0.5f, -0.5f, -0.3f, -0.2f, -0.2f, -0.3f, -0.5f, -0.5f }, 
        {-0.2f, -0.2f, -0.1f, +0.0f, +0.0f, -0.1f, -0.2f, -0.2f }, 
        {+0.1f, +0.1f, +0.0f, +0.0f, +0.0f, +0.0f, +0.1f, +0.1f }, 
        {+1.0f, +0.8f, +0.8f, -0.5f, -0.3f, -0.5f, +0.8f, +1.0f } 
    };

    readonly public float[,] black_rook_map_early = new float [8,8] { 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f },
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }, 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }, 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }, 
        {-0.5f, -0.5f, -0.3f, -0.2f, -0.2f, -0.3f, -0.5f, -0.5f }, 
        {-0.2f, -0.2f, -0.1f, +0.0f, +0.0f, -0.1f, -0.2f, -0.2f }, 
        {+0.1f, +0.1f, +0.0f, +0.0f, +0.0f, +0.0f, +0.1f, +0.1f }, 
        {+1.0f, +0.8f, +0.8f, +0.5f, +0.3f, +0.5f, +0.8f, +1.0f }
    };
}   
}