using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PIECE_CONTROLLER : MonoBehaviour
{
    // OTHER SCRIPTS
    private TOOLS tools;
    private Board b;

    private bool isDragging = false;
    private Vector3 starting_position;
    private Vector3 offset = new Vector3(0, 0, 0);

    

    private void Start() {
        b = FindObjectOfType<GAME>().b; // Get Game board
        tools = new TOOLS();
    }


    private void Update() {
        if (isDragging && Input.GetMouseButton(0)) {
            DragPiece();
        }
        else if (Input.GetMouseButtonDown(0) && IsMouseOnObject()) {
            isDragging = true;
            starting_position = transform.position;
            b.MovingPiece(name);
        }
        else if (isDragging && Input.GetMouseButtonUp(0)) {
            isDragging = false;
            Piece_Drop();
        }
    }


    private bool IsMouseOnObject() {
        // Converts mouse position to a ray (vector) to see where its pointing
        Vector2 mouse_position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouse_position, Vector2.zero);

        if (hit.collider != null) {
            return hit.collider.gameObject == gameObject;
        }

        return false;
    }

    private void DragPiece() {
        Vector2 mouse_position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 mouse_world_position = new Vector3(mouse_position.x, mouse_position.y, 0);


        transform.position = mouse_world_position + offset;
    }

    private void Piece_Drop() {
        int x = (int)Mathf.Round(transform.localPosition.x);
        int y = (int)Mathf.Round(transform.localPosition.y);

        String square = tools.uci_converter(new List<int>() {y,x});

        bool outcome = b.DroppedPiece(name, square);

        if (outcome) {
            transform.localPosition = new Vector2(x, y);
            name = square;
        }
        else {
            transform.position = starting_position;
        }
    }

    public void ExternalMove(List<int> new_position) {
        transform.localPosition = new Vector2(new_position[1], new_position[0]);
        name = tools.uci_converter(new_position);
    }
}
