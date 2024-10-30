using Android.AccessibilityServices;
using Android.Views.Accessibility;
using Android.Util;
using Android.App;
using Android.Content;
using Android.Provider;
using Android.Text;
using System.Collections.Generic;
using Java.Lang;
using Help.ui;

[Service(Label = "Searcher", Permission = "android.permission.BIND_ACCESSIBILITY_SERVICE")]
[IntentFilter(new[] { "android.accessibilityservice.AccessibilityService" })]
[MetaData("android.accessibilityservice", Resource = "@xml/accessibility_service_config")]
public class Searcher : AccessibilityService
{
    private const string Tag = "SearcherService"; // name of the service
    private static List<AccessibilityNodeInfo> ScreenElements = new List<AccessibilityNodeInfo>();
    private string appPackageName;
    public static List<string> InfoAboutNodes = new List<string>();

    // Gets all the elements of the screen (non-static)
    public List<AccessibilityNodeInfo> GetScreenElements()
    {
        return new List<AccessibilityNodeInfo>(ScreenElements); // Devuelve una copia de la lista
    }
    public static List<string> GetInfoAboutNodes()
    {
        return new List<string>(InfoAboutNodes);
    }
    // Gets all the elements of the screen (static)
    public static List<AccessibilityNodeInfo> GetScreenElementsStatic()
    {
        return new List<AccessibilityNodeInfo>(ScreenElements); // Devuelve una copia de los elementos en pantalla
    }

    //  Cheks if the service is active
    public static bool IsAccessibilityServiceEnabled(Context context, Class accessibilityServiceClass)
    {
        int accessibilityEnabled = 0;
        string service = context.PackageName + "/" + accessibilityServiceClass.Name;

        try
        {
            accessibilityEnabled = Settings.Secure.GetInt(
                context.ContentResolver,
                Settings.Secure.AccessibilityEnabled
            );
        }
        catch (Settings.SettingNotFoundException e)
        {
            Log.Warn(Tag, "Error al buscar configuraciones de accesibilidad: " + e.Message);
            return false;
        }

        TextUtils.SimpleStringSplitter colonSplitter = new TextUtils.SimpleStringSplitter(':');
        if (accessibilityEnabled == 1)
        {
            string settingValue = Settings.Secure.GetString(
                context.ContentResolver,
                Settings.Secure.EnabledAccessibilityServices
            );

            if (settingValue != null)
            {
                colonSplitter.SetString(settingValue);
                while (colonSplitter.HasNext)
                {
                    string componentName = colonSplitter.Next();
                    if (componentName.Equals(service, System.StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    // Accesibilty event, triggers whenever the UI changees  
    public override void OnAccessibilityEvent(AccessibilityEvent e)
    { 
        //Log.Info(Tag, "Accessibility Event Received: " + e.EventType);
        string eventPackageName = e.PackageName?.ToString();
        if (eventPackageName == "com.companyname.help.ui")
        {
            Console.WriteLine("el usuario se encuentra en nuestra aplicacion ");
            return;
        }
        //Log.Info(Tag, $"Nombre de la aplicacion  {eventPackageName}");
        var source = e.Source;

        if (source != null)
        {
            ScreenElements.Clear(); 
            ExploreNodeInfo(source); // Explores the elements of the source, the UI app

            if (ScreenElements.Count == 0)
            {
                //Log.Info(Tag, "No se encontraron elementos de accesibilidad en la pantalla actual.");
            }
            else
            {
                InfoAboutNodes.Clear();
                //Log.Info(Tag, $"Se encontraron {ScreenElements.Count} elementos en la pantalla.");
                foreach (var item in ScreenElements)
                {
                    /*
                     * var Text= item.Text;
                    var Description = item.ContentDescription;
                    var TypeElement = item.ClassName;
                    var Cords = item.GetBoundsInScreen;
                    var Enabled = item.Enabled;
                    var Clickable = item.Clickable;
                    var Focusable = item.Focusable;
                    var Selected = item.Selected;
                    var Scrollable = item.Scrollable;
                    var Actions = item.ActionList;
                    var HintText = item.HintText;
                     */
                    var NodeInfo = item.ToString();
                    //Console.WriteLine($" {NodeInfo} ");
                    InfoAboutNodes.Add( NodeInfo );
                }
            }

            source?.Recycle();         
        }
        else
        {
            Log.Warn(Tag, "No se pudo acceder a la fuente del evento de accesibilidad.");
        }
    }

    // Explores the UI in a recursive way 
    private void ExploreNodeInfo(AccessibilityNodeInfo node)
    {
        if (node == null) return;

        // filters nodes of this application
        string nodePackageName = node.PackageName?.ToString();
        if (!string.IsNullOrEmpty(nodePackageName) && !nodePackageName.Equals(appPackageName, StringComparison.OrdinalIgnoreCase))
        {
            ScreenElements.Add(node);
        }

        // searchs for all the son nodes
        for (int i = 0; i < node.ChildCount; i++)
        {
            var child = node.GetChild(i);
            ExploreNodeInfo(child);
            child?.Recycle(); 
        }
    }

    //Whenever the service its interrupted
    public override void OnInterrupt()
    {
        Log.Warn(Tag, "Accessibility Service Interrumpido");
    }

    // Executes when the service connects
    protected override void OnServiceConnected()
    {   
        base.OnServiceConnected();
        appPackageName = PackageName; // Obtener el nombre del paquete de la aplicación
        AccessibilityServiceInfo info = new AccessibilityServiceInfo
        {
            EventTypes = EventTypes.WindowStateChanged | EventTypes.WindowContentChanged, // captures only those events
            FeedbackType = FeedbackFlags.Spoken,
            NotificationTimeout = 100,
            Flags = AccessibilityServiceFlags.Default | AccessibilityServiceFlags.IncludeNotImportantViews // Incluye vistas no importantes
        };
        SetServiceInfo(info);
        Log.Info(Tag, "Accessibility Service Connected");
    }
}
