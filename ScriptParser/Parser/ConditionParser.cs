using System;
using System.Collections.Generic;
using System.Linq;
using static ScriptParser.Parser.ScriptParser;

namespace ScriptParser.Parser
{
    public static class ConditionParser
    {
        public static ConditionNode Parse(List<string> lines)
        {
            if (lines.Count == 0)
                throw new Exception("条件式ブロックが空です");

            // -------------------------
            // 1. 行を分類（Comparison / AND / OR / NOT）
            // -------------------------
            var parsed = new List<(string type, string content)>();
            foreach (var raw in lines)
            {
                var line = raw.Trim();

                // コメント除去
                var noComment = line.Split("//")[0].Trim();
                if (string.IsNullOrEmpty(noComment))
                    continue;

                line = noComment;

                // --- AND / OR / NOT（スペース無し） ---
                if (line.StartsWith("&&"))
                {
                    var content = line.Substring(2).Trim();
                    parsed.Add(content.StartsWith("!")
                        ? ("NOT", content.Substring(1).Trim())
                        : ("AND", content));
                    continue;
                }

                if (line.StartsWith("||"))
                {
                    var content = line.Substring(2).Trim();
                    parsed.Add(content.StartsWith("!")
                        ? ("NOT", content.Substring(1).Trim())
                        : ("OR", content));
                    continue;
                }

                // --- AND / OR / NOT（スペース付き） ---
                if (line.StartsWith("AND "))
                {
                    var content = line.Substring(4).Trim();
                    parsed.Add(content.StartsWith("!")
                        ? ("NOT", content.Substring(1).Trim())
                        : ("AND", content));
                    continue;
                }

                if (line.StartsWith("OR "))
                {
                    var content = line.Substring(3).Trim();
                    parsed.Add(content.StartsWith("!")
                        ? ("NOT", content.Substring(1).Trim())
                        : ("OR", content));
                    continue;
                }

                if (line.StartsWith("NOT "))
                {
                    parsed.Add(("NOT", line.Substring(4).Trim()));
                    continue;
                }

                // --- NOT（単独） ---
                if (line.StartsWith("!"))
                {
                    parsed.Add(("NOT", line.Substring(1).Trim()));
                    continue;
                }

                // --- 比較式 or 単一変数 ---
                if (line.Contains(' '))
                    parsed.Add(("CMP", line));   // 比較式
                else
                    parsed.Add(("BOOL", line));  // 単一変数 → flag == true
            }

            // -------------------------
            // 2. AND/OR の構造チェック
            // -------------------------
            if (parsed[0].type == "AND" || parsed[0].type == "OR")
                throw new Exception("条件式の最初の行に AND / OR は書けません");

            // -------------------------
            // 3. OR グループに分割
            // -------------------------
            var orGroups = new List<OrGroup>();
            var currentAndGroup = new List<AndGroup>();
            var currentTerms = new List<SingleCondition>();

            void FlushAndGroup()
            {
                if (currentTerms.Count > 0)
                {
                    currentAndGroup.Add(new AndGroup(new List<SingleCondition>(currentTerms)));
                    currentTerms.Clear();
                }
            }

            void FlushOrGroup()
            {
                FlushAndGroup();
                if (currentAndGroup.Count > 0)
                {
                    orGroups.Add(new OrGroup(new List<AndGroup>(currentAndGroup)));
                    currentAndGroup.Clear();
                }
            }

            // -------------------------
            // 4. 行を走査して AST を構築
            // -------------------------
            foreach (var (type, content) in parsed)
            {
                switch (type)
                {
                    case "CMP":
                        currentTerms.Add(ParseComparison(content));
                        break;

                    case "NOT":
                        {
                            // content が比較式か単一変数かを判定
                            if (content.Contains(' '))
                            {
                                // 比較式の否定 → 比較式をパースして反転
                                var cmp = ParseComparison(content);
                                var inverted = InvertComparison(cmp);
                                currentTerms.Add(inverted);
                            }
                            else
                            {
                                // 単一変数の否定 → flag == false に変換
                                ValidateBooleanVariable(content);
                                currentTerms.Add(new ComparisonCondition(content, "==", "false", false));
                            }
                            break;
                        }

                    case "AND":
                        if (content.Contains(' '))
                            currentTerms.Add(ParseComparison(content));
                        else
                            currentTerms.Add(AsBooleanTrue(content));
                        break;

                    case "OR":
                        FlushAndGroup();
                        if (content.Contains(' '))
                            currentTerms.Add(ParseComparison(content));
                        else
                            currentTerms.Add(AsBooleanTrue(content));
                        break;
                    case "BOOL":
                        currentTerms.Add(AsBooleanTrue(content));
                        break;
                }
            }

            // 最後のグループを閉じる
            FlushOrGroup();

            return new ConditionNode(orGroups);
        }

