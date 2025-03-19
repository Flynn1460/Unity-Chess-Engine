using System;
using System.Collections.Generic;
using System.Diagnostics;


public class NewMoveGenerator
{
    // Piece Move Patterns
    (int, int)[] rook_directions = {(1, 0), (-1, 0), (0, 1), (0, -1)};
    (int, int)[] knight_directions =  { ( -2, -1 ), ( -2, 1 ), ( -1, -2 ), ( -1, 2 ), ( 1, -2 ), ( 1, 2 ), ( 2, -1 ), ( 2, 1 )  };
    (int, int)[] bishop_directions = {(1, 1), (1, -1), (-1, 1), (-1, -1)};
    (int, int)[] queen_directions = {(1, 1), (1, 0), (1, -1), (0, -1), (-1, -1), (-1, 0), (-1, 1), (0, 1)};
    (int, int)[] king_directions = {(1, -1), (1, 0), (1, 1), (0, -1), (0, 1), (-1, -1), (-1, 0), (-1, 1)};

    Move lastMove = new Move();


    // MOVE GENERATION
    public List<Move> GenerateLegalMoves(Board board, bool safe_search=false) {
        List<Move> legal_moves = new List<Move>(64);
        List<Square> piece_squares_for_turn = GetSquareType(board, PieceGroup.FRIENDLY);

        if (board.move_list.Count != 0) lastMove = board.move_list[board.move_list.Count - 1];

        foreach(Square piece_square in piece_squares_for_turn) {
            switch(piece_square.GetPieceType()) {
                case 1: GetPawnMoves(board, piece_square, legal_moves, safe_search); break;
                case 2: GetRookMoves(board, piece_square, legal_moves); break;
                case 3: GetKnightMoves(board, piece_square, legal_moves); break;
                case 4: GetBishopMoves(board, piece_square, legal_moves); break;
                case 5: GetQueenMoves(board, piece_square, legal_moves); break;
                case 6: GetKingMoves(board, piece_square, legal_moves, safe_search); break;
            }
        }

        legal_moves = GetDiscardedMoves(board, legal_moves);
        return legal_moves;
    }
    
    public List<Move> GenerateCaptureMoves(Board board) {
        List<Move> legal_moves = new List<Move>(64);
        List<Square> piece_squares_for_turn = GetSquareType(board, PieceGroup.FRIENDLY);

        if (board.move_list.Count != 0) lastMove = board.move_list[board.move_list.Count - 1];

        foreach(Square piece_square in piece_squares_for_turn) {
            // Check if you are only searching for a singular piece

            switch(piece_square.GetPieceType()) {
                case 1: GetPawnMoves(board, piece_square, legal_moves, true); break;
                case 2: GetRookMoves(board, piece_square, legal_moves); break;
                case 3: GetKnightMoves(board, piece_square, legal_moves); break;
                case 4: GetBishopMoves(board, piece_square, legal_moves); break;
                case 5: GetQueenMoves(board, piece_square, legal_moves); break;
                case 6: GetKingMoves(board, piece_square, legal_moves, true); break;
            }
        }

        legal_moves = GetDiscardedMoves(board, legal_moves);
        legal_moves = DiscardNonCaptures(legal_moves);

        return legal_moves;
    }

    public List<Move> GenerateLegalMovesForSquare(Board board, Square filter_square) {
        List<Move> legal_moves = new List<Move>(64);

        if (board.move_list.Count != 0) lastMove = board.move_list[board.move_list.Count - 1];

        switch(filter_square.GetPieceType()) {
            case 1: GetPawnMoves(board, filter_square, legal_moves, false); break;
            case 2: GetRookMoves(board, filter_square, legal_moves); break;
            case 3: GetKnightMoves(board, filter_square, legal_moves); break;
            case 4: GetBishopMoves(board, filter_square, legal_moves); break;
            case 5: GetQueenMoves(board, filter_square, legal_moves); break;
            case 6: GetKingMoves(board, filter_square, legal_moves, false); break;
        }

        legal_moves = GetDiscardedMoves(board, legal_moves);
        return legal_moves;
    }


