using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using System.IO;
using TMPro;

public class GAME : MonoBehaviour
{
    [Header("GAME SETTINGS")]
    [SerializeField] private int ALLOWED_TIME = 180;
    [SerializeField] private int engine_move_time = 10000;

    [Range(0,5)][SerializeField] private int white_id;
    [Range(0,5)][SerializeField] private int black_id;
    
    [SerializeField] private string board_fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    [Header("TEST SETTINGS")]
    [SerializeField] private bool do_move_scan = false;
    [SerializeField] private bool do_move_test = false;
    [SerializeField] private bool do_state_test = false;
    [SerializeField] private bool auto_restart_game = false;
    [SerializeField] private bool clear_console = false;

    [Header("GAME SUITE")]
    [SerializeField] private bool DO_SUITE = false;
    
    [SerializeField] private List<int> ENGINE_1 = new List<int>();
    [SerializeField] private List<int> ENGINE_2 = new List<int>();

    private int ENGINE_1_INDEX = 0;
    private int ENGINE_2_INDEX = 0;

    private String ENGINE_1_NAME = "";
    private String ENGINE_2_NAME = "";

    [SerializeField] private int GAME_NUM = 0;

    private List<String> GAMES_MOVE_LIST = new List<String>();
    private (int, int, int) RUNNING_GAMES = (0, 0, 0);
    private int current_game_num = 0;
    private bool suite_turn;


    // Board
    public BoardManager board_manager;
    private GameMatcher gamematcher;

    private SpriteRenderer bg;
    private Color32 norm_colour = new Color32(98, 106, 105, 255);
    private Color32 test_colour = new Color32(38, 113, 103, 255);

    private bool turn;
    private bool isGameOver = false;

    // Game Controller Objects
    private PieceManager piece_setup;
    private CONTROLLER_Timer timer_controller;

    private TextMeshProUGUI white_text;
    private TextMeshProUGUI black_text;

    private Thread legal_move_thread;
    private Thread legal_test_thread;
    private Thread state_test_thread;

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

    private void RunStateTest() {
        Stopwatch st = new Stopwatch();
        st.Start();

        for (int i=0; i<1000; i++) {
            board_manager.move_generator.isDraw(board_manager.board);
        }

        st.Stop();

        String avr_time = (double)st.ElapsedMilliseconds + "μs";

        UnityEngine.Debug.Log("Avr. Draw Time : "+avr_time);

        st = new Stopwatch();
        st.Start();

        for (int i=0; i<1000; i++) {
            board_manager.move_generator.isCheckmate(board_manager.board);
        }

        avr_time = (double)st.ElapsedMilliseconds + "μs";

        UnityEngine.Debug.Log("Avr. Checkmate Time : "+avr_time);
    }

    // Standard Start
    void Awake() {   
        board_manager = new BoardManager();
        gamematcher = new GameMatcher(board_manager, engine_move_time);

        if (engine_move_time < 50) {
            UnityEngine.Debug.Log("WARNING: Low move time can cause engine issues");
        }

        if (DO_SUITE) {
            ENGINE_1_NAME = gamematcher.GetName(ENGINE_1[ENGINE_1_INDEX]);
            ENGINE_2_NAME = gamematcher.GetName(ENGINE_2[ENGINE_2_INDEX]);

            board_manager.board.white_id = ENGINE_1[ENGINE_1_INDEX];
            board_manager.board.black_id = ENGINE_2[ENGINE_2_INDEX];

            board_manager.board.turn_id = ENGINE_1[ENGINE_1_INDEX];
        }
        else {
            ENGINE_1_NAME = gamematcher.GetName(white_id);
            ENGINE_2_NAME = gamematcher.GetName(black_id);

            board_manager.board.white_id = white_id;
            board_manager.board.black_id = black_id;

            board_manager.board.turn_id = white_id;
        }


        legal_move_thread = new Thread(RunLegalMoves);
        legal_test_thread = new Thread(RunLegalTest);
        state_test_thread = new Thread(RunStateTest);
    }

    void RepeatableAwake() {
        board_manager = new BoardManager();
        gamematcher.UpdateNewGame(board_manager);

        if (engine_move_time < 50) {
            UnityEngine.Debug.Log("WARNING: Low move time can cause engine issues");
        }

        if (DO_SUITE) {
            ENGINE_1_NAME = gamematcher.GetName(ENGINE_1[ENGINE_1_INDEX]);
            ENGINE_2_NAME = gamematcher.GetName(ENGINE_2[ENGINE_2_INDEX]);

            board_manager.board.white_id = ENGINE_1[ENGINE_1_INDEX];
            board_manager.board.black_id = ENGINE_2[ENGINE_2_INDEX];

            board_manager.board.turn_id = ENGINE_1[ENGINE_1_INDEX];
        }
        else {
            ENGINE_1_NAME = gamematcher.GetName(white_id);
            ENGINE_2_NAME = gamematcher.GetName(black_id);

            board_manager.board.white_id = white_id;
            board_manager.board.black_id = black_id;

            board_manager.board.turn_id = white_id;
        }


        legal_move_thread = new Thread(RunLegalMoves);
        legal_test_thread = new Thread(RunLegalTest);
        state_test_thread = new Thread(RunStateTest);
    }

