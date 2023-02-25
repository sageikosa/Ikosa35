using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class FurnishingActionSet
    {
        public FurnishingActionSet(ICommand command, IEnumerable<IFurnishingAction> actions)
        {
            _Command = command;
            _Actions = actions.ToList();
        }

        private ICommand _Command;
        private List<IFurnishingAction> _Actions;

        public ICommand Command => _Command;

        public PivotActionModel Pivot
            => _Actions.OfType<PivotActionModel>().FirstOrDefault();

        public TiltActionModel Tilt
            => _Actions.OfType<TiltActionModel>().FirstOrDefault();

        public ReleaseActionModel Release
            => _Actions.OfType<ReleaseActionModel>().FirstOrDefault();

        public Visibility TiltVisibility
            => (_Actions?.OfType<TiltActionModel>()?.Any() ?? false) ? Visibility.Visible : Visibility.Hidden;

        public Visibility PivotVisibility
            => (_Actions?.OfType<PivotActionModel>()?.Any() ?? false) ? Visibility.Visible : Visibility.Hidden;

        public Visibility ReleaseVisibility
            => (_Actions?.OfType<ReleaseActionModel>()?.Any() ?? false) ? Visibility.Visible : Visibility.Hidden;

        public Visibility Visibility
            => (_Actions?.Count > 0) ? Visibility.Visible : Visibility.Collapsed;
    }
}
