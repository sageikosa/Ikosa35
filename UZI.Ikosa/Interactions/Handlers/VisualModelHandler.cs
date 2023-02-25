using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using System.Windows.Media.Media3D;
using Uzi.Visualize;
using Uzi.Visualize.Contracts.Tactical;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Universal;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class VisualModelHandler : IInteractHandler
    {
        public void HandleInteraction(Interaction workSet)
        {
            if (workSet.InteractData is VisualPresentationData _visModelData)
            {
                var _loc = _visModelData.TargetLocator;
                if (_loc != null)
                {
                    var _grZ = _loc.GeometricRegion.LowerZ;
                    var _grY = _loc.GeometricRegion.LowerY;
                    var _grX = _loc.GeometricRegion.LowerX;
                    var _grZHeight = _loc.GeometricRegion.UpperZ - _loc.GeometricRegion.LowerZ + 1;
                    var _grYLength = _loc.GeometricRegion.UpperY - _loc.GeometricRegion.LowerY + 1;
                    var _grXLength = _loc.GeometricRegion.UpperX - _loc.GeometricRegion.LowerX + 1;
                    var _baseFace = _loc.BaseFace;
                    if (_loc is ObjectPresenter _objPresenter)
                    {
                        // model presentation?
                        if (!string.IsNullOrWhiteSpace(_objPresenter.ModelKey))
                        {
                            // common
                            var _visModelBack = new VisualModelFeedback(this);
                            _visModelBack.ModelPresentation.CubeFitScale = _loc.CubeFitScale;
                            _visModelBack.ModelPresentation.Z = _grZ;
                            _visModelBack.ModelPresentation.Y = _grY;
                            _visModelBack.ModelPresentation.X = _grX;
                            _visModelBack.ModelPresentation.ZHeight = _grZHeight;
                            _visModelBack.ModelPresentation.YLength = _grYLength;
                            _visModelBack.ModelPresentation.XLength = _grXLength;
                            _visModelBack.ModelPresentation.BaseFace = _baseFace;
                            _visModelBack.ModelPresentation.IntraModelOffset = _loc.IntraModelOffset;
                            _visModelBack.ModelPresentation.MoveFrom = _loc.MoveFrom;
                            _visModelBack.ModelPresentation.SerialState = _loc.SerialState;

                            _visModelBack.ModelPresentation.ModelKey = _objPresenter.ModelKey;
                            if (workSet.Target is ISensorHost _subject)
                            {
                                // NOTE: model is expected to fill a 5' cube
                                _visModelBack.ModelPresentation.Pivot = _subject.Heading * 45d - 90d;
                                _visModelBack.ModelPresentation.CubeFitScale = new Vector3D(_subject.Width / 5, _subject.Length / 5, _subject.Height / 5);

                                // draw pogs for creatures
                                if (_subject is Creature _critter)
                                {
                                    // when truly an actor asking, and awareness
                                    if (workSet?.Actor is Creature _actor
                                        && ((_visModelData.Sensors?.Awarenesses[_critter.ID] ?? AwarenessLevel.Aware) == AwarenessLevel.Aware))
                                    {
                                        var _tracker = (_actor.ProcessManager as IkosaProcessManager)?.LocalTurnTracker;
                                        _visModelBack.ModelPresentation.Adornments.Add(new CharacterPogAdornment
                                        {
                                            IsSelf = _actor.ID == _critter.ID,
                                            IsTeam = _actor.GetSameTeams(_critter).Any(),
                                            HasInitiative = _tracker.FocusedBudget?.Actor.ID == _critter.ID
                                        });
                                    }
                                    else if (_visModelData.Sensors == null)
                                    {
                                        _visModelBack.ModelPresentation.Adornments.Add(new CharacterPogAdornment
                                        {
                                            IsSelf = false,
                                            IsTeam = false,
                                            HasInitiative = false
                                        });
                                    }
                                }
                            }
                            else
                            {
                                _visModelBack.ModelPresentation.Pivot = _objPresenter.Pivot;
                            }

                            _visModelBack.ModelPresentation.Tilt = _objPresenter.Tilt;
                            _visModelBack.ModelPresentation.ApparentScale = _objPresenter.ApparentScale;
                            workSet.Feedback.Add(_visModelBack);
                        }
                        else
                        {
                            // no model key, try getting icon keys
                            var _visIconBack = new VisualIconFeedback(this);
                            _visIconBack.IconPresentation.Z = _grZ;
                            _visIconBack.IconPresentation.Y = _grY;
                            _visIconBack.IconPresentation.X = _grX;
                            _visIconBack.IconPresentation.ZHeight = _grZHeight;
                            _visIconBack.IconPresentation.YLength = _grYLength;
                            _visIconBack.IconPresentation.XLength = _grXLength;
                            _visIconBack.IconPresentation.BaseFace = _baseFace;
                            _visIconBack.IconPresentation.MoveFrom = _loc.MoveFrom;
                            _visIconBack.IconPresentation.SerialState = _loc.SerialState;
                            var _critter = workSet.Target as Creature;
                            foreach (var _iconic in _loc.AllAccessible(_critter).OfType<ICoreIconic>())
                            {
                                _visIconBack.IconPresentation.IconRefs.Add(new IconReferenceInfo
                                {
                                    Keys = _iconic.PresentationKeys.ToArray(),
                                    IconColorMap = _iconic.IconColorMap,
                                    IconAngle = _iconic.IconAngle,
                                    IconScale = _iconic.IconScale
                                });
                            }
                            workSet.Feedback.Add(_visIconBack);
                        }
                    }
                    else
                    {
                        // not a model presenter, so try working with icon infos instead
                        var _visIconBack = new VisualIconFeedback(this);
                        _visIconBack.IconPresentation.Z = _grZ;
                        _visIconBack.IconPresentation.Y = _grY;
                        _visIconBack.IconPresentation.X = _grX;
                        _visIconBack.IconPresentation.ZHeight = _grZHeight;
                        _visIconBack.IconPresentation.YLength = _grYLength;
                        _visIconBack.IconPresentation.XLength = _grXLength;
                        _visIconBack.IconPresentation.BaseFace = _baseFace;
                        _visIconBack.IconPresentation.MoveFrom = _loc.MoveFrom;
                        _visIconBack.IconPresentation.SerialState = _loc.SerialState;
                        var _critter = workSet.Target as Creature;
                        foreach (var _iconic in _loc.AllAccessible(_critter).OfType<ICoreIconic>())
                        {
                            _visIconBack.IconPresentation.IconRefs.Add(new IconReferenceInfo
                            {
                                Keys = _iconic.PresentationKeys.ToArray(),
                                IconColorMap = _iconic.IconColorMap,
                                IconAngle = _iconic.IconAngle,
                                IconScale = _iconic.IconScale
                            });
                        }
                        workSet.Feedback.Add(_visIconBack);
                    }
                }
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
            => typeof(VisualPresentationData).ToEnumerable();

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => false;
    }
}
