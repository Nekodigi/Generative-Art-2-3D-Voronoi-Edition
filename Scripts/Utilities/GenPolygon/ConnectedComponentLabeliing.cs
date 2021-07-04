using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectedComponentLabeliing
{
    int[,] binaryCanvas;
    int[,] labels;
    int ni, nj;
    public int currentLabel;

    // Update is called once per frame
    public int[,] Generate(int[,] binaryCanvas)
    {
        currentLabel = 0;//current label(start from 1)
        this.binaryCanvas = binaryCanvas;
        ni = binaryCanvas.GetLength(0);
        nj = binaryCanvas.GetLength(1);
        this.labels = new int[ni, nj];
        for (int i = 0; i < ni; i++)
        {
            for (int j = 0; j < nj; j++)
            {
                int d = binaryCanvas[i,j];
                if (d == 1 && labels[i,j] == 0)
                {
                    currentLabel++;
                    labels[i,j] = currentLabel;
                    flood(i, j);
                }
            }
        }
        return labels;
    }

    int[] set(params int[] x)
    {
        return x;
    }

    void flood(int i, int j)
    {//set label to all connecting cell 
        Queue<int[]> cells = new Queue<int[]>();
        cells.Enqueue(set(i, j));
        while (cells.Count > 0)
        {
            int[] cell = cells.Dequeue();
            if (cell[0] > 0      && binaryCanvas[cell[0] - 1,cell[1]] == 1 && labels[cell[0] - 1,cell[1]] == 0) { cells.Enqueue(set(cell[0] - 1, cell[1])); labels[cell[0] - 1,cell[1]] = currentLabel; }
            if (cell[0] < ni - 1 && binaryCanvas[cell[0] + 1,cell[1]] == 1 && labels[cell[0] + 1,cell[1]] == 0) { cells.Enqueue(set(cell[0] + 1, cell[1])); labels[cell[0] + 1,cell[1]] = currentLabel; }
            if (cell[1] > 0      && binaryCanvas[cell[0],cell[1] - 1] == 1 && labels[cell[0],cell[1] - 1] == 0) { cells.Enqueue(set(cell[0], cell[1] - 1)); labels[cell[0],cell[1] - 1] = currentLabel; }
            if (cell[1] < nj - 1 && binaryCanvas[cell[0],cell[1] + 1] == 1 && labels[cell[0],cell[1] + 1] == 0) { cells.Enqueue(set(cell[0], cell[1] + 1)); labels[cell[0],cell[1] + 1] = currentLabel; }

        }
    }
}
