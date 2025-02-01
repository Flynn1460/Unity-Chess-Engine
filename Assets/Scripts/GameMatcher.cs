using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.UIElements;


public class ConsoleUCIInterface
{
    private Process engine;
    private StreamWriter engine_input;
    private Thread engine_thread;
    private Queue<string> response_queue = new Queue<string>();

    private string engine_path = "";


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

}


public class GameMatcher
{
    private ConsoleUCIInterface ENGINE_stockfish;
    private TOOLS tools;

    private Dictionary<int, ConsoleUCIInterface> engine_refrence;
    private string move;
    private int move_time;

    public GameMatcher(int engine_movetime) {
        tools = new TOOLS();
        engine_refrence = new Dictionary<int, ConsoleUCIInterface>();

        ENGINE_stockfish = new ConsoleUCIInterface(@"C:/Users/flynn/OneDrive/Dokumentumok/Programming/Unity Projects/Chess Programming/Assets/Scripts/ENGINES/ENGINE_Stockfish/stockfish/stockfish.exe");
        engine_refrence.Add(1, ENGINE_stockfish);    

        move_time = engine_movetime;
    }



    public void GetEngineMove(BoardManager bm) {

        string engine_move = GetMoveFromUCI(bm.board, engine_refrence[bm.board.turn_id], move_time);
        engine_move = engine_move.Substring(9, 4); // Remove filler text

        UnityEngine.Debug.Log(engine_move);

        bm.board.move(engine_move);
        bm.MoveGOPieces(engine_move);
    }

    private string GetMoveFromUCI(Board board, ConsoleUCIInterface engine, int movetime=100) {
        string string_move_list = String.Join(" ", board.move_list);

        engine.SendCommand("position startpos moves "+string_move_list, null);

        string response = engine.SendCommand("go movetime "+movetime, "bestmove");

        return response;
    }
}
