using Microsoft.Extensions.DependencyInjection;

namespace SaveMo
{
    public partial class App : Application
    {
        /// <summary>
        /// Tracks whether dark mode is currently enabled
        /// </summary>
        public static bool IsDarkModeEnabled { get; private set; } = false;

        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Apply light theme on startup
            ApplyDayTheme();

            return new Window(new AppShell());
        }

        /// <summary>
        /// Toggles between light and dark theme
        /// </summary>
        public static void ToggleTheme()
        {
            if (IsDarkModeEnabled)
            {
                ApplyDayTheme();
            }
            else
            {
                ApplyNightTheme();
            }
        }

        /// <summary>
        /// Applies the day theme (light colors)
        /// </summary>
        private static void ApplyDayTheme()
        {
            ResourceDictionary resources = Current.Resources;

            resources["DynamicPrimary"] = Color.FromArgb("#3E2723");      // Deep Brown
            resources["DynamicSecondary"] = Color.FromArgb("#795548");    // Medium Brown
            resources["DynamicAccent"] = Color.FromArgb("#D4AF37");       // Brass Gold
            resources["DynamicBackground"] = Color.FromArgb("#FFFFFF");   // White
            resources["DynamicText"] = Color.FromArgb("#2C2C2C");         // Dark Text
            resources["DynamicCardBackground"] = Color.FromArgb("#F5F5F5"); // Light Gray
            resources["DynamicCardBorder"] = Color.FromArgb("#E8E8E8");   // Lighter Gray

            IsDarkModeEnabled = false;
        }

        /// <summary>
        /// Applies the night theme (dark colors)
        /// </summary>
        private static void ApplyNightTheme()
        {
            ResourceDictionary resources = Current.Resources;

            resources["DynamicPrimary"] = Color.FromArgb("#1B0000");      // Very Dark Brown
            resources["DynamicSecondary"] = Color.FromArgb("#5D4037");    // Dark Brown
            resources["DynamicAccent"] = Color.FromArgb("#E8C547");       // Light Gold
            resources["DynamicBackground"] = Color.FromArgb("#141414");   // Almost Black
            resources["DynamicText"] = Color.FromArgb("#F5F5F5");         // Light Text
            resources["DynamicCardBackground"] = Color.FromArgb("#212121"); // Dark Gray
            resources["DynamicCardBorder"] = Color.FromArgb("#404040");   // Medium Dark Gray

            IsDarkModeEnabled = true;
        }
    }
}
