#pragma warning disable CS0219

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;

public class BoardHighlighter
{   
    private TOOLS tools = new TOOLS();
    private Transform WHITE_TILE_TRA = GameObject.Find("WHITE_TILES").transform;
    private Transform BLACK_TILE_TRA = GameObject.Find("BLACK_TILES").transform;

    private Color WHITE_COLOUR;
    private Color BLACK_COLOUR;
    private Color32 HIGHLIGHT_COLOUR = new Color32(47, 169, 140, 255);

    List<GameObject> white_tiles;
    List<GameObject> black_tiles;
    
    public BoardHighlighter() {
        // SETUP STARTING COLOURS
        Transform white_test_child = WHITE_TILE_TRA.GetChild(0);
        Transform black_test_child = BLACK_TILE_TRA.GetChild(0);

        WHITE_COLOUR = white_test_child.GetComponent<SpriteRenderer>().color;
        BLACK_COLOUR = black_test_child.GetComponent<SpriteRenderer>().color;
    }



    public void Highlight_Tiles(List<String> uci_string_list) {
        white_tiles = new List<GameObject>();
        black_tiles = new List<GameObject>();

        foreach (String uci_string in uci_string_list) {
            foreach(Transform child in WHITE_TILE_TRA) {
                if ((char.ToUpper(uci_string[0])+uci_string.Substring(1)) == child.name) {
                    white_tiles.Add(child.gameObject);
                }
            }
            foreach(Transform child in BLACK_TILE_TRA) {
                if ((char.ToUpper(uci_string[0])+uci_string.Substring(1)) == child.name) {
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
