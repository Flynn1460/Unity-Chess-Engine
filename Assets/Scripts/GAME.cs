#pragma warning disable CS0219

using UnityEngine;
using UnityEngine.SceneManagement;

public class GAME : MonoBehaviour
{
    [Header("GAME SETTINGS")]
    [SerializeField] private int ALLOWED_TIME = 180;

    public BoardManager board_manager;
    private TEXT_TIMER_CONTROLLER timer_controller;
    private bool turn;

    void Awake() {   
        board_manager = new BoardManager();
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
    }

    void GameOver() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
