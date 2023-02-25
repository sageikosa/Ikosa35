using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;
using Uzi.Visualize.Contracts.Tactical;
using Uzi.Visualize.Packaging;

namespace Uzi.Ikosa.UI
{
    public class IconCustomizationVM : INotifyPropertyChanged
    {
        public IconCustomizationVM(PresentableContext context)
        {
            _Context = context;
            _AddIconKey = new RelayCommand<IconPartListItem>(
                part =>
                {
                    _Context?.CoreObject?.AddAdjunct(new IconKeyAdjunct(part.IconPart.Name, 0));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IconKeys)));
                    RefreshObjectIcon();
                },
                part => (part != null) && (!_Context?.CoreObject?.Adjuncts.OfType<IconKeyAdjunct>().Any() ?? true));
            _RemoveIconKey = new RelayCommand(RemoveIconKey, () => ObjectSource?.Adjuncts.OfType<IconKeyAdjunct>().Any() ?? false);
            _AddColorMap = new RelayCommand(AddColorMap, () => !(ObjectSource?.Adjuncts.OfType<ColorMapAdjunct>().Any() ?? true));
            _RemoveColorMap = new RelayCommand(RemoveColorMap, () => ObjectSource?.Adjuncts.OfType<ColorMapAdjunct>().Any() ?? false);
            _RefreshColorMap = new RelayCommand(RefreshColorMap, () => ObjectSource?.Adjuncts.OfType<ColorMapAdjunct>().Any() ?? false);
            var _cMap = _Context?.CoreObject?.Adjuncts.OfType<ColorMapAdjunct>().FirstOrDefault();
            if (_cMap != null)
            {
                _Colors = new List<ColorVM>();
                _Colors.AddRange(_cMap.ColorMap
                    .Select(_kvp => new ColorVM(_cMap, _kvp.Key, RefreshObjectIcon)));
            }
        }

        private PresentableContext _Context;
        private List<ColorVM> _Colors;

        private readonly RelayCommand _RemoveIconKey;
        private readonly RelayCommand<IconPartListItem> _AddIconKey;

        private readonly RelayCommand _AddColorMap;
        private readonly RelayCommand _RemoveColorMap;
        private readonly RelayCommand _RefreshColorMap;

        public event PropertyChangedEventHandler PropertyChanged;

        public PresentableContext PresentableContext => _Context;
        public ICoreObject ObjectSource => _Context?.CoreObject;

        public IEnumerable<IconPartListItem> ResolvableIcons
            => _Context?.VisualResources.ResolvableIcons.OrderBy(_p => _p.IconPart.Name).ToList() ?? new List<IconPartListItem>();

        public IEnumerable<string> IconKeys
            => ObjectSource?.IconKeys.ToList() ?? new List<string>();

        public double IconAngle
        {
            get => ObjectSource?.IconAngle ?? 0;
            set
            {
                if (ObjectSource != null)
                {
                    ObjectSource.IconAngle = value;
                    RefreshObjectIcon();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IconAngle)));
                }
            }
        }

        public double IconScale
        {
            get => ObjectSource?.IconScale ?? 1;
            set
            {
                if (ObjectSource != null)
                {
                    ObjectSource.IconScale = value;
                    RefreshObjectIcon();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IconScale)));
                }
            }
        }

        public RelayCommand<IconPartListItem> DoAddIconKey => _AddIconKey;
        public RelayCommand DoRemoveIconKey => _RemoveIconKey;
        public RelayCommand DoAddColorMap => _AddColorMap;
        public RelayCommand DoRemoveColorMap => _RemoveColorMap;
        public RelayCommand DoRefreshColorMap => _RemoveColorMap;

        // TODO: IconKeyAdjuncts + resolvable icons: from Map.Resource.ResolvableIcons

        public void RefreshObjectIcon()
        {
            var _object = _Context;
            _Context = null;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PresentableContext)));
            _Context = _object;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PresentableContext)));
        }

        public IList<ColorVM> ColorMapping
            => _Colors;

        private void RemoveIconKey()
        {
            var _keyAdjunct = ObjectSource?.Adjuncts.OfType<IconKeyAdjunct>().FirstOrDefault();
            ObjectSource?.RemoveAdjunct(_keyAdjunct);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IconKeys)));
            RefreshObjectIcon();
        }

        private void AddColorMap()
        {
            if (_Context?.VisualResources != null)
            {
                foreach (var _key in ObjectSource.IconKeys)
                {
                    var _icon = _Context.VisualResources.ResolveIconVisual(_key, new IconReferenceInfo { IconScale = 1 });
                    if (_icon != null)
                    {
                        var _newMap = ColorVal.GetKeyedColors();
                        if (_newMap != null)
                        {
                            ObjectSource.Adjuncts.OfType<ColorMapAdjunct>().FirstOrDefault()?.Eject();

                            var _colorMap = new ColorMapAdjunct(_newMap);
                            ObjectSource.AddAdjunct(_colorMap);
                            _Colors = new List<ColorVM>();
                            _Colors.AddRange(_newMap
                                .Select(_kvp => new ColorVM(_colorMap, _kvp.Key, RefreshObjectIcon)));
                        }
                    }
                }
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ColorMapping)));
            RefreshObjectIcon();
        }

        private void RemoveColorMap()
        {
            ObjectSource.RemoveAdjunct(ObjectSource.Adjuncts.OfType<ColorMapAdjunct>().FirstOrDefault());
            _Colors = null;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ColorMapping)));
            RefreshObjectIcon();
        }

        private void RefreshColorMap()
        {
            if (_Context.VisualResources != null)
            {
                var _cMap = ObjectSource.Adjuncts.OfType<ColorMapAdjunct>().FirstOrDefault();
                foreach (var _key in ObjectSource.IconKeys)
                {
                    var _currMap = _cMap.ColorMap;
                    var _icon = _Context.VisualResources.ResolveIconVisual(_key, ObjectSource);
                    if (_icon != null)
                    {
                        var _newMap = ColorVal.GetKeyedColors();
                        foreach (var _kvp in _newMap.Where(_nm => !_currMap.ContainsKey(_nm.Key)))
                        {
                            _currMap[_kvp.Key] = _kvp.Value;
                            _Colors.Add(new ColorVM(_cMap, _kvp.Key, RefreshObjectIcon));
                        }
                    }
                }
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ColorMapping)));
            RefreshObjectIcon();
        }
    }

    public class ColorVM : INotifyPropertyChanged
    {
        public ColorVM(ColorMapAdjunct colorMap, string key, Action refresh)
        {
            _CMap = colorMap;
            _Key = key;
            _Refresh = refresh;
        }

        private readonly ColorMapAdjunct _CMap;
        private readonly string _Key;
        private readonly Action _Refresh;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Key
            => _Key;

        public string ColorString
            => _CMap.ColorMap[_Key];

        private static Color _getColor(string colorStr)
            => !string.IsNullOrWhiteSpace(colorStr)
            ? (Color)ColorConverter.ConvertFromString(colorStr)
            : Colors.Transparent;

        public Color ColorValue
        {
            get => _getColor(ColorString);
            set
            {
                string _setColor(Color color)
                   => color == Colors.Transparent ? null : color.ToString();
                _CMap.ColorMap[_Key] = _setColor(value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ColorValue)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ColorString)));
                _Refresh?.Invoke();
            }
        }
    }
}