    // NORMAL GAME

    void Start() { 
        // Setup controllers
        timer_controller = FindFirstObjectByType<CONTROLLER_Timer>(); 
        piece_setup = FindFirstObjectByType<PieceManager>();

        white_text = GameObject.Find("WHITE NAME").GetComponent<TextMeshProUGUI>();
        black_text = GameObject.Find("BLACK NAME").GetComponent<TextMeshProUGUI>();

        bg = GameObject.Find("Panel").GetComponent<SpriteRenderer>();
        
        if (DO_SUITE) bg.color = test_colour;
        else          bg.color = norm_colour;

        timer_controller.change_timer(ALLOWED_TIME);

        if (DO_SUITE) {
            SUITE_START();
        }
        else {
            // Setup Board
            if (board_fen == "start") board_fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

            white_text.text = ENGINE_1_NAME;
            black_text.text = ENGINE_2_NAME;

            board_manager.board.set_fen(board_fen);
            turn = !board_manager.board.turn;

            piece_setup.ClearPieces();
            piece_setup.SetupPieces(board_manager.board.b);

            if (do_move_scan) legal_move_thread.Start();
            if (do_move_test) legal_test_thread.Start();
            if (do_state_test) state_test_thread.Start();
        }
    }

    void Update() {
        if (isGameOver) return;

        if (DO_SUITE) SUITE_UPDATE();

        if (board_manager.board.turn != turn && !DO_SUITE) {
            piece_setup.ClearPieces();
            piece_setup.SetupPieces(board_manager.board.b);

            board_manager.board.is_checkmate = board_manager.move_generator.isCheckmate(board_manager.board);
            board_manager.board.is_draw = board_manager.move_generator.isDraw(board_manager.board);

            if (board_manager.board.is_checkmate != 0 || timer_controller.IS_TIMEOUT) {  GameOver(board_manager.board.is_checkmate); return;  }
            if (board_manager.board.is_draw) {  GameOver(0); return;  }

            turn = !turn;
            timer_controller.flip_turn(turn, white_text, black_text);

            if (board_manager.board.turn_id > 0) {
                gamematcher.GetEngineMove();
            }
        }
    }

    void GameOver(int state, bool print_output=true) {
        if (state == 1  && print_output) UnityEngine.Debug.Log("WHITE CHECKMATE");
        if (state == 0  && print_output) UnityEngine.Debug.Log("DRAW");
        if (state == -1 && print_output) UnityEngine.Debug.Log("BLACK CHECKMATE");

        isGameOver = true;

        if (auto_restart_game && !DO_SUITE) RestartButton_PRESSED();
    }

    // SUITE TESTING

    void SUITE_START() {
        if (current_game_num >= GAME_NUM) {
            bool is_first_save = ENGINE_1_INDEX == 0 && ENGINE_2_INDEX == 0;

            if (ENGINE_1_INDEX == ENGINE_1.Count-1 && ENGINE_2_INDEX == ENGINE_2.Count-1) {
                SaveSuiteData(final_save:true, first_save:is_first_save);
                GameOver(0, print_output:false);
            }
            else {
                SaveSuiteData(first_save:is_first_save);

                // Loop next index

                current_game_num = 1;
                RUNNING_GAMES = (0, 0, 0);
                
                if (ENGINE_2_INDEX == ENGINE_2.Count-1) {
                    ENGINE_2_INDEX = 0;
                    ENGINE_1_INDEX++;
                }
                else {
                    ENGINE_2_INDEX++;
                }
            }
        }
        else {
            current_game_num++;
            UnityEngine.Debug.Log("GAME: "+current_game_num + " ¦  "+ENGINE_1_NAME+": " + RUNNING_GAMES);

            // Setup Board
            RepeatableAwake();

            timer_controller.change_timer(ALLOWED_TIME);

            if (board_fen == "start") board_fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

            board_manager.board.set_fen(board_fen);
            suite_turn = !board_manager.board.turn;

            if (current_game_num%2 == 0) {
                board_manager.board.white_id = ENGINE_1[ENGINE_1_INDEX];
                board_manager.board.black_id = ENGINE_2[ENGINE_2_INDEX];

                white_text.text = ENGINE_1_NAME;
                black_text.text = ENGINE_2_NAME;
            }
            else {
                board_manager.board.white_id = ENGINE_2[ENGINE_2_INDEX];
                board_manager.board.black_id = ENGINE_1[ENGINE_1_INDEX];
                
                white_text.text = ENGINE_2_NAME;
                black_text.text = ENGINE_1_NAME;
            }

            piece_setup.ClearPieces();
            piece_setup.SetupPieces(board_manager.board.b);
        }
    }