    public bool isSquareAttacked(Board board, Square attacked_square, bool check_search=false) {
        int attacked_square_raw = attacked_square.GetRAW();
        List<Move> moves_for_square = new List<Move>(8);

        if (check_search) return false;

        List<Square> dia_liab = GetDiagonalLiabilities(board, attacked_square);  
        foreach(Square piece_square in dia_liab) {
            switch(piece_square.GetPieceType()) {
                case 4: return true;
                case 5: return true;

                case 1: 
                    moves_for_square.Clear();
                    GetPawnMoves(board, piece_square, moves_for_square, include_theory:true); 
                    foreach(Move pos_move in moves_for_square) { if (pos_move.end_square.GetRAW() == attacked_square_raw) return true; }
                    break;

                case 6: 
                    moves_for_square.Clear();
                    GetKingMoves(board, piece_square, moves_for_square, safe_search:true);
                    foreach(Move pos_move in moves_for_square) { if (pos_move.end_square.GetRAW() == attacked_square_raw) return true; } 
                    break;
            }   
        }

        List<Square> str_liab = GetStraightLiabilities(board, attacked_square);  
        foreach(Square piece_square in str_liab) {
            if (piece_square.GetPieceType() == 2 || piece_square.GetPieceType() == 5 ) return true;
            if (piece_square.GetPieceType() != 6) continue;

            moves_for_square.Clear();
            GetKingMoves(board, piece_square, moves_for_square, safe_search:true); 
            foreach(Move pos_move in moves_for_square) { if (pos_move.end_square.GetRAW() == attacked_square_raw) return true; }
        }

        List<Square> kni_liab = GetKnightLiabilities(board, attacked_square);  
        foreach(Square piece_square in kni_liab) {
            if (piece_square.GetPieceType() == 3) return true;
        }

        return false;
    }

    // PLY TESTING
    public List<double> GenerateLegalPly(Board board, int ply, bool move_breakdown=false) {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        double running_move_total = PlyDepthSearcher(board, 1, ply);

        return new List<double>() {running_move_total, stopwatch.ElapsedMilliseconds};
    }

    private int PlyDepthSearcher(Board b, int ply, int max_ply) {
        List<Move> moves = GenerateLegalMoves(b);

        if (ply == max_ply){
            return moves.Count;
        } 

        // String y = "";
        int local_count = 0;

        foreach(Move mv in moves) {
            b.advanced_move(mv);

            int x = PlyDepthSearcher(b, ply+1, max_ply);
            // if (ply == 1) y += (mv + " - " + (x)) + "\n";
            local_count += x;

            b.advanced_undo_move();
        }

        // if (ply == 1) UnityEngine.Debug.Log(y);

        return local_count;
    }


    // End Game Cases
    public int isCheckmate(Board board_) {
        if (!isCheck(board_, true)) return 0;

        Board board = board_.copy();
        List<Move> playable_moves = GenerateLegalMoves(board);

        foreach(Move playable_move in playable_moves) {
            board.advanced_move(playable_move);

            if (!isCheck(board)) return 0;

            board.advanced_undo_move();
        }

        return board.turn ? -1 : 1;
    }

