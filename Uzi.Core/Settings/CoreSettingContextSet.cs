using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Uzi.Core
{
    /// <summary>
    /// A setting can have multiple contexts, indicating separate sets of tokens
    /// </summary>
    [Serializable]
    public class CoreSettingContextSet : IEnumerable<CoreSettingContext>
    {
        #region ctor()
        public CoreSettingContextSet(CoreSetting setting)
        {
            _Setting = setting;
            _ProcessManager = _Setting.GenerateProcessManager();
            _Contexts = [];
            _AdjunctGroups = new AdjunctGroupSet();
            ProcessManager.ContextReactors.Reactors.Add(_AdjunctGroups, _AdjunctGroups);
            _Reactors = new ReactorSet();
            _CoreIdx = new ConcurrentDictionary<Guid, ICore>();
        }
        #endregion

        #region data
        private readonly ReactorSet _Reactors;
        private readonly List<CoreSettingContext> _Contexts;
        private readonly AdjunctGroupSet _AdjunctGroups;
        private readonly CoreProcessManager _ProcessManager;
        private CoreSetting _Setting;

        [NonSerialized]
        protected ConcurrentDictionary<Guid, ICore> _CoreIdx;
        #endregion

        public CoreSetting Setting { get => _Setting; set => _Setting = value; }
        public CoreProcessManager ProcessManager => _ProcessManager;

        /// <summary>Adjunct groups at context set level indicate adjunct groups can be used across contexts</summary>
        public AdjunctGroupSet AdjunctGroups => _AdjunctGroups;
        public ReactorSet SettingReactors => _Reactors;

        public void RebuildCoreIndex()
        {
            _CoreIdx = null;
            GetCoreIndex();
        }

        #region public ConcurrentDictionary<Guid, (ICore iCore, CoreToken token)> GetCoreIndex()
        public ConcurrentDictionary<Guid, ICore> GetCoreIndex()
        {
            if (_CoreIdx == null)
            {
                // icore and token index rebuild
                var _idx = new ConcurrentDictionary<Guid, ICore>();
                foreach (var _tok in _Contexts.SelectMany(_c => _c.AllTokens))
                {
                    foreach (var _iCore in _tok.AllConnected())
                    {
                        _idx[_iCore.ID] = _iCore;
                    }
                }

                // current
                _CoreIdx = _idx;
            }
            return _CoreIdx;
        }
        #endregion

        public ICore GetICore(Guid id)
        {
            var _idx = GetCoreIndex();
            if (_idx.TryGetValue(id, out var result))
            {
                return result;
            }

            return null;
        }

        public void PathChanged(IAdjunctable coreObject)
        {
            var _idx = GetCoreIndex();
            if (coreObject.Setting != Setting)
            {
                _idx.TryRemove(coreObject.ID, out var iCore);
            }
            else
            {
                _idx.AddOrUpdate(coreObject.ID, coreObject, (_id, _obj) => _obj);
            }
        }

        #region public void RebindAllAdjuncts()
        /// <summary>
        /// Traverses all objects in all contexts, notifies of setting and 
        /// fetches any group member adjuncts to reconnect them if needed.
        /// Removes empty groups.
        /// </summary>
        public void RebindAllAdjuncts()
        {
            if (!ProcessManager.ContextReactors.Reactors.ContainsKey(_AdjunctGroups))
            {
                ProcessManager.ContextReactors.Reactors.Add(_AdjunctGroups, _AdjunctGroups);
            }

            // rebind adjunct groups
            foreach (var _tokObj in from _ctx in _Contexts
                                    from _co in _ctx.AllOf<CoreObject>()
                                    select _co)
            {
                _tokObj.CoreSettingNotify();
                foreach (var _adj in RebindAdjunctGroups(_tokObj).ToList())
                {
                    // drop any unbindable adjuncts
                    _adj.Eject();
                }
                foreach (var _innerObj in _tokObj.AllConnected(null))
                {
                    _innerObj.CoreSettingNotify();
                    foreach (var _adj in RebindAdjunctGroups(_innerObj).ToList())
                    {
                        // drop any unbindable adjuncts
                        _adj.Eject();
                    }
                }
            }

            // cleanup empty groups
            var _clean = AdjunctGroups.All().Where(_a => !_a.Members.Any()).ToArray();
            foreach (var _grp in _clean)
            {
                AdjunctGroups.Remove(_grp);
            }
        }
        #endregion

        #region private IEnumerable<Adjunct> RebindAdjunctGroups(CoreObject coreObj)
        /// <summary>Rebinds all group adjuncts in a core object.  Yields unbindable adjuncts.</summary>
        private IEnumerable<Adjunct> RebindAdjunctGroups(ICoreObject coreObj)
        {
            // get any unbound group member adjuncts
            foreach (var _member in from _memb in coreObj.Adjuncts.OfType<GroupMemberAdjunct>()
                                    where (_memb.Group == null)
                                    select _memb)
            {
                // find the group needed to bind
                var _group = AdjunctGroups[_member.GroupID];
                if (_group != null)
                {
                    // found the group
                    _member.Group = _group;
                    if (!_group.Members.Contains(_member))
                    {
                        // group didn't have the member listed
                        _group.AddMember(_member);
                    }
                }
                else
                {
                    // couldn't find it
                    yield return _member;
                }
            }
            yield break;
        }
        #endregion

        #region Collection-Like < CoreSettingContext >
        public void Add(CoreSettingContext context) => _Contexts.Add(context);
        public void Remove(CoreSettingContext context) => _Contexts.Remove(context);
        public int IndexOf(CoreSettingContext item) => _Contexts.IndexOf(item);
        public void RemoveAt(int index) => _Contexts.RemoveAt(index);
        public CoreSettingContext this[int index] => _Contexts[index];
        public bool Contains(CoreSettingContext item) => _Contexts.Contains(item);
        public int Count => _Contexts.Count;
        public IEnumerable<CoreSettingContext> All() => _Contexts.AsEnumerable();
        #endregion

        public IEnumerable<CoreToken> FindAll(ICore core)
            => from _ctx in _Contexts
               from _tok in _ctx.AllTokens
               from _obj in _tok.AllConnected()
               where _obj.ID == core.ID
               select _tok;

        #region IEnumerable<CoreSettingContext> Members

        public IEnumerator<CoreSettingContext> GetEnumerator()
        {
            foreach (var _ctx in _Contexts)
            {
                yield return _ctx;
            }

            yield break;
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (var _ctx in _Contexts)
            {
                yield return _ctx;
            }

            yield break;
        }

        #endregion
    }
}
