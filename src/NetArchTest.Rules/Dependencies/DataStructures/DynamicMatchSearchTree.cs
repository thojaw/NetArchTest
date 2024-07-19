using Mono.Cecil;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NetArchTest.Rules.Dependencies.DataStructures
{
    internal class DynamicMatchSearchTree : ISearchTree
    {
        private readonly Regex regex;

        public DynamicMatchSearchTree(Regex regex)
        {
            this.regex = regex;
        }

        public int TerminatedNodesCount => 0;

        public IEnumerable<string> GetAllMatchingNames(TypeReference type)
        {
            return GetAllMatchingNames(type.FullName);
        }

        public IEnumerable<string> GetAllMatchingNames(string fullName)
        {
            if (this.regex.IsMatch(fullName))
            {
                yield return fullName;
            }
        }
    }
}
