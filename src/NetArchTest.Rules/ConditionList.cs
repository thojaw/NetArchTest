namespace NetArchTest.Rules
{
    using Mono.Cecil;
    using NetArchTest.Rules.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A set of conditions and types that have have conjunctions (i.e. "and", "or")
    /// and executors (i.e. Types(), GetResult()) applied to them.
    /// </summary>
    public sealed class ConditionList : IDisposable
    {
        /// <summary>
        /// The parant to dispose.
        /// </summary>
        private readonly Types _types;

        /// <summary>
        /// A list of types that conditions can be applied to.
        /// </summary>
        private readonly IEnumerable<TypeDefinition> _typeDefinitions;

        /// <summary>
        /// The sequence of conditions that is applied to the type of list.
        /// </summary>
        private readonly FunctionSequence _sequence;

        /// <summary>
        /// Determines the polarity of the selection, i.e. "should" or "should not".
        /// </summary>
        private readonly bool _should;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionList"/> class.
        /// </summary>
        internal ConditionList(Types types, bool should, FunctionSequence sequence)
        {
            _types = types;
            _typeDefinitions = types.GetTypeDefinitions();
            _should = should;
            _sequence = sequence;
        }

        /// <summary>
        /// Returns an indication of whether all the selected types satisfy the conditions.
        /// </summary>
        /// <returns>An indication of whether the conditions are true, along with a list of types failing the check if they are not.</returns>
        public TestResult GetResult(bool disposeReferences = true)
        {
            var resultingDefinitions = _sequence
                .Execute(_typeDefinitions);

            var success = _should
                ? resultingDefinitions.Count() == _typeDefinitions.Count()
                : !resultingDefinitions.Any();

            try
            {
                if (success)
                {
                    return TestResult.Success();
                }

                var resultList = _should
                        ? _typeDefinitions.Except(resultingDefinitions).ToList()
                        : resultingDefinitions.Distinct().ToList();

                // Filter to only relevant dependency information
                var dependenciesList = resultingDefinitions.DependencyInfo?
                    .Where(x => resultList.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);

                return TestResult.Failure(
                    resultList,
                    dependenciesList);
            }
            finally
            {
                if (disposeReferences)
                {
                    _types.Dispose();
                }
            }
        }

        /// <summary>
        /// Returns the number of types that satisfy the conditions.
        /// </summary>
        /// <returns>A list of types.</returns>
        public int Count()
            => _sequence.Execute(_typeDefinitions).Count();

        /// <summary>
        /// Returns the list of types that satisfy the conditions.
        /// </summary>
        /// <returns>A list of types.</returns>
        public IEnumerable<Type> GetTypes()
            => _sequence.Execute(_typeDefinitions).Select(t => t.ToType());

        /// <summary>
        /// Returns the list of type names that satisfy the conditions.
        /// </summary>
        /// <remarks>
        /// This is a "safer" way of getting a list of types that satisfy the conditions as it does not load the types when enumerating the list. This can lead to dependency loading errors.
        /// </remarks>
        public IEnumerable<string> GetTypeNames()
            => _sequence.Execute(_typeDefinitions).Select(t => t.FullName);

        /// <summary>
        /// Specifies that any subsequent condition should be treated as an "and" condition.
        /// </summary>
        /// <returns>An set of conditions that can be applied to a list of classes.</returns>
        /// <remarks>And() has higher priority than Or() and it is computed first.</remarks>
        public Conditions And()
            => new(_types, _should, _sequence);

        /// <summary>
        /// Specifies that any subsequent conditions should be treated as part of an "or" condition.
        /// </summary>
        /// <returns>An set of conditions that can be applied to a list of classes.</returns>
        public Conditions Or()
        {
            // Create a new group of functions - this has the effect of creating an "or" condition
            _sequence.CreateGroup();
            return new Conditions(_types, _should, _sequence);
        }

        /// <inheritdoc />
        public void Dispose() => _types.Dispose();
    }
}
