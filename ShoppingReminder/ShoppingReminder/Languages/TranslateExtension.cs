using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShoppingReminder.Languages
{
    [ContentProperty("Text")]
    public class TranslateExtension : IMarkupExtension
    {
        readonly CultureInfo ci;
        const string ResourceId = "ShoppingReminder.Languages.Resource";

        public TranslateExtension()
        {
            ci = DependencyService.Get<ILocalize>().GetCurrentCultureInfo();
        }

        public string Before { get; set; }
        public string Text { get; set; }
        public string After { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Text == null)
                return "";

            ResourceManager resmgr = new ResourceManager(ResourceId,
                        typeof(TranslateExtension).GetTypeInfo().Assembly);

            var translation = string.Empty;
            if (Before != null)
                translation += Before;
            translation += resmgr.GetString(Text, ci);
            if (string.IsNullOrEmpty(translation))
            {
                translation = Text;
            }
            if(After!=null)
                translation += After;
            return translation;
        }
    }
}
