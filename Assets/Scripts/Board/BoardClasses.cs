using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


// ENUMS
public enum PieceGroup {
    FRIENDLY = 1,
    ENEMY = 2,
    BOTH = 3,

    WHITE = 4,
    BLACK = 5
}

public enum MBool {
    F = 0,
    T = 1,
    N = 2
}


// STRUCTS
public struct FEN_TEST {
    public String TestName;
    public String FEN;
    public List<int> expected_output;

    public FEN_TEST(String TestName_, String FEN_, List<int> expected_output_) {
        TestName = TestName_;
        FEN = FEN_;
        expected_output = expected_output_;
    }
}


// CLASSES
public struct Square
{
    public int col;
    public int row;
    public List<int> sq;

    public bool isWhite;
    public int piece;
    public int piece_type;

    public int start_row;

    public Square(int[,] board, int col, int row) {
        this.col = col;
        this.row = row;

        this.sq = new List<int>() {col, row};
        
        try {
            this.piece = board[row, col];
            this.piece_type = piece % 7;

            this.isWhite = (piece == piece_type);
            this.start_row = isWhite ? 1 : 6;
        }
        catch {
            this.piece = -1;
            this.piece_type = -1;

            this.isWhite = false;
            this.start_row = -1;
        }
    }

    public Square(int piece, int col, int row) {
        this.col = col;
        this.row = row;

        this.sq = new List<int>() {col, row};
        
        this.piece = piece;
        this.piece_type = piece % 7;

        this.isWhite = (piece == piece_type);

        this.start_row = isWhite ? 1 : 6;
    }

    public Square(Board board, string sq_name, MBool col_rep=MBool.N) {
        char col_char = sq_name[0];
        char row_char = sq_name[1];

        int col_num = col_char - 'a'; // Cancels out ASCII offset and adds 1
        int row_num = (int)Char.GetNumericValue(row_char) - 1;

        this.col = col_num;
        this.row = row_num;

        this.sq = new List<int>() {col, row};
        
        this.piece = board.b[row, col];
        this.piece_type = piece % 7;

        if (col_rep == MBool.N) this.isWhite = (piece == piece_type);
        if (col_rep == MBool.T) this.isWhite = board.turn;
        if (col_rep == MBool.F) this.isWhite = !board.turn;
        else {  this.isWhite = board.turn;  }

        this.start_row = isWhite ? 1 : 6;
    }


    public Square copy() {  return new Square(this.piece, this.col, this.row);  }

    public override string ToString() {  return str_uci();  }
    public string str_uci() {  return (char)('a' + col) + (row+1).ToString();  }
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
    
    public int[,] board_before_move;

    public Board before_board;

    public List<String> mv_change_list = new List<String>();


    // DECLARATIONS
    public Move(Board board, int col1, int row1, int col2, int row2) {
        
        start_square = new Square( board.b, col1, row1 );
        end_square = new Square( board.b, col2, row2 );
    }

    public Move(Board board, Square start_square_, int col2, int row2) {
        start_square = start_square_;
        end_square = new Square( board.b, col2, row2 );
    }


    public Move(Square start_square_, Square end_square_, int promotion=5) {
        start_square = start_square_;
        end_square = end_square_;

        if ((end_square.row == 7 && start_square.piece == 1) || (end_square.row == 0 && start_square.piece == 8)) promote = start_square.isWhite ? promotion : promotion+7;
    }

