using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Markup;
using System.Diagnostics;

namespace Uzi.Visualize
{
    public class Visualization
    {
        #region Construction
        public Visualization(IPresentationInputBinder presentationInputBinder,
            ContainerUIElement3D tokenContainer, Model3DGroup opaqueGroup, Model3DGroup alphaGroup,
            Model3DGroup transientGroup, FrameworkElement element)
        {
            _OpaqueTerrain = opaqueGroup;
            _AlphaTerrain = alphaGroup;
            _TokenContainer = tokenContainer;
            _TransientGroup = transientGroup;
            _PresentationBinder = presentationInputBinder;
            _Transients = [];

            // namescope for animations
            _Element = element;
            _Scope = new NameScope();
            NameScope.SetNameScope(_Element, _Scope);
        }
        #endregion

        #region data
        private Model3DGroup _OpaqueTerrain;
        private Model3DGroup _AlphaTerrain;
        private ContainerUIElement3D _TokenContainer;
        private IPresentationInputBinder _PresentationBinder;
        private Storyboard _TokenBoard;

        private FrameworkElement _Element;
        private Model3DGroup _TransientGroup;
        private List<TransientVisualizer> _Transients;
        private INameScope _Scope;
        private Storyboard _TransientBoard;
        #endregion

        public INameScope Scope => _Scope;
        public Model3DGroup TransientGroup => _TransientGroup;

        #region public void SetTerrain(IEnumerable<BuildableGroup> terrain)
        public void SetTerrain(IEnumerable<BuildableGroup> terrain)
        {
            _OpaqueTerrain.Children.Clear();
            _AlphaTerrain.Children.Clear();
            foreach (var _group in terrain)
            {
                if (_group.Opaque != null)
                {
                    _OpaqueTerrain.Children.Add(_group.Opaque);
                }

                if (_group.Alpha != null)
                {
                    _AlphaTerrain.Children.Add(_group.Alpha);
                }
            }
        }
        #endregion

        #region private IEnumerable<Timeline> GetTokenOffsetAnimations(Vector3D fromVector, string scopeName)
        private IEnumerable<Timeline> GetTokenOffsetAnimations(Vector3D fromVector, string scopeName)
        {
            // TODO: duration of movement
            Func<string, double, DoubleAnimation> _makeAnimation = (name, endValue) =>
            {
                var _anim = new DoubleAnimation(endValue, 0, TimeSpan.FromMilliseconds(75), FillBehavior.Stop);
                Storyboard.SetTargetName(_anim, scopeName);
                Storyboard.SetTargetProperty(_anim, new PropertyPath(string.Concat(@"Transform.(TranslateTransform3D.", name, @")")));
                _anim.FillBehavior = FillBehavior.HoldEnd;
                return _anim;
            };

            // animate position
            yield return _makeAnimation(@"OffsetX", fromVector.X);
            yield return _makeAnimation(@"OffsetY", fromVector.Y);
            yield return _makeAnimation(@"OffsetZ", fromVector.Z);
            yield break;
        }
        #endregion

        #region public void SetTokens(IEnumerable<Presentable> presentables)
        /// <summary>
        /// Adds any Model from each Presentable to a ModelUIElement3D, adds InputBindings and finally adds to the TokenContainer
        /// </summary>
        public void SetTokens(IEnumerable<Presentable> presentables, bool animateMove, ulong serialState)
        {
            // token container
            _TokenBoard = new Storyboard();
            _TokenContainer.Children.Clear();
            foreach (var _present in presentables)
            {
                if (_present.Presentations.Any())
                {
                    var _elem = new ModelUIElement3D { Model = _present.Model3D };
                    _elem.InputBindings.AddRange(_PresentationBinder.GetBindings(_present).ToList());

                    // add
                    _TokenContainer.Children.Add(_elem);

                    // move-from animation
                    if (animateMove
                        && (_present.SerialState == serialState)
                        && (_present.MoveFrom.Length > 0))
                    {
                        // create transform
                        _elem.Transform = new TranslateTransform3D();

                        // register name
                        var _name = $@"my{(Guid.NewGuid()).ToString().Replace(@"-", string.Empty)}";
                        Scope.RegisterName(_name, _elem);

                        // animate
                        var _anim = GetTokenOffsetAnimations(_present.MoveFrom, _name).ToList();
                        if (_anim.Any())
                        {
                            // TODO: duration of movement
                            var _visible = new ParallelTimeline(TimeSpan.FromTicks(0), TimeSpan.FromMilliseconds(75));
                            foreach (var _a in _anim)
                            {
                                _visible.Children.Add(_a);
                            }

                            _TokenBoard.Children.Add(_visible);
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($@"Presenter => Empty!");
                }
            }
            _TokenBoard.FillBehavior = FillBehavior.Stop;
            _TokenBoard.Begin(_Element);
        }
        #endregion

        public void SetTransients(IEnumerable<TransientVisualizer> transients)
        {
            _TransientBoard = new Storyboard();
            _Transients = transients.ToList();
        }

        #region public void AnimateTransients()
        /// <summary>Generates and runs animations for all transient visualizers</summary>
        public void AnimateTransients()
        {
            // clear drawing group and namescope
            _TransientGroup.Children.Clear();

            // build storyboard of animations
            foreach (var _transient in _Transients)
            {
                var _timeline = _transient.GetTimeline(this);
                if (_timeline != null)
                {
                    _TransientBoard.Children.Add(_timeline);
                }
            }

            // when done, must stop
            _TransientBoard.FillBehavior = FillBehavior.Stop;
            _TransientBoard.Begin(_Element);
        }
        #endregion

        public void ReAnimateTransients()
        {
            if (_TransientBoard != null)
            {
                _TransientBoard.Begin(_Element);
            }
        }

        // TODO: persistent visuals: cell-fillers and virtual walls

        // TODO: heads-up overlay: targeting, info markers and direction info markers (include commands)
    }
}
