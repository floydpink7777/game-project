using ScriptParser.Parser;                         // AST
using GameEngine.Events.RuntimeNode;        // RuntimeNode
using System.Collections.Generic;

namespace BuildTool.Converters
{
    public static class NodeConverter
    {
        public static NodeBase ToRuntime(IScriptNode ast)
        {
            return ast switch
            {
                ScriptParser.Parser.DialogueNode d => ToRuntimeDialogue(d),
                ScriptParser.Parser.ChoiceNode c => ToRuntimeChoice(c),
                ScriptParser.Parser.CommandNode cmd => ToRuntimeCommand(cmd),
                _ => throw new System.Exception($"Unknown AST node type: {ast.GetType().Name}")
            };
        }

        private static GameEngine.Events.RuntimeNode.DialogueNode ToRuntimeDialogue(
            ScriptParser.Parser.DialogueNode ast)
        {
            return new GameEngine.Events.RuntimeNode.DialogueNode
            {
                Type = ast.type,
                Speaker = ast.speaker,
                Text = ast.text
            };
        }

        private static GameEngine.Events.RuntimeNode.ChoiceNode ToRuntimeChoice(
            ScriptParser.Parser.ChoiceNode ast)
        {
            var runtime = new GameEngine.Events.RuntimeNode.ChoiceNode
            {
                Type = ast.type,
                Options = new List<GameEngine.Events.RuntimeNode.ChoiceOption>()
            };

            foreach (var opt in ast.options)
            {
                runtime.Options.Add(new GameEngine.Events.RuntimeNode.ChoiceOption
                {
                    Text = opt.text,
                    Jump = opt.jump
                });
            }

            return runtime;
        }

        private static GameEngine.Events.RuntimeNode.CommandNode ToRuntimeCommand(
            ScriptParser.Parser.CommandNode ast)
        {
            return new GameEngine.Events.RuntimeNode.CommandNode
            {
                Type = ast.type,
                Name = ast.name,
                Args = new List<object>(ast.args)
            };
        }
    }
}