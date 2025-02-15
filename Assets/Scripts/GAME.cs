#pragma warning disable CS0219

using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GAME : MonoBehaviour
{
    [Header("GAME SETTINGS")]
    [SerializeField] private int ALLOWED_TIME = 180;
    [SerializeField] private int engine_move_time = 100;

    [Range(0,3)][SerializeField] private int white_id;
    [Range(0,3)][SerializeField] private int black_id;
    
    [SerializeField] private bool do_move_scan = false;
    [SerializeField] private bool auto_restart_game = false;

    [SerializeField] private string board_fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    // Board
    public BoardManager board_manager;
    private GameMatcher gamematcher;
    private MoveGenerator move_generator;

    private bool turn;
    private bool isGameOver = false;

    // Game Controller Objects
    private CONTROLLER_PieceSetup piece_setup;
    private CONTROLLER_Timer timer_controller;


    void RunLegalMoves() {
        int x = 0;

        board_manager.board.PrintBoard();

        while (true) {
            x += 1;
            Debug.Log(board_manager.move_generator.GenerateLegalPly(board_manager.board, x));
        }
    }

    void Awake() {   
        board_manager = new BoardManager();
        gamematcher = new GameMatcher(engine_move_time);

        board_manager.board.white_id = white_id;
        board_manager.board.black_id = black_id;

        board_manager.board.turn_id = white_id;
    }

    void Start() {
        turn = !board_manager.board.turn;
        
        // Setup controllers
        timer_controller = FindFirstObjectByType<CONTROLLER_Timer>(); 
        piece_setup = FindFirstObjectByType<CONTROLLER_PieceSetup>();

        timer_controller.change_timer(ALLOWED_TIME);

        // Setup Board

        if (board_fen == "start") board_fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        board_manager.board.set_fen(board_fen);
        piece_setup.ClearPieces();
        piece_setup.SetupPieces(board_manager.board.b);

        if (do_move_scan) {
            Thread thread = new Thread(RunLegalMoves);
            thread.Start();
        }
    }


    void Update() {
        if (isGameOver) return;

        if (board_manager.board.turn != turn) {
            piece_setup.ClearPieces();
            piece_setup.SetupPieces(board_manager.board.b);

            board_manager.board.is_checkmate = board_manager.move_generator.isCheckmate(board_manager.board);
            board_manager.board.is_draw = board_manager.move_generator.isDraw(board_manager.board);

            if (board_manager.board.is_draw) {  GameOver(0); return;  }
            if (board_manager.board.is_checkmate != 0 || timer_controller.IS_TIMEOUT) {  GameOver(board_manager.board.is_checkmate); return;  }
        
            turn = board_manager.board.turn;
            timer_controller.flip_turn(turn);

            if (board_manager.board.turn_id > 0) {
                gamematcher.GetEngineMove(board_manager);
            }
        }
    }

    void GameOver(int state) {
        if (state == 1 ) Debug.Log("WHITE CHECKMATE");
        if (state == 0 ) Debug.Log("DRAW");
        if (state == -1) Debug.Log("BLACK CHECKMATE");

        isGameOver = true;

        if (auto_restart_game) RestartButton_PRESSED();
    }

    public void RestartButton_PRESSED() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
