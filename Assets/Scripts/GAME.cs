#pragma warning disable CS0219

using UnityEngine;

public class GAME : MonoBehaviour
{
    public Board b;
    void Awake()
    {
        b = new Board();
    }
}
