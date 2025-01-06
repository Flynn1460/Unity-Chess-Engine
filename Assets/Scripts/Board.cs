#pragma warning disable CS0219

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class Board
{
    private TOOLS tools = new TOOLS();
    private BoardHighlighter boardHighlighter = new BoardHighlighter();

    private Sprite queen_texture = Resources.Load<Sprite>("Chess/wq");

    public bool TURN = true;
    public int[,] BOARD = new int[8,8] {
            { 2,  3,  4,  5,  6,  4,  3, 2 },
            { 1,  1,  1,  1,  1,  1,  1, 1 },
            { 0,  0,  0,  0,  0,  0,  0, 0 },
            { 0,  0,  0,  0,  0,  0,  0, 0 },
            { 0,  0,  0,  0,  0,  0,  0, 0 },
            { 0,  0,  0,  0,  0,  0,  0, 0 },
            { 7,  7,  7,  7,  7,  7,  7, 7 },
            { 8,  9, 10, 11, 12, 10,  9, 8 }
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


    private MoveGenerator moveGenerator = new MoveGenerator();
    
    public Board(string FEN="8/8/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1") {
        set_fen(FEN);
    }

    private void set_fen(string FEN) {
        int f = 0; //Files
        int r = 7; //Ranks
        bool turn = false;

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
                if (turn_char == 'w') turn = true;
                if (turn_char == 'b') turn = false;
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
                    case 'p': BOARD[r, f] = 7; break;
                    case 'r': BOARD[r, f] = 8; break;
                    case 'n': BOARD[r, f] = 9; break;
                    case 'b': BOARD[r, f] = 10; break;
                    case 'q': BOARD[r, f] = 11; break;
                    case 'k': BOARD[r, f] = 12; break;
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
        return moveGenerator.GenerateLegalMoves(BOARD, TURN);
    }


    public void MovingPiece(string piecePosition) {
        List<String> piece_legal_moves = moveGenerator.GenerateLegalMoves_ForPiece(BOARD, TURN, tools.uci_converter(piecePosition));
        List<String> stripped_moves = tools.strip_moves(piece_legal_moves, false, include_promotion:false); // Get list of moves, ignore promotion data

        Debug.Log(String.Join(", ", piece_legal_moves));
        boardHighlighter.Highlight_Tiles(stripped_moves);
    }

    public bool DroppedPiece(string old_piece_position, string new_piece_position) {
        List<String> piece_legal_moves = moveGenerator.GenerateLegalMoves_ForPiece(BOARD, TURN, tools.uci_converter(old_piece_position));
        List<String> stripped_moves = tools.strip_moves(piece_legal_moves, false);

        boardHighlighter.Reset_Tiles();

        List<int> old_position = tools.uci_converter(old_piece_position);
        List<int> new_position = tools.uci_converter(new_piece_position);


        if (stripped_moves.Contains(new_piece_position)) {
            //TURN = !TURN;
            int piece = BOARD[old_position[0], old_position[1]];

            BOARD[new_position[0], new_position[1]] = piece;
            BOARD[old_position[0], old_position[1]] = 0;

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

            return true;
        }
        else if (stripped_moves.Contains(new_piece_position + "=Q")) {
            new_piece_position += "=Q";

            if(new_piece_position.Substring(2,2) == "=Q") {
                BOARD[new_position[0], new_position[1]] = 5; // QUEEN
                BOARD[old_position[0], old_position[1]] = 0;

                // CHANGE TEXTURE
                GameObject piece_obj = GameObject.Find(old_piece_position);
                piece_obj.GetComponent<SpriteRenderer>().sprite = queen_texture;
            }

            if(new_piece_position.Substring(2,2) == "=N") Debug.Log("Knight");
            if(new_piece_position.Substring(2,2) == "=R") Debug.Log("Rook");
            if(new_piece_position.Substring(2,2) == "=B") Debug.Log("Bishop");
            
            return true;
        }
        else{
            return false;
        }
    }


}
