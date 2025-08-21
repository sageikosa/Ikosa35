using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic
{
    public static class IMagicPowerDefInfoHelper
    {
        #region ToPowerDefInfo
        private static PDInfo ToPDInfo<PDInfo>(this IPowerDef self)
            where PDInfo : PowerDefInfo, new()
        {
            return new PDInfo
            {
                Message = self.DisplayName,
                Description = self.Description,
                Descriptors = self.Descriptors.Select(_d => _d.Name).ToArray(),
                Key = self.Key
            };
        }

        public static PowerDefInfo GetPowerDefInfo(this IPowerDef self)
        {
            return self.ToPDInfo<PowerDefInfo>();
        }
        #endregion

        #region ToMagicPowerDefInfo
        private static MPDInfo ToMPDInfo<MPDInfo>(this IMagicPowerDef self)
            where MPDInfo : MagicPowerDefInfo, new()
        {
            var _info = self.ToPDInfo<MPDInfo>();
            _info.MagicStyle = self.MagicStyle.StyleName;
            return _info;
        }

        /// <summary>Returns a SpellDefInfo if the IMagicPowerDef is an ISpellDef</summary>
        public static MagicPowerDefInfo GetMagicPowerDefInfo(this IMagicPowerDef self)
            => (self as ISpellDef)?.GetSpellDefInfo() 
            ?? self.ToMPDInfo<MagicPowerDefInfo>();
        #endregion

        #region private static List<MetaMagicInfo> GetMetaMagicList(this ISpellDef self, List<MetaMagicInfo> appendTo = null)
        private static List<MetaMagicInfo> GetMetaMagicList(this ISpellDef self, List<MetaMagicInfo> appendTo = null)
        {
            // ensure appentTo is initialized
            appendTo = appendTo ?? [];

            if (self is MetaMagicSpellDef _meta)
            {
                // self
                appendTo.Add(new MetaMagicInfo
                {
                    MetaTag = _meta.MetaTag,
                    SlotAdjustment = _meta.SlotAdjustment,
                    Message = _meta.Benefit,
                    PresenterID = _meta.PresenterID
                });

                // and any more
                _meta.Wrapped?.GetMetaMagicList(appendTo);
            }
            return appendTo;
        }
        #endregion

        #region ToSpellDefInfo
        public static SpellDefInfo GetSpellDefInfo(this ISpellDef self)
        {
            var _info = self.ToMPDInfo<SpellDefInfo>();
            _info.ArcaneCharisma = self.ArcaneCharisma;
            _info.MetaMagics = self.GetMetaMagicList();
            return _info;
        }
        #endregion

        /// <summary>
        /// Spell Def key and Meta-Magics must mach
        /// </summary>
        /// <param name="self"></param>
        /// <param name="spellDef"></param>
        /// <returns></returns>
        public static bool DoSpellDefsMatch(this ISpellDef self, SpellDefInfo spellDef)
        {
            if (self.Key == spellDef?.Key)
            {
                // all meta magics match in both direction
                var _meta = self.GetMetaMagicList();
                return _meta.All(_mm => spellDef.MetaMagics.Any(_smm => _smm.MetaTag == _mm.MetaTag))
                    && spellDef.MetaMagics.All(_smm => _meta.Any(_mm => _mm.MetaTag == _smm.MetaTag));
            }
            return false;
        }
    }
}
