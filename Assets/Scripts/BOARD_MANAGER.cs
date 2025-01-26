using UnityEngine;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

public class BOARD_MANAGER : MonoBehaviour
{
    private Process stockfishProcess;
    private StreamWriter writer;
    private StreamReader reader;

    public string stockfishPath = "C:/Users/flynn/Documents/STOCKFISH/stockfish/stockfish-17.exe";

    private void SEND(String command, bool getResponse=false, String expected_response=null) {
        writer.WriteLine(command);
        String returned_text = reader.ReadLine();

        while (reader.ReadLine() != expected_response) continue;

        if (getResponse) UnityEngine.Debug.Log(returned_text);
    }

    void Start()
    {
        StartStockfish();
    }

    void StartStockfish()
    {
        // Initialize the process for Stockfish
        stockfishProcess = new Process();
        stockfishProcess.StartInfo.FileName = stockfishPath;
        stockfishProcess.StartInfo.UseShellExecute = false;
        stockfishProcess.StartInfo.RedirectStandardInput = true;
        stockfishProcess.StartInfo.RedirectStandardOutput = true;
        stockfishProcess.StartInfo.CreateNoWindow = true;
        
        // Start the process
        stockfishProcess.Start();

        // Set up stream writers and readers
        writer = stockfishProcess.StandardInput;
        reader = stockfishProcess.StandardOutput;
        
        reader.ReadLine();
        SEND("uci", true, expected_response:"uciok");
        SEND("isready", true, expected_response:"readyok");
        SEND("go movetime 1000", true);
        
        string response = reader.ReadLine();
    }
}
