using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Forms;

using Application = System.Windows.Forms.Application;
using MessageBox = System.Windows.MessageBox;

namespace Key2Screen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IDisposable
    {
        #region Constants and Fields

        // Dependency Property
        public static readonly DependencyProperty ListVisibilityProperty =
             DependencyProperty.Register("ListVisibility", typeof(Visibility),
             typeof(MainWindow), new FrameworkPropertyMetadata(Visibility.Visible));

        // .NET Property wrapper
        public Visibility ListVisibility
        {
            get { return (Visibility)GetValue(ListVisibilityProperty); }
            set { SetValue(ListVisibilityProperty, value); }
        }


        private const Keys ModifierDisplayFinishMessageBox = Keys.Alt | Keys.Control | Keys.Shift;
        private const string WelcomeText = "  Key2Screen V.{0} active. Have fun with your Kata!  ";
        private readonly ObservableCollection<KeyPressedData> pressedKeys = new ObservableCollection<KeyPressedData>();
        private readonly DockStyle dockStyle;
        private readonly Timer fadeoutTimer = new Timer();
        private readonly bool fadeoutTimerEnabled;
        private readonly int historyLength = 3;
        private readonly double initialItemHeight;
        private readonly Timer keyLogger = new Timer();
        private KeyboardHook keyboardHook = new KeyboardHook();
        private KeyEventData lastLoggedKey;

        #endregion

        #region Constructors and Destructors

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Key2Screen.KeyPressedData.#ctor(System.Double,System.String)")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Mobility", "CA1601:DoNotUseTimersThatPreventPowerStateChanges")]
        public MainWindow(int fadeoutDelay,
            int historyLength,
            DockStyle dockStyle,
            double height)
        {
            this.historyLength = historyLength;
            this.dockStyle = dockStyle;

            InitializeComponent();

            Height = height;
            Width = Screen.PrimaryScreen.WorkingArea.Width;
            switch (dockStyle)
            {
                case DockStyle.Top:
                    Top = 0;
                    break;
                case DockStyle.Bottom:
                    Top = Screen.PrimaryScreen.WorkingArea.Height - Height;
                    break;
            }

            KeyboardHook.HookAllKeys = true;
            keyboardHook.Hook();

            keyLogger.Interval = 50;
            keyLogger.Tick += OnKeyLoggerTick;
            keyLogger.Enabled = true;

            fadeoutTimerEnabled = fadeoutDelay > 0;
            if (fadeoutDelay > 0)
            {
                fadeoutTimer.Tick += OnFadeOutTimerTick;
                fadeoutTimer.Interval = fadeoutDelay;
            }

            initialItemHeight = Height / 2;
            pressedKeys.Add(new KeyPressedData(initialItemHeight, string.Format(CultureInfo.InvariantCulture, WelcomeText, Application.ProductVersion)));

            DataContext = pressedKeys;
        }

        protected MainWindow()
        {
            
        }

        #endregion

        #region Public Methods and Operators

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        #endregion

        #region Methods

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            keyboardHook.Unhook();
            keyboardHook = null;
            fadeoutTimer.Enabled = false;
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                fadeoutTimer.Enabled = false;
                fadeoutTimer.Dispose();

                keyLogger.Enabled = false;
                keyLogger.Dispose();
            }
        }


        private void OnFadeOutTimerTick(object sender,
            EventArgs e)
        {
            fadeoutTimer.Enabled = false;
            ListVisibility = Visibility.Collapsed;
        }

        protected void OnKeyLoggerTick(object sender,
            EventArgs e)
        {
            keyLogger.Enabled = false;
            KeyEventData keyEventData;
            while (KeyboardHook.PressedKeys.TryDequeue(out keyEventData))
            {
                if (((keyEventData.Modifier & ModifierDisplayFinishMessageBox) == ModifierDisplayFinishMessageBox)
                    && keyEventData.Key == Keys.F4)
                {
                    if (MessageBox.Show(this, "Stop logging keys?", Title, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                    {
                        Close();
                        return;
                    }
                    while (KeyboardHook.PressedKeys.TryDequeue(out keyEventData))
                    {
                    }
                    keyLogger.Enabled = true;
                    return;
                }
                if ((lastLoggedKey != null)
                    && (lastLoggedKey.Equals(keyEventData)))
                {
                    lastLoggedKey.Repetitions++;
                    SetText(lastLoggedKey);
                }
                else
                {
                    lastLoggedKey = keyEventData;
                    SetText(keyEventData);
                }
            }
            keyLogger.Enabled = true;
        }

        protected ConcurrentQueue<KeyEventData> PressedKeys {
            get { return KeyboardHook.PressedKeys; }
        }

        protected virtual void SetText(KeyEventData keyEventData)
        {
            var lastLoggedItem = GetLastLoggedItem();

            if (lastLoggedItem != null
                && lastLoggedItem.KeyEventData.Equals(keyEventData))
            {
                lastLoggedItem.KeyEventData = keyEventData;
            }
            else
            {
                if ((lastLoggedItem != null)
                    && (pressedKeys.Count >= historyLength))
                {
                    if (dockStyle == DockStyle.Top)
                    {
                        pressedKeys.RemoveAt(pressedKeys.Count - 1);
                    }
                    else
                    {
                        pressedKeys.RemoveAt(0);
                    }
                }

                foreach (var keyPressedData in pressedKeys)
                {
                    keyPressedData.HalfYourHeight();
                }

                var newKeyPressedData = new KeyPressedData(initialItemHeight, keyEventData);
                if (dockStyle == DockStyle.Top)
                {
                    pressedKeys.Insert(0, newKeyPressedData);
                }
                else
                {
                    pressedKeys.Add(newKeyPressedData);
                }
            }

            fadeoutTimer.Enabled = false;
            fadeoutTimer.Enabled = fadeoutTimerEnabled;
            ListVisibility = Visibility.Visible;
        }

        private KeyPressedData GetLastLoggedItem()
        {
            if (pressedKeys.Count == 0)
            {
                return null;
            }
            return pressedKeys[dockStyle == DockStyle.Top ? 0 : pressedKeys.Count - 1];
        }

        #endregion
    }
}