using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Visualize
{
    /// <summary>stack of meta model resolution context</summary>
    public static class MetaModelResolutionStack
    {
        [ThreadStatic]
        private static Stack<MetaModelFragmentNode> _ResolveStack = new Stack<MetaModelFragmentNode>();
        [ThreadStatic]
        private static bool _HitTestable = false;
        [ThreadStatic]
        private static MetaModelState _State = null;

        private static Stack<MetaModelFragmentNode> ResolveStack
        {
            get
            {
                _ResolveStack ??= new Stack<MetaModelFragmentNode>();
                return _ResolveStack;
            }
        }

        public static void Clear(MetaModelState state)
        {
            ResolveStack.Clear();
            _State = state;
        }

        public static bool IsHitTestable { get { return _HitTestable; } set { _HitTestable = value; } }

        public static void Push(MetaModelFragmentNode node)
        {
            ResolveStack.Push(node);
        }

        public static bool Any()
        {
            return ResolveStack.Any();
        }

        public static MetaModelFragmentNode Peek()
        {
            return ResolveStack.Peek();
        }

        public static MetaModelFragmentNode Pop()
        {
            return ResolveStack.Pop();
        }

        public static MetaModelState State => _State;
    }
}
