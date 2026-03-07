using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class VariableStore
{
    // 非公式変数（値コピー）
    private readonly Dictionary<string, object> _values = new();

    // 公式変数（ゲーム内オブジェクトと同期）
    private readonly Dictionary<string, (Func<object> getter, Action<object> setter)> _bindings
        = new();

    private readonly HashSet<string> _officialKeys = new();

    public bool IsOfficial(string key) => _officialKeys.Contains(key);
    public bool IsUnofficial(string key) => !_officialKeys.Contains(key);

    public bool HasValue(string key) => _values.ContainsKey(key);

    // -------------------------
    // 値の設定（コピー）
    // -------------------------
    public void SetValue(string key, object value)
    {
        if (IsOfficial(key))
        {
            var (_, setter) = _bindings[key];
            setter(value);
        }
        else
        {
            if (value is int i)
                _values[key] = (long)i;
            else if (value is long l)
                _values[key] = l;
            else
                _values[key] = DeepCopy(value);
        }
    }

    // -------------------------
    // 値の取得
    // -------------------------
    public T GetValue<T>(string key)
    {
        if (IsOfficial(key))
        {
            var (getter, _) = _bindings[key];
            return (T)getter();
        }

        if (!_values.TryGetValue(key, out var v))
            return default;

        // long → int の安全変換
        if (typeof(T) == typeof(int) && v is long l)
            return (T)(object)(int)l;

        return (T)v;
    }

    // -------------------------
    // 公式変数のバインド
    // -------------------------
    public void Bind(string key, Func<object> getter, Action<object> setter)
    {
        if (_officialKeys.Contains(key))
            throw new Exception($"Official variable '{key}' is already bound.");

        _officialKeys.Add(key);
        _bindings[key] = (getter, setter);
    }

    // -------------------------
    // @key 展開（※ゲーム内オブジェクトアクセスは別 Evaluator が担当）
    // -------------------------
    public string Expand(string text)
    {
        return Regex.Replace(text, @"@([a-zA-Z0-9_\.]+)", m =>
        {
            var key = m.Groups[1].Value;

            // 公式変数
            if (IsOfficial(key))
            {
                var (getter, _) = _bindings[key];
                return getter()?.ToString() ?? $"@{key}";
            }

            // 非公式変数
            return _values.TryGetValue(key, out var v) ? v.ToString() : $"@{key}";
        });
    }

    // -------------------------
    // ディープコピー（必要に応じて拡張）
    // -------------------------
    private object DeepCopy(object value)
    {
        // プリミティブ・文字列はそのまま
        if (value == null || value.GetType().IsPrimitive || value is string)
            return value;

        // TODO: 必要に応じてオブジェクトコピーを実装
        // 現状はシャローコピー
        return value;
    }
}