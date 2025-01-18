#pragma warning disable CS0219

using System;
using System.Collections.Generic;
using UnityEngine;

public class TOOLS
{
    public List<int> uci_converter(String uci_string) {
        char file = uci_string[0];
        char rank = uci_string[1];

        int file_num = file - 'a'; // Cancels out ASCII offset and adds 1
        int rank_num = (int)Char.GetNumericValue(rank) - 1;

        if (uci_string.Length == 2) {
            return new List<int>() {rank_num, file_num};
        }

        else {
            char file2 = uci_string[2];
            char rank2 = uci_string[3];

            int file_num2 = file2 - 'a'; // Cancels out ASCII offset and adds 1
            int rank_num2 = (int)Char.GetNumericValue(rank2) - 1;

            return new List<int>() {file_num, rank_num, file_num2, rank_num2};
        }
    }

    public String uci_converter(List<int> coord_values) {
        int file = coord_values[1];
        int rank = coord_values[0];

        if (coord_values.Count == 2) {
           return (char)('a' + file) + (rank+1).ToString();
        }
        else {
            int file2 = coord_values[3];
            int rank2 = coord_values[2];

            return (char)('a' + file) + (rank+1).ToString() + (char)('a' + file2) + (rank2+1).ToString();
        }
    }

    public String uci_converter(List<int> coord_value1, List<int> coord_value2) {
            int file = coord_value1[1];
            int rank = coord_value1[0];

            int file2 = coord_value2[1];
            int rank2 = coord_value2[0];

            return (char)('a' + file) + (rank+1).ToString() + (char)('a' + file2) + (rank2+1).ToString();
    }



    public List<String> strip_moves(List<string> moves, bool stripFRONTorBACK, bool include_promotion=true) {
        List<String> newMoves = new List<string>();

        foreach(String move in moves) {
            if (stripFRONTorBACK) newMoves.Add(move.Substring(0, 2));

            else if (include_promotion) newMoves.Add(move.Substring(2));
            else newMoves.Add(move.Substring(2, 2));
        }

        return newMoves;
    }


    public int bool_num(bool x) { return x ? 1 : 0; }
}
