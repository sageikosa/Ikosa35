using System;
using Uzi.Ikosa.Feats;

namespace Uzi.Ikosa
{
    public interface IResolveFeat
    {
        FeatBase GetFeat(Type selectedType, object source);
        int PowerLevel { get; }
    }
}
