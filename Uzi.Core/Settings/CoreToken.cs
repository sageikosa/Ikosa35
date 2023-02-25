using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Core
{
    /// <summary>
    /// An object token is a set of objects that are grouped together in a setting.
    /// An object can belong to one token per setting, and multiple settings per campaign.
    /// </summary>
    [Serializable]
    public abstract class CoreToken
    {
        #region Construction
        protected CoreToken(ICore iCore, CoreSettingContext context)
        {
            _ICore = iCore;
            _Context = context;
            if (iCore is ICoreObject)
            {
                (iCore as ICoreObject).BindToSetting();
            }
        }
        #endregion

        #region private data
        private readonly ICore _ICore;
        private readonly CoreSettingContext _Context;
        private readonly string _Name = string.Empty;
        #endregion

        #region public IEnumerable<ICore> AllConnected()
        /// <summary>Yields ICore and ICoreConnected ICores</summary>
        public IEnumerable<ICore> AllConnected()
        {
            yield return _ICore;
            if (_ICore is ICoreObject)
            {
                var _iCoreObj = _ICore as ICoreObject;
                foreach (var _obj in from _o in _iCoreObj.AllConnected(null)
                                     select _o)
                    yield return _obj;
            }
            yield break;
        }
        #endregion

        #region public IEnumerable<ICore> AllAccessible(CoreActor actor)
        /// <summary>Yields ICore, and accessible ICores</summary>
        public IEnumerable<ICore> AllAccessible(CoreActor actor)
        {
            yield return _ICore;
            if (_ICore is ICoreObject)
            {
                var _iCoreObj = _ICore as ICoreObject;
                foreach (var _obj in from _o in _iCoreObj.AllAccessible(null, actor)
                                     select _o)
                    yield return _obj;
            }
            yield break;
        }
        #endregion

        /// <summary>All connected of specified type</summary>
        public IEnumerable<SpecType> AllConnectedOf<SpecType>() where SpecType : ICore
            => AllConnected().OfType<SpecType>();

        public ICore ICore => _ICore;
        public CoreSettingContext Context => _Context;

        /// <summary>Yields the item cast as the type, or yields nothing</summary>
        public IEnumerable<IType> ICoreAs<IType>() where IType : ICore
        {
            if (_ICore is IType)
                yield return (IType)_ICore;
            yield break;
        }

        public CoreActor Chief
            => _ICore as CoreActor;

        public virtual string Name => (_ICore as ICoreObject)?.Name ?? _Name ?? GetType().Name;

        protected void DoPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propName));

        [field:NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
