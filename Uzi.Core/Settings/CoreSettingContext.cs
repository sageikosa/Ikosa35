using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Core
{
    /// <summary>
    /// Setting contexts hold all the information about the objects in a setting as an authoratative object graph.
    /// </summary>
    [Serializable]
    public abstract class CoreSettingContext
    {
        #region Construction
        public CoreSettingContext(string name, CoreSettingContextSet contextSet)
        {
            _Name = name;
            _ContextSet = contextSet;
            _Tokens = new List<CoreToken>();
        }
        #endregion

        #region data
        private CoreSettingContextSet _ContextSet;
        private string _Name;
        protected List<CoreToken> _Tokens;
        #endregion

        public CoreSettingContextSet ContextSet => _ContextSet;
        public string Name => _Name;

        #region public IEnumerable<SpecType> AllOf<SpecType>() where SpecType : class, ICore
        /// <summary>
        /// Iterates over all objects directly in all tokens, returning those that are of a specific type
        /// </summary>
        /// <typeparam name="SpecType">specifies only objects of (or derived from) a specific type are returned</typeparam>
        /// <returns></returns>
        public IEnumerable<SpecType> AllOf<SpecType>() where SpecType : class, ICore
        {
            return (from _on in _Tokens
                    from _obj in _on.ICoreAs<SpecType>()
                    where _on != null
                    select _obj);
        }
        #endregion

        #region public IEnumerable<SpecType> AllTokensOf<SpecType>() where SpecType : BaseToken
        /// <summary>
        /// Iterates over all tokens that are of a specific type
        /// </summary>
        /// <typeparam name="SpecType"></typeparam>
        /// <returns></returns>
        public IEnumerable<SpecType> AllTokensOf<SpecType>() where SpecType : CoreToken
            => from _on in _Tokens.OfType<SpecType>()
               select _on;
        #endregion

        public IEnumerable<CoreToken> AllTokens
            => from _on in _Tokens
               select _on;

        protected virtual void OnAdd(CoreToken newItem)
        {
            var _idx = ContextSet.GetCoreIndex();
            foreach (var _obj in newItem.ICoreAs<ICoreObject>())
            {
                _obj.BindToSetting();
                _idx.AddOrUpdate(_obj.ID, _obj, (_g, _o) => (_o));
            }
        }

        public void Add(CoreToken newItem)
        {
            _Tokens.Add(newItem);
            OnAdd(newItem);
        }

        protected virtual void OnRemove(CoreToken oldItem)
        {
            var _idx = ContextSet.GetCoreIndex();
            foreach (var _obj in oldItem.ICoreAs<ICoreObject>())
            {
                _obj.UnbindFromSetting();
                _idx.TryRemove(_obj.ID, out var result);
            }
        }

        public bool Remove(CoreToken oldItem)
        {
            if (_Tokens.Remove(oldItem))
            {
                OnRemove(oldItem);
                return true;
            }
            return false;
        }
    }
}
