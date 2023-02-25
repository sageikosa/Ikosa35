using System;
using System.Windows.Markup;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class MoveParameters
    {
        public int Heading { get; set; }
        public int UpDownAdjust { get; set; }
    }

    [MarkupExtensionReturnType(typeof(MoveParameters))]
    public class MoveParam : MarkupExtension
    {
        public int Heading { get; set; }
        public int UpDownAdjust { get; set; }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new MoveParameters { Heading = Heading, UpDownAdjust = UpDownAdjust };
        }
    }
}
