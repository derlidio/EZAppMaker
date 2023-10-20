/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

using System.Reflection;

using EZAppMaker.Attributes;
using EZAppMaker.Support;

namespace EZAppMaker.Components
{
    public class EZContentView : ContentView
    {
        public static readonly BindableProperty ItemIdProperty = BindableProperty.Create(nameof(ItemId), typeof(string), typeof(EZContentView), null);
        public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(EZContentView), null);
        public static readonly BindableProperty GroupProperty = BindableProperty.Create(nameof(Group), typeof(string), typeof(EZContentView), null);

        public EZContainer Container { get; set; } = null;
        public double ScrollY { get; set; } = 0;

        public delegate void OnMessageHandler(string MessageId, object MessageData, object Sender);
        public event OnMessageHandler OnMessage;

        public delegate void OnReadyHandler();
        public event OnReadyHandler OnReady;

        private double width = -1;
        private double height = -1;

        public EZContentView() : base()
        {
            SizeChanged += OnSizeChanged;
        }

        //  ___      _ _   _      _ _         _   _          
        // |_ _|_ _ (_) |_(_)__ _| (_)_____ _| |_(_)___ _ _  
        //  | || ' \| |  _| / _` | | |_ / _` |  _| / _ \ ' \ 
        // |___|_||_|_|\__|_\__,_|_|_/__\__,_|\__|_\___/_||_|

