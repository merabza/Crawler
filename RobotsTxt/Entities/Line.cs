using System;
using RobotsTxt.Enums;

namespace RobotsTxt.Entities;

public class Line
{
    public LineType Type { get; private set; }
    public string Field { get; private set; }
    public string Value { get; private set; }

    public Line(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            throw new ArgumentException("Can't create a new instance of Line class with an empty line.", nameof(line));
        }

        line = line.Trim();

        if (line.StartsWith("#"))
        {
            // whole line is comment
            Type = LineType.Comment;
            return;
        }

        // if line contains comments, get rid of them
        if (line.IndexOf('#') > 0)
        {
            line = line.Remove(line.IndexOf('#'));
        }

        var field = GetField(line);
        if (string.IsNullOrWhiteSpace(field))
        {
            // If could not find the first ':' char or if there wasn't a field declaration before ':'
            Type = LineType.Unknown;
            return;
        }

        Field = field.Trim();
        Type = EnumHelper.GetLineType(field.Trim().ToLowerInvariant());
        Value = line[(field.Length + 1)..].Trim(); //we remove <field>:
    }

    private static string GetField(string line)
    {
        var index = line.IndexOf(':');
        return index == -1 ? string.Empty : line[..index];
    }
}