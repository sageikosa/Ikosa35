using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    public interface ICoreObject :
        ICore, IAdjunctable, ICorePhysical, IInteract, IInteractHandlerExtendable, ICoreIconic
    {
        Guid CreatorID { get; }
        string Name { get; }

        /// <summary>Intrinsic indication that the object is targetable</summary>
        bool IsTargetable { get; }

        /// <summary>ICoreObjects that make up this ICoreObject</summary>
        IEnumerable<ICoreObject> Connected { get; }

        /// <summary>Recursively enumerating all ICoreObjects that make up this ICoreObject</summary>
        IEnumerable<ICoreObject> AllConnected(HashSet<Guid> listed);

        IEnumerable<ICoreObject> Accessible(ICoreObject principal);

        /// <summary>Recursively enumerating all ICoreObjects that make up this ICoreObject, accessible by line of effect</summary>
        IEnumerable<ICoreObject> AllAccessible(HashSet<Guid> listed, ICoreObject principal);

        /// <summary>Recursively yield all adjuncts on all connected objects</summary>
        IEnumerable<EffectType> GetAllConnectedAdjuncts<EffectType>();

        /// <summary>Get information; either the default (base) non-hidden information, or specialized detectable information</summary>
        Info GetInfo(CoreActor observer, bool baseValues);

        /// <summary>Allows fetched information from an object to be merged with information from connected objects (or properties)</summary>
        Info MergeConnectedInfos(Info fetchedInfo, CoreActor observer);

        /// <summary>Name by which an actor knows the item</summary>
        string GetKnownName(CoreActor actor);
    }

    public static class CoreObjectHelper
    {
        public static void BindToSetting(this ICoreObject self)
        {
            var _setting = self.Setting;
            if (_setting != null)
            {
                foreach (var _bind in self.Adjuncts.OfType<IBindToSetting>().ToList())
                {
                    _bind.BindToSetting();
                }
                foreach (var _conn in self.Connected.ToList())
                {
                    _conn.BindToSetting();
                }
            }
        }

        public static void UnbindFromSetting(this ICoreObject self)
        {
            var _setting = self.Setting;
            if (_setting != null)
            {
                foreach (var _conn in self.Connected.ToList())
                {
                    _conn.UnbindFromSetting();
                }
                foreach (var _bind in self.Adjuncts.OfType<IBindToSetting>().ToList())
                {
                    _bind.UnbindFromSetting();
                }
            }
        }
    }
}
