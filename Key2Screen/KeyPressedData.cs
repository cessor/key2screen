using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Key2Screen
{
    public class KeyPressedData : INotifyPropertyChanged
    {
        #region Constants and Fields

        private double height;
        private KeyEventData keyEventData;
        private string text;

        #endregion

        #region Constructors and Destructors

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public KeyPressedData(double height, 
            string text)
        {
            this.height = height;
            KeyEventData = new KeyEventData();
            this.text = text;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public KeyPressedData(double height,
            KeyEventData keyEventData)
        {
            this.height = height;
            KeyEventData = keyEventData;
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public double Height
        {
            get { return height; }
            set
            {
                height = value;
                RaisePropertyChanged("Height");
            }
        }

        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                RaisePropertyChanged("Text");
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Key2Screen.KeyPressedData.set_Text(System.String)")]
        public KeyEventData KeyEventData
        {
            get { return keyEventData; }
            set
            {
                keyEventData = value;
                if (keyEventData == null)
                {
                    Text = string.Empty;
                }
                else
                {
                    Text = keyEventData.Repetitions == 1 ? keyEventData.ToString() : string.Format(CultureInfo.InvariantCulture, "{0} ({1}x)", keyEventData, keyEventData.Repetitions);
                }
            }
        }

        #endregion

        #region Public Methods and Operators

        public void HalfYourHeight()
        {
            Height = Height / 2;
        }

        #endregion

        #region Methods

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
        protected virtual void RaisePropertyChanged(string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}