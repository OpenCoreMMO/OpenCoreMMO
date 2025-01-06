using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace NeoServer.Data.Helpers;

public class DBResult
{
    private readonly List<Dictionary<string, object>> _rows;
    private int _currentIndex = 0;

    public DBResult(DbDataReader dbReader)
    {
        var rows = new List<Dictionary<string, object>>();

        while (dbReader.Read())
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < dbReader.FieldCount; i++)
            {
                row[dbReader.GetName(i).ToLowerInvariant()] = dbReader.GetValue(i);
            }
            rows.Add(row);
        }

        _rows = rows.ToList();
    }

    public bool HasNext => _currentIndex + 1 < _rows.Count;

    public bool Next()
    {
        if (HasNext)
        {
            _currentIndex++;
            return true;
        }
        return false;
    }

    public T Get<T>(string columnName)
    {
        if (_currentIndex < 0 || _currentIndex >= _rows.Count)
            throw new InvalidOperationException("No row selected. Call Next() first.");

        if (!_rows[_currentIndex].ContainsKey(columnName.ToLowerInvariant()))
            throw new ArgumentException($"Column '{columnName}' does not exist in the result set.");

        var value = _rows[_currentIndex][columnName.ToLowerInvariant()];
        if (value == null || value is DBNull)
            return default;

        return (T)Convert.ChangeType(value, typeof(T));
    }
}

