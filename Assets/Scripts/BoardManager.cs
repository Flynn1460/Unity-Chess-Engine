#pragma warning disable CS0219

using System;
using System.Collections.Generic;
using UnityEngine;


public class Board {
    private TOOLS tools = new TOOLS();

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

    public List<String> move_list = new List<String>();

    public bool is_checkmate = false;

    /*
    * -1 # BLACK WIN
    *  0 # DRAW
    *  1 # WHITE WIN
    */
    public int is_gameover = 0; 


    public bool is_a1_rook_moved = false;
    public bool is_a8_rook_moved = false;
    public bool is_h1_rook_moved = false;
    public bool is_h8_rook_moved = false;

    public bool is_wking_moved = false;
    public bool is_bking_moved = false;

    public int white_id = 0;
    public int black_id = 0;
    
    public int turn_id = 0;


    public void reset_board() {
        b = EMPTY_BOARD;
    }

    public void set_fen(string FEN) {
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


    // MOVEMENT
    public void move(List<int> move, bool flip_turn=true) {  move_root(move, flip_turn);  }
    public void move(string move, bool flip_turn=true) {  move_root(tools.NEW_uci_converter(move), flip_turn);  }
    public void move(List<int> start_move, List<int> end_move, bool flip_turn=true)  {  move_root(tools.ls_combine(start_move, end_move), flip_turn);  }


    private void move_root(List<int> move, bool flip_turn) {
        // File, Rank, File, Rank
        b[move[2], move[3]] = b[move[0], move[1]];
        b[move[0], move[1]] = 0;

        move_list.Add(tools.uci_converter(move));

        if (flip_turn) {
            turn = !turn;

            if (turn)  turn_id = white_id;
            if (!turn) turn_id = black_id;
        }
    }



    public List<int> find_value(int piece) {
        for (int r=0; r<8;r++) {
            for (int c=0; c<8; c++) {
                if (b[r, c] == piece) return new List<int>() {r, c};
            }
        }
        return null;
    }
}




public class BoardManager
{
    // Classes
    public Board board = new Board();
    private TOOLS tools = new TOOLS();
    private BoardHighlighter boardHighlighter = new BoardHighlighter();
    public NewMoveGenerator newmoveGenerator = new NewMoveGenerator();


    // Textures
    private Sprite queen_texture = Resources.Load<Sprite>("Chess/wq");

    public List<String> GenerateLegalMoves(List<int> filter_square=null) {

        if (board.turn_id == 0) {
            return newmoveGenerator.GenerateLegalMoves(board, filter_square:filter_square);
        }
        else {
            return new List<String>();
        }
    }


    public void MovingPiece(string piecePosition) {
        if (board.turn_id == 0) {
            List<String> piece_legal_moves = newmoveGenerator.GenerateLegalMoves(board, tools.uci_converter(piecePosition));
            List<String> stripped_moves = tools.strip_moves(piece_legal_moves, false, include_promotion:false); // Get list of moves, ignore promotion data

            boardHighlighter.Highlight_Tiles(stripped_moves);
        }
    }

    
    public void MoveGOPieces(string string_move) {
        List<int> move = tools.uci_converter(string_move);
        string piece_location = string_move.Substring(0,2);
        int piece_num = board.b[move[2], move[3]];

        Debug.Log(String.Join(", ", move) + " - " + piece_location + " - " + piece_num);


        PIECE_CONTROLLER piece = GameObject.Find(piece_location).GetComponent<PIECE_CONTROLLER>();
        piece.ExternalMove(new List<int>() {move[3], move[2]});

        // KING CASTLING
        if (piece_num == 6 || piece_num == 12) {
            if (string_move == "e1c1") {
                board.b[0, 0] = 0; //
                board.b[0, 3] = 2; // PLACE ROOK

                PIECE_CONTROLLER pc = GameObject.Find("a1").GetComponent<PIECE_CONTROLLER>();
                pc.ExternalMove(new List<int>() {0, 3});
            }   
            if (string_move == "e1g1") {
                board.b[0, 7] = 0; //
                board.b[0, 5] = 2; // PLACE ROOK

                PIECE_CONTROLLER pc = GameObject.Find("h1").GetComponent<PIECE_CONTROLLER>();
                pc.ExternalMove(new List<int>() {0, 5});
            }
            if (string_move == "e8c8") {
                board.b[7, 0] = 0; //
                board.b[7, 3] = 2; // PLACE ROOK

                PIECE_CONTROLLER pc = GameObject.Find("a8").GetComponent<PIECE_CONTROLLER>();
                pc.ExternalMove(new List<int>() {7, 3});
            }   
            if (string_move == "e8g8") {
                board.b[7, 7] = 0; //
                board.b[7, 5] = 2; // PLACE ROOK

                PIECE_CONTROLLER pc = GameObject.Find("h8").GetComponent<PIECE_CONTROLLER>();
                pc.ExternalMove(new List<int>() {7, 5});
            }
        }
    }

    public void Push(List<int> old_position, List<int> new_position, int pawn_promote_piece=-1, bool is_enpas=false){
        // If replace piece is set to something make piece otherwise use what was at the position
        int piece = pawn_promote_piece == -1 ? board.b[old_position[0], old_position[1]] : pawn_promote_piece;

        board.move(old_position, new_position);

        if (piece == 1 && new_position[0] == 7) board.b[new_position[0], new_position[1]] = 5; // WHITE QUEEN PROMO
        if (piece == 8 && new_position[0] == 0) board.b[new_position[0], new_position[1]] = 12; // BLACK QUEEN PROMO

        
        // If move is pushed then highlighting isn't needed
        boardHighlighter.Reset_Tiles();

        // KING CASTLING
        if (piece == 6 || piece == 12) {
            String move = tools.uci_converter(new List<int>() {old_position[0], old_position[1], new_position[0], new_position[1]});
            if (move == "e1c1") {
                board.b[0, 0] = 0; //
                board.b[0, 3] = 2; // PLACE ROOK

                PIECE_CONTROLLER pc = GameObject.Find("a1").GetComponent<PIECE_CONTROLLER>();
                pc.ExternalMove(new List<int>() {0, 3});
            }   
            if (move == "e1g1") {
                board.b[0, 7] = 0; //
                board.b[0, 5] = 2; // PLACE ROOK

                PIECE_CONTROLLER pc = GameObject.Find("h1").GetComponent<PIECE_CONTROLLER>();
                pc.ExternalMove(new List<int>() {0, 5});
            }
        }
    
        // CHECKMATE
        if (newmoveGenerator.isCheckmate(board)) {
            board.is_checkmate = true;
        }
    } 
}
