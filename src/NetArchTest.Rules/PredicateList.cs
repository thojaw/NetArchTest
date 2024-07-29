namespace NetArchTest.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NetArchTest.Rules.Extensions;
    using Mono.Cecil;

    /// <summary>
    /// A set of predicates and types that have have conjunctions (i.e. "and", "or")
    /// and executors (i.e. Types(), TypeDefinitions()) applied to them.
    /// </summary>
    public sealed class PredicateList : IDisposable
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
        /// Initializes a new instance of the <see cref="PredicateList"/> class.
        /// </summary>
        internal PredicateList(Types types, FunctionSequence sequence) 
        {
            _types = types;
            _typeDefinitions = types.GetTypeDefinitions();
            _sequence = sequence;
        }

        /// <summary>
        /// Links a predicate defining a set of classes to a condition that tests them.
        /// </summary>
        /// <returns>A condition that tests classes against a given criteria.</returns>
        public Conditions Should()
            => new(_types.UsingPredicate(_sequence), true);

        /// <summary>
        /// Links a predicate defining a set of classes to a condition that tests them.
        /// </summary>
        /// <returns>A condition that tests classes against a given criteria.</returns>
        public Conditions ShouldNot()
            => new(_types.UsingPredicate(_sequence), false);

        /// <summary>
        /// Returns the type definitions returned by these predicate.
        /// </summary>
        /// <returns>A list of type definitions.</returns>
        internal IEnumerable<TypeDefinition> GetTypeDefinitions()
            => _sequence.Execute(_typeDefinitions);

        /// <summary>
        /// Returns the types returned by these predicates.
        /// </summary>
        /// <returns>A list of types.</returns>
        public IEnumerable<Type> GetTypes()
            => _sequence.Execute(_typeDefinitions)
                .Select(t => t.ToType());

        /// <summary>
        /// Returns the list of type names returned by these predicates.
        /// </summary>
        /// <remarks>
        /// This is a "safer" way of getting a list of types returned by these predicates as it does not load the types when enumerating the list. This can lead to dependency loading errors.
        /// </remarks>
        public IEnumerable<string> GetTypeNames()
            => _sequence.Execute(_typeDefinitions).Select(t => t.FullName);

        /// <summary>
        /// Specifies that any subsequent predicates should be treated as "and" conditions.
        /// </summary>
        /// <returns>An set of predicates that can be applied to a list of classes.</returns>
        /// <remarks>And() has higher priority than Or() and it is computed first.</remarks>
        public Predicates And()
            => new(_types, _sequence);

        /// <summary>
        /// Specifies that any subsequent predicates should be treated as part of an "or" condition.
        /// </summary>
        /// <returns>An set of predicates that can be applied to a list of classes.</returns>
        public Predicates Or()
        {
            // Create a new group of functions - this has the effect of creating an "or" condition
            _sequence.CreateGroup();
            return new Predicates(_types, _sequence);
        }

        /// <inheritdoc />
        public void Dispose() => _types.Dispose();
    }
}
