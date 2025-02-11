using System;
using System.Collections.Generic;
using UnityEngine;

public class CONTROLLER_Piece : MonoBehaviour
{
    // OTHER SCRIPTS
    private BoardManager bm;

    // SERIALIZED FIELDS
    [Header("Pawn Promotional Textures")]
    [SerializeField] private Sprite QUEEN_TEXTURE;
    [SerializeField] private Sprite KNIGHT_TEXTURE;
    [SerializeField] private Sprite BISHOP_TEXTURE;
    [SerializeField] private Sprite ROOK_TEXTURE;

    [SerializeField] private int PIECE_COLOUR;

    // GENERAL
    private Vector3 offset = Vector3.zero;
    private Vector3 starting_position = Vector3.zero;
    private bool is_mouse_dragging_piece = false;


    // START & UPDATE UNITY FUNCTIONS
    private void Start() {
        bm = FindFirstObjectByType<GAME>().board_manager; // Get Game board
    }

    private void Update() {
        // If Piece is clicked
        if (Input.GetMouseButtonDown(0) && is_mouse_over_piece()) {
            is_mouse_dragging_piece = true;
            starting_position = transform.position;
            GetComponent<SpriteRenderer>().sortingOrder = 1; // Have the moving piece on top of others.

            bm.Highlight_Piece_Moves(new Square(bm.board, name));
        }

        // If Piece is held
        else if (Input.GetMouseButton(0) && is_mouse_dragging_piece) {
            Piece_Drag();
        }

        // If Piece is dropped
        else if (is_mouse_dragging_piece && Input.GetMouseButtonUp(0)) {
            is_mouse_dragging_piece = false;
            Piece_Drop();
        }
    }

    private bool is_mouse_over_piece() {
        // Converts mouse position to a ray (vector) to see where its pointing
        Vector2 mouse_position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouse_position, Vector2.zero);

        if (hit.collider != null) return hit.collider.gameObject == gameObject;
        return false;
    }


    // PIECE MOVING
    private void Piece_Drag() {
        Vector2 mouse_position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 mouse_world_position = new Vector3(mouse_position.x, mouse_position.y, 0);
        
        // Move Piece
        transform.position = mouse_world_position + offset;
    }

    private void Piece_Drop() {
        int col = (int) Mathf.Round(transform.localPosition.x);
        int row = (int) Mathf.Round(transform.localPosition.y);

        Square start_square = new Square(bm.board, name);
        Square end_square = new Square(bm.board, col, row);
        Move piece_move = new Move(bm.board, start_square, end_square);

        int outcome = DroppedPiece(piece_move);
        GetComponent<SpriteRenderer>().sortingOrder = 0; // Plant piece

        if (outcome == 1) { // MOVE ACCEPTED
            transform.localPosition = new Vector2(col, row);

            GameObject existing_piece = GameObject.Find(end_square.ToString());
            if (existing_piece != null) Destroy(existing_piece);
            name = end_square.ToString();
        }

        else if (outcome == 2) { // MOVE ACCEPTED - QUEEN PROMOTION
            
        }

        else { // MOVE DENIED
            transform.position = starting_position;
        }
    }


    public int DroppedPiece(Move piece_move) {
        List<Move> piece_legal_moves = bm.GenerateLegalMoves(filter_square:piece_move.start_square);

        foreach(Move move in piece_legal_moves) {
            if (move.str_uci() == piece_move.str_uci()) {
                bm.Push(move);
                return 1;
            }
        }

        return 0;
    }
}
