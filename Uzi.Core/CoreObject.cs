using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Specialized;
using System.Linq;
using Uzi.Core.Contracts;
using Newtonsoft.Json;

namespace Uzi.Core
{
    [Serializable]
    public abstract class CoreObject : ICoreObject
    {
        #region Constructor
        public CoreObject(string name)
        {
            ID = Guid.NewGuid();
            Name = name;
            _Adjuncts = new AdjunctSet(this);
            _PhysCtrl = new ChangeController<Physical>(this, new Physical(Physical.PhysicalType.Weight, 0d));
            _InteractionHandlers = [];
            InitInteractionHandlers();
        }
        #endregion

        #region state
        private string _Name;
        private Guid _Creator = Guid.Empty;
        private double _Weight = 0d;
        private double _Length = 0d;
        private double _Width = 0d;
        private double _Height = 0d;
        private double _IAngle = 0d;
        private double _IScale = 1d;
        private readonly AdjunctSet _Adjuncts;
        private readonly ChangeController<Physical> _PhysCtrl;
        protected Dictionary<Type, LinkedList<IInteractHandler>> _InteractionHandlers;
        #endregion

        public virtual string Name { get => _Name; set { _Name = value; DoPropertyChanged(nameof(Name)); } }
        public virtual Guid CreatorID { get => _Creator; set => _Creator = value; }

        /// <summary>Intrinsic indication that the object is targetable</summary>
        public abstract bool IsTargetable { get; }

        /// <summary>ICoreObjects that are directly connected to this ICoreObject</summary>
        public virtual IEnumerable<ICoreObject> Connected { get { yield break; } }

        #region public virtual IEnumerable<ICoreObject> AllConnected(Collection<Guid> listed)
        /// <summary>Recursively enumerating all ICoreObjects that make up this ICoreObject</summary>
        public IEnumerable<ICoreObject> AllConnected(HashSet<Guid> listed)
        {
            // this is for recursive loop detection, since no hierarchical integrity is enforced
            listed ??= [];

            // all direct objects
            foreach (var _obj in Connected)
            {
                if (!listed.Contains(_obj.ID))
                {
                    // the direct object itself
                    listed.Add(_obj.ID);
                    yield return _obj;

                    // and all the direct object's parts
                    foreach (var _sub in _obj.AllConnected(listed))
                    {
                        yield return _sub;
                    }
                }
            }
        }
        #endregion

        /// <summary>ICoreObjects that are directly accessible to this ICoreObject by the actor</summary>
        public virtual IEnumerable<ICoreObject> Accessible(ICoreObject principal)
            => Connected;

        #region public virtual IEnumerable<ICoreObject> AllAccessible(Collection<Guid> listed, ICoreObject principal)
        public IEnumerable<ICoreObject> AllAccessible(HashSet<Guid> listed, ICoreObject principal)
        {
            // this is for recursive loop detection, since no hierarchical integrity is enforced
            listed ??= [];

            // all direct objects
            foreach (var _obj in Accessible(principal))
            {
                if (!listed.Contains(_obj.ID))
                {
                    // the direct object itself
                    listed.Add(_obj.ID);
                    yield return _obj;

                    // and all the direct object's parts
                    foreach (var _sub in _obj.AllAccessible(listed, principal))
                    {
                        yield return _sub;
                    }
                }
            }
        }
        #endregion

        #region public IEnumerable<EffectType> GetAllConnectedAdjuncts<EffectType>()
        /// <summary>Gets all adjuncts on self and connected objects</summary>
        public IEnumerable<EffectType> GetAllConnectedAdjuncts<EffectType>()
        {
            // all effects on object
            foreach (var _effect in Adjuncts.OfType<EffectType>())
            {
                yield return _effect;
            }

            // all effects on connected objects
            foreach (var _effect in from _o in AllConnected(null)
                                    from _et in _o.GetAllConnectedAdjuncts<EffectType>()
                                    select _et)
            {
                yield return _effect;
            }

            yield break;
        }
        #endregion

        #region IAdjunctable Members
        // Handles anchoring, activating and deactivating adjuncts for this CoreObject instance
        public AdjunctSet Adjuncts => _Adjuncts;

        public virtual bool AddAdjunct(Adjunct adjunct)
        {
            var _add = new Interaction(null, adjunct.Source, this, new AddAdjunctData(null, adjunct), true);
            HandleInteraction(_add);
            var _boolBack = _add.Feedback.OfType<ValueFeedback<bool>>().FirstOrDefault();
            if (_boolBack != null)
            {
                return _boolBack.Value;
            }

            return false;
        }

