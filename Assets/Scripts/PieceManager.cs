using System.Collections.Generic;
using UnityEngine;

public class PieceManager : MonoBehaviour
{
    [Header("Piece Prefabs")]
    [SerializeField] private GameObject WP;
    [SerializeField] private GameObject WR;
    [SerializeField] private GameObject WN;
    [SerializeField] private GameObject WB;
    [SerializeField] private GameObject WQ;
    [SerializeField] private GameObject WK;

    [SerializeField] private GameObject BP;
    [SerializeField] private GameObject BR;
    [SerializeField] private GameObject BN;
    [SerializeField] private GameObject BB;
    [SerializeField] private GameObject BQ;
    [SerializeField] private GameObject BK;

    [Header("bo")]


    // Mapping FEN characters to prefabs
    private Dictionary<int, GameObject> pieceMapping;


    private Transform white_pieces;
    private Transform black_pieces;

    private readonly int[,] starting_board = new int[8,8] {
            { 2,  3,  4,  5,  6,  4,  3, 2 },
            { 1,  1,  1,  1,  1,  1,  1, 1 },
            { 0,  0,  0,  0,  0,  0,  0, 0 },
            { 0,  0,  0,  0,  0,  0,  0, 0 },
            { 0,  0,  0,  0,  0,  0,  0, 0 },
            { 0,  0,  0,  0,  0,  0,  0, 0 },
            { 8,  8,  8,  8,  8,  8,  8, 8 },
            { 9, 10, 11, 12, 13, 11, 10, 9 }
    };


    void Awake() {
        white_pieces = GameObject.Find("WHITE_PIECES").transform;
        black_pieces = GameObject.Find("BLACK_PIECES").transform;

        // Initialize the piece prefab dictionary
        pieceMapping = new Dictionary<int, GameObject>
        {
            { 1, WP }, { 2, WR }, { 3, WN },  { 4, WB },  { 5, WQ },  { 6, WK },  // White pieces
            { 8, BP }, { 9, BR }, { 10, BN }, { 11, BB }, { 12, BQ }, { 13, BK }  // Black pieces
        };
    }

    public void ClearPieces() {

        foreach(Transform child in white_pieces.transform) {  GameObject.Destroy(child.gameObject);  }
        foreach(Transform child in black_pieces.transform) {  GameObject.Destroy(child.gameObject);  }
    }

    public void SetupPieces(int[,] boardArray = null)
    {
        if (boardArray == null) boardArray = starting_board;

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                int pieceId = boardArray[row, col];

                if (pieceId != 0) {
                    Vector3 position = new Vector3(col, row); // Convert array index to world position
                    GameObject piece = Instantiate(pieceMapping[pieceId]);

                    // SET PARENT FOR PIECE

                    if (pieceId > 0 && pieceId < 8) { // WHITE PIECE
                        piece.transform.parent = white_pieces;
                    }
                    if (pieceId >= 8) { // BLACK PIECE
                        piece.transform.parent = black_pieces;
                    }


                    piece.transform.localPosition = position;
                    piece.transform.name = (char)('a' + col) + (row+1).ToString();
                }
            }
        }
    }
}