    void SUITE_UPDATE() {
        if (board_manager.board.turn != suite_turn) {
            piece_setup.ClearPieces();
            piece_setup.SetupPieces(board_manager.board.b);

            board_manager.board.is_checkmate = board_manager.move_generator.isCheckmate(board_manager.board);
            board_manager.board.is_draw = board_manager.move_generator.isDraw(board_manager.board);

            if (board_manager.board.is_checkmate != 0 || timer_controller.IS_TIMEOUT) {  GameOver_SUITE(board_manager.board.is_checkmate); return;  }
            if (board_manager.board.is_draw) {  GameOver_SUITE(0); return;  }

            suite_turn = board_manager.board.turn;
            timer_controller.flip_turn(suite_turn, white_text, black_text);

            if (board_manager.board.turn_id > 0) {
                gamematcher.GetEngineMove();
            }
        }
    }

    void GameOver_SUITE(int state) {
        (int losses, int draws, int wins) = RUNNING_GAMES;

        if ((state ==  1 && current_game_num%2==0) || (state == -1 && current_game_num%2==1)) wins++;
        if ((state == -1 && current_game_num%2==0) || (state ==  1 && current_game_num%2==1)) losses++;
        if (state == 0 )draws++;

        RUNNING_GAMES = (losses, draws, wins);

        String state_text = "";

        if ((state ==  1 && current_game_num%2==0) || (state == -1 && current_game_num%2==1)) state_text = ENGINE_1_NAME + "  1-0  " + ENGINE_2_NAME;
        if ((state == -1 && current_game_num%2==0) || (state ==  1 && current_game_num%2==1)) state_text = ENGINE_1_NAME + "  0-1  " + ENGINE_2_NAME;
        if (state == 0 )                                                                      state_text = ENGINE_1_NAME + "  h-h  " + ENGINE_2_NAME;

        GAMES_MOVE_LIST.Add(state_text+"    "+String.Join(" ", board_manager.board.move_list));

        SUITE_START();
    }

    void SaveSuiteData(bool first_save=false, bool final_save=false) {
        UnityEngine.Debug.Log("SUITE FINISHED");
        (int wins, int draws, int losses) = RUNNING_GAMES;

        String e1_col = ((losses*-1)+(wins)) > 0 ? "#59de81" : "#e35454";
        String e2_col = ((wins*-1)+(losses)) > 0 ? "#59de81" : "#e35454";

        if (wins == losses) {
            e1_col = "yellow";
            e2_col = "yellow";
        }

        UnityEngine.Debug.Log($"RESULTS\n\n<color={e1_col}>{ENGINE_2_NAME}: {wins}, {draws},{losses}</color>\n<color={e2_col}>{ENGINE_1_NAME}: {losses}, {draws},{wins}</color>\n\n");
        
        String full_log_file = "./Assets/Scripts/ENGINE_LOGS/deep_suite_logs.txt";
        String log_file = "./Assets/Scripts/ENGINE_LOGS/suite_logs.txt";

        String time = DateTime.Now.ToString("dd-MM-yy HH:mm:ss");

        String content = "", content_full = "";

        if (first_save) {
            content = $"\n\n=======================================================================================\nSUITE TEST: {time}\n\n\n{ENGINE_1_NAME}: {losses}W, {draws}D, {wins}L\n{ENGINE_2_NAME}: {wins}W, {draws}D, {losses}L\n";
            content_full = $"\n\n=======================================================================================\nSUITE TEST: {time}\n\n\n{ENGINE_1_NAME}: {losses}W, {draws}D, {wins}L\n{ENGINE_2_NAME}: {wins}W, {draws}D, {losses}L\n";
        }
        else {
            content = $"\n{ENGINE_1_NAME}: {losses}W, {draws}D, {wins}L\n{ENGINE_2_NAME}: {wins}W, {draws}D, {losses}L\n";
            content_full = $"\n{ENGINE_1_NAME}: {losses}W, {draws}D, {wins}L\n{ENGINE_2_NAME}: {wins}W, {draws}D, {losses}L\n";
        }
        
        if (final_save) {
            int game=0;

            foreach(String GAME_MV in GAMES_MOVE_LIST) {
                game++;

                content_full += $"\n{game}.    " + String.Join(" ", GAME_MV);
            }
        }

        File.AppendAllText(log_file, content);
        File.AppendAllText(full_log_file, content_full);
    }

    // EXTERNAL FUNCTIONS
    public void RestartButton_PRESSED() {
        legal_move_thread.Abort();

        if (clear_console) {
            Type logEntries = Type.GetType("UnityEditor.LogEntries, UnityEditor");
            MethodInfo clearMethod = logEntries?.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
            clearMethod?.Invoke(null, null);
        }


        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