        /// <summary>Remove any non-protected adjunct</summary>
        public virtual bool RemoveAdjunct(Adjunct adjunct)
        {
            // if adjunct is protected, quit
            if (adjunct.IsProtected)
            {
                return false;
            }

            var _rmv = new RemoveAdjunctData(null, adjunct);
            var _interact = new Interaction(null, adjunct.Source, this, _rmv, true);
            HandleInteraction(_interact);
            return _rmv.DidRemove(_interact);
        }
        #endregion

        #region IControlChange<Physical> Members
        public void AddChangeMonitor(IMonitorChange<Physical> monitor)
        {
            _PhysCtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<Physical> monitor)
        {
            _PhysCtrl.RemoveChangeMonitor(monitor);
        }
        #endregion

        #region WPF notifications
        /// <summary>
        /// Allow the PropertyChanged event to be invoked from derived classes
        /// </summary>
        /// <param name="propName"></param>
        protected void DoPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propName));

        [field: NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region protected virtual void InitInteractionHandlers()
        protected virtual void InitInteractionHandlers()
        {
        }
        #endregion

        #region public void AddIInteractHandler(IInteractHandler handler)
        public void AddIInteractHandler(IInteractHandler handler)
        {
            // find keyed collections to which the handler is added
            foreach (var _type in handler.GetInteractionTypes())
            {
                // does collection exist?
                LinkedList<IInteractHandler> _list = null;
                if (!_InteractionHandlers.ContainsKey(_type))
                {
                    // no, so make the collection
                    _list = new LinkedList<IInteractHandler>();
                    _InteractionHandlers.Add(_type, _list);
                }
                else
                {
                    // otherwise get the collection
                    _list = _InteractionHandlers[_type];
                }

                // and add the handler
                LinkedListNode<IInteractHandler> _found = null;
                foreach (var _existing in _list)
                {
                    if (handler.LinkBefore(_type, _existing))
                    {
                        _found = _list.Find(_existing);
                        break;
                    }
                }
                if (_found != null)
                {
                    _list.AddBefore(_found, handler);
                }
                else
                {
                    _list.AddLast(handler);
                }
            }
        }
        #endregion

        #region public void RemoveIInteractHandler(IInteractHandler handler)
        public void RemoveIInteractHandler(IInteractHandler handler)
        {
            var _toRemove = new Collection<Type>();
            foreach (var _type in handler.GetInteractionTypes()
                .Where(_it => _InteractionHandlers.ContainsKey(_it)))
            {
                // find collection object claims to be in
                var _list = _InteractionHandlers[_type];
                if (_list != null)
                {
                    // try to remove it
                    if (_list.Remove(handler))
                    {
                        // if collection is empty, mark for deletion
                        if (_list.Count == 0)
                        {
                            _toRemove.Add(_type);
                        }
                    }
                }
            }

            // remove empty collections
            foreach (var _type in _toRemove)
            {
                _InteractionHandlers.Remove(_type);
            }
        }
        #endregion

        public IEnumerable<IInteractHandler> InteractionHandlers
            => _InteractionHandlers.SelectMany(_kvp => _kvp.Value);

        #region IInteract Members
        protected virtual void PostProcess(Stack<IProcessFeedback> postProcessors, Interaction interact)
        {
            while (postProcessors.Count > 0)
            {
                postProcessors.Pop().ProcessFeedback(interact);
            }
            return;
        }

        protected bool DoHandleInteraction(Interaction interact, IInteractHandler handler, Stack<IProcessFeedback> postHandlers)
        {
            if (handler == null)
            {
                return false;
            }

            // see if the handler will do post-processing
            if (handler is IProcessFeedback _feedHandler)
            {
                postHandlers.Push(_feedHandler);
            }

            handler.HandleInteraction(interact);

            // got feedback, so stop forward process, and try post-processing
            if (interact.Feedback.Count != 0)
            {
                PostProcess(postHandlers, interact);
                return true;
            }
            return false;
        }

        /// <summary>Provide a handler after chained handlers, but before interaction default handler</summary>
        protected virtual IInteractHandler PostChainHandler(Interaction interact)
        {
            return null;
        }

        public virtual void HandleInteraction(Interaction interact)
        {
            var _iType = interact.InteractData.GetType();
            var _postHandlers = new Stack<IProcessFeedback>();

            // chained handlers
            if (_InteractionHandlers.TryGetValue(_iType, out var _chain))
            {
                // try each handler in the chain
                foreach (var _handler in _chain)
                {
                    if (DoHandleInteraction(interact, _handler, _postHandlers))
                    {
                        return;
                    }
                }
            }

            // indirect chained handlers
            var _pType = interact.InteractData.ProcessType;
            if (_pType != _iType)
            {
                if (_InteractionHandlers.TryGetValue(_pType, out _chain))
                {
                    // try each handler in the chain
                    foreach (var _handler in _chain)
                    {
                        if (DoHandleInteraction(interact, _handler, _postHandlers))
                        {
                            return;
                        }
                    }
                }
            }

            if (DoHandleInteraction(interact, PostChainHandler(interact), _postHandlers))
            {
                return;
            }

            // interaction default handlers
            if (!interact.Feedback.Any())
            {
                foreach (var _h in interact.DefaultHandlers)
                {
                    if (DoHandleInteraction(interact, _h, _postHandlers))
                    {
                        return;
                    }
                }
            }

            PostProcess(_postHandlers, interact);
        }
        #endregion

        #region non-package File IO scaffolding
        public static CoreObject ReadFile(string fileName)
        {
            var _in = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            var _fmt = new BinaryFormatter();
            var _obj = _fmt.Deserialize(_in);
            _in.Close();
            return (CoreObject)_obj;
        }

        public void WriteFile(string fileName)
        {
            var _out = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            var _fmt = new BinaryFormatter();
            _fmt.Serialize(_out, this);
            _out.Close();
        }
        #endregion

        public Guid ID { get; set; }

        public abstract CoreSetting Setting { get; }

        public CoreProcessManager ProcessManager
            => Setting?.ContextSet.ProcessManager;

        /// <summary>Get information; either the default (base) non-hidden information, or specialized detected information</summary>
        public abstract Info GetInfo(CoreActor actor, bool baseValues);

        /// <summary>Allows fetched information from an object to be merged with information from connected objects (or properties)</summary>
        public abstract Info MergeConnectedInfos(Info fetchedInfo, CoreActor actor);

        public virtual string GetKnownName(CoreActor actor)
            => GetInfoData.GetInfoFeedback(this, actor)?.Message
            ?? Name;

        #region public double Weight { get; set; }
        /// <summary>performs change notification on weight</summary>
        protected void CoreSetWeight(double value)
        {
            if (_Weight != value)
            {
                var _val = new Physical(Physical.PhysicalType.Weight, value);
                _PhysCtrl.DoPreValueChanged(_val);
                _Weight = value;
                _PhysCtrl.DoValueChanged(_val);
                DoPropertyChanged(@"Weight");
            }
        }

        public virtual double Weight { get { return _Weight; } set { CoreSetWeight(value); } }
        #endregion

        #region public double Length { get; set; }
        /// <summary>performs change notification on Length</summary>
        protected virtual void CoreSetLength(double value)
        {
            if (_Length != value)
            {
                var _val = new Physical(Physical.PhysicalType.Length, value);
                _PhysCtrl.DoPreValueChanged(_val);
                _Length = value;
                _PhysCtrl.DoValueChanged(_val);
                DoPropertyChanged(@"Length");
            }
        }

        public virtual double Length { get { return _Length; } set { CoreSetLength(value); } }
        #endregion

        #region public double Width { get; set; }
        /// <summary>performs change notification on Width</summary>
        protected virtual void CoreSetWidth(double value)
        {
            if (_Width != value)
            {
                var _val = new Physical(Physical.PhysicalType.Width, value);
                _PhysCtrl.DoPreValueChanged(_val);
                _Width = value;
                _PhysCtrl.DoValueChanged(_val);
                DoPropertyChanged(@"Width");
            }
        }

        public virtual double Width { get { return _Width; } set { CoreSetWidth(value); } }
        #endregion

        #region public double Height { get; set; }
        /// <summary>performs change notification on Height</summary>
        protected virtual void CoreSetHeight(double value)
        {
            if (_Height != value)
            {
                var _val = new Physical(Physical.PhysicalType.Height, value);
                _PhysCtrl.DoPreValueChanged(_val);
                _Height = value;
                _PhysCtrl.DoValueChanged(_val);
                DoPropertyChanged(@"Height");
            }
        }

        public virtual double Height { get { return _Height; } set { CoreSetHeight(value); } }
        #endregion

        // ICoreIconic

        protected abstract string ClassIconKey { get; }

        public virtual IEnumerable<string> IconKeys
        {
            get
            {
                // provide any overrides
                foreach (var _iKey in IconKeyAdjunct.GetIconKeys(this))
                {
                    yield return _iKey;
                }

                // ... and then the class key
                yield return ClassIconKey;
                yield break;
            }
        }

        public virtual IDictionary<string, string> IconColorMap
            => Adjuncts.OfType<ColorMapAdjunct>().FirstOrDefault()?.ColorMap;

        public virtual IEnumerable<string> PresentationKeys
            => IconKeys;

        public double IconAngle
        {
            get => _IAngle;
            set
            {
                _IAngle = value;
                DoPropertyChanged(nameof(IconAngle));
            }
        }

        public double IconScale
        {
            get
            {
                if (_IScale == 0)
                {
                    _IScale = 1;
                }

                return _IScale;
            }
            set
            {
                _IScale = value;
                DoPropertyChanged(nameof(IconScale));
            }
        }
    }
}