        // -------------------------
        // 比較式の解析
        // -------------------------
        private static ComparisonCondition ParseComparison(string line)
        {
            var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length != 3)
                throw new Exception($"比較式が不正です: {line}");

            var left = tokens[0];
            var op = tokens[1];
            var rightRaw = tokens[2];

            // 左辺は変数名のみ
            if (!IsIdentifier(left))
                throw new Exception($"左辺は変数名のみです: {left}");

            // 演算子チェック
            if (!IsValidOperator(op))
                throw new Exception($"不正な演算子です: {op}");

            // 右辺の型判定
            object? rightValue = ParseLiteral(rightRaw);

            // 型チェック
            ValidateType(left, op, rightRaw, rightValue);

            return new ComparisonCondition(left, op, rightRaw, rightValue);
        }

        // -------------------------
        // 右辺のリテラル解析
        // -------------------------
        private static object? ParseLiteral(string token)
        {
            if (token.StartsWith("\"") && token.EndsWith("\""))
                return token.Substring(1, token.Length - 2);

            if (int.TryParse(token, out var i))
                return i;

            if (bool.TryParse(token, out var b))
                return b;

            // 変数名として扱う
            return token;
        }

        // -------------------------
        // 型チェック
        // -------------------------
        private static void ValidateType(string left, string op, string rightRaw, object? rightValue)
        {
            bool leftIsVar = IsIdentifier(left);

            // 右辺が変数名の場合は型不明 → 実行時にチェック
            if (rightValue is string && IsIdentifier((string)rightValue))
                return;

            // 数値比較
            if (rightValue is int)
            {
                if (op == "==" || op == "!=" || op == "<" || op == ">" || op == "<=" || op == ">=")
                    return;

                throw new Exception($"数値に対して不正な演算子です: {op}");
            }

            // 文字列比較
            if (rightValue is string)
            {
                if (op == "==" || op == "!=")
                    return;

                throw new Exception($"文字列に < > <= >= は使えません: {op}");
            }

            // bool 比較
            if (rightValue is bool)
            {
                if (op == "==" || op == "!=")
                    return;

                throw new Exception($"bool に < > <= >= は使えません: {op}");
            }
        }

        private static bool IsIdentifier(string s)
        {
            return s.All(ch => char.IsLetterOrDigit(ch) || ch == '_' || ch == '.');
        }

        private static bool IsValidOperator(string op)
        {
            return op switch
            {
                "==" => true,
                "!=" => true,
                "<" => true,
                ">" => true,
                "<=" => true,
                ">=" => true,
                _ => false
            };
        }

        private static void ValidateBooleanVariable(string variable)
        {
            // 変数名チェック
            if (!IsIdentifier(variable))
                throw new Exception($"NOT の対象が変数名ではありません: {variable}");

            //// 型チェック（symbolTable がある前提）
            //if (!symbolTable.Exists(variable))
            //    throw new Exception($"未定義の変数です: {variable}");

            //if (symbolTable.GetType(variable) != VarType.Boolean)
            //    throw new Exception($"NOT は boolean にしか使えません: {variable}");
        }

        private static ComparisonCondition InvertComparison(ComparisonCondition cmp)
        {
            string invertedOp = cmp.Operator switch
            {
                "==" => "!=",
                "!=" => "==",
                "<" => ">=",
                ">" => "<=",
                "<=" => ">",
                ">=" => "<",
                _ => throw new Exception($"NOT で反転できない演算子です: {cmp.Operator}")
            };

            return new ComparisonCondition(cmp.Left, invertedOp, cmp.RightRaw, cmp.RightValue);
        }

        private static ComparisonCondition AsBooleanTrue(string variable)
        {
            ValidateBooleanVariable(variable);
            return new ComparisonCondition(variable, "==", "true", true);
        }
    }
}