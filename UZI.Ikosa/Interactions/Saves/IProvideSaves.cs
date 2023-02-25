using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Interactions
{
    public interface IProvideSaves : IInteract
    {
        BestSoftQualifiedDelta GetBestSoftSave(SavingThrowData saveData);
        bool AlwaysFailsSave { get; }
    }
}
