using System;
using System.Collections.Generic;
using UnityEngine;

public class Square
{
    public int col;
    public int row;
    public List<int> sq;

    public bool isWhite;
    public int piece;
    public int piece_type;

    public Square(Board board, int col, int row) {
        this.col = col;
        this.row = row;

        this.sq = new List<int>() {col, row};
        
        this.piece = board.b[row, col];
        this.piece_type = piece % 7;

        this.isWhite = (piece == piece_type);
    }

    public Square(Board board, string sq_name) {
        char col_char = sq_name[0];
        char row_char = sq_name[1];

        int col_num = col_char - 'a'; // Cancels out ASCII offset and adds 1
        int row_num = (int)Char.GetNumericValue(row_char) - 1;

        this.col = col_num;
        this.row = row_num;

        this.sq = new List<int>() {col, row};
        
        this.piece = board.b[row, col];
        this.piece_type = piece % 7;

        this.isWhite = (piece == piece_type);
    }

    public override string ToString() {
        return (char)('a' + col) + (row+1).ToString();
    }

    public string uci_str() {
        return (char)('a' + col) + (row+1).ToString();
    }
}



public class Move
{
    public Square start_square;
    public Square end_square;

    // DECLARATIONS
    public Move(Board board, int col1, int row1, int col2, int row2) {
        
        start_square = new Square( board, col1, row1 );
        end_square = new Square( board, col2, row2 );
    }

    public Move(Board board, Square start_square_, Square end_square_) {
        start_square = start_square_;
        end_square = end_square_;
    }

    public Move(Board board, string uci_string) {
        start_square = new Square( board, uci_string.Substring(0, 2) );
        end_square = new Square( board, uci_string.Substring(2, 2) );
    }


    public override string ToString()
    {
        return (char)('a' + start_square.col) + (start_square.row+1).ToString() + (char)('a' + end_square.col) + (end_square.row+1).ToString();
    }

    public string str_uci()
    {
        return (char)('a' + start_square.col) + (start_square.row+1).ToString() + (char)('a' + end_square.col) + (end_square.row+1).ToString();
    }
}



public class Board{
    public int[,] b = new int[8,8] {
            { 2,  3,  4,  5,  6,  4,  3, 2 },
            { 1,  1,  1,  1,  1,  1,  1, 1 },
            { 0,  0,  0,  0,  0,  0,  0, 0 },
            { 0,  0,  0,  0,  0,  0,  0, 0 },
            { 0,  0,  0,  0,  0,  0,  0, 0 },
            { 0,  0,  0,  0,  0,  0,  0, 0 },
            { 8,  8,  8,  8,  8,  8,  8, 8 },
            { 9, 10, 11, 12, 13, 11, 10, 9 }
    };
    private int[,] EMPTY_BOARD = new int[8,8] {
        { 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0 }
    };

    public bool turn = true;
    public List<Move> move_list = new List<Move>();
    public bool is_checkmate = false;

    /*
    * -1 # BLACK WIN
    *  0 # DRAW
    *  1 # WHITE WIN
    */
    public int is_gameover = 0; 

    // Castling Items
    public bool is_a1_rook_moved = false;
    public bool is_a8_rook_moved = false;
    public bool is_h1_rook_moved = false;
    public bool is_h8_rook_moved = false;

    public bool is_wking_moved = false;
    public bool is_bking_moved = false;

    // Turn IDs
    public int white_id = 0;
    public int black_id = 0;
    public int turn_id = 0;

    public string SetupFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";


    public void set_fen(string FEN) {
        SetupFen = FEN;

        int f = 0; //Files
        int r = 7; //Ranks
        reset_board();

        foreach(char i in FEN) {
            if (Char.IsDigit(i)) {
                f += (int)Char.GetNumericValue(i);
            }

            else if (i == '/') {
                r -= 1;
                f = 0;
            }
            
            else if (i == ' ') {
                char turn_char = FEN[i+1];
                if (turn_char == 'w') turn = true;
                if (turn_char == 'b') turn = false;
                break;
            }

            else {
                switch(i) {
                    // White
                    case 'P': b[r, f] = 1; break;
                    case 'R': b[r, f] = 2; break;
                    case 'N': b[r, f] = 3; break;
                    case 'B': b[r, f] = 4; break;
                    case 'Q': b[r, f] = 5; break;
                    case 'K': b[r, f] = 6; break;

                    // Black
                    case 'p': b[r, f] = 8; break;
                    case 'r': b[r, f] = 9; break;
                    case 'n': b[r, f] = 10; break;
                    case 'b': b[r, f] = 11; break;
                    case 'q': b[r, f] = 12; break;
                    case 'k': b[r, f] = 13; break;
                }
                f += 1;
            }
        }
    }


    [Obsolete("Function should not be necessary with Square object")]
    public List<int> find_piece_location(int piece) {
        for (int r=0; r<8;r++) {
            for (int c=0; c<8; c++) {
                if (b[r, c] == piece) return new List<int>() {r, c};
            }
        }
        
        Debug.LogWarning("Board Piece Finder could not find piece : " + piece);
        return null;
    }

    public void reset_board() {  b = EMPTY_BOARD;  }


    // MOVEMENT
    public void move(Move move, bool flip_turn=true) {
        // File, Rank, File, Rank
        b[move.end_square.row, move.end_square.col] = b[move.start_square.row, move.start_square.col];
        b[move.start_square.row, move.start_square.col] = 0;


        move_list.Add(move);

        if (flip_turn) {
            turn = !turn;

            if (turn)  turn_id = white_id;
            if (!turn) turn_id = black_id;
        }
    }
}