    public bool isDraw(Board board_) {
        if (isCheck(board_, true)) return false;
        
        Board board = board_.copy();

        List<int> wp = new List<int>();
        List<int> bp = new List<int>();

        int piece = 0;
        for(int row=0; row<8; row++) {
            for (int col=0; col<8; col++) {
                piece = board.b[row, col];
                if (piece < 7 && piece != 0) wp.Add(piece);
                if (piece > 7) bp.Add(piece);
            }
        }

        // Just Kings
        if ((wp.Contains(6) && wp.Count == 1) && (bp.Contains(13) && bp.Count == 1)) return true;
        
        // White Bishop & Black King
        if ((wp.Contains(6) && wp.Contains(4) && wp.Count == 2) && (bp.Contains(13) && bp.Count == 1)) return true;

        // Black Bishop & White King
        if ((wp.Contains(6) && wp.Count == 1) && (bp.Contains(13) && bp.Contains(11) && bp.Count == 2)) return true;
        
        // White Knight & Black King
        if ((wp.Contains(6) && wp.Contains(3) && wp.Count == 2) && (bp.Contains(13) && bp.Count == 1)) return true;

        // Black Knight & White King
        if ((wp.Contains(6) && wp.Count == 1) && (bp.Contains(13) && bp.Contains(10) && bp.Count == 2)) return true;

        HASH current_hash = board.hashGen.MAKE_HASH(board);
        int repeated = 1;

        if (board.move_list.Count > 5) {
            foreach(Move mv in board.move_list) {
                if (board.hashGen.COMPARE_HASH(current_hash, mv.board_before_HASH)) {
                    repeated++;
                }

                if (repeated >= 3) return true;
            }
        }


        List<Move> moves = new List<Move>();
        List<Square> piece_squares_for_turn = GetSquareType(board, PieceGroup.FRIENDLY);
        foreach(Square piece_square in piece_squares_for_turn) {

            switch(piece_square.GetPieceType()) {
                case 1: GetPawnMoves(board, piece_square, moves, false); break;
                case 2: GetRookMoves(board, piece_square, moves); break;
                case 3: GetKnightMoves(board, piece_square, moves); break;
                case 4: GetBishopMoves(board, piece_square, moves); break;
                case 5: GetQueenMoves(board, piece_square, moves); break;
                case 6: GetKingMoves(board, piece_square, moves, false); break;
            }
            moves = GetDiscardedMoves(board, moves);  
            if (moves.Count != 0) return false;  
        }

        return true;
    }

    public bool isCheck(Board board, bool flip_turn=false) {
        if (flip_turn) board.turn = !board.turn;

        int king_piece = board.turn ? 13 : 6;
        Square king_location = board.find_piece_location(king_piece);

        if (flip_turn) board.turn = !board.turn;
        
        return isSquareAttacked(board, king_location);
    }

    public bool isGM(Board board_) {  return isCheckmate(board_) != 0 || isDraw(board_);  }


    // Move Filtering
    private List<Move> GetDiscardedMoves(Board board, List<Move> legal_moves) {
        List<Move> filtered_moves = new List<Move>();
        
        foreach (Move mv in legal_moves) {
            board.advanced_move(mv);

            if (!isCheck(board)) filtered_moves.Add(mv);

            board.advanced_undo_move();
        }

        return filtered_moves;
    }

    private static List<Move> DiscardNonCaptures(List<Move> legal_moves) {
        List<Move> filtered_moves = new List<Move>();

        foreach(Move move in legal_moves) {
            if (move.end_square.GetPiece() != 0) filtered_moves.Add(move);
        }

        return filtered_moves;
    }

    // Square Filtering
    private List<Square> GetStraightLiabilities(Board board, Square piece_square) {
        List<Square> legal_moves = new List<Square>();

        foreach (var (dRow, dCol) in rook_directions)
        {
            int newRow = piece_square.GetRow() + dRow;
            int newCol = piece_square.GetCol() + dCol;

            while (IsInBounds(newRow, newCol) && !IsFriendlyPiece(board.b[newRow, newCol], piece_square.GetIsWhite()))
            {
                if (board.b[newRow, newCol] != 0) {  // Stop at capture
                    legal_moves.Add(  new Square(board.b, newCol, newRow)  );
                    break;
                }; 

                newRow += dRow;
                newCol += dCol;
            }
        }

        return legal_moves;
    }
    
    private List<Square> GetDiagonalLiabilities(Board board, Square piece_square) {
        List<Square> legal_moves = new List<Square>();

        foreach (var (dRow, dCol) in bishop_directions)
        {
            int newRow = piece_square.GetRow() + dRow;
            int newCol = piece_square.GetCol() + dCol;

            while (IsInBounds(newRow, newCol) && !IsFriendlyPiece(board.b[newRow, newCol], piece_square.GetIsWhite()))
            {
                if (board.b[newRow, newCol] != 0) {  // Stop at capture
                    legal_moves.Add(  new Square(board.b, newCol, newRow)  );
                    break;
                }; 

                newRow += dRow;
                newCol += dCol;
            }
        }

        return legal_moves;
    }
    
