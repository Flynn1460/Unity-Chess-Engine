using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SquareHighlight
{   
    private Transform WHITE_TILE_TRA = GameObject.Find("WHITE_TILES").transform;
    private Transform BLACK_TILE_TRA = GameObject.Find("BLACK_TILES").transform;

    private Color WHITE_COLOUR;
    private Color BLACK_COLOUR;
    
    private Color32 HIGHLIGHT_COLOUR = new Color32(47, 169, 140, 255);
    private Color32 PREV_MV_COLOUR = new Color32(252, 149, 199, 255);

    List<GameObject> white_tiles = new List<GameObject>();
    List<GameObject> black_tiles = new List<GameObject>();

    GameObject prev_mv_start_tile;
    GameObject prev_mv_end_tile;

    bool is_prev_mv_start_white;
    bool is_prev_mv_end_white;

    Move prev_mv_saved;
    
    public SquareHighlight() {
        // SETUP STARTING COLOURS
        Transform white_test_child = WHITE_TILE_TRA.GetChild(0);
        Transform black_test_child = BLACK_TILE_TRA.GetChild(0);

        WHITE_COLOUR = white_test_child.GetComponent<SpriteRenderer>().color;
        BLACK_COLOUR = black_test_child.GetComponent<SpriteRenderer>().color;
    }



    public void Highlight_Tiles(List<Move> highlight_moves) {
        Reset_Tiles();

        white_tiles = new List<GameObject>();
        black_tiles = new List<GameObject>();

        foreach (Move move in highlight_moves) {
            foreach(Transform child in WHITE_TILE_TRA) {

                if (move.end_square.str_uci().ToUpper() == child.name) {
                    white_tiles.Add(child.gameObject);
                }

            }
            foreach(Transform child in BLACK_TILE_TRA) {

                if (move.end_square.str_uci().ToUpper() == child.name) {
                    black_tiles.Add(child.gameObject);
                }

            }
        }

        List<GameObject> tiles = white_tiles.Concat(black_tiles).ToList();
        foreach(GameObject tile in tiles) {
            tile.GetComponent<SpriteRenderer>().color = HIGHLIGHT_COLOUR;
        }
    }

    public void Reset_Tiles(bool remove_prev_mv=false) {
        foreach (GameObject tile in white_tiles) {
            tile.GetComponent<SpriteRenderer>().color = WHITE_COLOUR;
        }
        foreach (GameObject tile in black_tiles) {
            tile.GetComponent<SpriteRenderer>().color = BLACK_COLOUR;
        }

        if (remove_prev_mv && prev_mv_start_tile != null && prev_mv_end_tile != null) {
            prev_mv_start_tile.GetComponent<SpriteRenderer>().color = is_prev_mv_start_white ? WHITE_COLOUR : BLACK_COLOUR;
            prev_mv_end_tile.GetComponent<SpriteRenderer>().color = is_prev_mv_end_white ? WHITE_COLOUR : BLACK_COLOUR;
        }
        else if (prev_mv_saved != null) {
            Highlight_Previous_Move(prev_mv_saved);
        }
    }

    public void Highlight_Previous_Move(Move prev_mv) {        
        foreach(Transform child in WHITE_TILE_TRA) {

            if (prev_mv.start_square.str_uci().ToUpper() == child.name) {
                prev_mv_start_tile = child.gameObject;
                is_prev_mv_start_white = true;
            }

            if (prev_mv.end_square.str_uci().ToUpper() == child.name) {
                prev_mv_end_tile = child.gameObject;
                is_prev_mv_end_white = true;
            }
        }
        
        foreach(Transform child in BLACK_TILE_TRA) {

            if (prev_mv.start_square.str_uci().ToUpper() == child.name) {
                prev_mv_start_tile = child.gameObject;
                is_prev_mv_start_white = false;
            }

            if (prev_mv.end_square.str_uci().ToUpper() == child.name) {
                prev_mv_end_tile = child.gameObject;
                is_prev_mv_end_white = false;
            }
        }

        prev_mv_start_tile.GetComponent<SpriteRenderer>().color = PREV_MV_COLOUR;
        prev_mv_end_tile.GetComponent<SpriteRenderer>().color = PREV_MV_COLOUR;

        prev_mv_saved = prev_mv.copy();
    }


}