    public Move(Board board, string uci_string) {
        if (uci_string != null) {

            start_square = new Square( board, uci_string.Substring(0, 2) );
            end_square = new Square( board, uci_string.Substring(2, 2) );

            if (uci_string.Length == 5) {
                if (uci_string[4] == 'r' && board.turn ) promote = 2; 
                if (uci_string[4] == 'r' && !board.turn) promote = 9;

                if (uci_string[4] == 'n' && board.turn ) promote = 3;
                if (uci_string[4] == 'n' && !board.turn) promote = 10;

                if (uci_string[4] == 'b' && board.turn ) promote = 4;
                if (uci_string[4] == 'b' && !board.turn) promote = 11;

                if (uci_string[4] == 'q' && board.turn ) promote = 5;
                if (uci_string[4] == 'q' && !board.turn) promote = 12;
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
        switch(promote%7) {
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
    private readonly int[,] EMPTY_BOARD = new int[8,8] {
        { 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0 }
    };

    public List<Move> move_list = new List<Move>();
    public bool turn = true;
    
    public int is_checkmate = 0;
    public bool is_draw = false;

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

    // Debug / Setup
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

                String x = FEN.Substring(i+3, 4);


                is_a1_rook_moved = true;
                is_a8_rook_moved = true;
                is_h1_rook_moved = true;
                is_h8_rook_moved = true;

                is_wking_moved = true;
                is_bking_moved = true;

                if (x.Contains('k'))   is_h8_rook_moved = false;
                if (x.Contains('q'))   is_a8_rook_moved = false;
                if (x.Contains('K'))   is_h1_rook_moved = false; 
                if (x.Contains('Q'))   is_a1_rook_moved = false; 
                if (x.Contains("K") || x.Contains("Q"))  is_wking_moved   = false; 
                if (x.Contains("k") || x.Contains("q"))  is_bking_moved   = false; 

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

    public void PrintBoard(bool print_arbs=true)
    {
        string output = "";

        output += String.Join(" ", move_list) + "\n";

        for (int i = 0; i < b.GetLength(0); i++)
        {
            for (int j = 0; j < b.GetLength(1); j++)
            {
                output += b[i, j].ToString("D2") + " "; // "D2" ensures two-digit formatting for alignment
            }
            output += "\n"; // New line after each row
        }

        if (print_arbs) {
            output += "\n";
            output += "WK : " + is_wking_moved + "\n";
            output += "BK : " + is_bking_moved + "\n";
            output += "A1 : " + is_a1_rook_moved + "\n";
            output += "A8 : " + is_a8_rook_moved + "\n";
            output += "H1 : " + is_h1_rook_moved + "\n";
            output += "H8 : " + is_h8_rook_moved + "\n";
            output += "\n";
        }
    }



    public void reset_board() {  b = EMPTY_BOARD;  }

    public Board copy() {
        Board board_cpy = new Board();

        board_cpy.turn = turn;
        board_cpy.is_checkmate = is_checkmate;

        board_cpy.is_a1_rook_moved = is_a1_rook_moved;
        board_cpy.is_a8_rook_moved = is_a8_rook_moved;
        board_cpy.is_h1_rook_moved = is_h1_rook_moved;
        board_cpy.is_h8_rook_moved = is_h8_rook_moved;
        board_cpy.is_wking_moved = is_wking_moved;
        board_cpy.is_bking_moved = is_bking_moved;

        board_cpy.white_id = white_id;
        board_cpy.black_id = black_id;
        board_cpy.turn_id = turn_id;

        board_cpy.b = (int[,])b.Clone();
        board_cpy.move_list = new List<Move>(move_list);
        
        return board_cpy;
    }

    public Square find_piece_location(int piece) {
        if (piece == 13) {
            for (int r=7; r>=0; r--) {
                for (int c=0; c<8; c++) {
                    if (b[r, c] == piece) return new Square(b, c, r);
                }
            }
        }
        else {
            for (int r=0; r<8;r++) {
                for (int c=0; c<8; c++) {
                    if (b[r, c] == piece) return new Square(b, c, r);
                }
            }
        }

        Debug.LogWarning("Board Piece Finder could not find piece : " + piece);
        return new Square(b, 0, 0);
    }


    // MOVEMENT
    public void move(Move mv, bool flip_turn=true) {
        mv.board_before_move = (int[,])b.Clone();

        try {
            if (mv.promote == -1) {
                mv.replaced_piece = b[mv.end_square.row, mv.end_square.col];
                b[mv.end_square.row, mv.end_square.col] = b[mv.start_square.row, mv.start_square.col];
                b[mv.start_square.row, mv.start_square.col] = 0;
            }
        }
        catch {  Debug.LogWarning("Invalid Move passed : " + mv);  }

        // WHITE ROOKS
        if (mv.start_square.piece_type == 2 && !is_a1_rook_moved && (mv.start_square.sq).SequenceEqual(new List<int>() {0, 0})) {  is_a1_rook_moved = true; mv.mv_change_list.Add("a1r");  }
        if (mv.start_square.piece_type == 2 && !is_h1_rook_moved && (mv.start_square.sq).SequenceEqual(new List<int>() {7, 0})) {  is_h1_rook_moved = true; mv.mv_change_list.Add("h1r");  }
        
        // BLACK ROOKS
        if (mv.start_square.piece_type == 2 && !is_a8_rook_moved && (mv.start_square.sq).SequenceEqual(new List<int>() {0, 7})) {  is_a8_rook_moved = true; mv.mv_change_list.Add("a8r");  }
        if (mv.start_square.piece_type == 2 && !is_h8_rook_moved && (mv.start_square.sq).SequenceEqual(new List<int>() {7, 7})) {  is_h8_rook_moved = true; mv.mv_change_list.Add("h8r");  }
        
        // KINGS
        if (mv.start_square.piece_type == 6 && !is_wking_moved && (mv.start_square.sq).SequenceEqual(new List<int>() {4, 0})) {   is_wking_moved = true; mv.mv_change_list.Add("wk");  }
        if (mv.start_square.piece_type == 6 && !is_bking_moved && (mv.start_square.sq).SequenceEqual(new List<int>() {4, 7})) {   is_bking_moved = true; mv.mv_change_list.Add("bk");  }

        // Special Mvs
        if (mv.isEnpassant) {
            int forward_pawn_dir = turn ? -1 : 1;
            b[mv.end_square.row + forward_pawn_dir, mv.end_square.col] = 0;
            mv.mv_change_list.Add("ep");
        }

        if (mv.str_uci() == "e1g1" && mv.start_square.piece_type == 6) {
            Move mv_ = new Move(this, "h1f1");
            b[mv_.end_square.row, mv_.end_square.col] = b[mv_.start_square.row, mv_.start_square.col];
            b[mv_.start_square.row, mv_.start_square.col] = 0;
        }
        if (mv.str_uci() == "e1c1" && mv.start_square.piece_type == 6) {
            Move mv_ = new Move(this, "a1d1");
            b[mv_.end_square.row, mv_.end_square.col] = b[mv_.start_square.row, mv_.start_square.col];
            b[mv_.start_square.row, mv_.start_square.col] = 0;

        }
        if (mv.str_uci() == "e8g8" && mv.start_square.piece_type == 6) {
            Move mv_ = new Move(this, "h8f8");
            b[mv_.end_square.row, mv_.end_square.col] = b[mv_.start_square.row, mv_.start_square.col];
            b[mv_.start_square.row, mv_.start_square.col] = 0;  
        }
        if (mv.str_uci() == "e8c8" && mv.start_square.piece_type == 6) {
            Move mv_ = new Move(this, "a8d8");
            b[mv_.end_square.row, mv_.end_square.col] = b[mv_.start_square.row, mv_.start_square.col];
            b[mv_.start_square.row, mv_.start_square.col] = 0;
        }

        if (mv.promote != -1) {
            b[mv.end_square.row, mv.end_square.col] = mv.promote;
            b[mv.start_square.row, mv.start_square.col] = 0;
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
        
        foreach(String change in prev_mv.mv_change_list) {
            if (change == "a1r") {  is_a1_rook_moved = false;  }
            if (change == "a8r") {  is_a8_rook_moved = false;  }
            if (change == "h1r") {  is_h1_rook_moved = false;  }
            if (change == "h8r") {  is_h8_rook_moved = false;  }
            if (change == "wk" ) {  is_wking_moved = false;    }
            if (change == "bk" ) {  is_bking_moved = false;    }
        }     

        b = (int[,])prev_mv.board_before_move.Clone();
        return;
    }
}
