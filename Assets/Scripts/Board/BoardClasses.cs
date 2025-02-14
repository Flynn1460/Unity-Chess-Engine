using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Square
{
    public int col;
    public int row;
    public List<int> sq;

    public bool isWhite;
    public int piece;
    public int piece_type;

    public int start_row;

    public Square(Board board, int col, int row) {
        try {
            this.col = col;
            this.row = row;

            this.sq = new List<int>() {col, row};
            
            this.piece = board.b[row, col];
            this.piece_type = piece % 7;

            this.isWhite = (piece == piece_type);

            this.start_row = isWhite ? 1 : 6;
        }
        catch {}
    }

    public Square(Board board, string sq_name) {
        try {
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

            this.start_row = isWhite ? 1 : 6;
        }
        catch {}
    }

    public Square() {}

    public Square copy() {
        return new Square()
        { 
            col = this.col,
            row = this.row, 
            sq = new List<int>(this.sq),
            isWhite = this.isWhite, 
            piece = this.piece, 
            piece_type = this.piece_type,
            start_row = this.start_row 
        };
    }

    public override string ToString() {
        return (char)('a' + col) + (row+1).ToString();
    }

    public string str_uci() {
        return (char)('a' + col) + (row+1).ToString();
    }
}



public class Move
{
    public Square start_square;
    public Square end_square;

    public bool isEnpassant = false;

    public bool is_castle_white_short = false;
    public bool is_castle_white_long = false;
    public bool is_castle_black_short = false;
    public bool is_castle_black_long = false;

    public int replaced_piece = 0;

    public int promote = -1; // If number is set to a valid piece type then it is a promoting piece
    
    public int[,] board_before_move = new int[8,8] {
            { 2,  3,  4,  5,  6,  4,  3, 2 },
            { 1,  1,  1,  1,  1,  1,  1, 1 },
            { 0,  0,  0,  0,  0,  0,  0, 0 },
            { 0,  0,  0,  0,  0,  0,  0, 0 },
            { 0,  0,  0,  0,  0,  0,  0, 0 },
            { 0,  0,  0,  0,  0,  0,  0, 0 },
            { 8,  8,  8,  8,  8,  8,  8, 8 },
            { 9, 10, 11, 12, 13, 11, 10, 9 }
    };


    // DECLARATIONS
    public Move(Board board, int col1, int row1, int col2, int row2) {
        
        start_square = new Square( board, col1, row1 );
        end_square = new Square( board, col2, row2 );
    }

    public Move(Board board, Square start_square_, int col2, int row2) {
        start_square = start_square_;
        end_square = new Square( board, col2, row2 );
    }


    public Move(Square start_square_, Square end_square_) {
        start_square = start_square_;
        end_square = end_square_;
    }

    public Move(Board board, string uci_string) {
        if (uci_string != null) {

            start_square = new Square( board, uci_string.Substring(0, 2) );
            end_square = new Square( board, uci_string.Substring(2, 2) );

            if (uci_string.Length == 5) {
                switch(uci_string[4]) {
                    case 'r': promote = 2; break; 
                    case 'n': promote = 3; break;
                    case 'b': promote = 4; break;
                    case 'q': promote = 5; break;
                }
            }
        }
    }



    public Move copy() {
        Move move_cpy = new Move(null, "a1a1");

        move_cpy.start_square = start_square.copy();
        move_cpy.end_square = end_square.copy();

        move_cpy.isEnpassant = isEnpassant;
        move_cpy.is_castle_white_short = is_castle_white_short;
        move_cpy.is_castle_white_long = is_castle_white_long;
        move_cpy.is_castle_black_short = is_castle_black_short;
        move_cpy.is_castle_black_long = is_castle_black_long;
        move_cpy.promote = promote;

        return move_cpy;
    }

