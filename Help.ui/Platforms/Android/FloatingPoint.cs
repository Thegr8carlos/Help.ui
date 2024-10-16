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
    private IWindowManager? _windowManager;
    private Android.Views.View? _floatingButton;
    private Android.Views.View? _menuView;
    private WindowManagerLayoutParams? _layoutParams;
    private WindowManagerLayoutParams? _menuLayoutParams;
    private FloatingButtonTouchListener? _floatingButtonTouchListener;

    public override IBinder? OnBind(Intent intent)
    {
        return null;
    }

    public override void OnCreate()
    {
        base.OnCreate();

        // Verificar permisos
        if (!Android.Provider.Settings.CanDrawOverlays(this))
        {
            Intent intent = new Intent(Android.Provider.Settings.ActionManageOverlayPermission,
                                       Android.Net.Uri.Parse("package:" + PackageName));
            intent.AddFlags(ActivityFlags.NewTask);
            StartActivity(intent);
            return;
        }

        // Inicializar WindowManager
        _windowManager = GetSystemService(Context.WindowService)?.JavaCast<IWindowManager>();

        if (_windowManager == null)
        {
            throw new InvalidOperationException("No se pudo obtener el WindowManager");
        }

        // Inflar el botón flotante
        var inflater = LayoutInflater.From(this);
        _floatingButton = inflater.Inflate(Help.ui.Resource.Layout.floating_button, null);

        if (_floatingButton == null)
        {
            throw new InvalidOperationException("No se pudo crear el botón flotante");
        }

        // Inflar la vista del menú
        _menuView = inflater.Inflate(Help.ui.Resource.Layout.AssistantMenu, null);

        if (_menuView == null)
        {
            throw new InvalidOperationException("No se pudo crear el menú");
        }

        // Configurar el tipo de ventana basado en la versión de Android
        WindowManagerTypes layoutType;
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            layoutType = WindowManagerTypes.ApplicationOverlay;
        }
        else
        {
            layoutType = WindowManagerTypes.SystemAlert;
        }

        // Configurar los parámetros del botón flotante
        _layoutParams = new WindowManagerLayoutParams(
            WindowManagerLayoutParams.WrapContent,
            WindowManagerLayoutParams.WrapContent,
            layoutType,
            WindowManagerFlags.NotFocusable,
            Format.Translucent)
        {
            Gravity = GravityFlags.Center | GravityFlags.Right,
            X = 0,
            Y = 0
        };

        // Configurar los parámetros del menú
        _menuLayoutParams = new WindowManagerLayoutParams(
            WindowManagerLayoutParams.MatchParent,
            WindowManagerLayoutParams.MatchParent,
            layoutType,
            WindowManagerFlags.WatchOutsideTouch,
            Format.Translucent)
        {
            Gravity = GravityFlags.Fill,
            X = 0,
            Y = 0
        };


        // Agregar el botón flotante a la ventana
        _windowManager.AddView(_floatingButton, _layoutParams);

        // Inicializar el listener de toque y configurarlo en el botón flotante
        _floatingButtonTouchListener = new FloatingButtonTouchListener(this, _layoutParams, _windowManager, _floatingButton);
        _floatingButton.SetOnTouchListener(_floatingButtonTouchListener);

        // Agregar funcionalidad al clic del botón flotante
        _floatingButton.Click += (sender, args) =>
        {
            OnFloatingButtonClick();
        };
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        // Remover el botón flotante
        if (_floatingButton != null)
        {
            try
            {
                _windowManager?.RemoveView(_floatingButton);
            }
            catch (Java.Lang.IllegalArgumentException e)
            {
                Console.WriteLine("El botón flotante ya ha sido removido.");
            }
        }

        // Remover el menú si está visible
        if (_menuView != null)
        {
            try
            {
                _windowManager?.RemoveView(_menuView);
            }
            catch (Java.Lang.IllegalArgumentException e)
            {
                Console.WriteLine("El menú ya ha sido removido.");
            }
        }
    }


    private void OnFloatingButtonClick()
    {
        // Remover el botón flotante
        if (_floatingButton != null)
        {
            _windowManager?.RemoveView(_floatingButton);
        }

        // Agregar la vista del menú
        if (_menuView != null)
        {
            _windowManager.AddView(_menuView, _menuLayoutParams);

            var button1 = _menuView.FindViewById<Android.Widget.Button>(_menuView.Context.Resources.GetIdentifier("button1", "id", _menuView.Context.PackageName));
            var button2 = _menuView.FindViewById<Android.Widget.Button>(_menuView.Context.Resources.GetIdentifier("button2", "id", _menuView.Context.PackageName));

            if (button1 != null)
            {
                button1.Click += (s, e) => OnButton1Click(s as Android.Views.View);
            }

            if (button2 != null)
            {
                button2.Click += (s, e) => OnButton2Click(s as Android.Views.View);
            }

            // Listener para detectar toques fuera del menú
            _menuView.SetOnTouchListener(new MenuTouchListener(this, _windowManager, _menuView, _floatingButton, _layoutParams));
        }
    }

    public void OnButton1Click(Android.Views.View view)
    {
        Console.WriteLine("Se hara la captura o toma de la informacion");

    }

    public void OnButton2Click(Android.Views.View view)
    {
        Console.WriteLine("Se hara la grabacion de audio y obtencion de la respuesta");
    }
}

