using System;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Senses
{
    /// <summary>
    /// Represents some extra information (not spatially bound), effectively an IInformable
    /// </summary>
    [Serializable]
    public class ExtraInfo : ISourcedObject
    {
        #region ctor()
        /// <summary>
        /// Represents some extra information, effectively an IInformable
        /// </summary>
        public ExtraInfo(ExtraInfoSource source, IInformable info, IActionProvider provider)
        {
            _Source = source;
            _Info = info;
            _Provider = provider;
        }
        #endregion

        #region state
        private IInformable _Info;
        private ExtraInfoSource _Source;
        private IActionProvider _Provider;
        #endregion

        /// <summary>Set of informations</summary>
        public IInformable Informations => _Info;

        public object Source => _Source;
        public ExtraInfoSource InfoSource => _Source;
        public IActionProvider ActionProvider => _Provider;

        protected EIInfo ToInfo<EIInfo>(CoreActor actor)
            where EIInfo : ExtraInfoInfo, new()
            => new EIInfo
            {
                SourceID = InfoSource.ID,
                SourceTitle = InfoSource.Message,
                Informations = Informations.Inform(actor).ToArray(),
                ProviderID = ActionProvider?.ID
            };

        public ExtraInfoInfo ToExtraInfoInfo(CoreActor actor)
            => ToInfo<ExtraInfoInfo>(actor);
    }
}
