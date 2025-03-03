using System;
using System.Collections.Generic;
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

public struct Square
{
    public readonly int col;
    public readonly int row;
    public readonly (int, int) sq;

    public readonly bool isWhite;
    public int piece;
    public int piece_type;

    public readonly int start_row;

    public Square(int[,] board, int col, int row) {
        this.col = col;
        this.row = row;

        this.sq = (col, row);
        
    if (row >= 0 && row < board.GetLength(0) && col >= 0 && col < board.GetLength(1))
    {
        this.piece = board[row, col];
        this.piece_type = piece % 7;
        this.isWhite = (piece == piece_type);
        this.start_row = isWhite ? 1 : 6;
    }
    else
    {
        this.piece = -1;
        this.piece_type = -1;
        this.isWhite = false;
        this.start_row = -1;
    }
    }

    public Square(int piece, int col, int row) {
        this.col = col;
        this.row = row;

        this.sq = (col, row);
        
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

        this.sq = (col, row);
        try {
            this.piece = board.b[row, col];
        }
        catch {
            this.piece = 0;
            Debug.Log("ERROR : " + row + ", " + col);
            Debug.Log("STR : " + sq_name);
        }
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


// CLASSES
public class Move
{
    public Square start_square;
    public Square end_square;

    public bool isEnpassant = false;

    public bool is_castle_white_short = false;
    public bool is_castle_white_long = false;
    public bool is_castle_black_short = false;
    public bool is_castle_black_long = false;

    public int replaced_piece;
    

    public int promote = -1; // If number is set to a valid piece type then it is a promoting piece
    
    public int[,] board_before_move;
    public (int, int) wking_pos;
    public (int, int) bking_pos;

    public uint MOVE_BIT_REP = 0b__000000_0000;

    // First two for generic move, next two for castling or en passant
    public int[] board_mv_changes = new int[4];

    public string changed_castle_bool;


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
            start_square = new Square( board, $"{uci_string[0]}{uci_string[1]}" );
            end_square = new Square( board, $"{uci_string[2]}{uci_string[3]} )" );

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

            if (start_square.piece_type == 1 && end_square.piece == 0) {
                if (Math.Abs(start_square.col - end_square.col) == 1) {
                    isEnpassant = true;
                }
            }
        }
    }


    public Move() {}
    public Move copy() {
        Move move_cpy = new Move();

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

    public (int, int) wking_pos = (0, 4);
    public (int, int) bking_pos = (7, 4);

    // Turn IDs
    public int white_id = 0;
    public int black_id = 0;
    public int turn_id = 0;

    public string SetupFen = "NULL";

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
    
        for (int row=0; row<8;row++) {
            for (int c=0; c<8; c++) {
                if (b[row, c] == 6) wking_pos = (row, c);
                if (b[row, c] == 13) bking_pos = (row, c);
            }
        }
    }

    public void PrintBoard(NewMoveGenerator mg)
    {
        string output = "";

        output += "MOVE LIST: "+String.Join(" ", move_list) + "\n";
        output += "LEGAL MOVES: "+String.Join(" ", mg.GenerateLegalMoves(this)) + "\n\n";

        for (int i = 0; i < b.GetLength(0); i++)
        {
            for (int j = 0; j < b.GetLength(1); j++)
            {
                output += b[i, j].ToString("D2") + " "; // "D2" ensures two-digit formatting for alignment
            }
            output += "\n"; // New line after each row
        }

        output += "TURN : " + turn;

        output += "\n";
        output += "WK : " + is_wking_moved + "\n";
        output += "BK : " + is_bking_moved + "\n";
        output += "A1 : " + is_a1_rook_moved + "\n";
        output += "A8 : " + is_a8_rook_moved + "\n";
        output += "H1 : " + is_h1_rook_moved + "\n";
        output += "H8 : " + is_h8_rook_moved + "\n";
        output += "\n";

        Debug.Log(output);
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

        board_cpy.wking_pos = wking_pos;
        board_cpy.bking_pos = bking_pos;

        board_cpy.SetupFen = SetupFen;

        board_cpy.b = (int[,])b.Clone();
        board_cpy.move_list = new List<Move>(move_list);
        
        return board_cpy;
    }

