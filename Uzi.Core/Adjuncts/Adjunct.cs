using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Uzi.Core
{
    /// <summary>
    /// Dynamic add-ons to an IAdjunctable
    /// </summary>
    [Serializable]
    public abstract class Adjunct : IActivatable, ISourcedObject, ICloneable, IEquatable<Adjunct>
    {
        #region Construction
        /// <summary>Starts off as inactive without being anchored</summary>
        protected Adjunct(object source)
        {
            _Src = source;
            _Active = false;
            _Anchor = null;
            _ActCtrlr = new ChangeController<Activation>(this, new Activation(this, _Active));
        }
        #endregion

        #region data
        private object _Src;
        private IAdjunctable _Anchor;
        private bool _InitActive = true;
        private Guid _ID = Guid.NewGuid();
        private bool _Active;
        private ChangeController<Activation> _ActCtrlr;
        #endregion

        /// <summary>Source of the Adjunct</summary>
        public virtual object Source => _Src;

        /// <summary>Protected adjuncts cannot be removed through RemoveAdjunct.</summary>
        public virtual bool IsProtected
            => false;

        public Guid ID => _ID;
        public virtual Guid PresenterID => _ID;

        public virtual Guid? MergeID => _ID;

        #region public virtual bool Eject()
        /// <summary>Eject an adjunct from its anchor</summary>
        public virtual bool Eject()
        {
            if (_Anchor != null)
            {
                if (_Anchor is IInteract)
                {
                    // run through an interaction
                    var _target = _Anchor as IInteract;
                    var _rmv = new RemoveAdjunctData(null, this);
                    var _interact = new Interaction(null, Source, _target, _rmv, true);
                    _target.HandleInteraction(_interact);
                    return _rmv.DidRemove(_interact);
                }
                else
                {
                    // always succeeds?
                    return _Anchor.Adjuncts.Remove(this);
                }
            }

            // wasn't anchored, so not anchored
            return true;
        }
        #endregion

        /// <summary>By default any adjunct is initially active when added</summary>
        public bool InitialActive { get => _InitActive; set => _InitActive = value; }

        #region public virtual IAdjunctable Anchor { get; internal set; }
        /// <summary>Object to which the adjunct is anchored</summary>
        public virtual IAdjunctable Anchor
        {
            get { return _Anchor; }
            internal set
            {
                var _oldAnchor = _Anchor;
                var _oldSetting = _oldAnchor?.Setting;
                if (value != null)
                {
                    if (CanAnchor(value))
                    {
                        _Anchor = value;
                        OnAnchorSet(_oldAnchor, _oldSetting);
                    }
                }
                else
                {
                    if (CanUnAnchor())
                    {
                        _Anchor = value;
                        OnAnchorSet(_oldAnchor, _oldSetting);
                    }
                }
            }
        }
        #endregion

        /// <summary>Override to conditionally test whether the adjunct should anchor to an adjunct holder</summary>
        public virtual bool CanAnchor(IAdjunctable newAnchor)
            => true;

        /// <summary>Override to conditionally test whether the adjunct can be unanchored</summary>
        public virtual bool CanUnAnchor()
            => true;

        /// <summary>Called after the Anchor property changes</summary>
        protected virtual void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting) { }

        /// <summary>Setting this property indicates the adjunct itself sources the change, such as when being added or removed</summary>
        public bool IsActive { get => _Active; set => Activation = new Activation(this, value); }

        /// <summary>Set IsActive to true.  Usable with null accessor.</summary>
        public void ActivateAdjunct() => IsActive = true;

        /// <summary>Set IsActive to false.  Usable with null accessor.</summary>
        public void DeActivateAdjunct() => IsActive = false;

        #region public Activation Activation { get; set; }
        /// <summary>Setting this property allows sources other than the adjunct itself to source the change; such as temporary suppressions</summary>
        public Activation Activation
        {
            get => _ActCtrlr.LastValue;
            set
            {
                // if desired state different, or a different source
                if ((_ActCtrlr.LastValue.IsActive != value.IsActive)
                    || (_ActCtrlr.LastValue.Source != value.Source))
                {
                    // prevents recursive OnActivate/OnDeactivate
                    if (_Active != value.IsActive)
                    {
                        if (_ActCtrlr.WillAbortChange(value))
                        {
                            return;
                        }

                        if (value.IsActive && !OnPreActivate(value.Source))
                        {
                            return;
                        }
                        else if (!value.IsActive && !OnPreDeactivate(value.Source))
                        {
                            return;
                        }

                        // do pre-value changed
                        _ActCtrlr.DoPreValueChanged(value);
                        _Active = value.IsActive;
                        if (_Active)
                        {
                            OnActivate(value.Source);
                        }
                        else
                        {
                            OnDeactivate(value.Source);
                        }

                        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(@"IsActive"));
                    }

                    // do value changed (either passed validations, or source changed)
                    _ActCtrlr.DoValueChanged(value);
                    PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(@"Activation"));
                }
            }
        }
        #endregion

        public virtual IEnumerable<IActivatable> Dependents
        {
            get
            {
                yield break;
            }
        }

        /// <summary>True if no anchor set, the source is not an adjunct, or the source adjunct isn't anchored to this adjunct's anchor</summary>
        public virtual bool IsCloneRoot
            => (Anchor == null) || !((Source as Adjunct)?.Anchor == Anchor);

        /// <summary>return false to veto the activation</summary>
        protected virtual bool OnPreActivate(object source)
            => true;

        /// <summary>return false to veto the deactivation</summary>
        protected virtual bool OnPreDeactivate(object source)
            => true;

        protected virtual void OnActivate(object source) { }
        protected virtual void OnDeactivate(object source) { }

        #region IControlChange<Activation> Members
        public void AddChangeMonitor(IMonitorChange<Activation> monitor)
        {
            _ActCtrlr.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<Activation> monitor)
        {
            _ActCtrlr.RemoveChangeMonitor(monitor);
        }
        #endregion

        protected void DoPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propName));

        [field: NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public abstract object Clone();

        /// <summary>Default: MergeIDs Equal or either source is null</summary>
        public virtual bool Equals(Adjunct other)
            => MergeID == other.MergeID;
    }
}
