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
    private WindowManagerLayoutParams? _layoutParams;
    private FloatingButtonTouchListener? _floatingButtonTouchListener;

    public override IBinder? OnBind(Intent intent)
    {
        return null;
    }

    public override void OnCreate()
    {
        base.OnCreate();

        // checks for permissions
        if (!Android.Provider.Settings.CanDrawOverlays(this))
        {
            Intent intent = new Intent(Android.Provider.Settings.ActionManageOverlayPermission,
                                       Android.Net.Uri.Parse("package:" + PackageName));
            intent.AddFlags(ActivityFlags.NewTask);
            StartActivity(intent);
            return;
        }

        // innits  WindowManager
        _windowManager = GetSystemService(Context.WindowService)?.JavaCast<IWindowManager>();

        if (_windowManager == null)
        {
            throw new InvalidOperationException("No se pudo obtener el WindowManager");
        }

        // inflate floatingButton
        var inflater = LayoutInflater.From(this);
        _floatingButton = inflater.Inflate(Help.ui.Resource.Layout.floating_button, null);

        if (_floatingButton == null)
        {
            throw new InvalidOperationException("No se pudo crear el botón flotante");
        }

        // sets the screen based on api level
        WindowManagerTypes layoutType;
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            layoutType = WindowManagerTypes.ApplicationOverlay;
        }
        else
        {
            layoutType = WindowManagerTypes.SystemAlert;
        }

        //  sets layouts parameters 
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

        // adds flaotingButton to the window
        _windowManager.AddView(_floatingButton, _layoutParams);

        // innits listener class and sets in the floatingButton
        _floatingButtonTouchListener = new FloatingButtonTouchListener(this, _layoutParams, _windowManager, _floatingButton);
        _floatingButton.SetOnTouchListener(_floatingButtonTouchListener);

        // adds click functionality
        _floatingButton.Click += (sender, args) =>
        {
            _floatingButtonTouchListener.OnFloatingButtonClick(); 
        };
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        // removes the floatinButton
        if (_floatingButton != null)
        {
            _windowManager?.RemoveView(_floatingButton);
        }
    }
}

public class FloatingButtonTouchListener : Java.Lang.Object, Android.Views.View.IOnTouchListener
{
    private WindowManagerLayoutParams _layoutParams;
    private IWindowManager _windowManager;
    private Android.Views.View _floatingButton;
    private int initialY, touchY;
    private readonly Context _context;
    private long startClickTime;
    private static readonly int MAX_CLICK_DURATION = 200; // max duration to consider a click

    // Constructor
    public FloatingButtonTouchListener(Context context, WindowManagerLayoutParams layoutParams, IWindowManager windowManager, Android.Views.View floatingButton)
    {
        _layoutParams = layoutParams;
        _windowManager = windowManager;
        _floatingButton = floatingButton;
        _context = context;
    }

    //  (OnTouch) event
    public bool OnTouch(Android.Views.View v, MotionEvent e)
    {
        switch (e.Action)
        {
            case MotionEventActions.Down:
                startClickTime = Java.Lang.JavaSystem.CurrentTimeMillis(); // Register the time the user touchs the button
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
                    v.PerformClick(); // activates the click functionality
                }
                return true;
        }
        return false;
    }

    // click on floatig point
    public void OnFloatingButtonClick()
    {
        Console.WriteLine("Botón flotante presionado");
    }
}









