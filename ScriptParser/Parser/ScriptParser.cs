using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptParser.Parser
{
    public class ScriptParser
    {
        private enum ParseState
        {
            Meta,
            Scene,
            ChoiceBlock,
            TextBlock
        }

        private ParseState state = ParseState.Meta;

        public EventDefinition EventInfo { get; private set; } = new EventDefinition();
        public Dictionary<string, SceneNode> Scenes { get; private set; } = new Dictionary<string, SceneNode>();

        private SceneNode? currentScene;
        private ChoiceNode? currentChoice;

        private List<string> buffer = new();

        // コマンドの引数チェック用
        private static readonly Dictionary<string, ArgType[]> CommandArgTypes = new()
        {
            { "/set",  new[]{ ArgType.Variable, ArgType.String } },
            { "/add",  new[]{ ArgType.Variable, ArgType.Int } },
            { "/flag", new[]{ ArgType.Variable, ArgType.Bool } },
            { "/jump", new[]{ ArgType.String } },  // ラベル名
            { "/end",  Array.Empty<ArgType>() }
        };

        public enum ArgType
        {
            Int,
            Bool,
            String,
            Variable   // 変数参照（string だが識別子扱い）
        }

        public void Parse(string[] lines)
        {
            for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
            {
                var raw = lines[lineNumber];
                var line = StripComment(raw).Trim();
                if (string.IsNullOrEmpty(line))
                    continue;
                try
                {
                    switch (state)
                    {
                        case ParseState.Meta:
                            ParseMetaOrSceneStart(line);
                            break;

                        case ParseState.Scene:
                            ParseSceneLine(line, lineNumber);
                            break;

                        case ParseState.ChoiceBlock:
                            ParseChoiceLine(line);
                            break;
                        case ParseState.TextBlock:
                            ParseTextBlockLine(line);
                            break;
                    }
                }
                catch(Exception ex)
                {
                    throw new Exception($"[行 {lineNumber + 1}] {ex.Message}");
                }
            }

            foreach (var scene in Scenes.Values)
            {
                foreach (var node in scene.Nodes.OfType<CommandNode>())
                {
                    if (node.name == "/jump")
                    {
                        var target = node.args[0].ToString();

                        if (!Scenes.ContainsKey(target))
                            throw new Exception($"未定義ラベル '{target}' へのジャンプがあります");
                    }
                }
            }

            if (state == ParseState.ChoiceBlock)
                throw new Exception("選択肢ブロックが閉じられていません");
        }

        // -----------------------------
        // Lexer: コメント除去
        // -----------------------------
        private string StripComment(string line)
        {
            var idx = line.IndexOf("//");
            return idx >= 0 ? line[..idx] : line;
        }

        // -----------------------------
        // Parser: メタ情報 or シーン開始
        // -----------------------------
        private void ParseMetaOrSceneStart(string line)
        {
            if (line.StartsWith("#") && line.Contains("="))
            {
                var parts = line[1..].Split("=", 2);
                var key = parts[0].Trim();
                var value = parts[1].Trim().Trim('"');

                switch (key)
                {
                    case "event_name": EventInfo.EventName = value; break;
                    case "event_type": EventInfo.EventType = value; break;
                    case "event_member":
                        EventInfo.Members = value.Split(',')
                            .Select(v => v.Trim())
                            .ToList();
                        break;
                    default:
                        throw new Exception($"未知のメタ情報キーです: {key}");
                }
                return;
            }

            if (line.StartsWith("#"))
            {
                var label = line[1..].Trim();

                if (Scenes.ContainsKey(label))
                    throw new Exception($"Scene '{label}' はすでに定義されています");

                currentScene = new SceneNode(label);
                Scenes[label] = currentScene;
                state = ParseState.Scene;
                return;
            }
        }

        // -----------------------------
        // Parser: シーン行
        // -----------------------------
        private void ParseSceneLine(string line, int lineNumber)
        {
            // /choice
            if (line.StartsWith("/choice"))
            {
                currentChoice = new ChoiceNode();
                currentScene.Nodes.Add(currentChoice);
                state = ParseState.ChoiceBlock;
                return;
            }

            // ★ ラベル（シーン開始）
            if (line.StartsWith("#"))
            {
                // メタ情報の誤記を防ぐ
                if (line.Contains("="))
                    throw new Exception($"Scene フェーズでメタ情報は使用できません: {line}");

                // まず前のシーンを閉じる（/end 自動補完）
                if (currentScene != null &&
                    (currentScene.Nodes.Count == 0 ||
                     currentScene.Nodes.Last() is not CommandNode cmd || cmd.name != "/end"))
                {
                    currentScene.Nodes.Add(new CommandNode("/end", new List<object>()));
                }

                // 新しいシーン開始
                var label = line[1..].Trim();

                if (Scenes.ContainsKey(label))
                    throw new Exception($"Scene '{label}' はすでに定義されています");

                currentScene = new SceneNode(label);
                Scenes[label] = currentScene;
                return;
            }

            // /textblock
            if (line.StartsWith("/textblock"))
            {
                buffer.Clear();
                state = ParseState.TextBlock;
                return;
            }

            // Dialogue
            if (line.Contains(":") &&
                !line.StartsWith("/") &&
                !line.StartsWith("#") &&
                !line.StartsWith(">"))
            {
                int idx = line.IndexOf(":");
                var speaker = line[..idx].Trim();
                var text = line[(idx + 1)..].Trim();
                currentScene.Nodes.Add(new DialogueNode(speaker, text));
                return;
            }

            // ★ NEW: コロン無しの行は「テキストのみの DialogueNode」
            if (!line.StartsWith("/") && !line.StartsWith("#") && !line.StartsWith(">"))
            {
                currentScene.Nodes.Add(new DialogueNode("", line));
                return;
            }

            // コマンド
            if (line.StartsWith("/"))
            {
                var tokens = Tokenize(line);
                var name = tokens[0];
                var rawArgs = tokens.Skip(1).ToList();
                var parsedArgs = rawArgs.Select(ParseValue).ToList();

                if (!CommandArgTypes.TryGetValue(name, out var expectedTypes))
                    throw new Exception($"未知のコマンドです: {name}");

                if (parsedArgs.Count != expectedTypes.Length)
                    throw new Exception($"{name} の引数数が正しくありません: 必要 {expectedTypes.Length}, 実際 {parsedArgs.Count}");

                for (int i = 0; i < expectedTypes.Length; i++)
                {
                    if (!CheckArgType(parsedArgs[i], expectedTypes[i]))
                        throw new Exception($"{name} の第 {i + 1} 引数の型が不正です: 期待 {expectedTypes[i]}, 実際 {parsedArgs[i]}");
                }

                currentScene.Nodes.Add(new CommandNode(name, parsedArgs));
                return;
            }

            throw new Exception($"不正な構文です: {line}");
        }

        private object ParseValue(string token)
        {
            // 文字列リテラル
            if (token.StartsWith("\"") && token.EndsWith("\""))
                return token.Substring(1, token.Length - 2);

            // 数値
            if (int.TryParse(token, out var i))
                return i;

            // 真偽値
            if (bool.TryParse(token, out var b))
                return b;

            // それ以外は変数参照（string のまま）
            return token;
        }

        // -----------------------------
        // Parser: Choice ブロック
        // -----------------------------
        private void ParseChoiceLine(string line)
        {
            if (line == "}")
            {
                state = ParseState.Scene;
                return;
            }

            if (line.StartsWith(">"))
            {
                var parts = line[1..].Split("->");
                var text = parts[0].Trim();
                var jump = parts[1].Trim().TrimStart('$');

                currentChoice.options.Add(new ChoiceOption(text, jump));
            }
        }

        // -----------------------------
        // Lexer: 文字列リテラル対応トークナイザ
        // -----------------------------
        public static List<string> Tokenize(string line)
        {
            var tokens = new List<string>();
            var sb = new StringBuilder();
            bool inString = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    sb.Append(c);

                    if (inString)
                    {
                        tokens.Add(sb.ToString());
                        sb.Clear();
                        inString = false;
                    }
                    else
                    {
                        inString = true;
                    }

                    continue;
                }

                if (inString)
                {
                    sb.Append(c);
                    continue;
                }

                if (char.IsWhiteSpace(c))
                {
                    if (sb.Length > 0)
                    {
                        tokens.Add(sb.ToString());
                        sb.Clear();
                    }
                    continue;
                }

                sb.Append(c);
            }

            if (sb.Length > 0)
                tokens.Add(sb.ToString());

            return tokens;
        }

        private bool CheckArgType(object value, ArgType expected)
        {
            return expected switch
            {
                ArgType.Int => value is int,
                ArgType.Bool => value is bool,

                // ArgType.String は「任意型」を許可する
                ArgType.String => true,

                // Variable は「識別子としての string」
                ArgType.Variable => value is string,

                _ => false
            };
        }

        private void ParseTextBlockLine(string line)
        {
            // ブロック終了
            if (line == "}")
            {
                var text = string.Join("\n", buffer);
                currentScene.Nodes.Add(new DialogueNode("", text));
                buffer.Clear();
                state = ParseState.Scene;
                return;
            }

            // 空行は無視
            if (string.IsNullOrWhiteSpace(line))
                return;

            // すべてナレーション扱い
            buffer.Add(line);
        }
    }
}