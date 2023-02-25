using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Proxy;

namespace Uzi.Ikosa.Client.UI
{
    /// <summary>
    /// Interaction logic for LogOutput.xaml
    /// </summary>
    public partial class LogOutput : UserControl
    {
        public LogOutput()
        {
            try { InitializeComponent(); } catch { }
            rtbOutput.Document.Blocks.Clear();
            _Blocks = new Dictionary<long, Block>();
        }

        static LogOutput()
        {
            _CheckBG = new SolidColorBrush(Color.FromRgb(16, 16, 20));
            _CheckBG.Freeze();
            _DeltaCalcBG = new SolidColorBrush(Color.FromRgb(20, 20, 24));
            _DeltaCalcBG.Freeze();
        }

        private static readonly Brush _CheckBG;
        private static readonly Brush _DeltaCalcBG;

        private readonly Dictionary<long, Block> _Blocks;

        public Func<Guid, string> PrincipalPrefixer { get; set; }

        private string GetPrincipalPrefix(Guid id)
            => PrincipalPrefixer?.Invoke(id) ?? string.Empty;

        public ObservableCollection<SysNotifyVM> SysNotifies
        {
            get { return (ObservableCollection<SysNotifyVM>)GetValue(SysNotifiesProperty); }
            set { SetValue(SysNotifiesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SysNotifies.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SysNotifiesProperty =
            DependencyProperty.Register(nameof(SysNotifies), typeof(ObservableCollection<SysNotifyVM>), typeof(LogOutput),
                new PropertyMetadata(null, SysNotifies_Changed));

        private static void SysNotifies_Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if (dependencyObject is LogOutput _log)
            {
                if (args.OldValue is ObservableCollection<SysNotifyVM> _old)
                {
                    _old.CollectionChanged -= _log.SysNotifyContent_Changed;
                }

                // clear blocks
                _log.ClearBlocks();

                if (args.NewValue is ObservableCollection<SysNotifyVM> _new)
                {
                    // generate initial blocks
                    _new.CollectionChanged += _log.SysNotifyContent_Changed;
                    foreach (var _item in _new)
                    {
                        var _block = _log.GetBlock(_item);
                        if ((_block != null) && !_log._Blocks.ContainsKey(_item.ID))
                        {
                            _log._Blocks.Add(_item.ID, _block);
                            _log.rtbOutput.Document.Blocks.Add(_block);
                        }
                    }
                }
                _log.rtbOutput.ScrollToEnd();
            }
        }

        private void SysNotifyContent_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var _item in e.NewItems.OfType<SysNotifyVM>())
                    {
                        // add new blocks
                        var _block = GetBlock(_item);
                        if ((_block != null) && !_Blocks.ContainsKey(_item.ID))
                        {
                            _Blocks.Add(_item.ID, _block);
                            rtbOutput.Document.Blocks.Add(_block);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var _item in e.OldItems.OfType<SysNotifyVM>())
                    {
                        // remove old blocks
                        if (_Blocks.TryGetValue(_item.ID, out var _remove))
                        {
                            _Blocks.Remove(_item.ID);
                            rtbOutput.Document.Blocks.Remove(_remove);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    ClearBlocks();
                    break;
            }
            rtbOutput.ScrollToEnd();
        }

        private void ClearBlocks()
        {
            // clear blocks
            _Blocks.Clear();
            Dispatcher.BeginInvoke(new Action(() =>
            {
                rtbOutput.Document.Blocks.Clear();
                rtbOutput.ScrollToEnd();
            }));
        }

        private Block GetBlock(SysNotifyVM sysNotify)
        {
            // TODO: if master... lookup principal ID to get creature/actor

            List<string> _getInfoList(DeltaCalcInfo info, int indent)
            {
                var _list = new List<String>
                {
                    $@"{GetPrincipalPrefix(info.PrincipalID)}{info.Title}",
                    $@"{string.Empty.PadLeft(indent)}{info.Result,-4:+0;-0} = {info.BaseValue,-4:+0;-0} Base/Roll"
                };
                if ((info.Deltas != null) && info.Deltas.Any(_d => _d.Value != 0))
                {
                    foreach (var _d in info.Deltas.Where(_d => _d.Value != 0)
                        .OrderByDescending(_d => _d.Value)
                        .ThenBy(_d => _d.Name))
                    {
                        _list.Add($@"{string.Empty.PadLeft(7 + indent)}{_d.Value,-4:+0;-0} {_d.Name}");
                    }
                }
                return _list;
            }

            Paragraph _addStyles(Paragraph source, Action<Paragraph> styler)
            {
                styler(source);
                return source;
            }
            switch (sysNotify.Notify)
            {
                case ActivityResultNotify _actRslt:
                    if (_actRslt.Activity != null)
                    {
                        var _s = new Section();
                        var _p = new Paragraph(new Run($@"{GetPrincipalPrefix(_actRslt.Activity.ActorID)}"))
                        {
                            Foreground = Brushes.Yellow,
                            Background = Brushes.DarkGreen
                        };
                        _p.Inlines.Add(new Run(_actRslt.Activity?.Message));
                        _s.Blocks.Add(_p);

                        var _l = new List();
                        Paragraph _lp;
                        if (_actRslt.Activity.Implement != null)
                        {
                            _lp = new Paragraph(new Run(@"using "));
                            _lp.Inlines.Add(GetInfoRun(_actRslt.Activity.Implement));
                            _l.ListItems.Add(new ListItem(_lp));
                        }
                        if (_actRslt.Activity.Targets != null)
                        {
                            if (_actRslt.Activity.Targets.Length == 1)
                            {
                                // single target inline
                                _p.Inlines.Add(GetInfoRun(_actRslt.Activity.Targets[0]));
                            }
                            else
                            {
                                foreach (var _t in _actRslt.Activity.Targets)
                                {
                                    _lp = new Paragraph(new Run(@"target "));
                                    _lp.Inlines.Add(GetInfoRun(_t));
                                    _l.ListItems.Add(new ListItem(_lp));
                                }
                            }
                        }
                        if (_l.ListItems.Any())
                        {
                            _s.Blocks.Add(_l);
                        }
                        AppendInfoList(_s, _actRslt.Infos);

                        // _actRslt.Activity.Details
                        // TODO: infos?
                        return _s;
                    }
                    return null;

                case BadNewsNotify _bad:
                    {
                        var _s = new Section();
                        var _p = new Paragraph(new Run($@"{GetPrincipalPrefix(_bad.PrincipalID)}{_bad.Topic}")) { Foreground = Brushes.Red };
                        _s.Blocks.Add(_p);
                        AppendInfoList(_s, _bad.Infos);
                        return _s;
                    }

                case GoodNewsNotify _good:
                    {
                        var _s = new Section();
                        var _p = new Paragraph(new Run($@"{GetPrincipalPrefix(_good.PrincipalID)}{_good.Topic}")) { Foreground = Brushes.LightGreen };
                        _s.Blocks.Add(_p);
                        AppendInfoList(_s, _good.Infos);
                        return _s;
                    }

                case DeltaCalcNotify _dCalc:
                    {
                        // information
                        var _dcList = _getInfoList(_dCalc.DeltaCalc, 2);

                        var _s = new Section();
                        var _p = new Paragraph() { Background = _DeltaCalcBG };
                        _s.Blocks.Add(_p);

                        for (var _lx = 0; _lx < _dcList.Count; _lx++)
                        {
                            if (_lx != 0)
                            {
                                _p.Inlines.Add(new LineBreak());
                            }
                            _p.Inlines.Add(new Run($@"{_dcList[_lx]}")
                            {
                                Foreground = Brushes.White
                            });
                        }
                        return _s;
                    }

                case CheckNotify _check:
                    {
                        // information columns
                        var _ciList = _getInfoList(_check.CheckInfo, 2);
                        var _oiList = _getInfoList(_check.OpposedInfo, 0);

                        var _s = new Section();
                        var _p = new Paragraph() { Background = _CheckBG };
                        _s.Blocks.Add(_p);

                        // details
                        var _cBrush = _check.CheckInfo.Result >= _check.OpposedInfo.Result ? Brushes.Lime : Brushes.Crimson;
                        var _oBrush = _check.CheckInfo.Result < _check.OpposedInfo.Result ? Brushes.Lime : Brushes.Crimson;
                        var _maxCI = _ciList.Max(_cii => _cii.Length);
                        if (_ciList.Any() || _oiList.Any())
                        {
                            for (var _lx = 0; (_lx < _ciList.Count) || (_lx < _oiList.Count); _lx++)
                            {
                                if (_lx != 0)
                                {
                                    _p.Inlines.Add(new LineBreak());
                                }
                                if (_lx < _ciList.Count)
                                {
                                    _p.Inlines.Add(new Run($@"{_ciList[_lx]}{string.Empty.PadLeft(_maxCI - _ciList[_lx].Length)}")
                                    {
                                        Foreground = _cBrush
                                    });
                                }
                                else
                                {
                                    _p.Inlines.Add(new Run(string.Empty.PadLeft(_maxCI)));
                                }
                                if (_lx < _oiList.Count)
                                {
                                    _p.Inlines.Add(new Run(_lx == 0 ? @" /VS/ " : string.Empty.PadLeft(6)));
                                    _p.Inlines.Add(new Run(_oiList[_lx])
                                    {
                                        Foreground = _oBrush
                                    });
                                }
                            }
                        }

                        return _s;
                    }

                case RollNotify _roll:
                    {
                        var _s = new Section();
                        var _p = new Paragraph(new Run($@"{GetPrincipalPrefix(_roll.PrincipalID)}{_roll.Topic}"))
                        {
                            Foreground = Brushes.Cyan
                        };
                        _p.Inlines.Add(new Run(@" "));
                        _p.Inlines.Add(new Run(_roll.RollerLog.Expression)
                        {
                            FontStyle = FontStyles.Italic
                        });
                        _p.Inlines.Add(new Run(@" => "));
                        _p.Inlines.Add(new Run(_roll.RollerLog.Total.ToString())
                        {
                            FontWeight = FontWeights.Bold,
                            ToolTip = _roll.RollerLog.PartList()
                        });
                        _s.Blocks.Add(_p);
                        if (_roll.Infos.Count == 1)
                        {
                            _p.Inlines.Add(new Run(@" ("));
                            _p.Inlines.Add(GetInfoRun(_roll.Infos[0]));
                            _p.Inlines.Add(new Run(@")"));
                        }
                        else
                        {
                            AppendInfoList(_s, _roll.Infos);
                        }
                        return _s;
                    }

                case AttackedNotify _attacked:
                    {
                        var _s = new Section();
                        var _p = new Paragraph(new Run($@"{GetPrincipalPrefix(_attacked.PrincipalID)}{_attacked.Topic}"))
                        {
                            Foreground = Brushes.Red
                        };
                        _s.Blocks.Add(_p);
                        if (_attacked.ObservedActivity?.Actor != null)
                        {
                            _p.Inlines.Add(@" by ");
                            _p.Inlines.Add(GetInfoRun(_attacked.ObservedActivity.Actor));
                        }
                        else
                        {
                            AppendInfoList(_s, _attacked.Infos);
                        }
                        return _s;
                    }

                case CheckResultNotify _check:
                    {
                        var _s = new Section();
                        var _p = new Paragraph(new Run($@"{GetPrincipalPrefix(_check.PrincipalID)}{_check.Topic}"))
                        {
                            Foreground = Brushes.Cyan
                        };
                        _p.Inlines.Add(new Run(@" "));
                        _p.Inlines.Add(new Run(_check.Success ? @"Success" : @"Failure")
                        {
                            Background = _check.Success ? Brushes.DarkGreen : Brushes.DarkRed,
                            Foreground = Brushes.Yellow
                        });
                        _s.Blocks.Add(_p);
                        if (_check.Infos.Count == 1)
                        {
                            _p.Inlines.Add(new Run(@" ("));
                            _p.Inlines.Add(GetInfoRun(_check.Infos[0]));
                            _p.Inlines.Add(new Run(@")"));
                        }
                        else
                        {
                            AppendInfoList(_s, _check.Infos);
                        }
                        return _s;
                    }

                case ConditionNotify _condition:
                    {
                        var _s = new Section();
                        var _p = new Paragraph(new Run($@"{GetPrincipalPrefix(_condition.PrincipalID)}{_condition.Topic}"))
                        {
                            Foreground = _condition.IsEnding ? Brushes.YellowGreen : Brushes.Yellow
                        };
                        if (_condition.IsEnding)
                            _p.Inlines.Add(new Run(@" Ending"));
                        _s.Blocks.Add(_p);
                        _p.Inlines.Add(new Run($@": {_condition.Condition}"));
                        AppendInfoList(_s, _condition.Infos);
                        return _s;
                    }

                case DealingDamageNotify _dealing:
                    {
                        var _s = new Section();
                        var _p = new Paragraph(new Run($@"{GetPrincipalPrefix(_dealing.PrincipalID)}{_dealing.Topic}"));
                        _s.Blocks.Add(_p);
                        var _l = new List();
                        foreach (var _d in _dealing.Damages)
                        {
                            _l.ListItems.Add(new ListItem(GetDamageText(_d)));
                        }
                        _s.Blocks.Add(_l);
                        AppendInfoList(_s, _dealing.Infos);
                        return _s;
                    }

                case DealtDamageNotify _dealt:
                    {
                        var _s = new Section();
                        var _p = new Paragraph(new Run($@"{GetPrincipalPrefix(_dealt.PrincipalID)}{_dealt.Topic}"))
                        {
                            Foreground = Brushes.Red,
                            FontWeight = FontWeights.Bold
                        };
                        _s.Blocks.Add(_p);
                        var _l = new List { Foreground = Brushes.Red };
                        foreach (var _d in _dealt.Damages)
                        {
                            _l.ListItems.Add(
                                new ListItem(_addStyles(GetDamageText(_d), (p) => p.Foreground = Brushes.Red))
                                {
                                    Foreground = Brushes.Red
                                });
                        }
                        _s.Blocks.Add(_l);
                        AppendInfoList(_s, _dealt.Infos);
                        return _s;
                    }

                default:
                    {
                        var _s = new Section(new Paragraph(new Run(sysNotify.Notify.GetType().FullName)));
                        _s.Blocks.Add(new Paragraph(new Run(sysNotify.Notify.Topic)));
                        return _s;
                    }
            }
        }

        private void AppendInfoList(Section section, List<Info> infos)
        {
            var _l = new List();
            foreach (var _i in infos)
            {
                _l.ListItems.Add(new ListItem(new Paragraph(GetInfoRun(_i))));
            }
            if (_l.ListItems.Any())
                section.Blocks.Add(_l);
        }

        private Run GetInfoRun(Info info)
        {
            switch (info)
            {
                //case Description _descr:
                //    return new Run(_descr.Message);
                //case ActionProviderInfo _actProv:
                //    break;
                case AdjunctInfo _adj:
                    return new Run(_adj.Message);

                case FeatInfo _feat:
                    return new Run(_feat.FeatName);

                case MovementInfo _move:
                    return new Run($@"{_move.Message} ({_move.Value} ft)");

                case BodyInfo _body:
                    return new Run($@"{_body.Message}");

                case CreatureObjectInfo _critter:
                    return new Run(_critter.Message);

                case DoubleMeleeWeaponInfo _dbl:
                    return new Run(_dbl.Message);

                case MeleeWeaponInfo _melee:
                    return new Run(_melee.Message);

                case NaturalWeaponInfo _natrl:
                    return new Run(_natrl.Message);

                case ProjectileWeaponInfo _project:
                    return new Run(_project.Message);

                case ArmorInfo _armor:
                    return new Run(_armor.Message);

                case ShieldInfo _shield:
                    return new Run(_shield.Message);

                case ObjectInfo _obj:
                    return new Run(_obj.Message);

                case WeaponHeadInfo _wpnHead:
                    return new Run(_wpnHead.Message);

                case ItemSlotInfo _slot:
                    return new Run(_slot.Message);

                default:
                    return new Run(info?.Message);
            }
        }

        private Paragraph GetDamageText(DamageInfo damage)
        {
            // TODO: these need "log templates" (Infos)
            switch (damage)
            {
                case AbilityDamageInfo _ability:
                    {
                        var _p = new Paragraph(new Run(damage.Amount.ToString()));
                        _p.Inlines.Add(new Run($@" {_ability.AbilityMnemonic}")
                        {
                            FontWeight = FontWeights.Bold
                        });
                        _p.Inlines.Add(new Run(_ability.IsDrain ? @" Drain" : @" Damage"));
                        if (!string.IsNullOrWhiteSpace(damage.Extra))
                        {
                            _p.Inlines.Add(new Run($@" ({damage.Extra})"));
                        }
                        return _p;
                    }

                case EnergyDamageInfo _energy:
                    {
                        var _p = new Paragraph(new Run(damage.Amount.ToString()));
                        _p.Inlines.Add(new Run($@" {_energy.Energy}")
                        {
                            FontWeight = FontWeights.Bold
                        });
                        _p.Inlines.Add(@" Damage");
                        if (!string.IsNullOrWhiteSpace(damage.Extra))
                        {
                            _p.Inlines.Add(new Run($@" ({damage.Extra})"));
                        }
                        return _p;
                    }

                case NonLethalDamageInfo _:
                    {
                        var _p = new Paragraph(new Run(damage.Amount.ToString()));
                        _p.Inlines.Add(@" Non-Lethal Damage");
                        if (!string.IsNullOrWhiteSpace(damage.Extra))
                        {
                            _p.Inlines.Add(new Run($@" ({damage.Extra})"));
                        }
                        return _p;
                    }

                case LethalDamageInfo _:
                    {
                        var _p = new Paragraph(new Run(damage.Amount.ToString()));
                        _p.Inlines.Add(@" Damage");
                        if (!string.IsNullOrWhiteSpace(damage.Extra))
                        {
                            _p.Inlines.Add(new Run($@" ({damage.Extra})"));
                        }
                        return _p;
                    }
            }
            return new Paragraph(new Run(damage.Amount.ToString()));
        }

        // TODO: these need "log templates" (Infos)

        // ... Info
        // ... Description
    }
}
