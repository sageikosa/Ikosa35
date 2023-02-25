using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    /// <summary>Used to hold adjuncts that may target a point in space</summary>
    public class HolderObject : CoreObject, IVisible
    {
        /// <summary>Used to hold adjuncts that may target a point in space</summary>
        public HolderObject()
            : base(string.Empty)
        {
        }

        #region IVisible Members
        public bool IsVisible { get { return false; } }
        #endregion

        public override CoreSetting Setting
            => this.GetTokened()?.Token.Context.ContextSet.Setting;

        public override Info GetInfo(CoreActor actor, bool baseValues)
            => null;

        public override Info MergeConnectedInfos(Info fetchedInfo, CoreActor actor)
            => fetchedInfo;

        protected override string ClassIconKey
            => string.Empty;

        public override bool IsTargetable => false;
    }
}
