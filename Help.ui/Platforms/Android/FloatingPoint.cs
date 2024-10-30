using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Runtime;
using Help.ui;
using Help.ui.Platforms.Android;

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

        // Checks for permissions
        if (!Android.Provider.Settings.CanDrawOverlays(this))
        {
            Intent intent = new Intent(Android.Provider.Settings.ActionManageOverlayPermission,
                                       Android.Net.Uri.Parse("package:" + PackageName));
            intent.AddFlags(ActivityFlags.NewTask);
            StartActivity(intent);
            return;
        }

        // Inits WindowManager that handles the floatingButton and the menu
        _windowManager = GetSystemService(Context.WindowService)?.JavaCast<IWindowManager>();

        if (_windowManager == null)
        {
            throw new InvalidOperationException("No se pudo obtener el WindowManager");
        }

        // Inflates the floating button i.e. fills the Layer from an xml file and returns a view
        var inflater = LayoutInflater.From(this);
        _floatingButton = inflater.Inflate(Help.ui.Resource.Layout.floating_button, null);

        if (_floatingButton == null)
        {
            throw new InvalidOperationException("No se pudo crear el botón flotante");
        }

        // inflates the menu xml and retur the view of the menu
        _menuView = inflater.Inflate(Help.ui.Resource.Layout.AssistantMenu, null);

        if (_menuView == null)
        {
            throw new InvalidOperationException("No se pudo crear el menú");
        }

        // sets the window based on android version  
        WindowManagerTypes layoutType;
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            layoutType = WindowManagerTypes.ApplicationOverlay;
        }
        else
        {
            layoutType = WindowManagerTypes.SystemAlert;
        }

        // Sets floating button parameters of the windowLayout that handles the fb
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

        // Sets menu parameters of his layout 
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


        // Adds floating point into the wiundow manager 
        _windowManager.AddView(_floatingButton, _layoutParams);

        //  Inits listener of touch and incorporate into the floating button
        _floatingButtonTouchListener = new FloatingButtonTouchListener(this, _layoutParams, _windowManager, _floatingButton);
        _floatingButton.SetOnTouchListener(_floatingButtonTouchListener);

        // Adds click functionality of floating button
        _floatingButton.Click += (sender, args) =>
        {
            OnFloatingButtonClick();
        };
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        // Removes floating button
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

        // Removes menu if it's visible
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
        // Removes floating button
        if (_floatingButton != null)
        {
            _windowManager?.RemoveView(_floatingButton);
        }

        // Adds menu view in window manager
        if (_menuView != null)
        {
            _windowManager.AddView(_menuView, _menuLayoutParams);
            // adds manually the two buttons.
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

            // Listener of out of menu clicks  
            _menuView.SetOnTouchListener(new MenuTouchListener(this, _windowManager, _menuView, _floatingButton, _layoutParams));
        }
    }

    public string ProcessText(string text)
    {
        List<string> filteredElements = new List<string>();
        string[] elements = text.Split(';');

        foreach (var element in elements)
        {
            // Condición corregida
            if ((element.Contains("null") || element.Contains("packageName") || element.Contains("NodeInfo") || element.Contains("boundsInParent") || element.Contains("boundsInWindow") || element.Contains("false") || element.Contains("-1")) && !element.Contains('['))
            {
                continue;
            }
            else
            {
                filteredElements.Add(element);
            }
        }
        string result = string.Join(";", filteredElements);
        return result;

    }

    // function whenever the user wants an explication of his active screen, must send the information, recive it and display it or play the audio response
    public void OnButton1Click(Android.Views.View view)
    {
        Console.WriteLine("Se hara la captura o toma de la informacion");
        Console.WriteLine("Capturando los elementos de la pantalla...");

        if (Searcher.IsAccessibilityServiceEnabled(this, Java.Lang.Class.FromType(typeof(Searcher))))
        {
            Console.WriteLine("El servicio de accesibilidad está habilitado.");
            List<string> contextString;
            lock (Searcher.InfoAboutNodes)
            {
                contextString = Searcher.GetInfoAboutNodes();
            }
            string Context = "";
            Console.WriteLine("ELEMENTOS CON LIMPIEZA");
            foreach (var element in contextString)
            {
                //Console.WriteLine(element);
                Context += ProcessText(element);
            }
            Console.WriteLine(Context);
            Console.WriteLine(Context.Length);
        }
        else
        {
            Console.WriteLine("El servicio de accesibilidad NO está habilitado.");
            // moves the user to settings... (i think i need to make in the oncreate constructor of floating button but im not sure at all)
            Intent intent = new Intent(Android.Provider.Settings.ActionAccessibilitySettings);
            intent.AddFlags(ActivityFlags.NewTask);
            this.StartActivity(intent);
        }
    }

    public void OnButton2Click(Android.Views.View view)
    {
        Console.WriteLine("Se hara la grabacion de audio y obtencion de la respuesta");
    }
}

// handles tactil events of the floating button
public class FloatingButtonTouchListener : Java.Lang.Object, Android.Views.View.IOnTouchListener
{
    private WindowManagerLayoutParams _layoutParams;
    private IWindowManager _windowManager;
    private Android.Views.View _floatingButton;
    private int initialY, touchY;
    private readonly Context _context;
    private long startClickTime;
    private static readonly int MAX_CLICK_DURATION = 200; // max clic duration 

    // Constructor
    public FloatingButtonTouchListener(Context context, WindowManagerLayoutParams layoutParams, IWindowManager windowManager, Android.Views.View floatingButton)
    {
        _layoutParams = layoutParams;
        _windowManager = windowManager;
        _floatingButton = floatingButton;
        _context = context;
    }

    // OnTouch
    public bool OnTouch(Android.Views.View v, MotionEvent e)
    {
        switch (e.Action)
        {
            case MotionEventActions.Down:
                startClickTime = Java.Lang.JavaSystem.CurrentTimeMillis(); // Time register when the button it's clicked 
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
                    // fast click -> click event
                    v.PerformClick(); // sets click event
                }
                return true;
        }
        return false;
    }
}

// menu handler... (need to hanle better this...) 
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
            
            Android.Graphics.Rect menuRect = new Android.Graphics.Rect();
            _menuView.GetGlobalVisibleRect(menuRect);

            int minX = 266;
            int maxX = 800;
            int minY = 992;
            int maxY = 1401;

            
            int touchX = (int)e.RawX;
            int touchY = (int)e.RawY;

            
            if (touchX < minX || touchX > maxX || touchY < minY || touchY > maxY)
            {
                _windowManager.RemoveView(_menuView);
                _windowManager.AddView(_floatingButton, _floatingButtonLayoutParams);
                return true;
            }
        }
        return false;
    }
}

