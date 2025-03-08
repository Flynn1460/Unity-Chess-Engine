
using System.Collections.Generic;

namespace ENGINE_NAMESPACE_Minimax_V4 {

public struct HASH {
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


public class Transposition {  
    private Dictionary<HASH, double> HASH_ARRAY = new Dictionary<HASH, double>(25000, new HashEqualityComparer());

    private static readonly ulong[] pieceMap = {
        0, 0b0001, 0b0010, 0b0011, 0b0100, 0b0101, 0b0110, 
        0, 0b1001, 0b1010, 0b1011, 0b1100, 0b1101, 0b1111
    };

    private HASH generate_hash(Board board) {
        ulong R12 = 0, R34 = 0, R56 = 0, R78 = 0;
        int index = 0;

        for (int rowOffset = 0; rowOffset < 8; rowOffset += 2) {
            ulong hashValue = 0;

            for (int row = 0; row < 2; row++) {
                for (int col = 0; col < 8; col++) {
                    // Shift left by 4 and add mapped value
                    hashValue = (hashValue << 4) | pieceMap[board.b[row + rowOffset, col]];
                }
            }

            switch (index++) {
                case 0: R12 = hashValue; break;
                case 1: R34 = hashValue; break;
                case 2: R56 = hashValue; break;
                case 3: R78 = hashValue; break;
            }
        }

        ushort flags = (ushort)(
            ((board.is_a1_rook_moved | board.is_wking_moved) ? 0 : 0b1000) |
            ((board.is_h1_rook_moved | board.is_wking_moved) ? 0 : 0b0100) |
            ((board.is_a8_rook_moved | board.is_bking_moved) ? 0 : 0b0010) |
            ((board.is_h8_rook_moved | board.is_bking_moved) ? 0 : 0b0001));

        return new HASH(R12, R34, R56, R78, flags);
    }


    public double FIND_HASH(Board board) {
        HASH board_hash = generate_hash(board);

        if (HASH_ARRAY.TryGetValue(board_hash, out double eval)) return eval;

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

// Custom equality comparer for HASH struct to improve dictionary lookup speed
public class HashEqualityComparer : IEqualityComparer<HASH> {
    public bool Equals(HASH x, HASH y) {
        return x.r12 == y.r12 && x.r34 == y.r34 && x.r56 == y.r56 && x.r78 == y.r78 && x.flags == y.flags;
    }

    public int GetHashCode(HASH obj) {
        unchecked {
            int hash = 17;
            hash = hash * 23 + obj.r12.GetHashCode();
            hash = hash * 23 + obj.r34.GetHashCode();
            hash = hash * 23 + obj.r56.GetHashCode();
            hash = hash * 23 + obj.r78.GetHashCode();
            hash = hash * 23 + obj.flags.GetHashCode();
            return hash;
        }
    }
}

}