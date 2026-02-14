using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace DiplomObsAlarm
{
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleTop,
        ConfigurationChanges = ConfigChanges.ScreenSize |
                              ConfigChanges.Orientation |
                              ConfigChanges.UiMode |
                              ConfigChanges.ScreenLayout |
                              ConfigChanges.SmallestScreenSize |
                              ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Включаем immersive mode после создания активити
            Window.DecorView.Post(EnableImmersiveMode);
        }

        private void EnableImmersiveMode()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                var decorView = Window.DecorView;

                var uiOptions = (int)decorView.SystemUiVisibility;

                // Скрываем навигационную панель (снизу)
                uiOptions |= (int)SystemUiFlags.HideNavigation;

                // Полноэкранный режим (статус бар сверху)
                uiOptions |= (int)SystemUiFlags.Fullscreen;

                // "Липкий" immersive — панели появляются только при свайпе с края
                uiOptions |= (int)SystemUiFlags.ImmersiveSticky;

                // Важно: стабильная разметка, чтобы контент не прыгал
                uiOptions |= (int)SystemUiFlags.LayoutStable;

                // Контент располагается под системными панелями
                uiOptions |= (int)SystemUiFlags.LayoutHideNavigation;
                uiOptions |= (int)SystemUiFlags.LayoutFullscreen;

                decorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;
            }
        }

        // Восстанавливаем immersive mode при возврате фокуса
        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);

            if (hasFocus)
            {
                EnableImmersiveMode();
            }
        }
    }
}