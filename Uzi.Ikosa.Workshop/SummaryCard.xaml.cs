using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Uzi.Ikosa;
using Uzi.Ikosa.Creatures;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Movement;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for SummaryCard.xaml
    /// </summary>

    public partial class SummaryCard : System.Windows.Window
    {
        public SummaryCard(Creature creature)
        {
            InitializeComponent();

            // Size/Type
            Label _sizeType = new Label();
            _sizeType.Content = string.Format("{0} {1}", creature.Body.Sizer.Size.Name, creature.CreatureType.ToString());
            Grid.SetColumn(_sizeType, 1);
            Grid.SetRow(_sizeType, 0);
            gridSummary.Children.Add(_sizeType);

            // Hit Dice
            Label _hitDice = new Label();
            _hitDice.Content = string.Format("{0}+{1} ({2} hp)", creature.Classes.PowerLevelSummary(), creature.HealthPoints.FromConstitution + creature.HealthPoints.ExtraHealthPoints, creature.HealthPoints.TotalValue);
            Grid.SetColumn(_hitDice, 1);
            Grid.SetRow(_hitDice, 1);
            gridSummary.Children.Add(_hitDice);

            // Initiative
            modInit.InitDeltable(creature.Initiative);

            // Speed
            foreach (MovementBase _move in creature.Movements.AllMovements)
            {
                Label _moveLabel = new Label();
                _moveLabel.Content = string.Format("{0}{1}", (wrapSpeed.Children.Count > 0 ? ", " : ""), _move.ToString());
                wrapSpeed.Children.Add(_moveLabel);
            }

            // Armor Classes
            modAC.InitDeltable(creature.NormalArmorRating);
            modTAC.InitDeltable(creature.TouchArmorRating);

            // Base Attack / Grapple
            modBAB.InitDeltable(creature.BaseAttack);
            modGrapple.InitDeltable(creature.OpposedDeltable);

            // Attack
            // Full Attack
            // Space/Reach
            // Special Attacks
            // Special Qualities

            // Saves
            modFort.InitDeltable(creature.FortitudeSave);
            modReflex.InitDeltable(creature.ReflexSave);
            modWill.InitDeltable(creature.WillSave);

            // Abilities
            modStr.InitDeltable(creature.Abilities.Strength);
            modDex.InitDeltable(creature.Abilities.Dexterity);
            modCon.InitDeltable(creature.Abilities.Constitution);
            modInt.InitDeltable(creature.Abilities.Intelligence);
            modWis.InitDeltable(creature.Abilities.Wisdom);
            modCha.InitDeltable(creature.Abilities.Charisma);

            // Skills
            foreach (SkillBase _skill in creature.Skills)
            {
                if (_skill.BaseValue > 0)
                {
                    Label _skillLabel = new Label();
                    _skillLabel.Background = Brushes.Cornsilk;
                    _skillLabel.Content =  _skill.SkillName;
                    wrapSkills.Children.Add(_skillLabel);
                    LabelModifiable _delta = new LabelModifiable();
                    _delta.InitDeltable(_skill);
                    wrapSkills.Children.Add(_delta);
                }
            }

            // Feats
            foreach (FeatBase _feat in creature.Feats)
            {
                Label _featLabel = new Label();
                _featLabel.Content = string.Format("{0}{1}", (wrapFeats.Children.Count > 0 ? ", " : ""), _feat.Name);
                _featLabel.ToolTip = _feat.Benefit;
                wrapFeats.Children.Add(_featLabel);
            }
        }
    }
}