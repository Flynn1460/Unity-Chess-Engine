using System;

namespace ENGINE_NAMESPACE_Minimax_V2 {

public class Eval {
    private Random rn = new Random();
    private MoveGenerator mg = new MoveGenerator();
    private PieceMaps pieceMaps = new PieceMaps();

    public double EvaluateBoard(Board board) {
        double eval_bias;

        if (mg.isCheckmate(board) == 1 ) {
            eval_bias = +900;
        }
        else if (mg.isCheckmate(board) == -1) {
            eval_bias = -900;
        }
        else if (mg.isDraw(board)) {
            eval_bias = 0;
        }
        else {
            eval_bias = PieceSum(board);
        }

        eval_bias += (rn.NextDouble() * 0.02f) - 0.01f;
        return Math.Round(eval_bias, 2);
    }

    public double PieceSum(Board board) {
        double eval_bias = 0;

        for (int row=0; row<8; row++) {
            for (int col=0; col<8; col++) {
                int piece = board.b[row, col];


                switch (piece) {
                    case 1: eval_bias += 1; eval_bias += pieceMaps.white_pawn_map_early[row,col]; break;
                    case 2: eval_bias += 5; eval_bias += pieceMaps.white_rook_map_early[row,col]; break;
                    case 3: eval_bias += 3; eval_bias += pieceMaps.white_knight_map_early[row,col]; break;
                    case 4: eval_bias += 3; eval_bias += pieceMaps.white_bishop_map_early[row,col]; break;
                    case 5: eval_bias += 9; eval_bias += pieceMaps.white_queen_map_early[row,col]; break;
                    case 6: eval_bias += 100; eval_bias += pieceMaps.white_king_map_early[row,col]; break;

                    case 8: eval_bias -= 1; eval_bias -= pieceMaps.black_pawn_map_early[row,col]; break;
                    case 9: eval_bias -= 5; eval_bias -= pieceMaps.black_rook_map_early[row,col]; break;
                    case 10: eval_bias -= 3; eval_bias -= pieceMaps.black_knight_map_early[row,col]; break;
                    case 11: eval_bias -= 3; eval_bias -= pieceMaps.black_bishop_map_early[row,col]; break;
                    case 12: eval_bias -= 9; eval_bias -= pieceMaps.black_queen_map_early[row,col]; break;
                    case 13: eval_bias -= 100; eval_bias -= pieceMaps.black_king_map_early[row,col]; break;
                }
            }
        }

        return eval_bias;
    }


}
}


public class PieceMaps {
    private float MULTIPLIER = 1;
    
    public PieceMaps (float multiplier=1){
        MULTIPLIER = multiplier;
    }

    // Start at white work down

