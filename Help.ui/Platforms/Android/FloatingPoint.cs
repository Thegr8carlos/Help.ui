using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Runtime;
using Help.ui;
using Android.Content.PM;
using System.Linq;
using Android.Views.Accessibility;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;




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
            var button2 = _menuView.FindViewById<Android.Widget.Button>(_menuView.Context.Resources.GetIdentifier("button_combined", "id", _menuView.Context.PackageName));

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
        List<string> actionElements = new List<string>();
        string[] elements = text.Split(';');

        foreach (var element in elements)
        {
            // Filtrar elementos relevantes para información general
            if ((element.Contains("className") || element.Contains("text") || element.Contains("contentDescription") || element.Contains("boundsInScreen") || element.Contains("Appname"))
                && !element.Contains(": null") && !element.Contains(": false"))
            {
                filteredElements.Add(element);
            }

            // Filtrar específicamente las acciones
            if (element.Contains("actions: [") && !element.Contains(": null"))  
            {
                actionElements.Add(element.Split("actions: [")[1].Split(']')[0]); // Extraer la lista de acciones
            }
        }

        // Combinar las listas para ver la información general y las acciones
        string result = string.Join(";", filteredElements) + "\nAcciones: " + string.Join(", ", actionElements);
        return result;
    }
    public void OpenAppByPackageName(Context context, string packageName)
    {
        try
        {
            // Obtener el intent de lanzamiento para el paquete
            Intent launchIntent = context.PackageManager.GetLaunchIntentForPackage(packageName);
            if (launchIntent != null)
            {
                // Agregar flags si se llama desde un servicio
                launchIntent.AddFlags(ActivityFlags.NewTask);
                context.StartActivity(launchIntent);
            }
            else
            {
                Console.WriteLine($"No se encontró un intent para la aplicación: {packageName}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error al intentar abrir la aplicación: " + ex.Message);
        }
    }

    public async void OnButton1Click(Android.Views.View view)
    {
        Console.WriteLine("Se hará la captura o toma de la información");
        Console.WriteLine("Capturando los elementos de la pantalla...");

        if (Searcher.IsAccessibilityServiceEnabled(this, Java.Lang.Class.FromType(typeof(Searcher))))
        {
            Console.WriteLine("El servicio de accesibilidad está habilitado.");
            List<AccessibilityNodeInfo> contextString;
            lock (Searcher.InfoAboutNodes)
            {
                contextString = Searcher.GetScreenElementsStatic();
            }
            //string context = "";
            Console.WriteLine("ELEMENTOS sin limpieza  de la aplicacion ");
            Console.WriteLine(Searcher.appPackageName);
            foreach (var element in contextString)
            {
                Console.WriteLine(element);
                //context += ProcessText(element); // Limpiar o procesar el texto
            }


            string userMessage = $"Hola, que se puede hacer con la siguiente aplicacion {Searcher.appPackageName}";

            // calls the huggin face api 
            string apiResponse = await HuggingFaceAPI.SendRequestToHuggingFace(userMessage);

            // shows the response 
            Console.WriteLine("Respuesta de la API: " + apiResponse);

            //Console.WriteLine(context);
            //Console.WriteLine($"Tamaño del contexto: {context.Length}");


            //Console.WriteLine(AppName);




            //string packageName = "com.google.android.youtube"; // Nombre del paquete de YouTube
            //Console.WriteLine($"Intentando abrir la aplicación: {packageName}");

            //OpenAppByPackageName(this, packageName);



            //var installedApps = InstalledApps.GetInstalledApps(this);


            //installedApps.ForEach(appName => Console.WriteLine(appName));

            //Console.WriteLine($"{installedApps.Count} aplicaciones están instaladas.");

        }
        else
        {
            Console.WriteLine("El servicio de accesibilidad NO está habilitado.");

            // Abre la configuración de accesibilidad para que el usuario lo habilite
            Intent intent = new Intent(Android.Provider.Settings.ActionAccessibilitySettings);
            intent.AddFlags(ActivityFlags.NewTask);
            this.StartActivity(intent);
        }
    }

    public void OnButton2Click(Android.Views.View view)
    {
        Console.WriteLine("Se hara la grabacion de audio y obtencion de la respuesta");
        int num = new Random().Next(0, 6);
        SearcherActions.PerformGlobalActionStatic(num);
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

public static class HuggingFaceAPI
{
    private const string ApiUrl = "https://api-inference.huggingface.co/models/google/gemma-2-2b-it/v1/chat/completions";
    private const string ApiKey = ""; // replace this with the api key 

    public static async Task<string> SendRequestToHuggingFace(string userMessage)
    {
        var requestBody = new
        {
            model = "google/gemma-2-2b-it",
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = userMessage
                }
            },
            max_tokens = 500,
            stream = true
        };

        string requestBodyJson = JsonConvert.SerializeObject(requestBody);

        try
        {
            using (HttpClient client = new HttpClient())
            {
                // header with the huggin face key 
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");

                // creates the content based on the json and sets the name 
                StringContent content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json");

                // sednd POST request 
                HttpResponseMessage response = await client.PostAsync(ApiUrl, content);

                // checks if the response was succesfull
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    return responseContent; // returns the api response 
                }
                else
                {
                    return $"Error: {response.StatusCode} - {response.ReasonPhrase}";
                }
            }
        }
        catch (Exception ex)
        {
            return $"Error al realizar la solicitud: {ex.Message}";
        }
    }
}

public class InstalledApps
{
    public static List<string> GetInstalledApps(Context context)
    {
        var packageManager = context.PackageManager;
        var packages = packageManager.GetInstalledPackages(PackageInfoFlags.MetaData);

        return packages.Select(pkg => pkg.ApplicationInfo.LoadLabel(packageManager).ToString()).ToList();
    }
}




