#pragma warning disable CS0219

using UnityEngine;
using UnityEngine.SceneManagement;

public class GAME : MonoBehaviour
{
    [Header("GAME SETTINGS")]
    [SerializeField] private int ALLOWED_TIME = 180;

    public Board board;
    private TEXT_TIMER_CONTROLLER timer_controller;
    private bool turn;


    void Awake()
    {
        board = new Board();
        turn = board.TURN;
    }

    void Start() {   
        timer_controller = FindObjectOfType<TEXT_TIMER_CONTROLLER>(); 
        timer_controller.change_timer(ALLOWED_TIME);
    }

    void Update() {
        if (board.TURN != turn) {
            turn = board.TURN;
            timer_controller.flip_colours(turn);
        }

        if (board.IS_CHECKMATE || timer_controller.IS_TIMEOUT) {
            GameOver();
        }
    }

    void GameOver() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