    public override string ToString() {  return str_uci();  }
    public string str_uci()
    {
        switch(promote) {
            case 2: return (char)('a' + start_square.col) + (start_square.row+1).ToString() + (char)('a' + end_square.col) + (end_square.row+1).ToString() + 'r';
            case 3: return (char)('a' + start_square.col) + (start_square.row+1).ToString() + (char)('a' + end_square.col) + (end_square.row+1).ToString() + 'n';
            case 4: return (char)('a' + start_square.col) + (start_square.row+1).ToString() + (char)('a' + end_square.col) + (end_square.row+1).ToString() + 'b';
            case 5: return (char)('a' + start_square.col) + (start_square.row+1).ToString() + (char)('a' + end_square.col) + (end_square.row+1).ToString() + 'q';
        }
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
    
    public int is_checkmate = 0;
    public bool is_draw = false;

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

    private MoveGenerator move_generator = new MoveGenerator();


    public void set_fen(string FEN) {
        SetupFen = FEN;

        int f = 0; //Files
        int r = 7; //Ranks
        reset_board();

        for(int i=0; i<FEN.Length; i++) {
            if (Char.IsDigit(FEN[i])) {
                f += (int)Char.GetNumericValue(FEN[i]);
            }

            else if (FEN[i] == '/') {
                r -= 1;
                f = 0;
            }
            
            else if (FEN[i] == ' ') {
                char turn_char = FEN[i+1];
                if (turn_char == 'w') turn = true;
                if (turn_char == 'b') turn = false;
                break;
            }

            else {
                switch(FEN[i]) {
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


    public Square find_piece_location(int piece) {
        if (piece == 13) {
            for (int r=7; r>=0; r--) {
                for (int c=0; c<8; c++) {
                    if (b[r, c] == piece) return new Square(this, c, r);
                }
            }
        }

        else {
            for (int r=0; r<8;r++) {
                for (int c=0; c<8; c++) {
                    if (b[r, c] == piece) return new Square(this, c, r);
                }
            }
        }
        
        Debug.LogWarning("Board Piece Finder could not find piece : " + piece);
        return null;
    }

    public void reset_board() {  b = EMPTY_BOARD;  }

    public Board copy() {
        Board board_cpy = new Board();

        board_cpy.turn = turn;
        board_cpy.is_checkmate = is_checkmate;
        board_cpy.is_gameover = is_gameover;
        board_cpy.is_a1_rook_moved = is_a1_rook_moved;
        board_cpy.is_a8_rook_moved = is_a8_rook_moved;
        board_cpy.is_h1_rook_moved = is_h1_rook_moved;
        board_cpy.is_h8_rook_moved = is_h8_rook_moved;
        board_cpy.is_wking_moved = is_wking_moved;
        board_cpy.is_bking_moved = is_bking_moved;
        board_cpy.white_id = white_id;
        board_cpy.black_id = black_id;
        board_cpy.turn_id = turn_id;
        board_cpy.SetupFen = SetupFen;

        board_cpy.b = (int[,])b.Clone();
        board_cpy.move_list = new List<Move>(move_list);
        
        return board_cpy;
    }

    // MOVEMENT
    public void move(Move mv, bool flip_turn=true, bool definate_move=false) {
        mv.board_before_move = (int[,])b.Clone();

        // if (definate_move && move_generator.isCheckmate(this) != 0) {
        //     is_checkmate = true;
        //     return;
        // }
        
        
        // File, Rank, File, Rank
        try {
            mv.replaced_piece = b[mv.end_square.row, mv.end_square.col];
            b[mv.end_square.row, mv.end_square.col] = b[mv.start_square.row, mv.start_square.col];
            b[mv.start_square.row, mv.start_square.col] = 0;
        }
        catch {  Debug.LogWarning("Invalid Move passed : " + mv);  }

        // Castle Bools
        // WHITE ROOKS
        if (mv.start_square.piece_type == 2 && (mv.start_square.sq).SequenceEqual(new List<int>() {0, 0})) {  is_a1_rook_moved = true;  }
        if (mv.start_square.piece_type == 2 && (mv.start_square.sq).SequenceEqual(new List<int>() {7, 0})) {  is_h1_rook_moved = true;  }
        
        // BLACK ROOKS
        if (mv.start_square.piece_type == 2 && (mv.start_square.sq).SequenceEqual(new List<int>() {0, 7})) {  is_a8_rook_moved = true;  }
        if (mv.start_square.piece_type == 2 && (mv.start_square.sq).SequenceEqual(new List<int>() {7, 7})) {  is_h8_rook_moved = true;  }
        
        // KINGS
        if (mv.start_square.piece_type == 6 && (mv.start_square.sq).SequenceEqual(new List<int>() {4, 0})) {   is_wking_moved = true;  }
        if (mv.start_square.piece_type == 6 && (mv.start_square.sq).SequenceEqual(new List<int>() {4, 7})) {   is_bking_moved = true;  }

        // Special Mvs
        if (mv.isEnpassant) {
            int forward_pawn_dir = turn ? -1 : 1;
            b[mv.end_square.row + forward_pawn_dir, mv.end_square.col] = 0;
        }

        if (mv.str_uci() == "e1g1" && mv.start_square.piece_type == 6) {
            Move mv_ = new Move(this, "h1f1");
            move(mv_, false);
        }
        if (mv.str_uci() == "e1c1" && mv.start_square.piece_type == 6) {
            Move mv_ = new Move(this, "a1d1");
            move(mv_, false);

        }
        if (mv.str_uci() == "e8g8" && mv.start_square.piece_type == 6) {
            Move mv_ = new Move(this, "h8f8");
            move(mv_, false);  
        }
        if (mv.str_uci() == "e8c8" && mv.start_square.piece_type == 6) {
            Move mv_ = new Move(this, "a8d8");
            move(mv_, false);
        }

        if (definate_move && mv.promote != -1) {
            b[mv.end_square.row, mv.end_square.col] = mv.promote;
        }


        if (flip_turn) {
            move_list.Add(mv);
            
            turn = !turn;

            if (turn)  turn_id = white_id;
            if (!turn) turn_id = black_id;
        }
    }

    public void undo_move(bool flip_turn=true) {
        Move prev_mv = move_list[move_list.Count-1];

        if (flip_turn) {
            move_list.RemoveAt(move_list.Count-1);

            turn = !turn;

            if (turn)  turn_id = white_id;
            if (!turn) turn_id = black_id;
        }

        b = (int[,])prev_mv.board_before_move.Clone();
    }
}
