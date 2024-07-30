using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;

namespace NetArchTest.Rules
{
    internal sealed class TypeDefinitionResult(
        IEnumerable<TypeDefinition> source,
        IReadOnlyDictionary<TypeDefinition,
        IEnumerable<string>> dependencyInfo = null) : IEnumerable<TypeDefinition>
    {
        public IReadOnlyDictionary<TypeDefinition, IEnumerable<string>> DependencyInfo => dependencyInfo;

        public IEnumerator<TypeDefinition> GetEnumerator() => source.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)source).GetEnumerator();
    }
}
