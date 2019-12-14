using System.Collections.Specialized;
using System.Configuration;
using System.Linq;

namespace TLO.Tools
{
    public class SettingsProvider : System.Configuration.SettingsProvider, IApplicationSettingsProvider
    {
        private static readonly string[] KnownProperties =
        {
            "WindowSize",
            "WindowLocation",
            "SettingsWindowSize",
            "SettingsWindowLocation",
            "ShowInTray",
            "HideToTray",
            "CloseToTray",
            "NotificationInTray",
            "DontRunCopy",
        };

        public override void Initialize(string name, NameValueCollection config)
        {
            if (string.IsNullOrEmpty(name)) name = nameof(GetType);
            base.Initialize(name, config);
        }

        public override SettingsPropertyValueCollection GetPropertyValues(
            SettingsContext context,
            SettingsPropertyCollection collection
        )
        {
            var result = new SettingsPropertyValueCollection();
            foreach (SettingsProperty property in collection)
            {
                var value = new SettingsPropertyValue(property);
                if (KnownProperties.Contains(property.Name))
                {
                    value.PropertyValue = Settings
                        .Current
                        .GetType()
                        .GetField(value.Property.Name)
                        .GetValue(Settings.Current);
                }

                result.Add(value);
            }

            return result;
        }

        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            foreach (SettingsPropertyValue value in collection)
            {
                if (KnownProperties.Contains(value.Property.Name))
                {
                    Settings
                        .Current
                        .GetType()
                        .GetField(value.Property.Name)
                        .SetValue(Settings.Current, value.PropertyValue);
                }
            }

            Settings.Current.Save();
        }

        public override string ApplicationName { get; set; }

        public SettingsPropertyValue GetPreviousVersion(SettingsContext context, SettingsProperty property)
        {
            return null;
        }

        public void Reset(SettingsContext context)
        {
        }

        public void Upgrade(SettingsContext context, SettingsPropertyCollection properties)
        {
        }
    }
}