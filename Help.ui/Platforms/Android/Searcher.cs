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
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Kotlin.Jvm.Functions;



[Service(Label = "Searcher", Permission = "android.permission.BIND_ACCESSIBILITY_SERVICE")]
[IntentFilter(new[] { "android.accessibilityservice.AccessibilityService" })]
[MetaData("android.accessibilityservice", Resource = "@xml/accessibility_service_config")]
public class Searcher : AccessibilityService
{

    private const string Tag = "SearcherService"; // name of the service
    private static List<AccessibilityNodeInfo> ScreenElements = new List<AccessibilityNodeInfo>();
    public  static string appPackageName = "";
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
        appPackageName = eventPackageName;
        if (source != null)
        {
            // Ejecutar el procesamiento en una tarea asíncrona
            Task.Run(() =>
            {
                // Bloque de código que se ejecutará en segundo plano
                ScreenElements.Clear();
                ExploreNodeInfo(source); // Explora los elementos de la fuente, la aplicación de UI

            if (ScreenElements.Count == 0)
            {
                // Log.Info(Tag, "No se encontraron elementos de accesibilidad en la pantalla actual.");
            }
            else
            {
                InfoAboutNodes.Clear();
                InfoAboutNodes.Add("Appname : " + eventPackageName + ";");
                    foreach (var item in ScreenElements)
                    {
                        lock (InfoAboutNodes)
                        {
                            InfoAboutNodes.Add(item.ToString());
                        }
                        // Reciclar el nodo para liberar recursos
                        item.Recycle();
                    }
                }

                source?.Recycle();
            });
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
        
        // code used do perform an action 
        //int test = AccessibilityNodeInfo.AccessibilityAction.ActionClick.Id;
        //bool performed = node.PerformAction(Android.Views.Accessibility.Action.Click);
        //if (performed)
        //{
        //    // succes
        //}
        //else
        //{
        //    // failure
        //}

        // filters nodes of this application
        string nodePackageName = node.PackageName?.ToString();
        if (!string.IsNullOrEmpty(nodePackageName) && !nodePackageName.Equals(appPackageName, StringComparison.OrdinalIgnoreCase))
        {
            if( node.Clickable)
            {
                ScreenElements.Add(node);
            } 
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

        SearcherActions.SetInstance(this);

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


public static class SearcherActions
{
    private static Searcher _instance;

    // Guarda la instancia actual del servicio
    public static void SetInstance(Searcher instance)
    {
        _instance = instance;
    }

    // Función estática para realizar acciones globales
    public static void PerformGlobalActionStatic(int action )
    {
        if (_instance == null)
        {
            Log.Warn("SearcherService", "El servicio de accesibilidad no está inicializado.");
            return;
        }
        ;
        bool result = false;
        //bool result = _instance.PerformGlobalAction(Android.AccessibilityServices.GlobalAction.);
        if (action == 0) {  result = _instance.PerformGlobalAction(Android.AccessibilityServices.GlobalAction.Home);    }
        if (action == 1) {  result = _instance.PerformGlobalAction(Android.AccessibilityServices.GlobalAction.Recents); }
        if (action == 2) {  result = _instance.PerformGlobalAction(Android.AccessibilityServices.GlobalAction.Notifications); }
        if (action == 3) {  result = _instance.PerformGlobalAction(Android.AccessibilityServices.GlobalAction.QuickSettings); }
        if (action == 4) {  result = _instance.PerformGlobalAction(Android.AccessibilityServices.GlobalAction.TakeScreenshot);  }
        if (action == 5) { result = _instance.PerformGlobalAction(Android.AccessibilityServices.GlobalAction.AccessibilityAllApps); }
        //-Android.AccessibilityServices.GlobalAction.Home
        //- Android.AccessibilityServices.GlobalAction.Recents
        //- Android.AccessibilityServices.GlobalAction.Notifications
        //- Android.AccessibilityServices.GlobalAction.QuickSettings
        //- Android.AccessibilityServices.GlobalAction.TakeScreenshot
        //- Android.AccessibilityServices.GlobalAction.AccessibilityAllApps



        //if (result)
        //{
        //    Log.Info("SearcherService", $"Acción global back ejecutada con éxito.");
        //}
        //else
        //{
        //    Log.Warn("SearcherService", $"Error al ejecutar la acción global back.");
        //}
    }
}
