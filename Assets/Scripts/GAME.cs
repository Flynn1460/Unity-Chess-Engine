#pragma warning disable CS0219

using UnityEngine;
using UnityEngine.SceneManagement;

public class GAME : MonoBehaviour
{
    [Header("GAME SETTINGS")]
    [SerializeField] private int ALLOWED_TIME = 180;
    [SerializeField] private int engine_move_time = 100;
    [Range(0,1)][SerializeField] private int white_id = 0;
    [Range(0,1)][SerializeField] private int black_id = 1;


    public BoardManager board_manager;
    private TEXT_TIMER_CONTROLLER timer_controller;
    private GameMatcher gamematcher;
    private Piece_Setup piece_Setup;
    private bool turn;

    void Awake() {   
        board_manager = new BoardManager();
        gamematcher = new GameMatcher(engine_move_time);

        board_manager.board.white_id = white_id;
        board_manager.board.black_id = black_id;

        board_manager.board.turn_id = white_id;
    }

    void Start() {
        turn = board_manager.board.turn;
        
        timer_controller = FindFirstObjectByType<TEXT_TIMER_CONTROLLER>(); 
        timer_controller.change_timer(ALLOWED_TIME);
    }

    void Update() {
        if (board_manager.board.turn != turn) {
            turn = board_manager.board.turn;
            timer_controller.flip_colours(turn);
        }

        if (board_manager.board.is_checkmate || timer_controller.IS_TIMEOUT) {
            GameOver();
        }

        // Handle ID

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
