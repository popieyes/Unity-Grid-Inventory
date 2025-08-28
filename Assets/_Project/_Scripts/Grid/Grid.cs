using UnityEngine;

public class Grid<T>
{
    private readonly T[,] _array;
    public T[,] Array => _array;   
    public Grid(int cols, int rows)
    {
        _array = new T[cols, rows];
    }
}
