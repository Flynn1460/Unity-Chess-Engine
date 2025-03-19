using System;

public struct HASH {
    public ulong r12;
    public ulong r34;
    public ulong r56;
    public ulong r78;

    public ushort flags;

    public HASH (ulong r12_, ulong r34_, ulong r56_, ulong r78_, ushort flags_){
        r12 = r12_;
        r34 = r34_;
        r56 = r56_;
        r78 = r78_;

        flags = flags_;
    }

    public override string ToString()
    {
        return $"{Convert.ToString((long)r12, 2)}-{Convert.ToString((long)r34, 2)}-{Convert.ToString((long)r56, 2)}-{Convert.ToString((long)r78, 2)}:{Convert.ToString((long)flags, 2)}";
    }
}


public class HashGen {  

    private static readonly ulong[] pieceMap = {
        0b0000, 0b0001, 0b0010, 0b0011, 0b0100, 0b0101, 0b0110, 0, 
        0b1001, 0b1010, 0b1011, 0b1100, 0b1101, 0b1110, 0b1111
    };

    private HASH generate_hash(Board board) {
        ulong R12 = 0, R34 = 0, R56 = 0, R78 = 0;
        int index = 0;

        for (int rowOffset = 0; rowOffset < 8; rowOffset += 2) {
            ulong hashValue = 0;

            for (int row = 0; row < 2; row++) {
                for (int col = 0; col < 8; col++) {
                    // Shift left by 4 and add mapped value
                    try {hashValue = (hashValue << 4) | pieceMap[board.b[row + rowOffset, col]];}
                    catch {
                        UnityEngine.Debug.Log(row+rowOffset + ", " + col);
                        board.PrintBoard(null);
                        UnityEngine.Debug.Log(board.b[1, 0]); 
                        UnityEngine.Debug.Log(pieceMap[board.b[1, 0]]);
                    }
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

    public HASH MAKE_HASH(Board board) {
        HASH gen_hash = generate_hash(board);

        return gen_hash;
    }

    public bool COMPARE_HASH(HASH x, HASH y) {
        if (x.r12 != y.r12) return false;
        if (x.r34 != y.r34) return false;
        if (x.r56 != y.r56) return false;
        if (x.r78 != y.r78) return false;
        if (x.flags != y.flags) return false;

        return true;
    }
}