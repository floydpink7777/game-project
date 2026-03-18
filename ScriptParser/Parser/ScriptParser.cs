using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ScriptParser.Parser
{
    public class ScriptParser
    {
        public EventDefinition EventInfo { get; private set; } = new EventDefinition();
        public Dictionary<string, SceneNode> Scenes { get; private set; } = new Dictionary<string, SceneNode>();

        private SceneNode? currentScene;

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

        private int index = 0;
        private string[] sourceLines;

        private string CurrentLine => sourceLines[index];

        public void Parse(string[] lines)
        {
            sourceLines = lines;
            index = 0;

            while (index < sourceLines.Length)
            {
                var raw = sourceLines[index];
                var line = StripComment(raw).Trim();

                if (string.IsNullOrEmpty(line))
                {
                    index++;
                    continue;
                }

                // メタ情報 or シーン開始
                if (line.StartsWith("#"))
                {
                    ParseMetaOrSceneStart(line);
                    index++;
                    continue;
                }

                // シーン本文
                if (currentScene != null)
                {
                    var node = ParseSceneStatement();
                    currentScene.Nodes.Add(node);
                    continue; // ← index++ をスキップ
                }

                throw new Exception($"シーン開始前に本文が出現しました: {line}");
            }

            // /jump チェック
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
            else if (line.StartsWith("#"))
            {
                var label = line[1..].Trim();

                if (Scenes.ContainsKey(label))
                    throw new Exception($"Scene '{label}' はすでに定義されています");

                currentScene = new SceneNode(label);
                Scenes[label] = currentScene;
                return;
            }
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

        private IScriptNode ParseSceneStatement()
        {
            var line = CurrentLine.Trim();

            if (line.StartsWith("/if{"))
                return ParseIfBlock();

            if (line.StartsWith("/choice{"))
                return ParseChoiceBlock();

            if (line.StartsWith("/textblock"))
                return ParseTextBlock();

            if (IsDialogue(line))
            {
                index++;
                return ParseDialogue(line);
            }

            if (IsPlainDialogue(line))
            {
                index++;
                return ParsePlainDialogue(line);
            }

            if (IsCommand(line))
            {
                index++;
                return ParseCommand(line);
            }

            throw new Exception($"不正な構文です: {line}");
        }

        private bool IsDialogue(string line)
        {
            return line.Contains(":")
                && !line.StartsWith("/")
                && !line.StartsWith("#")
                && !line.StartsWith(">");
        }

        private IScriptNode ParseDialogue(string line)
        {
            int idx = line.IndexOf(":");
            var speaker = line[..idx].Trim();
            var text = line[(idx + 1)..].Trim();
            return new DialogueNode(speaker, text);
        }

        private bool IsPlainDialogue(string line)
        {
            return !line.StartsWith("/")
                && !line.StartsWith("#")
                && !line.StartsWith(">");
        }

        private IScriptNode ParsePlainDialogue(string line)
        {
            return new DialogueNode("", line);
        }

        private bool IsElseOrEndif(string line)
        {
            var t = line.Trim();
            return t == "/else" || t == "/endif";
        }

        private IfNode ParseIfBlock()
        {
            // /if{ の確認
            if (!CurrentLine.Trim().StartsWith("/if{"))
                throw new Exception("/if{ が必要です");

            index++; // /if{ の次の行へ

            // -------------------------
            // 1. 条件式ブロックの収集
            // -------------------------
            var condLines = new List<string>();

            while (!CurrentLine.Trim().Equals("}"))
            {
                var l = CurrentLine.Trim();

                if (string.IsNullOrEmpty(l))
                    throw new Exception("条件式ブロック内の空行は禁止です");

                condLines.Add(l);
                index++;
            }

            index++; // } を読み飛ばす

            // -------------------------
            // 2. 条件式の解析（後で実装）
            // -------------------------
            var condition = ConditionParser.Parse(condLines);

            // -------------------------
            // 3. THEN 本文
            // -------------------------
            var thenBody = new List<IScriptNode>();

            while (!IsElseOrEndif(CurrentLine))
            {
                thenBody.Add(ParseSceneStatement());
            }

            // -------------------------
            // 4. ELSE
            // -------------------------
            List<IScriptNode>? elseBody = null;

            if (CurrentLine.Trim() == "/else")
            {
                index++; // /else の次へ
                elseBody = new List<IScriptNode>();

                while (!CurrentLine.Trim().Equals("/endif"))
                {
                    elseBody.Add(ParseSceneStatement());
                }
            }

            // -------------------------
            // 5. /endif
            // -------------------------
            if (CurrentLine.Trim() != "/endif")
                throw new Exception("/endif が必要です");

            index++; // /endif の次へ

            return new IfNode(condition, thenBody, elseBody);
        }

        private IScriptNode ParseChoiceBlock()
        {
            // /choice{ の行を取得
            var line = CurrentLine.Trim();

            // /choice{ の { を確認
            if (!line.EndsWith("{"))
                throw new Exception("/choice{ の形式が不正です");

            index++; // 次の行へ

            var choice = new ChoiceNode();

            while (true)
            {
                var l = CurrentLine.Trim();

                // ブロック終了
                if (l == "}")
                {
                    index++;
                    break;
                }

                // 選択肢行
                if (l.StartsWith(">"))
                {
                    var parts = l[1..].Split("->");
                    if (parts.Length != 2)
                        throw new Exception("選択肢の書式が不正です: " + l);

                    var text = parts[0].Trim();
                    var jump = parts[1].Trim().TrimStart('$');

                    choice.options.Add(new ChoiceOption(text, jump));
                    index++;
                    continue;
                }

                throw new Exception($"選択肢ブロック内で不正な行です: {l}");
            }

            return choice;
        }

        private IScriptNode ParseTextBlock()
        {
            index++; // /textblock の次の行へ

            var buffer = new List<string>();

            while (true)
            {
                var line = CurrentLine.Trim();

                // ブロック終了
                if (line == "}")
                {
                    index++; // } の次へ
                    break;
                }

                // 空行は無視
                if (!string.IsNullOrWhiteSpace(line))
                    buffer.Add(line);

                index++;
            }

            // 1つの DialogueNode として返す
            var text = string.Join("\n", buffer);
            return new DialogueNode("", text);
        }

        private bool IsCommand(string line)
        {
            return line.StartsWith("/");
        }

        private IScriptNode ParseCommand(string line)
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

            return new CommandNode(name, parsedArgs);
        }
    }
}