    private List<Square> GetKnightLiabilities(Board board, Square piece_square) {
        List<Square> legal_moves = new List<Square>();

        foreach (var (dRow, dCol) in knight_directions) {
            int newRow = piece_square.GetRow() + dRow;
            int newCol = piece_square.GetCol() + dCol;

            if (IsInBounds(newRow, newCol) && !IsFriendlyPiece(board.b[newRow, newCol], piece_square.GetIsWhite()))
            {
                legal_moves.Add(  new Square(board.b, newCol, newRow)  );
            }
        }

        return legal_moves;
    }
    

    private static List<Square> GetSquareType(Board board, PieceGroup p_type) {
        List<Square> type_squares = new List<Square>();

        bool white_focus = (p_type == PieceGroup.FRIENDLY &&  board.turn) || (p_type == PieceGroup.ENEMY && !board.turn) || (p_type == PieceGroup.BOTH)|| (p_type == PieceGroup.WHITE);
        bool black_focus = (p_type == PieceGroup.FRIENDLY && !board.turn) || (p_type == PieceGroup.ENEMY &&  board.turn) || (p_type == PieceGroup.BOTH)|| (p_type == PieceGroup.BLACK);

        // Get enemy squares
        for (int r=0; r<8; r++) {
            for (int c=0; c<8; c++) {
                if ((black_focus && board.b[r,c] >= 7) || (white_focus && board.b[r,c] > 0 && board.b[r,c] < 7)) {
                    type_squares.Add(new Square(board.b, c, r));
                }
            }
        }

        return type_squares;
    }


    // Hard Work Moves
    private void GetPawnMoves(Board board, Square piece_square, List<Move> legal_move_list, bool include_theory=false) {
        int direction = piece_square.GetIsWhite() ? 1 : -1; // White moves up (+1), Black moves down (-1)

        Square forward_sq = new Square(board.b, piece_square.GetCol(), piece_square.GetRow() + direction);
        Move forward_move = new Move(piece_square, forward_sq);


        if (forward_move.is_in_bounds && forward_move.end_square.GetPiece() == 0) {

            // Add forward move and set it as promotion square if needed
            if (forward_move.end_square.GetRow()%7 == 0) {
                forward_move = new Move(piece_square, forward_sq);
                forward_move.promote = board.turn ? 2 : 9; // Promote to Rook
                legal_move_list.Add(forward_move);

                forward_move = new Move(piece_square, forward_sq);
                forward_move.promote = board.turn ? 3 : 10; // Promote to Bishop
                legal_move_list.Add(forward_move);

                forward_move = new Move(piece_square, forward_sq);
                forward_move.promote = board.turn ? 4 : 11; // Promote to Knight
                legal_move_list.Add(forward_move);

                forward_move = new Move(piece_square, forward_sq);
                forward_move.promote = board.turn ? 5 : 12; // Promote to Queen
                legal_move_list.Add(forward_move);
            }
            else {
                legal_move_list.Add(forward_move); // Normal 1 space forward move
            }


            // DOUBLE MOVE
            Square double_forward_sq = new Square(board.b, piece_square.GetCol(), piece_square.GetRow() + (2*direction));
            Move double_forward_move = new Move(piece_square, double_forward_sq);

            bool is_on_starting_square = double_forward_move.start_square.GetRow() == double_forward_move.start_square.GetStartRow();

            if (is_on_starting_square && double_forward_move.end_square.GetPiece() == 0) {
                legal_move_list.Add(double_forward_move);
            }
        }

        
        // DIAGONAL MOVES
        Square l_diagonal_sq = new Square(board.b, piece_square.GetCol()-1, piece_square.GetRow()+direction);
        Square r_diagonal_sq = new Square(board.b, piece_square.GetCol()+1, piece_square.GetRow()+direction);
        
        Move l_diagonal_move = new Move(piece_square, l_diagonal_sq);
        Move r_diagonal_move = new Move(piece_square, r_diagonal_sq);
        Move diagonal_promotion_move;

        foreach(Move diagonal_move in new Move[] {l_diagonal_move, r_diagonal_move}) {
            if (diagonal_move.is_in_bounds) {
                
                int captured_piece = diagonal_move.end_square.GetPiece();
                if ((captured_piece != 0 && !IsFriendlyPiece(captured_piece, piece_square.GetIsWhite())) || include_theory) {

                    // Add end square and promotion if needed
                    if (diagonal_move.end_square.GetRow()%7 != 0) {
                        legal_move_list.Add(diagonal_move);
                        continue;
                    }

                    diagonal_promotion_move = new Move(piece_square, diagonal_move.end_square);
                    diagonal_promotion_move.promote = board.turn ? 2 : 9; // Promote to Rook
                    legal_move_list.Add(diagonal_promotion_move);

                    diagonal_promotion_move = new Move(piece_square, diagonal_move.end_square);
                    diagonal_promotion_move.promote = board.turn ? 3 : 10; // Promote to Bishop
                    legal_move_list.Add(diagonal_promotion_move);

                    diagonal_promotion_move = new Move(piece_square, diagonal_move.end_square);
                    diagonal_promotion_move.promote = board.turn ? 4 : 11; // Promote to Knight
                    legal_move_list.Add(diagonal_promotion_move);

                    diagonal_promotion_move = new Move(piece_square, diagonal_move.end_square);
                    diagonal_promotion_move.promote = board.turn ? 5 : 12; // Promote to Queen
                    legal_move_list.Add(diagonal_promotion_move);
                }
            }

        }

        // EN PASSANT
        GetEnPassantMoves(board, piece_square, legal_move_list);
    }

