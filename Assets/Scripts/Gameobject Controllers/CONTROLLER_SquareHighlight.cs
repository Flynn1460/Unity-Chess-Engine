#pragma warning disable CS0219

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CONTROLLER_SquareHighlight
{   
    private Transform WHITE_TILE_TRA = GameObject.Find("WHITE_TILES").transform;
    private Transform BLACK_TILE_TRA = GameObject.Find("BLACK_TILES").transform;

    private Color WHITE_COLOUR;
    private Color BLACK_COLOUR;
    private Color32 HIGHLIGHT_COLOUR = new Color32(47, 169, 140, 255);

    List<GameObject> white_tiles = new List<GameObject>();
    List<GameObject> black_tiles = new List<GameObject>();
    
    public CONTROLLER_SquareHighlight() {
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

    public void Reset_Tiles() {
        foreach (GameObject tile in white_tiles) {
            tile.GetComponent<SpriteRenderer>().color = WHITE_COLOUR;
        }
        foreach (GameObject tile in black_tiles) {
            tile.GetComponent<SpriteRenderer>().color = BLACK_COLOUR;
        }
    }
}
