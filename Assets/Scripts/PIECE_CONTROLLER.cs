using System;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class PIECE_CONTROLLER : MonoBehaviour
{
    // OTHER SCRIPTS
    private TOOLS tools;
    private Board b;

    // SERIALIZED FIELDS
    [Header("Pawn Promotional Textures")]
    [SerializeField] private Sprite QUEEN_TEXTURE;
    [SerializeField] private Sprite KNIGHT_TEXTURE;
    [SerializeField] private Sprite BISHOP_TEXTURE;
    [SerializeField] private Sprite ROOK_TEXTURE;

    [SerializeField] private int PIECE_COLOUR;

    // GENERAL
    private Vector3 offset = new Vector3(0, 0, 0);
    private Vector3 starting_position = new Vector3(0, 0, 0);
    private bool is_mouse_dragging_obj = false;


    // START & UPDATE UNITY FUNCTIONS
    private void Start() {
        b = FindObjectOfType<GAME>().board; // Get Game board
        tools = new TOOLS();
    }

    private void Update() {
        // If Piece is clicked
        if (Input.GetMouseButtonDown(0) && is_mouse_over_obj()) {
            is_mouse_dragging_obj = true;
            starting_position = transform.position;
            GetComponent<SpriteRenderer>().sortingOrder = 1; // Have the moving piece on top of others.

            b.MovingPiece(name);
        }

        // If Piece is held
        else if (Input.GetMouseButton(0) && is_mouse_dragging_obj) {
            DragPiece();
        }

        // If Piece is dropped
        else if (is_mouse_dragging_obj && Input.GetMouseButtonUp(0)) {
            is_mouse_dragging_obj = false;
            Piece_Drop();
        }
    }

    // HELPER FUNCTIONS
    private bool is_mouse_over_obj() {
        // Converts mouse position to a ray (vector) to see where its pointing
        Vector2 mouse_position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouse_position, Vector2.zero);

        if (hit.collider != null) return hit.collider.gameObject == gameObject;
        return false;
    }


    // PIECE MOVING
    private void DragPiece() {
        Vector2 mouse_position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 mouse_world_position = new Vector3(mouse_position.x, mouse_position.y, 0);
        
        // Move Piece
        transform.position = mouse_world_position + offset;
    }

    private void Piece_Drop() {
        int x = (int)Mathf.Round(transform.localPosition.x);
        int y = (int)Mathf.Round(transform.localPosition.y);

        String uci_square = tools.uci_converter(new List<int>() {y,x});
        int outcome = DroppedPiece(name, uci_square);

        GetComponent<SpriteRenderer>().sortingOrder = 0; // Plant piece

        if (outcome == 1) { // MOVE ACCEPTED
            transform.localPosition = new Vector2(x, y);

            GameObject existing_piece = GameObject.Find(uci_square);
            if (existing_piece != null) Destroy(existing_piece);

            name = uci_square;
        }

        else if (outcome == 2) { // MOVE ACCEPTED - QUEEN PROMOTION
            
        }

        else { // MOVE DENIED
            transform.position = starting_position;
        }
    }

    public void ExternalMove(List<int> new_position) {
        transform.localPosition = new Vector2(new_position[1], new_position[0]);
        string name_ = tools.uci_converter(new_position);

        GameObject existing_piece = GameObject.Find(name_);
        if (existing_piece != null) Destroy(existing_piece);

        name = tools.uci_converter(new_position);
    }


    public int DroppedPiece(string old_piece_position, string new_piece_position) {
        List<String> piece_legal_moves = b.GetPieceLegalMoves(tools.uci_converter(old_piece_position));
        List<String> stripped_moves = tools.strip_moves(piece_legal_moves, false);

        // Position Data
        List<int> old_position = tools.uci_converter(old_piece_position);
        List<int> new_position = tools.uci_converter(new_piece_position);


        if (stripped_moves.Contains(new_piece_position)) {
            b.Push(old_position, new_position);
            return 1;
        }

        else if (stripped_moves.Contains(new_piece_position + "=Q")) {
            b.Push(old_position, new_position, pawn_promote_piece: 5+(7*PIECE_COLOUR));
            GetComponent<SpriteRenderer>().sprite = QUEEN_TEXTURE;
            return 1;
        }

        else return 0;
    }
}
