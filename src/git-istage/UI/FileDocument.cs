﻿using System.Text;
using LibGit2Sharp;

namespace GitIStage.UI;

internal sealed class FileDocument : Document
{
    private readonly int _indexOfFirstFile;
    private readonly string[] _lines;
    private readonly IReadOnlyList<TreeEntryChanges> _changes;

    private FileDocument(int indexOfFirstFile, string[] lines, IReadOnlyList<TreeEntryChanges> changes, int width)
    {
        _indexOfFirstFile = indexOfFirstFile;
        _lines = lines;
        _changes = changes;
        Width = width;
    }

    public override int Height => _lines.Length;

    public override int Width { get; }

    public override int EntryCount => _changes.Count;

    public IReadOnlyList<TreeEntryChanges> Changes => _changes;

    public override string GetLine(int index)
    {
        return _lines[index];
    }

    public override int GetLineIndex(int index)
    {
        return _indexOfFirstFile + index;
    }

    public override int FindEntryIndex(int lineIndex)
    {
        var changeIndex = lineIndex - _indexOfFirstFile;
        return Math.Min(Math.Max(changeIndex, -1), _changes.Count - 1);
    }

    public TreeEntryChanges? GetChange(int index)
    {
        var changeIndex = index - _indexOfFirstFile;
        if (changeIndex < 0 || changeIndex >= _changes.Count)
            return null;

        return _changes[changeIndex];
    }

    public static FileDocument Create(IReadOnlyList<TreeEntryChanges> changes, bool viewStage)
    {
        var builder = new StringBuilder();
        if (changes.Any())
        {
            builder.AppendLine();
            builder.AppendLine(viewStage ? "Changes to be committed:" : "Changes not staged for commit:");

            var indent = new string(' ', 8);

            foreach (var c in changes)
            {
                var path = c.Path;
                var change = (c.Status.ToString().ToLower() + ":").PadRight(12);

                builder.AppendLine();
                builder.Append(indent);
                builder.Append(change);
                builder.Append(path);
            }
        }

        const int indexOfFirstFile = 3;
        var lines = builder.Length == 0
                    ? Array.Empty<string>()
                    : builder.ToString().Split(Environment.NewLine);

        var width = lines.Select(l => l.Length)
                         .DefaultIfEmpty(0)
                         .Max();

        return new FileDocument(indexOfFirstFile, lines, changes, width);
    }
}