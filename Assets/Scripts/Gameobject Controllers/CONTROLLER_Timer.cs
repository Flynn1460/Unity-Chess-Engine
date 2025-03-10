using System.Collections;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class CONTROLLER_Timer : MonoBehaviour
{
    private TextMeshProUGUI white_timer;
    private TextMeshProUGUI black_timer;

    private BoardManager board_manager;

    private float WHITE_TIME = 1f;
    private float BLACK_TIME = 1f;

    private Color32 NOT_TURN_COLOUR = new Color32(203, 148, 96, 255);
    private Color32 TURN_COLOUR = new Color32(91, 238, 111, 255);


    public bool IS_TIMEOUT = false;

    void Start()
    {
        board_manager = FindFirstObjectByType<GAME>().board_manager;

        GameObject whiteTimeObj = GameObject.Find("WHITE TIME");
        GameObject blackTimeObj = GameObject.Find("BLACK TIME");

        white_timer = whiteTimeObj.GetComponent<TextMeshProUGUI>();
        black_timer = blackTimeObj.GetComponent<TextMeshProUGUI>();

        white_timer.text = "3:00";
        black_timer.text = "3:00";

        StartCoroutine(UpdateClock());
    }


    private string int_to_str_time(float seconds) {
        int hours = (int) math.floor(seconds/3600);
        seconds -= 3600*hours;

        int mins = (int) math.floor(seconds/60);
        seconds -= 60*mins;

        string raw_time = string.Format("{0:D2}:{1:D2}:{2:D2}", hours, mins, (int)seconds);

        return compact_time_str(raw_time);
    }

    private string compact_time_str(string time_str) {

        if (time_str.Substring(0, 4) == "00:0") return time_str.Substring(4);
        if (time_str.Substring(0, 3) == "00:")  return time_str.Substring(3);
        if (time_str.Substring(0, 2) == "00")   return time_str.Substring(2);
        if (time_str.Substring(0, 1) == "0")    return time_str.Substring(1);


        return time_str;
    }

    public void change_timer(float new_time) {
        string time_str = int_to_str_time(new_time);

        white_timer.text = time_str;
        black_timer.text = time_str;

        WHITE_TIME = new_time;
        BLACK_TIME = new_time;
    }

    public void flip_turn(bool turn, TextMeshProUGUI wt, TextMeshProUGUI bt) {
        if (turn) {
            white_timer.color = TURN_COLOUR;
            black_timer.color = NOT_TURN_COLOUR;

            wt.color = TURN_COLOUR;
            bt.color = NOT_TURN_COLOUR;
        }
        else {
            white_timer.color = NOT_TURN_COLOUR;
            black_timer.color = TURN_COLOUR;

            wt.color = NOT_TURN_COLOUR;
            bt.color = TURN_COLOUR;
        }
    }

    public void update_timer(float W_TIME, float B_TIME) {
        white_timer.text = int_to_str_time(W_TIME);
        black_timer.text = int_to_str_time(B_TIME);
    }

    public void ResetBM(BoardManager bm) {
        board_manager = bm;
    }

    IEnumerator UpdateClock() {
        while (true) {
            if (board_manager.board.turn) {
                WHITE_TIME -= 0.1f;
                if (WHITE_TIME <= 0f) {
                    IS_TIMEOUT = true;
                }
            }

            if (!board_manager.board.turn) {
                BLACK_TIME -= 0.1f;
                if (BLACK_TIME <= 0f) {
                    IS_TIMEOUT = true;
                }
            }


            update_timer(math.ceil(WHITE_TIME), math.ceil(BLACK_TIME));


            yield return new WaitForSeconds(0.1f);
        }
    }
}