// Clase para manejar los eventos táctiles en el botón flotante
public class FloatingButtonTouchListener : Java.Lang.Object, Android.Views.View.IOnTouchListener
{
    private WindowManagerLayoutParams _layoutParams;
    private IWindowManager _windowManager;
    private Android.Views.View _floatingButton;
    private int initialY, touchY;
    private readonly Context _context;
    private long startClickTime;
    private static readonly int MAX_CLICK_DURATION = 200; // Duración máxima para considerar un clic

    // Constructor
    public FloatingButtonTouchListener(Context context, WindowManagerLayoutParams layoutParams, IWindowManager windowManager, Android.Views.View floatingButton)
    {
        _layoutParams = layoutParams;
        _windowManager = windowManager;
        _floatingButton = floatingButton;
        _context = context;
    }

    // Evento OnTouch
    public bool OnTouch(Android.Views.View v, MotionEvent e)
    {
        switch (e.Action)
        {
            case MotionEventActions.Down:
                startClickTime = Java.Lang.JavaSystem.CurrentTimeMillis(); // Registrar el tiempo del toque
                initialY = _layoutParams.Y;
                touchY = (int)e.RawY;
                return true;

            case MotionEventActions.Move:
                int newY = initialY + (int)e.RawY - touchY;
                _layoutParams.Y = newY;
                _windowManager.UpdateViewLayout(_floatingButton, _layoutParams);
                return true;

            case MotionEventActions.Up:
                long clickDuration = Java.Lang.JavaSystem.CurrentTimeMillis() - startClickTime;
                if (clickDuration < MAX_CLICK_DURATION)
                {
                    // Clic rápido -> evento de clic
                    v.PerformClick(); // Activa la funcionalidad de clic
                }
                return true;
        }
        return false;
    }
}

public class MenuTouchListener : Java.Lang.Object, Android.Views.View.IOnTouchListener
{
    private Context _context;
    private IWindowManager _windowManager;
    private Android.Views.View _menuView;
    private Android.Views.View _floatingButton;
    private WindowManagerLayoutParams _floatingButtonLayoutParams;

    public MenuTouchListener(Context context, IWindowManager windowManager, Android.Views.View menuView, Android.Views.View floatingButton, WindowManagerLayoutParams floatingButtonLayoutParams)
    {
        _context = context;
        _windowManager = windowManager;
        _menuView = menuView;
        _floatingButton = floatingButton;
        _floatingButtonLayoutParams = floatingButtonLayoutParams;
    }

    public bool OnTouch(Android.Views.View v, MotionEvent e)
    {
        if (e.Action == MotionEventActions.Down)
        {
            // Crear un rectángulo para obtener el área visible del menú
            Android.Graphics.Rect menuRect = new Android.Graphics.Rect();
            _menuView.GetGlobalVisibleRect(menuRect);

            int minX = 266;
            int maxX = 800;
            int minY = 992;
            int maxY = 1401;

            // Obtener las coordenadas del toque
            int touchX = (int)e.RawX;
            int touchY = (int)e.RawY;

            // Comprobar si está fuera del rectángulo
            if (touchX < minX || touchX > maxX || touchY < minY || touchY > maxY)
            {
                // Remover el menú y agregar el botón flotante nuevamente
                _windowManager.RemoveView(_menuView);
                _windowManager.AddView(_floatingButton, _floatingButtonLayoutParams);
                return true;
            }
        }
        return false;
    }
}