    private void GetEnPassantMoves(Board board, Square piece_square, List<Move> legal_move_list) {
        if (board.move_list.Count == 0) return;

        // Last move
        if (lastMove.start_square.GetPieceType() == 1)
        {
            // Check if the last move was a two-step pawn move
            bool is_double_move = Math.Abs(lastMove.start_square.GetRow() - lastMove.end_square.GetRow()) == 2;
            if (is_double_move && piece_square.GetRow() == lastMove.end_square.GetRow() && (piece_square.GetCol() == lastMove.end_square.GetCol()+1 || piece_square.GetCol() == lastMove.end_square.GetCol()-1))
            {
                int pawn_fwd_dir = piece_square.GetIsWhite() ? 1 : -1;

                Move en_passant_move = new Move(board, piece_square, lastMove.end_square.GetCol(), lastMove.end_square.GetRow() + pawn_fwd_dir);
                en_passant_move.isEnpassant = true;
                legal_move_list.Add(en_passant_move);
            }
        }
    }

    private void GetKingMoves(Board board, Square piece_square, List<Move> legal_move_list, bool safe_search) {
        AddConcreteMoves(board, piece_square, legal_move_list, king_directions);

        // CASTLING
        if (board.turn) {
            bool white_short = !board.is_h1_rook_moved && !board.is_wking_moved && board.turn;
            bool white_long  = !board.is_a1_rook_moved && !board.is_wking_moved && board.turn;

            if (white_short && board.b[0, 5] == 0 && board.b[0, 6] == 0 && board.b[0, 7]%7 == 2) {
            if (!isSquareAttacked(board, new Square(board, "f1", col_rep:MBool.T), check_search:safe_search)) {
            if (!isSquareAttacked(board, new Square(board, "e1", col_rep:MBool.T), check_search:safe_search)) {
                
                Move mv = new Move(board, "e1g1");
                mv.is_castle_white_short = true;
                legal_move_list.Add(mv);

            }}}

            if (white_long && board.b[0, 3] == 0 && board.b[0, 2] == 0 && board.b[0, 1] == 0 && board.b[0, 0]%7 == 2) {
            if (!isSquareAttacked(board, new Square(board, "d1", col_rep:MBool.T), check_search:safe_search)) {
            if (!isSquareAttacked(board, new Square(board, "e1", col_rep:MBool.T), check_search:safe_search)) {
                
                Move mv = new Move(board, "e1c1");
                mv.is_castle_white_long = true;
                legal_move_list.Add(mv);

            }}}
        }

        else {
            bool black_short = !board.is_h8_rook_moved && !board.is_bking_moved && !board.turn;
            bool black_long  = !board.is_a8_rook_moved && !board.is_bking_moved && !board.turn;

            if (black_short && board.b[7, 5] == 0 && board.b[7, 6] == 0 && board.b[7, 7]%7 == 2) {
            if (!isSquareAttacked(board, new Square(board, "f8", col_rep:MBool.T), check_search:safe_search)) {
            if (!isSquareAttacked(board, new Square(board, "e8", col_rep:MBool.T), check_search:safe_search)) {
                
                Move mv = new Move(board, "e8g8");
                mv.is_castle_black_short = true;
                legal_move_list.Add(mv);

            }}}

            if (black_long && board.b[7, 3] == 0 && board.b[7, 2] == 0 && board.b[7, 1] == 0 && board.b[7, 0]%7 == 2) {
            if (!isSquareAttacked(board, new Square(board, "d8", col_rep:MBool.T), check_search:safe_search)) {
            if (!isSquareAttacked(board, new Square(board, "e8", col_rep:MBool.T), check_search:safe_search)) {
                
                Move mv = new Move(board, "e8c8");
                mv.is_castle_black_long = true;
                legal_move_list.Add(mv);

            }}}
        }
    }


