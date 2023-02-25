using Uzi.Core.Contracts;
using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions.Steps
{
    /// <summary>
    /// Performs any use magic device or caster level checks prior to completing the spell
    /// </summary>
    [Serializable]
    public class SpellCompletionStep : CoreStep
    {
        #region Construction
        public SpellCompletionStep(CoreActivity activity)
            : base(activity)
        {
            _Dispensing = true;
        }
        #endregion

        #region Private Data
        private bool _Dispensing;
        #endregion

        public CoreActivity Activity
            => Process as CoreActivity;

        public CompleteSpell CompleteSpell
            => Activity.Action as CompleteSpell;

        public SuccessCheckPrerequisite UseScrollCheck
            => AllPrerequisites<SuccessCheckPrerequisite>(@"Skill.UseMagicDevice.Scroll").FirstOrDefault();

        public SuccessCheckPrerequisite EmulateCheck
            => AllPrerequisites<SuccessCheckPrerequisite>(@"Skill.UseMagicDevice.Emulate").FirstOrDefault();

        public SuccessCheckPrerequisite CasterCheck
            => AllPrerequisites<SuccessCheckPrerequisite>(@"CasterLevel.Check").FirstOrDefault();

        public SuccessCheckPrerequisite WisdomCheck
            => AllPrerequisites<SuccessCheckPrerequisite>(@"Ability.Wisdom").FirstOrDefault();

        public override bool IsDispensingPrerequisites => _Dispensing;

        #region public override StepPrerequisite NextPrerequisite()
        /// <summary>Determine whether Use Magic Device checks or Caster Level checks are needed</summary>
        protected override StepPrerequisite OnNextPrerequisite()
        {
            if (CompleteSpell == null)
                return null;
            if (!_Dispensing)
                return null;

            var _complete = CompleteSpell.SpellCompletion;
            var _critter = Activity.Actor as Creature;
            var _iAct = new Interaction(Activity.Actor, _complete.SpellSource, _critter, null);

            if (!_complete.UseDirectly(_critter))
            {
                // will emulation be needed? ...
                // ... divine magic and WIS too low, or INT and CHA both are too low...
                var _needsEmulation = ((_complete.SpellSource.CasterClass.MagicType == MagicType.Divine)
                    && (_critter.Abilities.Wisdom.MaxSpellLevel() < _complete.PowerLevel))
                    || ((_critter.Abilities.Intelligence.MaxSpellLevel() < _complete.PowerLevel)
                    && (_critter.Abilities.Charisma.MaxSpellLevel() < _complete.PowerLevel));

                #region Use Scroll
                var _useScroll = UseScrollCheck;
                if (_useScroll == null)
                {
                    // continue dispensing if emulation will be needed
                    _Dispensing = _needsEmulation;

                    // use scroll
                    return new SuccessCheckPrerequisite(Activity, _iAct, @"Skill.UseMagicDevice.Scroll", @"Use Magic Device",
                        new SuccessCheck(_critter.Skills.Skill<UseMagicItemSkill>(), 20 + _complete.CasterLevel, _complete.SpellSource), true);
                }
                if (!_useScroll.IsReady)
                    return null;
                #endregion

                #region Emulate Ability Score
                // divine magic and WIS too low, or INT and CHA both are too low...
                if (_useScroll.Success && _needsEmulation)
                {
                    var _emulate = EmulateCheck;
                    if (_emulate == null)
                    {
                        // emulate ability check needed
                        _Dispensing = false;
                        return new SuccessCheckPrerequisite(Activity, _iAct, @"Skill.UseMagicDevice.Emulate", @"Use Magic Device (Emulate)",
                            new SuccessCheck(_critter.Skills.Skill<UseMagicItemSkill>(), 25 + _complete.PowerLevel, _complete.SpellSource), true);
                    }
                }
                #endregion
            }
            else
            {
                var _spellType = _complete.SpellSource.SpellDef.SeedSpellDef.GetType();

                // get the highest caster-level class that can cast the spell using the correct type of magic
                var _class = (from _c in _critter.Classes
                              where typeof(ICasterClass).IsAssignableFrom(_c.GetType())
                              let _castClass = _c as ICasterClass
                              where (_castClass.MagicType == _complete.SpellSource.CasterClass.MagicType)       // correct magic type
                              from _classSpell in _castClass.UsableSpells
                              where _spellType.IsAssignableFrom(_classSpell.SpellDef.SeedSpellDef.GetType())    // has spell
                              && (_castClass.SpellDifficultyAbility.MaxSpellLevel() >= _classSpell.Level)       // and can cast it
                              orderby _castClass.ClassPowerLevel.QualifiedValue(_iAct) descending               // get the highest caster level class
                              select _castClass).FirstOrDefault();                                              // and only one
                if (_class == null)
                {
                    // probably shouldn't get this far...wouldn't be able to use directly, and would have gone into UMD checks
                }
                else
                {
                    if (_class.ClassPowerLevel.QualifiedValue(_iAct) < _complete.CasterLevel)
                    {
                        #region Caster Check
                        var _casterCheck = CasterCheck;
                        if (_casterCheck == null)
                        {
                            // highest caster level class that can cast this is not high enough
                            return new SuccessCheckPrerequisite(Activity, _iAct, @"CasterLevel.Check", @"Caster Level Check",
                                new SuccessCheck(_class, _complete.CasterLevel + 1, _complete.SpellSource), true);
                        }
                        if (!_casterCheck.IsReady)
                            return null;
                        #endregion

                        if (!_casterCheck.Success)
                        {
                            // NOTE: natural 1 will fail this check...
                            _Dispensing = false;
                            return new SuccessCheckPrerequisite(Activity, _iAct, @"Ability.Wisdom", @"Wisdom Check",
                                new SuccessCheck(_critter.Abilities.Wisdom, 5, _complete.SpellSource), false);
                        }
                    }
                }
            }

            // nothing else to do...
            _Dispensing = false;
            return null;
        }
        #endregion

        #region public void DoStep()
        protected override bool OnDoStep()
        {
            // TODO: mishap?
            var _useScroll = UseScrollCheck;
            var _emulate = EmulateCheck;
            var _casterCheck = CasterCheck;

            if (((_useScroll != null) && _useScroll.FailsProcess)
                || (_emulate != null) && _emulate.FailsProcess)
            {
                AppendFollowing(Activity.GetActivityResultNotifyStep(@"Use Magic Device Failure"));
            }
            else if ((_casterCheck != null) && _casterCheck.FailsProcess)
            {
                AppendFollowing(Activity.GetActivityResultNotifyStep(@"Caster Level Check Failure"));
            }
            else
            {
                // remove spell completion
                CompleteSpell.SpellCompletion.Eject();
                AppendFollowing(new PowerActivationStep<SpellSource>(Activity, CompleteSpell, Activity.Actor));
            }

            // done
            return true;
        }
        #endregion
    }
}
