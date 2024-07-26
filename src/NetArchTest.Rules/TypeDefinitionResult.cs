using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;

namespace NetArchTest.Rules
{
    internal class TypeDefinitionResult : IEnumerable<TypeDefinition>
    {
        private readonly IEnumerable<TypeDefinition> _source;

        public TypeDefinitionResult(IEnumerable<TypeDefinition> source)
        {
            _source = source;
        }

        public IEnumerator<TypeDefinition> GetEnumerator() => _source.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_source).GetEnumerator();
    }

    internal class ExtendedTypeDefinitionResult : TypeDefinitionResult
    {
        private readonly IReadOnlyDictionary<TypeDefinition, IEnumerable<string>> _source;

        public ExtendedTypeDefinitionResult(IReadOnlyDictionary<TypeDefinition, IEnumerable<string>> source) : base(source.Keys)
        {
            _source = source;
        }
    }
}
