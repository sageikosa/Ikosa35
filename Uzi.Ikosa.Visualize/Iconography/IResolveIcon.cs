using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Uzi.Visualize.Packaging;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Visualize
{
    public interface IResolveIcon
    {
        Visual GetIconVisual(string key, IIconReference iconRef);
        Material GetIconMaterial(string key, IIconReference iconRef, IconDetailLevel detailLevel);
        IResolveIcon IResolveIconParent { get; }
        IEnumerable<IconPartListItem> ResolvableIcons { get; }
    }

    public static class IResolveIconHelper
    {
        /// <summary>
        /// Walk through parent references if necessary to resolve icon
        /// </summary>
        /// <param name="self"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static Visual ResolveIconVisual(this IResolveIcon self, string key, IIconReference iconRef)
        {
            var _resolve = self;
            while (_resolve != null)
            {
                // resolve icon
                var _icon = _resolve.GetIconVisual(key, iconRef);
                if (_icon != null)
                {
                    return _icon;
                }
                _resolve = _resolve.IResolveIconParent;
            }
            return null;
        }

        public static Material ResolveIconMaterial(this IResolveIcon self, string key, IIconReference iconRef, IconDetailLevel detailLevel)
        {
            var _resolve = self;
            while (_resolve != null)
            {
                // resolve icon
                var _icon = _resolve.GetIconMaterial(key, iconRef, detailLevel);
                if (_icon != null)
                {
                    return _icon;
                }
                _resolve = _resolve.IResolveIconParent;
            }
            return null;
        }
    }
}
