using Uzi.Core;

namespace Uzi.Ikosa.Tactical
{
    public interface IAlterLocalLink : ICoreObject
    {
        /// <summary>Generally this will only be called if the alterer is within the link's cubic.</summary>
        bool CanAlterLink(LocalLink link);

        /// <summary>Return 0 for no light allowed.  Return 1 for unaltered.</summary>
        double AllowLightFactor(LocalLink link);

        /// <summary>Return 0 for no additional difficulty.  Usually +5 for a door.</summary>
        int GetExtraSoundDifficulty(LocalLink link);
    }
}