    public Square find_piece_location(int piece) {
        if (piece == 13) {
            (int rx, int cx) = bking_pos;
            return new Square(b, cx, rx);

        }
        if (piece == 6) {
            (int rx, int cx) = wking_pos;
            return new Square(b, cx, rx);
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
        // IF VALID MOVE
        if (mv.start_square.row <= 7 && mv.start_square.row >= 0 && mv.start_square.col <= 7 && mv.start_square.col >= 0 &&
            mv.end_square.row   <= 7 && mv.end_square.row   >= 0 && mv.end_square.col   <= 7 && mv.end_square.col   >= 0) {
        
        mv.board_before_move = (int[,])b.Clone();
        mv.wking_pos = wking_pos;
        mv.bking_pos = bking_pos;

        if (mv.promote == -1) {
            if (mv.isEnpassant) {
                int forward_pawn_dir = turn ? -1 : 1;
                b[mv.end_square.row + forward_pawn_dir, mv.end_square.col] = 0;
            }

            mv.replaced_piece = b[mv.end_square.row, mv.end_square.col];
            b[mv.end_square.row, mv.end_square.col] = b[mv.start_square.row, mv.start_square.col];
            b[mv.start_square.row, mv.start_square.col] = 0;

            if (mv.start_square.piece_type != 2 && mv.start_square.piece_type != 6) {
                mv_flip_turn(mv, flip_turn);
                return;
            }

            if (mv.start_square.piece == 6 ) {  wking_pos = (mv.end_square.row, mv.end_square.col);  }
            if (mv.start_square.piece == 13) {  bking_pos = (mv.end_square.row, mv.end_square.col);  }
        }
        else {
            b[mv.end_square.row, mv.end_square.col] = mv.promote;
            b[mv.start_square.row, mv.start_square.col] = 0;

            mv_flip_turn(mv, flip_turn);
            return;
        }
        

        // ROOKS
        if (mv.start_square.piece_type == 2) {
            if (!is_a1_rook_moved && mv.start_square.sq == (0, 0)) {  is_a1_rook_moved = true; mv.changed_castle_bool = "a1r"; mv_flip_turn(mv, flip_turn); return;  }
            if (!is_h1_rook_moved && mv.start_square.sq == (7, 0)) {  is_h1_rook_moved = true; mv.changed_castle_bool = "h1r"; mv_flip_turn(mv, flip_turn); return;  }
            
            if (!is_a8_rook_moved && mv.start_square.sq == (0, 7)) {  is_a8_rook_moved = true; mv.changed_castle_bool = "a8r"; mv_flip_turn(mv, flip_turn); return;  }
            if (!is_h8_rook_moved && mv.start_square.sq == (7, 7)) {  is_h8_rook_moved = true; mv.changed_castle_bool = "h8r"; mv_flip_turn(mv, flip_turn); return;  }
            
            mv_flip_turn(mv, flip_turn); 
            return;
        }


        // KINGS
        if (!is_wking_moved && mv.start_square.sq == (4, 0)) {   is_wking_moved = true; mv.changed_castle_bool = "wk";  }
        if (!is_bking_moved && mv.start_square.sq == (4, 7)) {   is_bking_moved = true; mv.changed_castle_bool = "bk";  }

        String king_move = mv.str_uci();

        if (king_move == "e1g1") {
            // ROOK h1f1
            b[0, 5] = b[0, 7];
            b[0, 7] = 0;

            wking_pos = (0, 6);

            mv_flip_turn(mv, flip_turn);
            return;
        }

        if (king_move == "e1c1") {
            // ROOK a1d1
            b[0, 3] = b[0, 0];
            b[0, 0] = 0;

            wking_pos = (0, 2);

            mv_flip_turn(mv, flip_turn);
            return;

        }

        if (king_move == "e8g8") {
            // ROOK h8f8
            b[7, 5] = b[7, 7];
            b[7, 7] = 0;  

            bking_pos = (7, 6);

            mv_flip_turn(mv, flip_turn);
            return;
        }

        if (king_move == "e8c8") {
            // ROOK a8d8
            b[7, 3] = b[7, 0];
            b[7, 0] = 0;

            bking_pos = (7, 2);

            mv_flip_turn(mv, flip_turn);
            return;
        }
        }
        else {  Debug.LogWarning("Invalid Move Passed");  }
        
        mv_flip_turn(mv, flip_turn);
    }

    public void undo_move(bool flip_turn=true) {
        Move prev_mv = move_list[move_list.Count-1];

        if (flip_turn) {
            move_list.RemoveAt(move_list.Count-1);
            
            turn = !turn;

            if (turn)  turn_id = white_id;
            if (!turn) turn_id = black_id;
        }
        
        if (prev_mv.changed_castle_bool != null) {
            if (prev_mv.changed_castle_bool == "a1r") {  is_a1_rook_moved = false;  }
            if (prev_mv.changed_castle_bool == "a8r") {  is_a8_rook_moved = false;  }
            if (prev_mv.changed_castle_bool == "h1r") {  is_h1_rook_moved = false;  }
            if (prev_mv.changed_castle_bool == "h8r") {  is_h8_rook_moved = false;  }
            if (prev_mv.changed_castle_bool == "wk" ) {  is_wking_moved = false;    }
            if (prev_mv.changed_castle_bool == "bk" ) {  is_bking_moved = false;    } 
        }

        b = (int[,])prev_mv.board_before_move.Clone();
        
        wking_pos = prev_mv.wking_pos;
        bking_pos = prev_mv.bking_pos;
        return;
    }



    public void advanced_move(Move mv) {
        move_list.Add(mv);

        turn = !turn;
        turn_id = turn ? white_id : black_id;


        if (!(mv.start_square.row <= 7 && mv.start_square.row >= 0 && mv.start_square.col <= 7 && mv.start_square.col >= 0 && mv.end_square.row   <= 7 
            && mv.end_square.row   >= 0 && mv.end_square.col   <= 7 && mv.end_square.col   >= 0)) return;
        
        mv.wking_pos = wking_pos;
        mv.bking_pos = bking_pos;

        if (mv.promote == -1) {
            b[mv.end_square.row, mv.end_square.col] = b[mv.start_square.row, mv.start_square.col];
            b[mv.start_square.row, mv.start_square.col] = 0;

            if (mv.isEnpassant) {
                int forward_pawn_dir = turn ? 1 : -1;

                mv.start_square.piece = b[mv.end_square.row + forward_pawn_dir, mv.end_square.col];
                b[mv.end_square.row + forward_pawn_dir, mv.end_square.col] = 0;
            }

            if (mv.start_square.piece_type != 2 && mv.start_square.piece_type != 6) return;

            wking_pos = (mv.start_square.piece == 6) ? (mv.end_square.row, mv.end_square.col) : wking_pos;
            bking_pos = (mv.start_square.piece == 13) ? (mv.end_square.row, mv.end_square.col) : bking_pos;

        }
        else {
            b[mv.end_square.row, mv.end_square.col] = mv.promote;
            b[mv.start_square.row, mv.start_square.col] = 0;

            return;
        }
        
        // ROOKS
        if (mv.start_square.piece_type == 2) {
            if (!is_a1_rook_moved && mv.start_square.sq == (0, 0)) {  
                is_a1_rook_moved = true; 
                mv.MOVE_BIT_REP |= 0b__100000_0000;
            }

            else if (!is_h1_rook_moved && mv.start_square.sq == (7, 0)) {  
                is_h1_rook_moved = true; 
                mv.MOVE_BIT_REP |= 0b__010000_0000;
            }
            
            else if (!is_a8_rook_moved && mv.start_square.sq == (0, 7)) {  
                is_a8_rook_moved = true; 
                mv.MOVE_BIT_REP |= 0b__001000_0000;
            
            }

            else if (!is_h8_rook_moved && mv.start_square.sq == (7, 7)) {  
                is_h8_rook_moved = true; 
                mv.MOVE_BIT_REP |= 0b__000100_0000;
            }
            
            return;
        }


        // KINGS
        if (!is_wking_moved && mv.start_square.sq == (4, 0)) {   is_wking_moved = true; mv.MOVE_BIT_REP |= 0b__000010_0000;  }
        else if (!is_bking_moved && mv.start_square.sq == (4, 7)) {   is_bking_moved = true; mv.MOVE_BIT_REP |= 0b__000001_0000;  }

        String king_move = mv.str_uci();

        if (king_move == "e1c1") { // e1c1
            // ROOK a1d1
            b[0, 3] = b[0, 0];
            b[0, 0] = 0;

            mv.MOVE_BIT_REP |= 0b__000000_1000;

            wking_pos = (0, 2);
        }

        else if (king_move == "e1g1") { // e1g1
            // ROOK h1f1
            b[0, 5] = b[0, 7];
            b[0, 7] = 0;

            mv.MOVE_BIT_REP |= 0b__000000_0100;

            wking_pos = (0, 6);
        }


        else if (king_move == "e8c8") { // e8c8
            // ROOK a8d8
            b[7, 3] = b[7, 0];
            b[7, 0] = 0;

            mv.MOVE_BIT_REP |= 0b__000000_0010;

            bking_pos = (7, 2);
        }

        else if (king_move == "e8g8") { //e8g8
            // ROOK h8f8
            b[7, 5] = b[7, 7];
            b[7, 7] = 0;  

            mv.MOVE_BIT_REP |= 0b__000000_0001;

            bking_pos = (7, 6);
        }
    }

    public void advanced_undo_move() {
        Move prev_mv = move_list[move_list.Count-1];
        
        move_list.RemoveAt(move_list.Count-1);
        
        turn = !turn;
        turn_id = turn ? white_id : black_id;

        if (!(prev_mv.start_square.row <= 7 && prev_mv.start_square.row >= 0 && prev_mv.start_square.col <= 7 && prev_mv.start_square.col >= 0 
            && prev_mv.end_square.row   <= 7 && prev_mv.end_square.row   >= 0 && prev_mv.end_square.col   <= 7 && prev_mv.end_square.col   >= 0)) return;

        b[prev_mv.start_square.row, prev_mv.start_square.col] = b[prev_mv.end_square.row, prev_mv.end_square.col];
        b[prev_mv.end_square.row, prev_mv.end_square.col] = prev_mv.end_square.piece;

        if (prev_mv.promote != -1) {
            b[prev_mv.start_square.row, prev_mv.start_square.col] = prev_mv.start_square.piece;
        }

        if (prev_mv.isEnpassant) {
            int forward_pawn_dir = turn ? -1 : 1;

            b[prev_mv.end_square.row + forward_pawn_dir, prev_mv.end_square.col] = prev_mv.start_square.piece;
        }

        if (prev_mv.start_square.piece_type != 6 && prev_mv.start_square.piece_type != 2) return;
           
        if (prev_mv.MOVE_BIT_REP != 0) {
                if ((prev_mv.MOVE_BIT_REP & (1 << 9)) != 0) {  is_a1_rook_moved = false; }
            else if ((prev_mv.MOVE_BIT_REP & (1 << 8)) != 0) {  is_h1_rook_moved = false; }
            else if ((prev_mv.MOVE_BIT_REP & (1 << 7)) != 0) {  is_a8_rook_moved = false; }
            else if ((prev_mv.MOVE_BIT_REP & (1 << 6)) != 0) {  is_h8_rook_moved = false; }
            else if ((prev_mv.MOVE_BIT_REP & (1 << 5)) != 0) {  is_wking_moved = false; }
            else if ((prev_mv.MOVE_BIT_REP & (1 << 4)) != 0) {  is_bking_moved = false; } 
        }

        if (prev_mv.start_square.piece_type != 6) return;

        wking_pos = prev_mv.wking_pos;
        bking_pos = prev_mv.bking_pos;

        if ((prev_mv.MOVE_BIT_REP & 0b1000) != 0) {
            // ROOK a1d1
            b[0, 0] = b[0, 3];
            b[0, 3] = 0;
        }

        else if ((prev_mv.MOVE_BIT_REP & 0b0100) != 0) {
            // ROOK h1f1
            b[0, 7] = b[0, 5];
            b[0, 5] = 0;
        }


        else if ((prev_mv.MOVE_BIT_REP & 0b0010) != 0) {
            // ROOK a8d8
            b[7, 0] = b[7, 3];
            b[7, 3] = 0;
        }
        
        else if ((prev_mv.MOVE_BIT_REP & 0b0001) != 0) {
            // ROOK h8f8
            b[7, 7] = b[7, 5];
            b[7, 5] = 0;  
        }

    }



    private void mv_flip_turn(Move mv, bool flip_turn) {
        if (!flip_turn) return;

        move_list.Add(mv);
        turn = !turn;

        if (turn)  turn_id = white_id;
        else turn_id = black_id;
    }

    public bool isEqualToBoard(int[,] other_board)
    {
        for (int i = 0; i < b.GetLength(0); i++)
        {
            for (int j = 0; j < b.GetLength(1); j++)
            {
                if (b[i, j] != other_board[i, j])
                {
                    return false;
                }
            }
        }

        return true;
    }
}
