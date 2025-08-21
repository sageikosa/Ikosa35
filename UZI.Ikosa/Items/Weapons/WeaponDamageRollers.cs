using System.Collections.Generic;
using Uzi.Core.Dice;

namespace Uzi.Ikosa.Items.Weapons
{
    public static class WeaponDamageRollers
    {
        static WeaponDamageRollers()
        {
            // constants
            _SharedRollers.Add(@"-", new ConstantRoller(0));
            _SharedRollers.Add(@"1", new ConstantRoller(1));

            // singletons
            _SharedRollers.Add(@"1d2", new DieRoller(2));
            _SharedRollers.Add(@"1d3", new DieRoller(3));
            _SharedRollers.Add(@"1d4", new DieRoller(4));
            _SharedRollers.Add(@"1d6", new DieRoller(6));
            _SharedRollers.Add(@"1d8", new DieRoller(8));
            _SharedRollers.Add(@"1d10", new DieRoller(10));
            _SharedRollers.Add(@"1d12", new DieRoller(12));

            // multi
            _SharedRollers.Add(@"2d4", new DiceRoller(2, 4));
            _SharedRollers.Add(@"2d6", new DiceRoller(2, 6));
            _SharedRollers.Add(@"2d8", new DiceRoller(2, 8));
            _SharedRollers.Add(@"2d10", new DiceRoller(2, 10));
            _SharedRollers.Add(@"3d6", new DiceRoller(3, 6));
            _SharedRollers.Add(@"3d8", new DiceRoller(3, 8));
            _SharedRollers.Add(@"4d6", new DiceRoller(4, 6));
            _SharedRollers.Add(@"4d8", new DiceRoller(4, 8));
            _SharedRollers.Add(@"6d6", new DiceRoller(6, 6));
            _SharedRollers.Add(@"6d8", new DiceRoller(6, 8));
            _SharedRollers.Add(@"8d6", new DiceRoller(8, 6));
            _SharedRollers.Add(@"8d8", new DiceRoller(8, 8));
            _SharedRollers.Add(@"12d8", new DiceRoller(12, 8));

            // setup collection of collections
            DiceLookup = new Dictionary<string, Dictionary<int, Roller>>
            {
                {
                    @"1d2",
                    BuildRollerProgression(@"-", @"-", @"-", @"1", @"1d2", @"1d3", @"1d4", @"1d6", @"1d8")
                },
                {
                    @"1d3",
                    BuildRollerProgression(@"-", @"-", @"1", @"1d2", @"1d3", @"1d4", @"1d6", @"1d8", @"2d6")
                },
                {
                    @"1d4",
                    BuildRollerProgression(@"-", @"1", @"1d2", @"1d3", @"1d4", @"1d6", @"1d8", @"2d6", @"3d6")
                },
                {
                    @"1d6",
                    BuildRollerProgression(@"1", @"1d2", @"1d3", @"1d4", @"1d6", @"1d8", @"2d6", @"3d6", @"4d6")
                },
                {
                    @"1d8",
                    BuildRollerProgression(@"1d2", @"1d3", @"1d4", @"1d6", @"1d8", @"2d6", @"3d6", @"4d6", @"6d6")
                },
                {
                    @"1d10",
                    BuildRollerProgression(@"1d3", @"1d4", @"1d6", @"1d8", @"1d10", @"2d8", @"3d8", @"4d8", @"6d8")
                },
                {
                    @"1d12",
                    BuildRollerProgression(@"1d4", @"1d6", @"1d8", @"1d10", @"1d12", @"3d6", @"4d6", @"6d6", @"8d6")
                },
                {
                    @"2d4",
                    BuildRollerProgression(@"1d2", @"1d3", @"1d4", @"1d6", @"2d4", @"2d6", @"3d6", @"4d6", @"6d6")
                },
                {
                    @"2d6",
                    BuildRollerProgression(@"1d4", @"1d6", @"1d8", @"1d10", @"2d6", @"3d6", @"4d6", @"6d6", @"8d6")
                },
                {
                    @"2d8",
                    BuildRollerProgression(@"1d6", @"1d8", @"1d10", @"2d6", @"2d8", @"3d8", @"4d8", @"6d8", @"8d8")
                },
                {
                    @"2d10",
                    BuildRollerProgression(@"1d8", @"1d10", @"2d6", @"2d8", @"2d10", @"4d8", @"6d8", @"8d8", @"12d8")
                }
            };
        }

        public static Dictionary<int, Roller> BuildRollerProgression(
            string fine, string diminutive, string tiny, string small, string medium,
            string large, string huge, string gigantic, string colossal)
        {
            var _newSet = new Dictionary<int, Roller>
            {
                { Size.Fine.Order, _SharedRollers[fine] },
                { Size.Miniature.Order, _SharedRollers[diminutive] },
                { Size.Tiny.Order, _SharedRollers[tiny] },
                { Size.Small.Order, _SharedRollers[small] },
                { Size.Medium.Order, _SharedRollers[medium] },
                { Size.Large.Order, _SharedRollers[large] },
                { Size.Huge.Order, _SharedRollers[huge] },
                { Size.Gigantic.Order, _SharedRollers[gigantic] },
                { Size.Colossal.Order, _SharedRollers[colossal] }
            };
            return _newSet;
        }

        private static Dictionary<string, Roller> _SharedRollers = [];

        public readonly static Dictionary<string, Dictionary<int, Roller>> DiceLookup;
    }
}
