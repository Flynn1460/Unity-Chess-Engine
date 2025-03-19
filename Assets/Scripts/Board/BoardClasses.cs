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
    /*
    To store in binary

    -----CCCRRRWPPPP
    */

    private ushort square_info;

    public Square(int[,] board, int col, int row) {
        if (row >= 0 && row <= 7 && col >= 0 && col <= 7)
        {
            int piece = board[row, col];
            bool isWhite = piece < 7;

            this.square_info = (ushort)(
                (piece & 0b1111) | 
                ((isWhite ? 0b1 : 0b0) << 4) | 
                ((row & 0b111) << 5) | 
                ((col & 0b111) << 8)
            );

        }
        else this.square_info = 0b_000_000_0_0111;
    }

    public Square(int piece, int col, int row) {
        this.square_info = (ushort)(
            (piece & 0b1111) | 
            (((piece < 7) ? 0b1 : 0b0) << 4) | 
            ((row & 0b111) << 5) | 
            ((col & 0b111) << 8)
        );
    }

    public Square(Board board, string sq_name, MBool col_rep=MBool.N) {
        char col_char = sq_name[0];
        char row_char = sq_name[1];

        int col = col_char - 'a';
        int row = (int)Char.GetNumericValue(row_char) - 1;

        int piece = board.b[row, col];

        bool isWhite;
        if (col_rep == MBool.N) isWhite = piece < 7;
        else if (col_rep == MBool.T) isWhite = board.turn;
        else if (col_rep == MBool.F) isWhite = !board.turn;
        else {  isWhite = board.turn;  }

        this.square_info = (ushort)(
            (piece & 0b1111) | 
            ((isWhite ? 0b1 : 0b0) << 4) | 
            ((row & 0b111) << 5) | 
            ((col & 0b111) << 8)
        );

    }


    // Getters
    public int GetCol() =>        (square_info >> 8) & 0b111;   
    public int GetRow() =>        (square_info >> 5) & 0b111;   
    public (int, int) GetSq() =>  ((square_info >> 8) & 0b111, (square_info >> 5) & 0b111);  // (col, row) 
    public bool GetIsWhite() =>   ((square_info >> 4) & 0b1) == 1 ? true : false;   
    public int GetPieceType() =>  (square_info & 0b1111)%7;   // piece % 7
    public int GetStartRow() =>   ((square_info >> 4) & 0b1) == 1 ? 1 : 6;   // iswhite ? 1 : 6

    public int GetPiece() =>  (square_info & 0b1111) != 7 ? (square_info & 0b1111) : -1;   

    public int GetRAW() => square_info >> 5;

    // Setters
    public void SetPiece(int new_piece) => square_info = (ushort)((square_info & ~0b1111) | (new_piece & 0b1111));    

    public Square copy() => this;

    public override string ToString() => str_uci();
    public string str_uci() => $"{(char)('a' + GetCol())}{GetRow() + 1}";
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
    public HASH board_before_HASH;
    public (int, int) wking_pos;
    public (int, int) bking_pos;

    public uint MOVE_BIT_REP = 0b_000000;

    // First two for generic move, next two for castling or en passant
    public int[] board_mv_changes = new int[4];

    public string changed_castle_bool;

    public bool is_in_bounds;


    // DECLARATIONS
    public Move(Board board, int col1, int row1, int col2, int row2) {
        start_square = new Square( board.b, col1, row1 );
        end_square = new Square( board.b, col2, row2 );

        is_in_bounds = true;
        if (start_square.GetPiece() == -1 || end_square.GetPiece() == -1) {
            is_in_bounds = false;
        }
    }

    public Move(Board board, Square start_square_, int col2, int row2) {
        start_square = start_square_;
        end_square = new Square( board.b, col2, row2 );

        is_in_bounds = true;
        if (start_square.GetPiece() == -1 || end_square.GetPiece() == -1) {
            is_in_bounds = false;
        }
    }

    public Move(Square start_square_, Square end_square_, int promotion=5) {
        start_square = start_square_;
        end_square = end_square_;

        if ((end_square.GetRow() == 7 && start_square.GetPiece() == 1) || (end_square.GetRow() == 0 && start_square.GetPiece() == 8)) promote = start_square.GetIsWhite() ? promotion : promotion+7;

        is_in_bounds = true;
        if (start_square.GetPiece() == -1 || end_square.GetPiece() == -1) {
            is_in_bounds = false;
        }
    }


    public Move(Board board, string uci_string) {
        start_square = new Square( board, $"{uci_string[0]}{uci_string[1]}" );
        end_square = new Square( board, $"{uci_string[2]}{uci_string[3]} )" );

        // Promoting
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

        // En Passant
        if (start_square.GetPieceType() == 1 && end_square.GetPiece() == 0) {
            if (Math.Abs(start_square.GetCol() - end_square.GetCol()) == 1) {
                isEnpassant = true;
            }
        }

        if (start_square.GetPiece() == -1 || end_square.GetPiece() == -1) {
            is_in_bounds = true;
        }
    }


    public Move() {}
    public Move copy() {
        return new Move {
            start_square = start_square.copy(),
            end_square = end_square.copy(),

            isEnpassant = isEnpassant,
            is_castle_white_short = is_castle_white_short,
            is_castle_white_long = is_castle_white_long,
            is_castle_black_short = is_castle_black_short,
            is_castle_black_long = is_castle_black_long,
            promote = promote
        };
    }

    public override string ToString() => str_uci();
    public string str_uci()
    {
        switch(promote%7) {
            case 2: return (char)('a' + start_square.GetCol()) + (start_square.GetRow()+1).ToString() + (char)('a' + end_square.GetCol()) + (end_square.GetRow()+1).ToString() + 'r';
            case 3: return (char)('a' + start_square.GetCol()) + (start_square.GetRow()+1).ToString() + (char)('a' + end_square.GetCol()) + (end_square.GetRow()+1).ToString() + 'n';
            case 4: return (char)('a' + start_square.GetCol()) + (start_square.GetRow()+1).ToString() + (char)('a' + end_square.GetCol()) + (end_square.GetRow()+1).ToString() + 'b';
            case 5: return (char)('a' + start_square.GetCol()) + (start_square.GetRow()+1).ToString() + (char)('a' + end_square.GetCol()) + (end_square.GetRow()+1).ToString() + 'q';
        }
        return (char)('a' + start_square.GetCol()) + (start_square.GetRow()+1).ToString() + (char)('a' + end_square.GetCol()) + (end_square.GetRow()+1).ToString();
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

    public HashGen hashGen = new HashGen();

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

    public bool custom_fen = false;
    public string SetupFen = "NULL";

    // Debug / Setup
    public void set_fen(string FEN) {
        SetupFen = FEN;

        if (SetupFen != "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1") {custom_fen = true;}

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

        if (mg != null) output += "LEGAL MOVES: "+String.Join(" ", mg.GenerateLegalMoves(this)) + "\n\n";

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

        board_cpy.custom_fen = custom_fen;
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
        if (mv.start_square.GetRow() <= 7 && mv.start_square.GetRow() >= 0 && mv.start_square.GetCol() <= 7 && mv.start_square.GetCol() >= 0 &&
            mv.end_square.GetRow()   <= 7 && mv.end_square.GetRow()   >= 0 && mv.end_square.GetCol()   <= 7 && mv.end_square.GetCol()   >= 0) {
        
        mv.board_before_move = (int[,])b.Clone();
        mv.board_before_HASH = hashGen.MAKE_HASH(this);

        mv.wking_pos = wking_pos;
        mv.bking_pos = bking_pos;

        if (mv.promote == -1) {
            if (mv.isEnpassant) {
                int forward_pawn_dir = turn ? -1 : 1;
                b[mv.end_square.GetRow() + forward_pawn_dir, mv.end_square.GetCol()] = 0;
            }

            mv.replaced_piece = b[mv.end_square.GetRow(), mv.end_square.GetCol()];
            b[mv.end_square.GetRow(), mv.end_square.GetCol()] = b[mv.start_square.GetRow(), mv.start_square.GetCol()];
            b[mv.start_square.GetRow(), mv.start_square.GetCol()] = 0;

            if (mv.start_square.GetPieceType() != 2 && mv.start_square.GetPieceType() != 6) {
                mv_flip_turn(mv, flip_turn);
                return;
            }

            if (mv.start_square.GetPiece() == 6 ) {  wking_pos = (mv.end_square.GetRow(), mv.end_square.GetCol());  }
            if (mv.start_square.GetPiece() == 13) {  bking_pos = (mv.end_square.GetRow(), mv.end_square.GetCol());  }
        }
        else {
            b[mv.end_square.GetRow(), mv.end_square.GetCol()] = mv.promote;
            b[mv.start_square.GetRow(), mv.start_square.GetCol()] = 0;

            mv_flip_turn(mv, flip_turn);
            return;
        }
        

        // ROOKS
        if (mv.start_square.GetPieceType() == 2) {
            if (!is_a1_rook_moved && mv.start_square.GetSq() == (0, 0)) {  is_a1_rook_moved = true; mv.changed_castle_bool = "a1r"; mv_flip_turn(mv, flip_turn); return;  }
            if (!is_h1_rook_moved && mv.start_square.GetSq() == (7, 0)) {  is_h1_rook_moved = true; mv.changed_castle_bool = "h1r"; mv_flip_turn(mv, flip_turn); return;  }
            
            if (!is_a8_rook_moved && mv.start_square.GetSq() == (0, 7)) {  is_a8_rook_moved = true; mv.changed_castle_bool = "a8r"; mv_flip_turn(mv, flip_turn); return;  }
            if (!is_h8_rook_moved && mv.start_square.GetSq() == (7, 7)) {  is_h8_rook_moved = true; mv.changed_castle_bool = "h8r"; mv_flip_turn(mv, flip_turn); return;  }
            
            mv_flip_turn(mv, flip_turn); 
            return;
        }


        // KINGS
        if (!is_wking_moved && mv.start_square.GetSq() == (4, 0)) {   is_wking_moved = true; mv.changed_castle_bool = "wk";  }
        if (!is_bking_moved && mv.start_square.GetSq() == (4, 7)) {   is_bking_moved = true; mv.changed_castle_bool = "bk";  }

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
        
        mv.board_before_HASH = hashGen.MAKE_HASH(this);
        
        mv.wking_pos = wking_pos;
        mv.bking_pos = bking_pos;

        if (mv.promote == -1) {
            b[mv.end_square.GetRow(), mv.end_square.GetCol()] = b[mv.start_square.GetRow(), mv.start_square.GetCol()];
            b[mv.start_square.GetRow(), mv.start_square.GetCol()] = 0;

            if (mv.isEnpassant) {
                int forward_pawn_dir = turn ? 1 : -1;

                mv.start_square.SetPiece(   b[mv.end_square.GetRow() + forward_pawn_dir, mv.end_square.GetCol()]   );
                b[mv.end_square.GetRow() + forward_pawn_dir, mv.end_square.GetCol()] = 0;
            }

            if (mv.start_square.GetPieceType() != 2 && mv.start_square.GetPieceType() != 6) return;

            wking_pos = (mv.start_square.GetPiece() == 6) ? (mv.end_square.GetRow(), mv.end_square.GetCol()) : wking_pos;
            bking_pos = (mv.start_square.GetPiece() == 13) ? (mv.end_square.GetRow(), mv.end_square.GetCol()) : bking_pos;

        }
        else {
            b[mv.end_square.GetRow(), mv.end_square.GetCol()] = mv.promote;
            b[mv.start_square.GetRow(), mv.start_square.GetCol()] = 0;

            return;
        }
        
        // ROOKS
        if (mv.start_square.GetPieceType() == 2) {
            if (!is_a1_rook_moved && mv.start_square.GetSq() == (0, 0)) {  
                is_a1_rook_moved = true; 
                mv.MOVE_BIT_REP |= 0b_100000;
            }

            else if (!is_h1_rook_moved && mv.start_square.GetSq() == (7, 0)) {  
                is_h1_rook_moved = true; 
                mv.MOVE_BIT_REP |= 0b_010000;
            }
            
            else if (!is_a8_rook_moved && mv.start_square.GetSq() == (0, 7)) {  
                is_a8_rook_moved = true; 
                mv.MOVE_BIT_REP |= 0b_001000;
            
            }

            else if (!is_h8_rook_moved && mv.start_square.GetSq() == (7, 7)) {  
                is_h8_rook_moved = true; 
                mv.MOVE_BIT_REP |= 0b_000100;
            }
            
            return;
        }

        // KINGS
        if (!is_wking_moved && mv.start_square.GetSq() == (4, 0)) {   is_wking_moved = true; mv.MOVE_BIT_REP |= 0b_000010;  }
        else if (!is_bking_moved && mv.start_square.GetSq() == (4, 7)) {   is_bking_moved = true; mv.MOVE_BIT_REP |= 0b_000001;  }

        
        if (mv.is_castle_white_long) {  // e1c1
            b[0, 3] = b[0, 0];
            b[0, 0] = 0;

            wking_pos = (0, 2);
        }
        else if (mv.is_castle_white_short) { // e1g1
            b[0, 5] = b[0, 7];
            b[0, 7] = 0;

            wking_pos = (0, 6);
        }
        else if (mv.is_castle_black_long) { // e8c8
            b[7, 3] = b[7, 0];
            b[7, 0] = 0;

            bking_pos = (7, 2);
        }
        else if (mv.is_castle_black_short) { // e8g8
            b[7, 5] = b[7, 7];
            b[7, 7] = 0;

            bking_pos = (7, 6);
        }
    }

    public void advanced_undo_move() {
        Move prev_mv = move_list[move_list.Count-1];
        
        move_list.RemoveAt(move_list.Count-1);
        
        turn = !turn;
        turn_id = turn ? white_id : black_id;

        b[prev_mv.start_square.GetRow(), prev_mv.start_square.GetCol()] = b[prev_mv.end_square.GetRow(), prev_mv.end_square.GetCol()];
        b[prev_mv.end_square.GetRow(), prev_mv.end_square.GetCol()] = prev_mv.end_square.GetPiece();

        if (prev_mv.promote != -1) {
            b[prev_mv.start_square.GetRow(), prev_mv.start_square.GetCol()] = prev_mv.start_square.GetPiece();
        }

        if (prev_mv.isEnpassant) {
            int forward_pawn_dir = turn ? -1 : 1;

            b[prev_mv.end_square.GetRow() + forward_pawn_dir, prev_mv.end_square.GetCol()] = prev_mv.start_square.GetPiece();
        }

        if (prev_mv.start_square.GetPieceType() != 6 && prev_mv.start_square.GetPieceType() != 2) return;
           
        if (prev_mv.MOVE_BIT_REP != 0) {
                if ((prev_mv.MOVE_BIT_REP & (1 << 5)) != 0) {  is_a1_rook_moved = false; }
            else if ((prev_mv.MOVE_BIT_REP & (1 << 4)) != 0) {  is_h1_rook_moved = false; }
            else if ((prev_mv.MOVE_BIT_REP & (1 << 3)) != 0) {  is_a8_rook_moved = false; }
            else if ((prev_mv.MOVE_BIT_REP & (1 << 2)) != 0) {  is_h8_rook_moved = false; }
            else if ((prev_mv.MOVE_BIT_REP & (1 << 1)) != 0) {  is_wking_moved = false; }
            else if ((prev_mv.MOVE_BIT_REP & (1 << 0)) != 0) {  is_bking_moved = false; } 
        }

        if (prev_mv.start_square.GetPieceType() != 6) return;

        wking_pos = prev_mv.wking_pos;
        bking_pos = prev_mv.bking_pos;

        if (prev_mv.is_castle_white_long) {
            b[0, 0] = b[0, 3];
            b[0, 3] = 0;
        }
        else if (prev_mv.is_castle_white_short) {
            b[0, 7] = b[0, 5];
            b[0, 5] = 0;
        }
        else if (prev_mv.is_castle_black_long) {
            b[7, 0] = b[7, 3];
            b[7, 3] = 0;
        }
        else if (prev_mv.is_castle_black_short) {
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
        UnityEngine.Debug.Log(other_board[1,1]);

        for (int i = 0; i<8; i++)
        {
            for (int j = 0; j<8; j++)
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