    public float[,] white_pawn_map_early = new float [8,8] { 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f},
        {+0.0f, -0.0f, -0.3f, -0.4f, -0.4f, -0.3f, +0.0f, +0.0f},
        {-0.2f, +0.1f, +0.1f, +0.2f, +0.2f, +0.1f, +0.1f, -0.2f},
        {-0.4f, +0.0f, +0.4f, +0.6f, +0.6f, +0.4f, +0.0f, -0.4f},
        {-0.3f, -0.2f, +0.1f, +0.3f, +0.3f, +0.1f, -0.2f, -0.3f},
        {-0.3f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, -0.3f},
        {+1.2f, +1.0f, +0.8f, +0.6f, +0.6f, +0.8f, +1.0f, +1.2f},
        {+5.0f, +5.0f, +5.0f, +5.0f, +5.0f, +5.0f, +5.0f, +5.0f}
    };

    public float[,] white_bishop_map_early = new float [8,8] { 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f},
        {+0.2f, +0.2f, -0.3f, -0.4f, -0.4f, -0.3f, +0.2f, +0.2f},
        {+0.3f, +0.4f, +0.1f, +0.2f, +0.2f, +0.1f, +0.4f, +0.3f},
        {+0.0f, +0.0f, +0.5f, +0.3f, +0.3f, +0.5f, +0.0f, +0.0f},
        {-0.3f, -0.2f, +0.1f, +0.3f, +0.3f, +0.1f, -0.2f, -0.3f},
        {-0.3f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, -0.3f},
        {+0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f},
        {+0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f}
    };

    public float[,] white_queen_map_early = new float [8,8] { 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f},
        {+0.2f, +0.2f, -0.3f, -0.4f, -0.4f, -0.3f, +0.2f, +0.2f},
        {+0.3f, +0.4f, +0.1f, +0.2f, +0.2f, +0.1f, +0.4f, +0.3f},
        {+0.0f, +0.0f, +0.5f, +0.3f, +0.3f, +0.5f, +0.0f, +0.0f},
        {-0.3f, -0.2f, +0.1f, +0.3f, +0.3f, +0.1f, -0.2f, -0.3f},
        {-0.3f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, -0.3f},
        {+0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f},
        {+0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f}
    };

    public float[,] white_knight_map_early = new float [8,8] { 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f},
        {-0.2f, -0.2f, -0.3f, -0.4f, -0.4f, -0.3f, -0.2f, -0.2f},
        {-0.3f, +0.4f, +0.1f, +0.2f, +0.2f, +0.1f, +0.4f, -0.3f},
        {-0.2f, +0.0f, +0.5f, +0.3f, +0.3f, +0.5f, +0.0f, -0.2f},
        {-0.3f, +0.2f, +0.1f, +0.3f, +0.3f, +0.1f, +0.2f, -0.3f},
        {-0.3f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, -0.3f},
        {+0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f},
        {+0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f}
    };

    public float[,] white_king_map_early = new float [8,8] { 
        {+1.0f, +0.8f, +0.8f, -0.5f, -0.3f, -0.5f, +0.8f, +1.0f }, 
        {+0.1f, +0.1f, +0.0f, +0.0f, +0.0f, +0.0f, +0.1f, +0.1f }, 
        {-0.2f, -0.2f, -0.1f, +0.0f, +0.0f, -0.1f, -0.2f, -0.2f }, 
        {-0.5f, -0.5f, -0.3f, -0.2f, -0.2f, -0.3f, -0.5f, -0.5f }, 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }, 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }, 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }, 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }
    };

    public float[,] white_rook_map_early = new float [8,8] { 
        {+1.0f, +0.8f, +0.8f, +0.5f, +0.3f, +0.5f, +0.8f, +1.0f }, 
        {+0.1f, +0.1f, +0.0f, +0.0f, +0.0f, +0.0f, +0.1f, +0.1f }, 
        {-0.2f, -0.2f, -0.1f, +0.0f, +0.0f, -0.1f, -0.2f, -0.2f }, 
        {-0.5f, -0.5f, -0.3f, -0.2f, -0.2f, -0.3f, -0.5f, -0.5f }, 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }, 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }, 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }, 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }
    };


    public float[,] black_pawn_map_early = new float [8,8] { 
        {+5.0f, +5.0f, +5.0f, +5.0f, +5.0f, +5.0f, +5.0f, +5.0f},
        {+1.2f, +1.0f, +0.8f, +0.6f, +0.6f, +0.8f, +1.0f, +1.2f},
        {-0.3f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, -0.3f},
        {-0.3f, -0.2f, +0.1f, +0.3f, +0.3f, +0.1f, -0.2f, -0.3f},
        {-0.4f, +0.0f, +0.4f, +0.6f, +0.6f, +0.4f, +0.0f, -0.4f},
        {-0.2f, +0.1f, +0.1f, +0.2f, +0.2f, +0.1f, +0.1f, -0.2f},
        {+0.0f, -0.0f, -0.3f, -0.4f, -0.4f, -0.3f, +0.0f, +0.0f},
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f}
    };

    public float[,] black_bishop_map_early = new float [8,8] { 
        {+0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f},
        {+0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f},
        {-0.3f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, -0.3f},
        {-0.3f, -0.2f, +0.1f, +0.3f, +0.3f, +0.1f, -0.2f, -0.3f},
        {+0.0f, +0.0f, +0.5f, +0.3f, +0.3f, +0.5f, +0.0f, +0.0f},
        {+0.3f, +0.4f, +0.1f, +0.2f, +0.2f, +0.1f, +0.4f, +0.3f},
        {+0.2f, +0.2f, -0.3f, -0.4f, -0.4f, -0.3f, +0.2f, +0.2f},
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f}
    };

    public float[,] black_queen_map_early = new float [8,8] { 
        {+0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f},
        {+0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f},
        {-0.3f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, -0.3f},
        {-0.3f, -0.2f, +0.1f, +0.3f, +0.3f, +0.1f, -0.2f, -0.3f},
        {+0.0f, +0.0f, +0.5f, +0.3f, +0.3f, +0.5f, +0.0f, +0.0f},
        {+0.3f, +0.4f, +0.1f, +0.2f, +0.2f, +0.1f, +0.4f, +0.3f},
        {+0.2f, +0.2f, -0.3f, -0.4f, -0.4f, -0.3f, +0.2f, +0.2f},
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f}
    };

    public float[,] black_knight_map_early = new float [8,8] { 
        {+0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f},
        {+0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f},
        {-0.3f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, +0.0f, -0.3f},
        {-0.3f, +0.2f, +0.1f, +0.3f, +0.3f, +0.1f, +0.2f, -0.3f},
        {-0.2f, +0.0f, +0.5f, +0.3f, +0.3f, +0.5f, +0.0f, -0.2f},
        {-0.3f, +0.4f, +0.1f, +0.2f, +0.2f, +0.1f, +0.4f, -0.3f},
        {-0.2f, -0.2f, -0.3f, -0.4f, -0.4f, -0.3f, -0.2f, -0.2f},
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f}
    };

    public float[,] black_king_map_early = new float [8,8] { 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f },
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }, 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }, 
        {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f }, 
        {-0.5f, -0.5f, -0.3f, -0.2f, -0.2f, -0.3f, -0.5f, -0.5f }, 
        {-0.2f, -0.2f, -0.1f, +0.0f, +0.0f, -0.1f, -0.2f, -0.2f }, 
        {+0.1f, +0.1f, +0.0f, +0.0f, +0.0f, +0.0f, +0.1f, +0.1f }, 
        {+1.0f, +0.8f, +0.8f, -0.5f, -0.3f, -0.5f, +0.8f, +1.0f } 
    };

    public float[,] black_rook_map_early = new float [8,8] { 
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