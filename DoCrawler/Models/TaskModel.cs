using System.Collections.Generic;
using LibParameters;

namespace DoCrawler.Models;

public sealed class TaskModel : ItemData
{
    public List<string> StartPoints { get; set; } = new();


    public bool CheckNewStartPointValid(string oldStartPoint, string newStartPoint)
    {
        if (oldStartPoint == newStartPoint)
            return true;

        if (!StartPoints.Contains(oldStartPoint))
            return false;

        return !StartPoints.Contains(newStartPoint);
    }

    public bool RemoveStartPoint(string startPoint)
    {
        if (!StartPoints.Contains(startPoint))
            return false;
        StartPoints.Remove(startPoint);
        return true;
    }

    public bool AddStartPoint(string newStartPoint)
    {
        if (StartPoints.Contains(newStartPoint))
            return false;
        StartPoints.Add(newStartPoint);
        return true;
    }
}