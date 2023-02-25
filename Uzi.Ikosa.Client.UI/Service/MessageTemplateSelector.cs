using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Uzi.Ikosa.Contracts.Host;

namespace Uzi.Ikosa.Client.UI
{
    public class MessageTemplateSelector : DataTemplateSelector
    {
        public string UserName { get; set; }
        public DataTemplate OutboundTemplate { get; set; }
        public DataTemplate OutboundPublicTemplate { get; set; }
        public DataTemplate InboundTemplate { get; set; }
        public DataTemplate InboundPublicTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var _msg = item as UserMessage;
            if (_msg != null)
            {
                if (_msg.FromUser.Equals(UserName, StringComparison.OrdinalIgnoreCase))
                {
                    return _msg.IsPublic ? OutboundPublicTemplate : OutboundTemplate;
                }
                else
                    return _msg.IsPublic ? InboundPublicTemplate : InboundTemplate;
            }
            return null;
        }
    }
}
