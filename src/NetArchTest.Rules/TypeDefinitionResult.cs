using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;

namespace NetArchTest.Rules
{
    internal sealed class TypeDefinitionResult : IEnumerable<TypeDefinition>
    {
        private readonly IEnumerable<TypeDefinition> _source;
        private readonly IReadOnlyDictionary<TypeDefinition, IEnumerable<string>> _dependencyInfo;

        public TypeDefinitionResult(IEnumerable<TypeDefinition> source, IReadOnlyDictionary<TypeDefinition, IEnumerable<string>> dependencyInfo = null)
        {
            _source = source;
            _dependencyInfo = dependencyInfo;
        }

        public IReadOnlyDictionary<TypeDefinition, IEnumerable<string>> DependencyInfo => _dependencyInfo;

        public IEnumerator<TypeDefinition> GetEnumerator() => _source.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_source).GetEnumerator();
    }
}
