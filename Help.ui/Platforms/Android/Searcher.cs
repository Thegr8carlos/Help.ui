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
    private const string Tag = "SearcherService";

    // Lista estática para almacenar los elementos actuales de la pantalla
    private static List<AccessibilityNodeInfo> ScreenElements = new List<AccessibilityNodeInfo>();

    // Método público para obtener los elementos de la pantalla (no estático)
    public List<AccessibilityNodeInfo> GetScreenElements()
    {
        return new List<AccessibilityNodeInfo>(ScreenElements); // Devuelve una copia de la lista
    }

    // Método estático para obtener los elementos de la pantalla
    public static List<AccessibilityNodeInfo> GetScreenElementsStatic()
    {
        return new List<AccessibilityNodeInfo>(ScreenElements); // Devuelve una copia de los elementos en pantalla
    }

    // Método público para verificar si el servicio de accesibilidad está habilitado
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

    // Evento de accesibilidad que se dispara cuando hay cambios en la UI
    public override void OnAccessibilityEvent(AccessibilityEvent e)
    {
        Log.Info(Tag, "Accessibility Event Received: " + e.EventType);
        var source = e.Source;

        if (source != null)
        {
            ScreenElements.Clear(); // Limpiar la lista para el nuevo evento
            ExploreNodeInfo(source); // Capturar los nuevos elementos de la pantalla

            if (ScreenElements.Count == 0)
            {
                Log.Info(Tag, "No se encontraron elementos de accesibilidad en la pantalla actual.");
            }
            else
            {
                Log.Info(Tag, $"Se encontraron {ScreenElements.Count} elementos en la pantalla.");
            }
        }
        else
        {
            Log.Warn(Tag, "No se pudo acceder a la fuente del evento de accesibilidad.");
        }
    }

    // Método para explorar la jerarquía de la UI de manera recursiva
    private void ExploreNodeInfo(AccessibilityNodeInfo node)
    {
        if (node == null) return;

        // Añadir el nodo actual a la lista de elementos
        ScreenElements.Add(node);

        // Recursivamente buscar los hijos de este nodo
        for (int i = 0; i < node.ChildCount; i++)
        {
            var child = node.GetChild(i);
            ExploreNodeInfo(child); // Llamada recursiva para explorar en profundidad
        }
    }

    // Se ejecuta cuando el servicio es interrumpido
    public override void OnInterrupt()
    {
        Log.Warn(Tag, "Accessibility Service Interrupted");
    }

    // Se ejecuta cuando el servicio de accesibilidad se conecta
    protected override void OnServiceConnected()
    {
        AccessibilityServiceInfo info = new AccessibilityServiceInfo
        {
            EventTypes = EventTypes.AllMask, // Capturar todos los tipos de eventos
            FeedbackType = FeedbackFlags.Spoken,
            NotificationTimeout = 100,
            Flags = AccessibilityServiceFlags.Default | AccessibilityServiceFlags.IncludeNotImportantViews // Incluye vistas no importantes
        };
        SetServiceInfo(info);
        Log.Info(Tag, "Accessibility Service Connected");
    }
}
