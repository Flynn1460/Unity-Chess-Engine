using System.Collections.Generic;
using UnityEngine;

public class Piece_Setup : MonoBehaviour
{
    [Header("Piece Prefabs")]
    [SerializeField] public GameObject WP;
    [SerializeField] public GameObject WR;
    [SerializeField] public GameObject WN;
    [SerializeField] public GameObject WB;
    [SerializeField] public GameObject WQ;
    [SerializeField] public GameObject WK;

    [SerializeField] public GameObject BP;
    [SerializeField] public GameObject BR;
    [SerializeField] public GameObject BN;
    [SerializeField] public GameObject BB;
    [SerializeField] public GameObject BQ;
    [SerializeField] public GameObject BK;


    // Mapping FEN characters to prefabs
    private Dictionary<char, GameObject> pieceMapping;


    private GameObject white_pieces;
    private GameObject black_pieces;


    void Start() {
        white_pieces = GameObject.Find("WHITE_PIECES");
        black_pieces = GameObject.Find("BLACK_PIECES");

        // Initialize the piece prefab dictionary
        pieceMapping = new Dictionary<char, GameObject>
        {
            { 'P', WP }, { 'R', WR }, { 'N', WN }, { 'B', WB }, { 'Q', WQ }, { 'K', WK }, // White pieces
            { 'p', BP }, { 'r', BR }, { 'n', BN }, { 'b', BB }, { 'q', BQ }, { 'k', BK }  // Black pieces
        };


        Clear_Pieces();
        SetupPieces();
    }

    public void Clear_Pieces() {

        foreach(Transform child in white_pieces.transform) {  GameObject.Destroy(child.gameObject);  }
        foreach(Transform child in black_pieces.transform) {  GameObject.Destroy(child.gameObject);  }
    }

    public void SetupPieces(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
    {
        // Split the FEN string into rows
        string[] rows = fen.Split('/');

        for (int y = 0; y < rows.Length; y++)
        {
            int x = 0; // Start at the leftmost square of the row

            foreach (char c in rows[y])
            {
                if (c == ' ')
                {
                    // Break out of the loop entirely if a space is encountered
                    return;
                }

                if (char.IsDigit(c))
                {
                    // If the character is a number, skip that many empty squares
                    x += (int)char.GetNumericValue(c);
                }
                else if (pieceMapping.ContainsKey(c))
                {
                    // Determine the parent object based on piece color
                    GameObject parent = char.IsUpper(c) ? white_pieces : black_pieces;

                    // Instantiate the corresponding prefab
                    GameObject piece = Instantiate(pieceMapping[c]);

                    // Set the piece as a child of the parent
                    piece.transform.SetParent(parent.transform);

                    // Set the local position relative to the parent
                    piece.transform.localPosition = new Vector3(x, 7 - y, 0); // Relative to the parent's origin

                    // Create the square name (e.g., "a2", "b7", etc.)
                    string squareName = ((char)('a' + x)).ToString() + (8 - y).ToString();

                    // Set the name of the piece to its square (e.g., "a2", "b7")
                    piece.name = squareName;

                    x++; // Move to the next square
                }
            }
        }
    }

}
