// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Diagnostics;
using Windows.Devices.Gpio;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace PushButton
{
    public sealed partial class MainPage : Page
    {
        private const int LED_PIN = 5;
        private const int LED2_PIN = 4;
        private const int BUTTON_PIN = 6;
        private const int PIR_PIN = 18;
        private const int BUZZER = 17;
        private GpioPin ledPin;
        private GpioPin led2Pin;
        private GpioPin buttonPin;
        private GpioPin buzzerPin;
        private GpioPin pirPin;

        //private GpioPinValue ledPinValue = GpioPinValue.High;
        private SolidColorBrush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);


        public MainPage()
        {
            InitializeComponent();
            InitGPIO();
        }

        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                GpioStatus.Text = "There is no GPIO controller on this device.";
                return;
            }

            buttonPin = gpio.OpenPin(BUTTON_PIN);
            pirPin = gpio.OpenPin(PIR_PIN);

            ledPin = gpio.OpenPin(LED_PIN);
            led2Pin = gpio.OpenPin(LED2_PIN);
            buzzerPin = gpio.OpenPin(BUZZER);

            // Initialize LED to the OFF state by first writing a HIGH value
            // We write HIGH because the LED is wired in a active LOW configuration
            ledPin.Write(GpioPinValue.High);
            ledPin.SetDriveMode(GpioPinDriveMode.Output);
            led2Pin.Write(GpioPinValue.High);
            led2Pin.SetDriveMode(GpioPinDriveMode.Output);

            buzzerPin.Write(GpioPinValue.Low);
            buzzerPin.SetDriveMode(GpioPinDriveMode.Output);



            // Check if input pull-up resistors are supported
            if (buttonPin.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
                buttonPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
            else
                buttonPin.SetDriveMode(GpioPinDriveMode.Input);

            // Set a debounce timeout to filter out switch bounce noise from a button press
            buttonPin.DebounceTimeout = TimeSpan.FromMilliseconds(50);

            // Register for the ValueChanged event so our buttonPin_ValueChanged 
            // function is called when the button is pressed
            buttonPin.ValueChanged += buttonPin_ValueChanged;


            // Check if input pull-up resistors are supported
            if (pirPin.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
            {
                //pirPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
                pirPin.SetDriveMode(GpioPinDriveMode.Input);
            }
            else
                pirPin.SetDriveMode(GpioPinDriveMode.Input);

            // Set a debounce timeout to filter out switch bounce noise from a button press
            pirPin.DebounceTimeout = TimeSpan.FromMilliseconds(50);

            // Register for the ValueChanged event so our buttonPin_ValueChanged 
            // function is called when the button is pressed
            pirPin.ValueChanged += pirPin_ValueChanged;


            GpioStatus.Text = "GPIO pins initialized correctly.";
        }

        private void buttonPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            // toggle the state of the LED every time the button is pressed
            // puvodni kod
            //if (e.Edge == GpioPinEdge.FallingEdge)
            //{
            //    ledPinValue = (ledPinValue == GpioPinValue.Low) ?
            //        GpioPinValue.High : GpioPinValue.Low;
            //    ledPin.Write(ledPinValue);
            //}
            if (e.Edge == GpioPinEdge.RisingEdge)  // dveře se rozpojují - piny nejsou spojený takže na tom co není na zemi naměřím high
            {
                //Debug.WriteLine("Dveře-rising");
                //Debug.WriteLine(buttonPin.Read().ToString());

                ledPin.Write(GpioPinValue.Low);     // zapnu ledku
                buzzerPin.Write(GpioPinValue.High);
            }
            else
            {
                //Debug.WriteLine("Dveře-falling");
                //Debug.WriteLine(buttonPin.Read().ToString());

                ledPin.Write(GpioPinValue.High);
                buzzerPin.Write(GpioPinValue.Low);
            }
            Debug.WriteLine("--------------------------------------------------");


            // need to invoke UI updates on the UI thread because this event
            // handler gets invoked on a separate thread.
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                if (e.Edge == GpioPinEdge.FallingEdge)
                {
                    ledEllipse.Fill = grayBrush;
                    GpioStatus.Text = "Button Pressed";
                }
                else
                {
                    ledEllipse.Fill = redBrush;
                    GpioStatus.Text = "Button Released";
                }
            });
        }

        private void pirPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            // toggle the state of the LED every time the button is pressed
            // puvodni kod
            //if (e.Edge == GpioPinEdge.FallingEdge)
            //{
            //    ledPinValue = (ledPinValue == GpioPinValue.Low) ?
            //        GpioPinValue.High : GpioPinValue.Low;
            //    ledPin.Write(ledPinValue);
            //}
            
            //Debug.WriteLine(e.Edge.ToString());

            if (e.Edge == GpioPinEdge.RisingEdge)
            {
                Debug.WriteLine("PIR-rising");
                Debug.WriteLine(pirPin.Read().ToString());

                led2Pin.Write(GpioPinValue.Low);
            }
            else
            {
                Debug.WriteLine("PIR-falling");
                Debug.WriteLine(pirPin.Read().ToString());

                led2Pin.Write(GpioPinValue.High);
            }
        }

    }
}
