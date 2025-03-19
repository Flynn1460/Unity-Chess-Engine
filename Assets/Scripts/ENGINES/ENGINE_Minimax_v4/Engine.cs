using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ENGINE_NAMESPACE_Minimax_V4 {
public class MinimaxEngine {
    
    private NewMoveGenerator move_generator = new NewMoveGenerator();
    
    private Transposition trs = new Transposition();
    private Eval eval_cl = new Eval();

    private Stopwatch stopwatch = new Stopwatch();
    private Random rand = new Random();

    private int allocated_movetime = 2000000000;

    private List<String> opening_book = new List<String>(File.ReadAllLines("Assets/Scripts/ENGINES/ENGINE_Minimax_v4/opening_book.txt"));

    int max_depth = 0;


    public Move Get_SetMove(Board board, int set_depth) {
        allocated_movetime = 2000000000;
        max_depth = 100;

        stopwatch = new Stopwatch();
        stopwatch.Start();

        trs.CLEAR_HASH_TABLE();

        (Move returned_mv, double eval) = top_minimax(board, set_depth-1, -999, +999);

        UnityEngine.Debug.Log($"E4: Max Depth of ({set_depth} : {eval}) reached in: {stopwatch.ElapsedMilliseconds}ms");
        return returned_mv;
    }

    public Move Get_TimeMove(Board board, int movetime) {
        stopwatch = new Stopwatch();
        stopwatch.Start();
        
        allocated_movetime = movetime;

        Move best_move = new Move();

        max_depth = 1;
        double prev_eval = 0;


        while (true) {
            trs.CLEAR_HASH_TABLE();
            
            (Move returned_mv, double eval) = top_minimax(board, max_depth-1, -999, +999);


            if (stopwatch.ElapsedMilliseconds > 0) {

                // UnityEngine.Debug.Log("======================================================");
                UnityEngine.Debug.Log($"RETURNED : {returned_mv}   {max_depth-1}ply : {eval}    {stopwatch.ElapsedMilliseconds}ms");
                // UnityEngine.Debug.Log("======================================================");
                return returned_mv;
            }
            else if (eval == 900 || eval == -900) {
                return returned_mv;
            }
            else {
                // UnityEngine.Debug.Log($"  {returned_mv} {max_depth}ply : {eval}      {stopwatch.ElapsedMilliseconds}ms");
            }
            
            // ordered_moves = returned_ordered_moves;

            best_move = returned_mv;
            prev_eval = eval;
            max_depth++;
        }
    }

    public (Move mv, double eval) top_minimax(Board board, int depth, double alpha, double beta, List<Move> ordered_moves=null) {
        Move book_move = get_book_move(board);

        if (book_move.str_uci() != "a1a1") return (book_move, 0);

        List<Move> moves = move_generator.GenerateLegalMoves(board);
        moves = OrderMoves(moves, board.turn);

        double highest_eval = board.turn ? -1000 : +1000;
        double eval;

        Move highest_mv = new Move();

        foreach(Move move in moves) {
            board.advanced_move(move);

            eval = qui_minimax(board, alpha, beta);

            board.advanced_undo_move();

            if ((board.turn && highest_eval < eval) || (!board.turn && highest_eval > eval)) {
                highest_eval = eval;
                highest_mv = move;

                if (board.turn) alpha = Math.Max(alpha, eval);
                else beta = Math.Min(beta, eval);

                if (beta <= alpha) break;
            }
        }

        return (highest_mv, highest_eval);
    }

    public double bot_minimax(Board board, double alpha, double beta) {
        double searched_eval = trs.FIND_HASH(board);
        if (searched_eval != -1002) return searched_eval;

        List<Move> moves = move_generator.GenerateLegalMoves(board);
        if (moves.Count == 0) return eval_cl.EvaluateBoard(board);

        moves = OrderMoves(moves, board.turn);

        double highest_eval = board.turn ? -1000 : +1000;
        double eval;

        foreach(Move move in moves) {
            board.advanced_move(move);

            eval = qui_minimax(board, alpha, beta);

            board.advanced_undo_move();

            if ((board.turn && highest_eval < eval) || (!board.turn && highest_eval > eval)) {
                highest_eval = eval;

                if (board.turn) alpha = Math.Max(alpha, eval);
                else beta = Math.Min(beta, eval);

                if (beta <= alpha) break;
            }
        }

        trs.MAKE_HASH(board, highest_eval);

        return highest_eval;
    }

    public double qui_minimax(Board board, double alpha, double beta, double qui_depth=0) {
        double searched_eval = trs.FIND_HASH(board);
        if (searched_eval != -1002) return searched_eval;

        if (qui_depth > 6) return eval_cl.EvaluateBoard(board);

        List<Move> cap_moves = move_generator.GenerateCaptureMoves(board);

        if (cap_moves.Count == 0) return eval_cl.EvaluateBoard(board);

        bool turn = board.turn;
        double highest_eval = turn ? -1000 : +1000;
        double eval;

        foreach(Move cap_move in cap_moves) {
            board.advanced_move(cap_move);

            eval = qui_minimax(board, alpha, beta, qui_depth+1);

            board.advanced_undo_move();

            if ((turn && highest_eval < eval) || (!turn && highest_eval > eval)) {
                highest_eval = eval;

                if (turn) alpha = Math.Max(alpha, eval);
                else beta = Math.Min(beta, eval);

                if (beta <= alpha) break;
            }
        }

        trs.MAKE_HASH(board, highest_eval);

        return highest_eval;
    }

    
    public List<Move> OrderMoves(List<Move> moves, bool turn) {
        List<(Move mv, float diff)> ordered_moves = new List<(Move mv, float diff)>();

        foreach(Move move in moves) {
            ordered_moves.Add((move, move.end_square.GetPieceType()-move.start_square.GetPieceType()));
        }

        if  (turn)  ordered_moves = ordered_moves.OrderBy(x => x.diff).ToList();
        if (!turn)  ordered_moves = ordered_moves.OrderByDescending(x => x.diff).ToList();
        

        return ordered_moves.Select(x => x.mv).ToList();
    }


    public Move get_book_move(Board board) {
        if (board.move_list.Count <= 6 && !board.custom_fen) {
            if (board.move_list.Count > 0) {
                String mv_list = "#: "+String.Join(" ", board.move_list);

                int x = 0;
                foreach(String opening in opening_book) {
                    if (opening.Contains(mv_list)) {
                        x++;

                        return new Move(board, opening.Substring(mv_list.Length+1, 5));
                    }
                }
            }
            else {
                String random_opening = opening_book[rand.Next(opening_book.Count)];

                return new Move(board, random_opening.Substring(3, 4));
            }
        }

        return new Move();
    }
}
}
