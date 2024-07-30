namespace NetArchTest.Rules
{
    using Mono.Cecil;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A sequence of function calls that are combined to select types.
    /// </summary>
    public sealed class FunctionSequence
    {
        /// <summary> Holds the groups of function calls. </summary>
        private readonly List<List<FunctionCall>> _groups;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionSequence"/> class.
        /// </summary>
        internal FunctionSequence()
        {
            _groups = [];
            CreateGroup();
        }

        /// <summary>
        /// Adds a function call to the current list.
        /// </summary>
        internal void AddFunctionCall<T>(FunctionDelegates.FunctionDelegate<T> method, T value, bool condition)
        {
            _groups.Last().Add(new FunctionCall(method, value, condition));
        }

        /// <summary>
        /// Creates a new logical grouping of function calls.
        /// </summary>
        internal void CreateGroup()
        {
            _groups.Add([]);
        }

        /// <summary>
        /// Executes all the function calls that have been specified.
        /// </summary>
        /// <returns>A list of types that are selected by the predicates (or not selected if optional reversing flag is passed).</returns>
        internal TypeDefinitionResult Execute(IEnumerable<TypeDefinition> input, bool selected = true)
        {
            var resultSets = new HashSet<TypeDefinition>();
            var dependencyInfo = new Dictionary<TypeDefinition, HashSet<string>>();

            // Execute each group of calls - each group represents a separate "or"
            foreach (var group in _groups)
            {
                // Create a copy of the class collection
                var results = new List<TypeDefinition>(input);

                // Invoke the functions iteratively - functions within a group are treated as "and" statements
                foreach (var func in group)
                {
                    var funcResults = func.FunctionDelegate.DynamicInvoke(results, func.Value, func.Condition) as TypeDefinitionResult;
                    results = [.. funcResults];

                    if (funcResults.DependencyInfo != null)
                    {
                        foreach (var item in funcResults.DependencyInfo)
                        {
                            if (dependencyInfo.TryGetValue(item.Key, out var deps))
                            {
                                deps.UnionWith(item.Value);
                            }
                            else
                            {
                                dependencyInfo.Add(item.Key, new HashSet<string>(item.Value));
                            }
                        }
                    }
                }

                if (results.Count > 0)
                {
                    resultSets.UnionWith(results);
                }
            }

            if (selected)
            {
                // Return all the types that appear in at least one of the result sets
                return new TypeDefinitionResult(resultSets, dependencyInfo.ToDictionary(x => x.Key, x => (IEnumerable<string>)x.Value));
            }
            else
            {
                // Return all the types that *do not* appear in at least one of the result sets
                var selectedTypes = resultSets.Select(t => t.FullName);
                var notSelected = input.Where(t => !selectedTypes.Contains(t.FullName));
                return new TypeDefinitionResult(notSelected, dependencyInfo.ToDictionary(x => x.Key, x => (IEnumerable<string>)x.Value));
            }
        }


        /// <summary>
        /// Represents a single function call.
        /// </summary>
        internal class FunctionCall
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FunctionCall"/> class.
            /// </summary>
            internal FunctionCall(Delegate func, object value, bool condition)
            {
                this.FunctionDelegate = func;
                this.Value = value;
                this.Condition = condition;
            }

            /// <summary>
            /// A delegate for a function call.
            /// </summary>
            public Delegate FunctionDelegate { get; private set; }

            /// <summary>
            /// The input value for the function call.
            /// </summary>
            public object Value { get; private set; }

            /// <summary>
            /// The Condition to apply to the call - i.e. "is" or "is not".
            /// </summary>
            public bool Condition { get; private set; }

        }
    }
}
