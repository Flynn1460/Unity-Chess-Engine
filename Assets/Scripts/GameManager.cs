using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using ENGINE_NAMESPACE_Random;
using ENGINE_NAMESPACE_Minimax_1;
using System.Threading.Tasks;


public class ConsoleUCIInterface
{
    private Process engine;
    private StreamWriter engine_input;
    private Queue<string> response_queue = new Queue<string>();

    private string engine_path = "";


    // Wierd Process Things
    private void SetupEngine() {
        engine = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = engine_path,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            }
        };

        engine.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                lock (response_queue)
                {
                    response_queue.Enqueue(e.Data);
                }
            }
        };

        engine.Start();
        engine_input = engine.StandardInput;
        engine.BeginOutputReadLine(); // Asynchronous reading (prevents blocking)
    }

    public string SendCommand(string command, string response, bool report_non_response=false) {

        if (engine_input != null) {
            engine_input.WriteLine(command);
            engine_input.Flush();
        }

        Stopwatch timer = new Stopwatch();
        timer.Start();

        if (response == null) return null;

        while (timer.ElapsedMilliseconds < 10000) {
            lock (response_queue) {
                while (response_queue.Count > 0)
                {
                    string engine_response = response_queue.Dequeue();

                    if (engine_response.Contains(response)) return engine_response;
                    else if (report_non_response) UnityEngine.Debug.Log("Stockfish Response: " + engine_response + ", " + response);
                }
            }
            Thread.Sleep(50);
        }
        return null;
    }


    public ConsoleUCIInterface(string engine_path_) {
        engine_path = engine_path_;

        Engine();
    }


    void Engine() {
        SetupEngine();

        SendCommand("uci", "uciok");
    }

    public string GetMoveFromEngine(Board board, int movetime=100) {
        string string_move_list = String.Join(" ", board.move_list);

        SendCommand("position fen "+board.SetupFen+" moves "+string_move_list, null);
        string response = SendCommand("go movetime "+movetime, "bestmove");

        return response;
    }
}


public class CustomEngineInterface {
    private ConsoleUCIInterface uci_engine;
    private object engine_class;

    public String display_name;

    public CustomEngineInterface(String display_name_, Type engine_class_name) {
        engine_class = Activator.CreateInstance(engine_class_name);
        display_name = display_name_;  
    }

    public CustomEngineInterface(String display_name_, String engine_path_) {
        uci_engine = new ConsoleUCIInterface(engine_path_);
        display_name = display_name_;
    }

    public String GetMoveFromEngine(Board b, int movetime=100) {
        if (uci_engine != null) {
            string raw_engine_move = uci_engine.GetMoveFromEngine(b, movetime);
            try{
                return raw_engine_move.Substring(9, 5);
            }
            catch {
                return raw_engine_move.Substring(9, 4) + ' ';
            }
        }

        else {
            // Assuming GET_MOVE is a method of the object type and takes two parameters (Board, int)
            var methodInfo = engine_class.GetType().GetMethod("GET_MOVE");
            
            if (methodInfo != null)
            {
                // Invoke the method on the engine_class instance using reflection
                var result = methodInfo.Invoke(engine_class, new object[] { b, movetime });
                
                // Assuming the method returns a string
                return (string)result;
            }
            else {
                UnityEngine.Debug.LogWarning("Engine GET_MOVE function missing for " + engine_class.GetType().Name);
                return "";
            }
        }
    }
}



public class GameMatcher
{
    private BoardManager bm;

    private Dictionary<int, CustomEngineInterface> engine_refrence = new Dictionary<int, CustomEngineInterface>();
    private int move_time;

    private CustomEngineInterface ENGINE_OBJ_stockfish = new CustomEngineInterface("Stockfish 17", @"C:/Users/flynn/OneDrive/Dokumentumok/Programming/Unity Projects/Chess Programming/Assets/Scripts/ENGINES/ENGINE_Stockfish/stockfish/stockfish.exe");
    private CustomEngineInterface ENGINE_OBJ_Random = new CustomEngineInterface("Random", typeof(ENGINE_Random));
    private CustomEngineInterface ENGINE_OBJ_Minimax_1 = new CustomEngineInterface("Minimax 1", typeof(ENGINE_Minimax_1));


    public GameMatcher(BoardManager board_manager, int engine_movetime) {
        engine_refrence.Add(1, ENGINE_OBJ_stockfish);
        engine_refrence.Add(2, ENGINE_OBJ_Random);
        engine_refrence.Add(3, ENGINE_OBJ_Minimax_1);

        bm = board_manager;

        move_time = engine_movetime;
    }


    public async void GetEngineMove() {
        Move engine_move = await GetEngineMoveAsync();

        if (engine_move.str_uci() == "(-1o-1") {
            UnityEngine.Debug.LogWarning("STOCKFISH ERROR");
            UnityEngine.Debug.Log(bm.move_generator.isDraw(bm.board));
            UnityEngine.Debug.Log(bm.move_generator.isCheckmate(bm.board));
            return;
        }

        bm.board.move( engine_move ); 
    }    

    public async Task<Move> GetEngineMoveAsync()
    {
        CustomEngineInterface current_engine = engine_refrence[bm.board.turn_id];

        string raw_engine_move = await Task.Run(() => current_engine.GetMoveFromEngine(bm.board.copy(), move_time));

        if (raw_engine_move.Length == 4 || raw_engine_move[4] == ' ') 
            raw_engine_move = raw_engine_move.Substring(0, 4);

        return new Move(bm.board, raw_engine_move);
    }


    public String GetName(int engine_code) {  return engine_refrence[engine_code].display_name;  }
}
