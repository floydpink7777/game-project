using GameEngine.System.Core;
using GameEngine.System.State;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GameEngine.System.Evaluator
{
    public class ObjectEvaluator
    {
        private readonly GameWorld _world;
        private readonly VariableStore _vars;

        public ObjectEvaluator(GameWorld world, VariableStore vars)
        {
            _world = world;
            _vars = vars;
        }

        public object EvaluatePath(string path)
        {
            // ① VariableStore を最優先で参照
            if (TryGetFromVariableStore(path, out var value))
                return value;

            // ② VariableStore に無い場合だけ GameWorld を辿る
            return EvaluateFromGameWorld(path);
        }

        private bool TryGetFromVariableStore(string key, out object value)
        {
            value = null;

            // 公式変数
            if (_vars.IsOfficial(key))
            {
                value = _vars.GetValue<object>(key);
                return true;
            }

            // 非公式変数
            if (_vars.HasValue(key))
            {
                value = _vars.GetValue<object>(key);
                return true;
            }

            return false;
        }

        private object EvaluateFromGameWorld(string path)
        {
            var parts = path.Split('.');

            object current = ResolveRoot(parts[0]);

            for (int i = 1; i < parts.Length; i++)
            {
                current = GetPropertyOrIndex(current, parts[i]);
            }

            return current;
        }

        private object ResolveRoot(string root)
        {
            return root switch
            {
                "npc" => _world.NPCs,
                "player" => _world.Player,
                _ => throw new Exception($"Unknown root '{root}'")
            };
        }

        private object GetPropertyOrIndex(object obj, string key)
        {
            var dictType = obj.GetType();
            if (dictType.IsGenericType &&
                dictType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var keyType = dictType.GetGenericArguments()[0];

                if (keyType == typeof(string))
                {
                    var tryGetValue = dictType.GetMethod("TryGetValue");
                    object[] args = new object[] { key, null };

                    bool found = (bool)tryGetValue.Invoke(obj, args);
                    if (!found)
                        throw new Exception($"'{key}' not found in dictionary");

                    return args[1];
                }
            }

            var prop = obj.GetType().GetProperty(
                key,
                BindingFlags.IgnoreCase |
                BindingFlags.Public |
                BindingFlags.Instance
            );

            if (prop != null)
                return prop.GetValue(obj);

            throw new Exception($"'{key}' not found on {obj.GetType().Name}");
        }

        private static object ResolveValue(
            string name,
            VariableStore vars,
            ObjectEvaluator objEval
        )
        {
        // ObjectEvaluator が VariableStore → GameWorld の順で解決してくれる
        return objEval.EvaluatePath(name);
        }
    }
}