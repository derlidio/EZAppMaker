/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

using EZAppMaker.Attributes;
using EZAppMaker.Defaults;
using EZAppMaker.Support;
using EZAppMaker.Interfaces;
using System.Runtime.CompilerServices;

namespace EZAppMaker.Components
{
    public class EZPhoto : ContentView, IEZComponent
    {
        public static readonly BindableProperty ItemIdProperty = BindableProperty.Create(nameof(ItemId), typeof(string), typeof(EZPhoto), null);
        public static readonly BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(EZPhoto), null);
        public static readonly BindableProperty PhotoFileProperty = BindableProperty.Create(nameof(PhotoFile), typeof(string), typeof(EZPhoto), null);

        public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(EZPhoto), defaultValueCreator: bindable => Default.Color("ezphoto_border"));
        public static readonly BindableProperty LabelColorProperty = BindableProperty.Create(nameof(LabelColor), typeof(Color), typeof(EZPhoto), defaultValueCreator: bindable => Default.Color("ezphoto_label"));

        public static new readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(EZPhoto), defaultValueCreator: bindable => Default.Color("ezphoto_background"));

        private Image photo;
        private string initial;
        private string state;

        public delegate void OnChange(EZPhoto photo);
        public event OnChange OnChanged;

        public EZPhoto()
        {
            ControlTemplate = (ControlTemplate)EZDictionary.Resources["EZPhotoTemplate"];

            EZPathButton button;

            button = (EZPathButton)GetTemplateChild("ClearButton");
            button.OnTap += OnClearButtonTap;

            button = (EZPathButton)GetTemplateChild("CameraButton");
            button.OnTap += OnCameraButtonTap;

            photo = (Image)GetTemplateChild("Photo");
        }

        public void ThemeChanged()
        {
            LabelColor = Default.Color("ezphoto_label");
            BorderColor = Default.Color("ezphoto_border");
            BackgroundColor = Default.Color("ezphoto_background");

            EZPathButton button;

            button = (EZPathButton)GetTemplateChild("ClearButton"); button.ThemeChanged();
            button = (EZPathButton)GetTemplateChild("CameraButton"); button.ThemeChanged();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            initial = PhotoFile;
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == "PhotoFile")
            {
                photo.Source = null;

                if (!string.IsNullOrWhiteSpace(PhotoFile))
                {
                    try
                    {
                        photo.Source = ImageSource.FromFile(PhotoFile);
                    }
                    catch { /* Dismiss */ }
                }

                OnChanged?.Invoke(this);
            }
        }

        public void StateManager(StateFormAction action)
        {
            switch (action)
            {
                case StateFormAction.Save:

                    state = PhotoFile;
                    break;

                case StateFormAction.Restore:

                    if (PhotoFile != state)
                    {
                        PhotoFile = state;
                    }
                    break;
            }
        }

        public void Clear()
        {
            PhotoFile = null;
            photo.Source = null;
        }

        public bool Modified()
        {
            return PhotoFile != initial;
        }

        public object ToDatabaseValue(object target)
        {
            return PhotoFile;
        }

        public new Color BackgroundColor
        {
            get => (Color)GetValue(BackgroundColorProperty);
            set => SetValue(BackgroundColorProperty, value);
        }

        public Color BorderColor
        {
            get => (Color)GetValue(BorderColorProperty);
            set => SetValue(BorderColorProperty, value);
        }

        public Color LabelColor
        {
            get => (Color)GetValue(LabelColorProperty);
            set => SetValue(LabelColorProperty, value);
        }

        public bool Detached { get; set; }

        public string ItemId
        {
            get => (string)GetValue(ItemIdProperty);
            set => SetValue(ItemIdProperty, value);
        }

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public string PhotoFile
        {
            get => (string)GetValue(PhotoFileProperty);
            set => SetValue(PhotoFileProperty, value);
        }

        [ComponentEventHandler]
        private void OnClearButtonTap(EZPathButton button)
        {
            Clear();
        }

        [ComponentEventHandler, AsyncVoidOnPurpose]
        private void OnCameraButtonTap(EZPathButton button)
        {
            _ = TakePhotoAsync();
        }

        async Task TakePhotoAsync()
        {
            try
            {
                var photo = await MediaPicker.CapturePhotoAsync();

                await LoadPhotoAsync(photo);
            }
            catch (FeatureNotSupportedException)
            {
                // Feature is not supported on the device!
                EZApp.Alert(Default.Localization("ezphoto_not_supported"));
            }
            catch (PermissionException)
            {
                // Permissions not granted!
                EZApp.Alert(Default.Localization("ezphoto_no_permission"));
            }
            catch (Exception ex)
            {
                EZApp.Alert(Default.Localization("ezphoto_error"));
                System.Diagnostics.Debug.WriteLine($"CapturePhotoAsync: {ex.Message}");
            }
        }

        async Task LoadPhotoAsync(FileResult result)
        {
            if (result == null) return;

            // Save the file into app's local storage: /ez_photos/file.jpg

            string file = Path.Combine(EZLocalAppData.GetPhotosPath(), result.FileName);

            try
            {
                using (var stream = await result.OpenReadAsync())
                {
                    using (var newStream = File.OpenWrite(file))
                    {
                        await stream.CopyToAsync(newStream);
                    }
                }

                PhotoFile = file;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Could not save the photo!");
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }
    }
}