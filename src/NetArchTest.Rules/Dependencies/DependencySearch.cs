namespace NetArchTest.Rules.Dependencies
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Mono.Cecil;
    using NetArchTest.Rules.Dependencies.DataStructures;

    /// <summary>
    /// Finds dependencies within a given set of types.
    /// </summary>
    internal class DependencySearch
    {
        /// <summary>
        /// Finds types that have a dependency on any item in the given list of dependencies.
        /// </summary>
        /// <param name="input">The set of type definitions to search.</param>
        /// <param name="dependencies">The set of dependencies to look for.</param>
        /// <returns>A list of found types.</returns>
        public IReadOnlyDictionary<TypeDefinition, IEnumerable<string>> FindTypesThatHaveDependencyOnAny(IEnumerable<TypeDefinition> input, IEnumerable<string> dependencies)
        {  
            return FindTypes(input, TypeDefinitionCheckingResult.SearchType.HaveDependencyOnAny, dependencies, true);           
        }

        public IReadOnlyDictionary<TypeDefinition, IEnumerable<string>> FindTypesThatHaveDependencyOnAnyMatching(IEnumerable<TypeDefinition> input, Regex regex)
        {
            return FindTypes(input, TypeDefinitionCheckingResult.SearchType.HaveDependencyOnAny, regex, true);           
        }

        /// <summary>
        /// Finds types that have a dependency on every item in the given list of dependencies.
        /// </summary>
        /// <param name="input">The set of type definitions to search.</param>
        /// <param name="dependencies">The set of dependencies to look for.</param>
        /// <returns>A list of found types.</returns>
        public IReadOnlyDictionary<TypeDefinition, IEnumerable<string>> FindTypesThatHaveDependencyOnAll(IEnumerable<TypeDefinition> input, IEnumerable<string> dependencies)
        {  
            return FindTypes(input, TypeDefinitionCheckingResult.SearchType.HaveDependencyOnAll, dependencies, true);         
        }

        /// <summary>
        /// Finds types that may have a dependency on any item in the given list of dependencies, but cannot have a dependency that is not in the list.
        /// </summary>
        /// <param name="input">The set of type definitions to search.</param>
        /// <param name="dependencies">The set of dependencies to look for.</param>
        /// <returns>A list of found types.</returns>
        public IReadOnlyDictionary<TypeDefinition, IEnumerable<string>> FindTypesThatOnlyHaveDependenciesOnAnyOrNone(IEnumerable<TypeDefinition> input, IEnumerable<string> dependencies)
        {           
            return FindTypes(input, TypeDefinitionCheckingResult.SearchType.OnlyHaveDependenciesOnAnyOrNone, dependencies, false);
        }

        /// <summary>
        /// Finds types that have a dependency on any item in the given list of dependencies, but cannot have a dependency that is not in the list.
        /// </summary>
        /// <param name="input">The set of type definitions to search.</param>
        /// <param name="dependencies">The set of dependencies to look for.</param>
        /// <returns>A list of found types.</returns>
        public IReadOnlyDictionary<TypeDefinition, IEnumerable<string>> FindTypesThatOnlyHaveDependenciesOnAny(IEnumerable<TypeDefinition> input, IEnumerable<string> dependencies)
        {
            return FindTypes(input, TypeDefinitionCheckingResult.SearchType.OnlyHaveDependenciesOnAny, dependencies, false);
        }

        /// <summary>
        /// Finds types that have a dependency on every item in the given list of dependencies, but cannot have a dependency that is not in the list.
        /// </summary>
        /// <param name="input">The set of type definitions to search.</param>
        /// <param name="dependencies">The set of dependencies to look for.</param>
        /// <returns>A list of found types.</returns>
        public IReadOnlyDictionary<TypeDefinition, IEnumerable<string>> FindTypesThatOnlyOnlyHaveDependenciesOnAll(IEnumerable<TypeDefinition> input, IEnumerable<string> dependencies)
        {
            return FindTypes(input, TypeDefinitionCheckingResult.SearchType.OnlyHaveDependenciesOnAll, dependencies, false);
        }

        private IReadOnlyDictionary<TypeDefinition, IEnumerable<string>> FindTypes(IEnumerable<TypeDefinition> input, TypeDefinitionCheckingResult.SearchType searchType, IEnumerable<string> dependencies, bool searchForDependencyInFieldConstant)
        {
            var output = new Dictionary<TypeDefinition, IEnumerable<string>>();
            var searchTree = new CachedNamespaceTree(dependencies);

            foreach (var type in input)
            {
                var context = new TypeDefinitionCheckingContext(type, searchType, searchTree, searchForDependencyInFieldConstant);

                if (context.IsTypeFound())
                {
                    output.Add(type, context.GetFoundDependencies());
                }
            }

            return output;
        }  
        
        private IReadOnlyDictionary<TypeDefinition, IEnumerable<string>> FindTypes(IEnumerable<TypeDefinition> input, TypeDefinitionCheckingResult.SearchType searchType, Regex regex, bool searchForDependencyInFieldConstant)
        {
            var output = new Dictionary<TypeDefinition, IEnumerable<string>>();
            var searchTree = new DynamicMatchSearchTree(regex);

            foreach (var type in input)
            {
                var context = new TypeDefinitionCheckingContext(type, searchType, searchTree, searchForDependencyInFieldConstant);

                if (context.IsTypeFound())
                {
                    output.Add(type, context.GetFoundDependencies());
                }
            }

            return output;
        }
    }
}