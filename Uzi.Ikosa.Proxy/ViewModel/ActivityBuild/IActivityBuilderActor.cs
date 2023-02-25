using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Uzi.Ikosa.Contracts;
using Uzi.Visualize;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public interface IActivityBuilderActor
    {
        Guid ActorID { get; }
        ProxyModel Proxies { get; }
        void ClearActivityBuilding();
        Visibility PerformVisibility { get; }
        ObservableCollection<AwarenessInfo> Awarenesses { get; }
        IEnumerable<QueuedTargetItem> QueuedTargets { get; }
        IEnumerable<QueuedAwareness> QueuedAwarenesses { get; }
    }

    public interface IActivityBuilderTacticalActor : IActivityBuilderActor
    {
        AimPointActivation AimPointActivation { get; }
        LocaleViewModel LocaleViewModel { get; }
        IEnumerable<ICellLocation> QueuedLocations { get; }
    }
}
