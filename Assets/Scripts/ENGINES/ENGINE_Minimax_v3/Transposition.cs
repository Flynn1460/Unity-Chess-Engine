
using System.Collections.Generic;

namespace ENGINE_NAMESPACE_Minimax_V3 {

struct HASH {
    public ulong r12;
    public ulong r34;
    public ulong r56;
    public ulong r78;

    public uint flags;

    public HASH (ulong r12_, ulong r34_, ulong r56_, ulong r78_, uint flags_){
        r12 = r12_;
        r34 = r34_;
        r56 = r56_;
        r78 = r78_;

        flags = flags_;
    }
}


class Transposition {
    
    private Dictionary<HASH, double> HASH_ARRAY = new Dictionary<HASH, double>();


    private HASH generate_hash(Board board) {
        ulong[] sq_values = new ulong[16];


        for (int row = 0; row < 2; row++) {
            for (int col = 0; col < 8; col++) {
                switch(board.b[row,col]) {
                    case 0: sq_values[(row*8)+col] = 0b0000; break;
                    
                    case 1 : sq_values[(row*8)+col] = 0b0001; break;
                    case 2 : sq_values[(row*8)+col] = 0b0010; break;
                    case 3 : sq_values[(row*8)+col] = 0b0011; break;
                    case 4 : sq_values[(row*8)+col] = 0b0100; break;
                    case 5 : sq_values[(row*8)+col] = 0b0101; break;
                    case 6 : sq_values[(row*8)+col] = 0b0110; break;

                    case 8 : sq_values[(row*8)+col] = 0b1001; break;
                    case 9 : sq_values[(row*8)+col] = 0b1010; break;
                    case 10: sq_values[(row*8)+col] = 0b1011; break;
                    case 11: sq_values[(row*8)+col] = 0b1100; break;
                    case 12: sq_values[(row*8)+col] = 0b1101; break;
                    case 13: sq_values[(row*8)+col] = 0b1110; break;
                }
            }
        }

        ulong R12 = Combine4BitValues(sq_values);
        sq_values = new ulong[16];


        for (int row = 0; row < 2; row++) {
            for (int col = 0; col < 8; col++) {
                switch(board.b[row+2,col]) {
                    case 0: sq_values[(row*8)+col] = 0b0000; break;
                    
                    case 1 : sq_values[(row*8)+col] = 0b0001; break;
                    case 2 : sq_values[(row*8)+col] = 0b0010; break;
                    case 3 : sq_values[(row*8)+col] = 0b0011; break;
                    case 4 : sq_values[(row*8)+col] = 0b0100; break;
                    case 5 : sq_values[(row*8)+col] = 0b0101; break;
                    case 6 : sq_values[(row*8)+col] = 0b0110; break;

                    case 8 : sq_values[(row*8)+col] = 0b1001; break;
                    case 9 : sq_values[(row*8)+col] = 0b1010; break;
                    case 10: sq_values[(row*8)+col] = 0b1011; break;
                    case 11: sq_values[(row*8)+col] = 0b1100; break;
                    case 12: sq_values[(row*8)+col] = 0b1101; break;
                    case 13: sq_values[(row*8)+col] = 0b1110; break;
                }
            }
        }

        ulong R34 = Combine4BitValues(sq_values);
        sq_values = new ulong[16];


        for (int row = 0; row < 2; row++) {
            for (int col = 0; col < 8; col++) {
                switch(board.b[row+4,col]) {
                    case 0: sq_values[(row*8)+col] = 0b0000; break;
                    
                    case 1 : sq_values[(row*8)+col] = 0b0001; break;
                    case 2 : sq_values[(row*8)+col] = 0b0010; break;
                    case 3 : sq_values[(row*8)+col] = 0b0011; break;
                    case 4 : sq_values[(row*8)+col] = 0b0100; break;
                    case 5 : sq_values[(row*8)+col] = 0b0101; break;
                    case 6 : sq_values[(row*8)+col] = 0b0110; break;

                    case 8 : sq_values[(row*8)+col] = 0b1001; break;
                    case 9 : sq_values[(row*8)+col] = 0b1010; break;
                    case 10: sq_values[(row*8)+col] = 0b1011; break;
                    case 11: sq_values[(row*8)+col] = 0b1100; break;
                    case 12: sq_values[(row*8)+col] = 0b1101; break;
                    case 13: sq_values[(row*8)+col] = 0b1110; break;
                }
            }
        }

        ulong R56 = Combine4BitValues(sq_values);
        sq_values = new ulong[16];


        for (int row = 0; row < 2; row++) {
            for (int col = 0; col < 8; col++) {
                switch(board.b[row+6,col]) {
                    case 0: sq_values[(row*8)+col] = 0b0000; break;
                    
                    case 1 : sq_values[(row*8)+col] = 0b0001; break;
                    case 2 : sq_values[(row*8)+col] = 0b0010; break;
                    case 3 : sq_values[(row*8)+col] = 0b0011; break;
                    case 4 : sq_values[(row*8)+col] = 0b0100; break;
                    case 5 : sq_values[(row*8)+col] = 0b0101; break;
                    case 6 : sq_values[(row*8)+col] = 0b0110; break;

                    case 8 : sq_values[(row*8)+col] = 0b1001; break;
                    case 9 : sq_values[(row*8)+col] = 0b1010; break;
                    case 10: sq_values[(row*8)+col] = 0b1011; break;
                    case 11: sq_values[(row*8)+col] = 0b1100; break;
                    case 12: sq_values[(row*8)+col] = 0b1101; break;
                    case 13: sq_values[(row*8)+col] = 0b1110; break;
                }
            }
        }

        ulong R78 = Combine4BitValues(sq_values);


        uint flags = 0b0000;

        if (!board.is_a1_rook_moved && !board.is_wking_moved) {
            flags |= 0b1000;
        }

        if (!board.is_h1_rook_moved && !board.is_wking_moved) {
            flags |= 0b0100;
        }

        if (!board.is_a8_rook_moved && !board.is_bking_moved) {
            flags |= 0b0010;
        }

        if (!board.is_h8_rook_moved && !board.is_bking_moved) {
            flags |= 0b1001;
        }

        return new HASH(R12, R34, R56, R78, flags);
    }

    private ulong Combine4BitValues(ulong[] values)
    {
        ulong result = 0;
        
        // For each 4-bit value
        for (int i = 0; i < values.Length; i++)
        {
            // Shift the result left by 4 bits to make room for the next 4-bit value
            result <<= 4;

            // Add the current 4-bit value to the result
            result |= values[i];
        }

        return result;
    }



    public double FIND_HASH(Board board) {
        HASH board_hash = generate_hash(board);

        if (HASH_ARRAY.ContainsKey(board_hash)) return HASH_ARRAY[board_hash];

        return -1002;
    }

    public void MAKE_HASH(Board board, double eval) {
        HASH gen_hash = generate_hash(board);

        if (!HASH_ARRAY.ContainsKey(gen_hash)) HASH_ARRAY.Add(gen_hash, eval);
    }


    public void CLEAR_HASH_TABLE() {
        HASH_ARRAY = new Dictionary<HASH, double>();
    }
}
}