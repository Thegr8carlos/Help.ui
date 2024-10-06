using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Runtime;

[Service(Exported = true)]
public class FloatingButtonService : Service
{
    private IWindowManager? _windowManager; // Cambiado a IWindowManager y acepta null
    private Android.Views.View? _floatingButton;


    public override IBinder? OnBind(Intent intent) // Ajuste para permitir null en el valor retornado
    {
        return null; // Retorna null ya que no necesitamos un IBinder para este servicio
    }



    public override void OnCreate()
    {
        base.OnCreate();



        // Verificar si tenemos permiso para superposiciones
        if (!Android.Provider.Settings.CanDrawOverlays(this))
        {
            Intent intent = new Intent(Android.Provider.Settings.ActionManageOverlayPermission,
                                       Android.Net.Uri.Parse("package:" + PackageName));
            intent.AddFlags(ActivityFlags.NewTask); // Añadir FLAG_ACTIVITY_NEW_TASK
            StartActivity(intent);
            return; // No seguimos si no tenemos el permiso
        }   



        // Obtener el WindowManager para manejar las vistas en pantalla
        _windowManager = GetSystemService(Context.WindowService)?.JavaCast<IWindowManager>();

        if (_windowManager == null)
        {
            throw new InvalidOperationException("No se pudo obtener el WindowManager");
        }

        // Obtener el LayoutInflater
        var inflater = LayoutInflater.From(this);
        if (inflater == null)
        {
            throw new InvalidOperationException("No se pudo obtener el LayoutInflater");
        }

        // Crear el botón flotante desde un LayoutInflater
        _floatingButton = inflater.Inflate(Help.ui.Resource.Layout.floating_button, null);

        if (_floatingButton == null)
        {
            throw new InvalidOperationException("No se pudo crear el botón flotante");
        }

        // Verificar si el nivel de API es al menos 26 (Android Oreo)
        WindowManagerTypes layoutType;
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            layoutType = WindowManagerTypes.ApplicationOverlay; // Compatible con Android 26+
        }
        else
        {
            layoutType = WindowManagerTypes.SystemAlert; // Para versiones anteriores
        }

        // Configurar los parámetros del botón flotante
        var layoutParams = new WindowManagerLayoutParams(
            WindowManagerLayoutParams.WrapContent,
            WindowManagerLayoutParams.WrapContent,
            layoutType, // Usar el tipo determinado según el nivel de API
            WindowManagerFlags.NotFocusable,
            Format.Translucent)
        {
            Gravity = GravityFlags.Center | GravityFlags.Top // Puedes ajustar la posición como prefieras
        };

        // Añadir el botón flotante al WindowManager
        _windowManager.AddView(_floatingButton, layoutParams);

        // Añadir evento al botón
        _floatingButton.Click += (sender, args) =>
        {
            if (this != null)
            {
                Console.WriteLine("El botón flotante ha sido presionado");
            }
        };
    }


    
    public override void OnDestroy()
    {
        base.OnDestroy();
        // Remover el botón flotante al destruir el servicio
        if (_floatingButton != null)
        {
            _windowManager?.RemoveView(_floatingButton);
        }
    }
}