    // Easy Moves
    private void GetKnightMoves(Board board, Square piece_square, List<Move> legal_move_list) {  AddConcreteMoves(board, piece_square, legal_move_list, knight_directions);  }
    private void GetRookMoves(Board board, Square piece_square, List<Move> legal_move_list) {  AddSlidingMoves(board, piece_square, legal_move_list, rook_directions);  }
    private void GetBishopMoves(Board board, Square piece_square, List<Move> legal_move_list) {  AddSlidingMoves(board, piece_square, legal_move_list, bishop_directions);  } 
    private void GetQueenMoves(Board board, Square piece_square, List<Move> legal_move_list) {  AddSlidingMoves(board, piece_square, legal_move_list, queen_directions);  } 

    private static void AddSlidingMoves(Board board, Square piece_square, List<Move> legal_move_list, (int, int)[] directions)
    {
        foreach (var (dRow, dCol) in directions)
        {
            int newRow = piece_square.GetRow() + dRow;
            int newCol = piece_square.GetCol() + dCol;

            while (IsInBounds(newRow, newCol) && !IsFriendlyPiece(board.b[newRow, newCol], piece_square.GetIsWhite()))
            {
                Move mv = new Move(board, piece_square, newCol, newRow);
                legal_move_list.Add(mv);
                if (board.b[newRow, newCol] != 0) break;

                newRow += dRow;
                newCol += dCol;
            }
        }
    }

    private static void AddConcreteMoves(Board board, Square piece_square, List<Move> legal_move_list, (int, int)[] places) {
        foreach (var (dRow, dCol) in places) {
            int newRow = piece_square.GetRow() + dRow;
            int newCol = piece_square.GetCol() + dCol;

            Move knight_move = new Move(board, piece_square, newCol, newRow);

            if (knight_move.is_in_bounds && !IsFriendlyPiece(knight_move.end_square.GetPiece(), piece_square.GetIsWhite()))
            {
                legal_move_list.Add(knight_move);
            }
        }
    }
    

    // Helper Functions
    private static bool IsFriendlyPiece(int piece, bool isWhite) { return isWhite ? (piece > 0 && piece < 7) : (piece >= 7);}
    private static bool IsInBounds(int row, int col) => row >= 0 && row < 8 && col >= 0 && col < 8;
}
