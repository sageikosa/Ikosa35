using System;
namespace Uzi.Ikosa.Senses
{
    /// <summary>Implemented on AwarenessSet</summary>
    public interface IAwarenessLevels
    {
        AwarenessLevel GetAwarenessLevel(Guid guid);
        bool ShouldDraw(Guid guid);
    }
}
