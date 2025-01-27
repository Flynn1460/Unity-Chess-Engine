#pragma warning disable CS0219

using System;
using System.Collections.Generic;
using UnityEngine;

public class Board
{
    private TOOLS tools = new TOOLS();
    private BoardHighlighter boardHighlighter = new BoardHighlighter();

    private Sprite queen_texture = Resources.Load<Sprite>("Chess/wq");

    //private bool IS_TURN_RESTRICTIVE = false;

    public bool TURN = true;
    public int[,] BOARD = new int[8,8] {
            { 2,  3,  4,  5,  6,  4,  3, 2 },
            { 1,  1,  1,  1,  1,  1,  1, 1 },
            { 0,  0,  0,  0,  0,  0,  0, 0 },
            { 0,  0,  0,  0,  0,  0,  0, 0 },
            { 0,  0,  0,  0,  0,  0,  0, 0 },
            { 0,  0,  0,  0,  0,  0,  0, 0 },
            { 8,  8,  8,  8,  8,  8,  8, 8 },
            { 9, 10, 11, 12, 13, 11, 10, 9 }
    };

    public List<String> MOVE_LIST = new List<String>();

    public bool IS_CHECKMATE = false;

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


    private MoveGenerator moveGenerator = new MoveGenerator();
    private NewMoveGenerator newmoveGenerator = new NewMoveGenerator();
    
    public Board(string FEN="rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 0 1") {
        //set_fen(FEN);
    }

    private void set_fen(string FEN) {
        int f = 0; //Files
        int r = 7; //Ranks
        BOARD = EMPTY_BOARD;

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
                if (turn_char == 'w') TURN = true;
                if (turn_char == 'b') TURN = false;
                break;
            }

            else {
                switch(i) {
                    // White
                    case 'P': BOARD[r, f] = 1; break;
                    case 'R': BOARD[r, f] = 2; break;
                    case 'N': BOARD[r, f] = 3; break;
                    case 'B': BOARD[r, f] = 4; break;
                    case 'Q': BOARD[r, f] = 5; break;
                    case 'K': BOARD[r, f] = 6; break;

                    // Black
                    case 'p': BOARD[r, f] = 8; break;
                    case 'r': BOARD[r, f] = 9; break;
                    case 'n': BOARD[r, f] = 10; break;
                    case 'b': BOARD[r, f] = 11; break;
                    case 'q': BOARD[r, f] = 12; break;
                    case 'k': BOARD[r, f] = 13; break;
                }
                f += 1;
            }
        }
    }

    public String print_board() {
        String pBoard = "";

        for (int i=7; i>=0; i--) {
            for (int j=0; j<8; j++) {
                switch(BOARD[i, j]) {
                    case 0: pBoard += ".  "; break;

                    case 1: pBoard += "P "; break;
                    case 2: pBoard += "R "; break;
                    case 3: pBoard += "N "; break;
                    case 4: pBoard += "B "; break;
                    case 5: pBoard += "Q "; break;
                    case 6: pBoard += "K "; break;

                    case 7: pBoard += "p "; break;
                    case 8: pBoard += "r "; break;
                    case 9: pBoard += "n "; break;
                    case 10: pBoard += "b "; break;
                    case 11: pBoard += "q "; break;
                    case 12: pBoard += "k "; break;
                }
            }
            pBoard += "\n";
        }

        return pBoard;
    }


    public List<String> GetLegalMoves() {
        return newmoveGenerator.GenerateLegalMoves(BOARD, TURN, MOVE_LIST);

        //return moveGenerator.GenerateLegalMoves(BOARD, TURN);
    }

    public List<String> GetPieceLegalMoves(List<int> SQUARE) {
        return newmoveGenerator.GenerateLegalMoves(BOARD, TURN, MOVE_LIST, filter_square:SQUARE);

        //return moveGenerator.GenerateLegalMoves_ForPiece(BOARD, TURN, SQUARE);
    }

    public bool GetCheckmateCheck() {
        return newmoveGenerator.isCheckmate(BOARD, TURN, MOVE_LIST);
    }

    public void MovingPiece(string piecePosition) {
        List<String> piece_legal_moves = GetPieceLegalMoves(tools.uci_converter(piecePosition));
        List<String> stripped_moves = tools.strip_moves(piece_legal_moves, false, include_promotion:false); // Get list of moves, ignore promotion data

        boardHighlighter.Highlight_Tiles(stripped_moves);
    }

    
    public void Push(List<int> old_position, List<int> new_position, int pawn_promote_piece=-1, bool is_enpas=false){
        // If replace piece is set to something make piece otherwise use what was at the position
        int piece = pawn_promote_piece==-1 ? BOARD[old_position[0], old_position[1]] : pawn_promote_piece;

        BOARD[old_position[0], old_position[1]] = 0;
        BOARD[new_position[0], new_position[1]] = piece;

        if (piece == 1 && new_position[0] == 7) BOARD[new_position[0], new_position[1]] = 5; // WHITE QUEEN PROMO
        if (piece == 8 && new_position[0] == 0) BOARD[new_position[0], new_position[1]] = 12; // BLACK QUEEN PROMO

        MOVE_LIST.Add(tools.uci_converter(old_position, new_position));

        // If move is pushed then highlighting isn't needed
        boardHighlighter.Reset_Tiles();
        TURN = !TURN;

        // KING CASTLING
        if (piece == 6 || piece == 12) {
            String move = tools.uci_converter(new List<int>() {old_position[0], old_position[1], new_position[0], new_position[1]});
            if (move == "e1c1") {
                BOARD[0, 0] = 0; //
                BOARD[0, 3] = 2; // PLACE ROOK

                PIECE_CONTROLLER pc = GameObject.Find("a1").GetComponent<PIECE_CONTROLLER>();
                pc.ExternalMove(new List<int>() {0, 3});
            }   
            if (move == "e1g1") {
                BOARD[0, 7] = 0; //
                BOARD[0, 5] = 2; // PLACE ROOK

                PIECE_CONTROLLER pc = GameObject.Find("h1").GetComponent<PIECE_CONTROLLER>();
                pc.ExternalMove(new List<int>() {0, 5});
            }
        }
    
        // CHECKMATE
        if (GetCheckmateCheck()) {
            IS_CHECKMATE = true;
        }
    } 
}
