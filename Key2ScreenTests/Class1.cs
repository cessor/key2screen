using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Forms;
using Key2Screen;
using NUnit.Framework;

namespace Key2ScreenTests
{
    [TestFixture, RequiresSTA]
    public class Class1
    {
        public class TestableWindow : MainWindow
        {
            private ConcurrentQueue<KeyEventData> _pressedKeys;
            public KeyEventData KeyEventData { get; private set; }

            public new ConcurrentQueue<KeyEventData> PressedKeys
            {
                get { return _pressedKeys ?? base.PressedKeys; }
                set { _pressedKeys = value; }
            }

            public List<char> WordBuffer { get; set; }

            public TestableWindow()
            {
                WordBuffer = new List<char>();
            }

            public new void OnKeyLoggerTick(object sender, EventArgs e)
            {
                base.OnKeyLoggerTick(sender, e);
            }

            protected override void SetText(KeyEventData keyEventData)
            {
                KeyEventData = keyEventData;
            }
        }

        [Test]
        public void A_Key_was_pressed()
        {
            var data = new KeyPressedData(50.0, "");
            data.KeyEventData = new KeyEventData(Keys.A, Keys.Control, windows: false);
            Assert.That(data.Text, Is.EqualTo("Ctrl + A"));
        }

        [Test]
        public void I_would_like_to_show_a_character()
        {
            // Arrange
            var window = new TestableWindow();
            Assert.That(window.PressedKeys, Is.Empty);
            
            // Act
            window.PressedKeys.Enqueue(new KeyEventData(Keys.A, Keys.None, false));
            window.OnKeyLoggerTick(sender: null, e: null);

            // Assert
            Assert.That(window.KeyEventData.ToString(), Is.EqualTo("A"));
        }

        [Test]
        public void Repeated_characters_come_through_as_one_event()
        {
            // Arrange
            var window = new TestableWindow();
            Assert.That(window.PressedKeys, Is.Empty);

            // Act
            window.PressedKeys.Enqueue(new KeyEventData(Keys.A, Keys.None, false));
            window.PressedKeys.Enqueue(new KeyEventData(Keys.A, Keys.None, false));
            window.PressedKeys.Enqueue(new KeyEventData(Keys.A, Keys.None, false));
            window.PressedKeys.Enqueue(new KeyEventData(Keys.A, Keys.None, false));

            window.OnKeyLoggerTick(sender: null, e: null);

            // Assert
            Assert.That(window.KeyEventData.Repetitions, Is.EqualTo(4));
        }

        [Test]
        public void Word_characters_should_be_buffered()
        {
            // Arrange
            var window = new TestableWindow();
            Assert.That(window.PressedKeys, Is.Empty);

            // Act
            window.PressedKeys.Enqueue(new KeyEventData(Keys.A, Keys.None, false));
            window.PressedKeys.Enqueue(new KeyEventData(Keys.A, Keys.None, false));
            window.PressedKeys.Enqueue(new KeyEventData(Keys.A, Keys.None, false));
            window.PressedKeys.Enqueue(new KeyEventData(Keys.A, Keys.None, false));

            window.OnKeyLoggerTick(sender: null, e: null);

            // Assert
            CollectionAssert.AreEqual(new[] { 'A', 'A', 'A', 'A' }, window.WordBuffer);
        }

        [Test]
        public void No_Keypress_for_whatever_reason()
        {
            var data = new KeyPressedData(50.0, "");
            data.KeyEventData = new KeyEventData();
            Assert.That(data.Text, Is.EqualTo("None"));
        }
    }
}