        private void OnSizeChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"{ItemId}.OnSizeChanged(): [{Math.Floor(Width)}x{Math.Floor(Height)}]");

            if (Initialized)
            {
                if ((width != Width) || (height != Height))
                {
                    width = Width;
                    height = Height;

                    OnPropertyChanged(nameof(IsLandscape));
                    OnPropertyChanged(nameof(IsPortrait));

                    OnPropertyChanged(nameof(AvailableWidth));
                    OnPropertyChanged(nameof(AvailableHeight));
                }

                return;
            }

            Initialized = (Width > 0) && (Height > 0);

            if (Initialized)
            {
                width = Width;
                height = Height;

                OnReady?.Invoke();

                System.Diagnostics.Debug.WriteLine($"{ItemId}.OnReady(): [{Math.Floor(Width)}x{Math.Floor(Height)}]");
            }
        }

        //  ___                       _   _        
        // | _ \_ _ ___ _ __  ___ _ _| |_(_)___ ___
        // |  _/ '_/ _ \ '_ \/ -_) '_|  _| / -_|_-<
        // |_| |_| \___/ .__/\___|_|  \__|_\___/__/
        //             |_|

        public bool Initialized { get; private set; } = false;

        public EZExpander Expander { get; private set; } = null;

        public bool IsLandscape
        {
            get { return EZApp.MainPage.IsLandscape; }
        }

        public bool IsPortrait
        {
            get { return EZApp.MainPage.IsPortrait; }
        }

        public double AvailableHeight
        {
            get
            {
                return EZApp.Container.Height;
            }
        }

        public double AvailableWidth
        {
            get
            {
                return EZApp.Container.Width;
            }
        }

        public string ItemId
        {
            get { return (string)GetValue(ItemIdProperty); }
            set { SetValue(ItemIdProperty, value); }
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public string Group
        {
            get { return (string)GetValue(GroupProperty); }
            set { SetValue(GroupProperty, value); }
        }

        //  _  _          _           _   _            ___             _      
        // | \| |__ ___ _(_)__ _ __ _| |_(_)___ _ _   | __|_ _____ _ _| |_ ___
        // | .` / _` \ V / / _` / _` |  _| / _ \ ' \  | _|\ V / -_) ' \  _(_-<
        // |_|\_\__,_|\_/|_\__, \__,_|\__|_\___/_||_| |___|\_/\___|_||_\__/__/
        //                 |___/                                              

        /// <summary>
        ///  Called when the view appears for the first time. This is a one-time-only event.
        /// </summary>
        [NavigationEventHandler]
        public virtual void OnAppearing()
        {
            if (Expander == null)
            {
                IEnumerable<EZExpander> expanders = this.GetChildren<EZExpander>();

                if (expanders != null)
                {
                    Expander = expanders.FirstOrDefault();
                }
            }

            EZApp.Builder.CurrentViewChanged(this);

            System.Diagnostics.Debug.WriteLine($"EZContentView: OnAppearing -> {ItemId} with {(Expander == null ? "no Expander": "Expander")}");
        }

        /// <summary>
        /// Called every time the view is put on the top of the stack after appearing for the first time.
        /// Not called if OnAppearing has been fired.
        /// </summary>
        [NavigationEventHandler]
        public virtual void OnRaised()
        {
            EZApp.Builder.CurrentViewChanged(this);

            System.Diagnostics.Debug.WriteLine($"EZContentView: OnRaised -> {ItemId}");
        }

        /// <summary>
        /// Called before overlapping the view with another one (by PushContentView or RaiseContentView).
        /// You can return 'false' to cancel the operation that triggered this event and keep the current view on top.
        /// </summary>
        [NavigationEventHandler]
        public virtual bool OnBeforeHiding()
        {
            System.Diagnostics.Debug.WriteLine($"EZContentView: OnBeforeHiding -> {ItemId}");

            return true;
        }

        /// <summary>
        /// Called after the view has been overlapped by another one (it's not the top view anymore, but still alive in the stack).
        /// </summary>
        [NavigationEventHandler]
        public virtual void OnHidden()
        {
            System.Diagnostics.Debug.WriteLine($"EZContentView: OnHidden -> {ItemId}");
        }

        /// <summary>
        /// Called before the view is removed from the stack (and left to GC to collect it).
        /// </summary>
        [NavigationEventHandler]
        public virtual bool OnBeforeLeaving()
        {
            System.Diagnostics.Debug.WriteLine($"EZContentView: OnBeforeLeaving -> {ItemId}");

            return true;
        }

        /// <summary>
        /// Called after the view is removed from the stack (and left to GC to collect it).
        /// </summary>
        public virtual void OnLeaving()
        {
            System.Diagnostics.Debug.WriteLine($"EZContentView: OnLeaving -> {ItemId}");
        }

        //    _               ___ _                _    
        //   /_\  _ __ _ __  / __(_)__ _ _ _  __ _| |___
        //  / _ \| '_ \ '_ \ \__ \ / _` | ' \/ _` | (_-<
        // /_/ \_\ .__/ .__/ |___/_\__, |_||_\__,_|_/__/
        //       |_|  |_|          |___/

        public virtual void ThemeChanged()
        {
            foreach (Element v in EZXamarin.GetChildren<Element>(this))
            {
                MethodInfo method = v.GetType().GetMethod("ThemeChanged");

                method?.Invoke(v, new object[] { });
            }
        }

        //  ___      _    _ _      __  __     _   _            _    
        // | _ \_  _| |__| (_)__  |  \/  |___| |_| |_  ___  __| |___
        // |  _/ || | '_ \ | / _| | |\/| / -_)  _| ' \/ _ \/ _` (_-<
        // |_|  \_,_|_.__/_|_\__| |_|  |_\___|\__|_||_\___/\__,_/__/

        public void ProcessMessage(string MessageId, object MessageData, object Sender)
        {
            OnMessage?.Invoke(MessageId, MessageData, Sender);
        }

        public object GetReferenceTo(string objectName)
        {
            return FindByName(objectName);
        }

        public async Task Expand()
        {
            if (Expander == null) return;

            bool expanded = await Expander.Expand();

            if (expanded)
            {
                System.Diagnostics.Debug.WriteLine("<- Expanded ->");

                await EZApp.Container.TriggerLayout();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("<- No Need ->");
            }
        }

        public async Task Contract()
        {
            if (Expander == null) return;

            bool contracted = await Expander.Contract();

            if (contracted)
            {
                System.Diagnostics.Debug.WriteLine("-> Contracted <-");

                await EZApp.Container.TriggerLayout();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("-> No Need <-");
            }
        }
    }
}