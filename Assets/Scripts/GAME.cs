using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Diagnostics;

public class GAME : MonoBehaviour
{
    [Header("GAME SETTINGS")]
    [SerializeField] private int ALLOWED_TIME = 180;
    [SerializeField] private int engine_move_time = 100;

    [Range(0,3)][SerializeField] private int white_id;
    [Range(0,3)][SerializeField] private int black_id;
    
    [SerializeField] private bool do_move_scan = false;
    [SerializeField] private bool do_move_test = false;
    [SerializeField] private bool auto_restart_game = false;

    [SerializeField] private string board_fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    // Board
    public BoardManager board_manager;
    private GameMatcher gamematcher;

    private bool turn;
    private bool isGameOver = false;

    // Game Controller Objects
    private CONTROLLER_PieceSetup piece_setup;
    private CONTROLLER_Timer timer_controller;

    private Thread legal_move_thread;
    private Thread legal_test_thread;

    void RunLegalMoves() {
        Thread.Sleep(1000);

        int x = 0;

        Board board = board_manager.board.copy();

        while (true) {
            x += 1;
            List<double> move_data = board_manager.move_generator.GenerateLegalPly(board, x, move_breakdown:false);
            String mvs = move_data[0].ToString("N0");
            String nps = ((move_data[0] / move_data[1])*1000).ToString("N0");

            UnityEngine.Debug.Log(mvs + " ¦ " + x + " ply   " + move_data[1] + "ms  ¦ " + nps + " n/s");
        }
    }

    void RunLegalTest() {
        Thread.Sleep(1000);

        List<FEN_TEST> fen_tests = new List<FEN_TEST>() {
            new FEN_TEST("Kiwipete", "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1", new List<int>() {48, 2039, 97862}),
            new FEN_TEST("Endgame Pin", "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1", new List<int>() {14, 191, 2812, 43238, 674624}),
            new FEN_TEST("PBS", "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1", new List<int>() {6, 264, 9467, 422333}),
            new FEN_TEST("Balance", "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8", new List<int>() {44, 1486, 62379}),
            new FEN_TEST("Books", "r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10", new List<int>() {46, 2079, 89890}),
            new FEN_TEST("Promo", "r2qk2r/pp2p2p/b3b1Nn/5Pp1/P1BB1p2/1nP1pP1N/QP1P2PP/R3K2R w KQkq - 0 1", new List<int>() {45, 1761, 73388, 2854221})
        };

        Stopwatch st = new Stopwatch();
        st.Start();
        
        foreach(FEN_TEST fen_test in fen_tests) {
            Board board = new Board();
            board.set_fen(fen_test.FEN);

            for (int i=1; i<=fen_test.expected_output.Count; i++) {
                List<double> move_data = board_manager.move_generator.GenerateLegalPly(board, i, move_breakdown:false);

                String mvs_e = fen_test.expected_output[i-1].ToString("N0");
                String mvs_o = move_data[0].ToString("N0");
                String nps = ((move_data[0] / move_data[1])*1000).ToString("N0");
                String colour = (fen_test.expected_output[i-1] == move_data[0]) ? "<color=#59de81>" : "<color=#e35454>";

                UnityEngine.Debug.Log($"{colour}{fen_test.TestName}: {i}ply   [{mvs_e}:{mvs_o}]  {move_data[1]}ms  {nps} n/s  </color>");
            }
        }
        UnityEngine.Debug.Log($"PLY TEST COMPLETE IN {st.ElapsedMilliseconds}ms");
    }


    void Awake() {   
        board_manager = new BoardManager();
        gamematcher = new GameMatcher(engine_move_time);

        board_manager.board.white_id = white_id;
        board_manager.board.black_id = black_id;

        board_manager.board.turn_id = white_id;
    }

    void Start() { 
        // Setup controllers
        timer_controller = FindFirstObjectByType<CONTROLLER_Timer>(); 
        piece_setup = FindFirstObjectByType<CONTROLLER_PieceSetup>();

        timer_controller.change_timer(ALLOWED_TIME);

        // Setup Board
        if (board_fen == "start") board_fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        board_manager.board.set_fen(board_fen);
        turn = !board_manager.board.turn;

        piece_setup.ClearPieces();
        piece_setup.SetupPieces(board_manager.board.b);

        legal_move_thread = new Thread(RunLegalMoves);
        legal_test_thread = new Thread(RunLegalTest);

        if (do_move_scan) legal_move_thread.Start();
        if (do_move_test) legal_test_thread.Start();
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
        if (state == 1 ) UnityEngine.Debug.Log("WHITE CHECKMATE");
        if (state == 0 ) UnityEngine.Debug.Log("DRAW");
        if (state == -1) UnityEngine.Debug.Log("BLACK CHECKMATE");

        isGameOver = true;

        if (auto_restart_game) RestartButton_PRESSED();
    }

    public void RestartButton_PRESSED() {
        legal_move_thread.Abort();

        Type logEntries = Type.GetType("UnityEditor.LogEntries, UnityEditor");
        MethodInfo clearMethod = logEntries?.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
        clearMethod?.Invoke(null, null);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
