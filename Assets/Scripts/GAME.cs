#pragma warning disable CS0219

using UnityEngine;
using UnityEngine.SceneManagement;

public class GAME : MonoBehaviour
{
    [Header("GAME SETTINGS")]
    [SerializeField] private int ALLOWED_TIME = 180;
    [SerializeField] private int engine_move_time = 100;

    [Range(0,2)][SerializeField] private int white_id;
    [Range(0,2)][SerializeField] private int black_id;

    [SerializeField] private string board_fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    // Board
    public BoardManager board_manager;
    private GameMatcher gamematcher;
    private bool turn;

    // Game Controller Objects
    private CONTROLLER_PieceSetup piece_setup;
    private CONTROLLER_Timer timer_controller;

    
    void Awake() {   
        board_manager = new BoardManager();
        gamematcher = new GameMatcher(engine_move_time);

        board_manager.board.white_id = white_id;
        board_manager.board.black_id = black_id;

        board_manager.board.turn_id = white_id;
    }

    void Start() {
        turn = board_manager.board.turn;
        
        // Setup controllers
        timer_controller = FindFirstObjectByType<CONTROLLER_Timer>(); 
        piece_setup = FindFirstObjectByType<CONTROLLER_PieceSetup>();

        timer_controller.change_timer(ALLOWED_TIME);

        // Setup Board

        if (board_fen == "start") board_fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        board_manager.board.set_fen(board_fen);
        piece_setup.ClearPieces();
        piece_setup.SetupPieces(board_manager.board.b);
    }

    void Update() {
        if (board_manager.board.turn != turn) {
            turn = board_manager.board.turn;
            timer_controller.flip_turn(turn);

            // PIECES

            piece_setup.ClearPieces();
            piece_setup.SetupPieces(board_manager.board.b);
        }

        if (board_manager.board.is_checkmate || timer_controller.IS_TIMEOUT) {
            GameOver();
        }

        // ===== Handle ID =====
        // If ID is 0 it will be handled by BoardManager Automatically
        // If ID is >1 then its an engine to be dealt with by the game matcher
        if (board_manager.board.turn_id > 0) {
            gamematcher.GetEngineMove(board_manager);
        }

    }

    void GameOver() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
