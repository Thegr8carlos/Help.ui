using Android.AccessibilityServices;
using Android.Views.Accessibility;
using Android.Util;
using Android.App;

namespace Help.ui
{
    [Service(Label = "Searcher", Permission = "android.permission.BIND_ACCESSIBILITY_SERVICE")]
    [IntentFilter(new[] { "android.accessibilityservice.AccessibilityService" })]
    [MetaData("android.accessibilityservice", Resource = "@xml/accessibility_service_config")]
    public class Searcher : AccessibilityService
    {
        private const string Tag = "SearcherService";

        public override void OnAccessibilityEvent(AccessibilityEvent e)
        {
            Log.Info(Tag, "Accessibility Event Received: " + e.EventType);
            var source = e.Source;
            if (source != null)
            {
                Log.Info(Tag, "Content description: " + source.ContentDescription);
                Log.Info(Tag, "Class name: " + source.ClassName);
            }
        }

        public override void OnInterrupt()
        {
            Log.Warn(Tag, "Accessibility Service Interrupted");
        }

        protected override void OnServiceConnected()
        {
            AccessibilityServiceInfo info = new AccessibilityServiceInfo
            {
                EventTypes = EventTypes.AllMask,
                FeedbackType = FeedbackFlags.Spoken,
                NotificationTimeout = 100,
                Flags = AccessibilityServiceFlags.Default
            };
            SetServiceInfo(info);
            Log.Info(Tag, "Accessibility Service Connected");
        }
    }
}
