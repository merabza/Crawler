using System;
using RobotsTxt.Enums;

namespace RobotsTxt.Entities;

public class Line
{
    // ReSharper disable once ConvertToPrimaryConstructor
    private Line(LineType type, string? field, string? value)
    {
        Type = type;
        Field = field;
        Value = value;
    }

    public LineType Type { get; private set; }
    public string? Field { get; private set; }
    public string? Value { get; private set; }

    public static Line Create(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            throw new ArgumentException("Can't create a new instance of Line class with an empty line.", nameof(line));

        line = line.Trim();

        if (line.StartsWith('#'))
            // whole line is comment
            return new Line(LineType.Comment, null, null);

        // if line contains comments, get rid of them
        if (line.IndexOf('#') > 0) line = line.Remove(line.IndexOf('#'));

        var field = GetField(line);
        if (string.IsNullOrWhiteSpace(field))
            //If could not find the first ':' char or if there wasn't a field declaration before ':'
            return new Line(LineType.Unknown, null, null);

        field = field.Trim();
        var type = EnumHelper.GetLineType(field.Trim().ToLowerInvariant());
        var value = line[(field.Length + 1)..].Trim(); //we remove <field>:

        return new Line(type, field, value);
    }

    private static string GetField(string line)
    {
        var index = line.IndexOf(':');
        return index == -1 ? string.Empty : line[..index];
    }
}