using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SmartHome.Lib;
using SmartHome.Lib.Adapters;
using SmartHome.Lib.Environment;
using SmartHome.Lib.Hue;
using SmartHome.Lib.Lights;

namespace SmartHome.LightView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly SmartHub _smartHub;
        private readonly EnvironmentConfig _config;

        private bool _isActive;
        private Thread _workerThread;

        private int _updateDelay;
        private bool _updateRequired;

        private int _refreshTimer;
        private bool _refreshUpdate;

        private Room[] _selectedRooms;
        private Dictionary<Room, LightState> _storedStates;

        public MainWindow()
        {
            InitializeComponent();

            Slider.IsEnabled = false;
            OffButton.IsEnabled = false;
            OnButton.IsEnabled = false;
            NightButton.IsEnabled = false;

            _config = EnvironmentConfig.Load();

            _smartHub = new SmartHub
            {
                Environment = _config,
                LightAdapters = new List<ILightAdapter>
                {
                    new HueLightAdapter()
                },
                SensorAdapters = new List<ISensorAdapter>
                {
                    new HueSensorAdapater()
                }
            };

            LoadRooms();

            _isActive = true;

            _workerThread = new Thread(AsyncUpdate);
            _workerThread.Start();
        }

        private void LoadRooms()
        {
            RoomBox.Items.Add("All");

            foreach (Room room in _config.Rooms)
            {
                RoomBox.Items.Add(room.Name);
            }

            _refreshUpdate = true;

            RoomBox.SelectedIndex = 0;

            _selectedRooms = _config.Rooms;
        }

        private Dictionary<Room, LightState> GetRoomStates()
        {
            IList<ISmartLight> lights = _smartHub.PollLights();

            var roomStates = new Dictionary<Room, LightState>();

            foreach (Room room in _config.Rooms)
            {
                var lightStates = new List<LightState>();

                foreach (ISmartLight light in lights)
                {
                    if (room.Lights != null && room.ContainsLight(light.Id))
                    {
                        lightStates.Add(light.State);
                    }
                }

                roomStates.Add(room, lightStates.Count > 0 ? lightStates[0] : LightState.LightsOff);
            }

            return roomStates;
        }

        private bool RoomStatesChanged()
        {
            var roomStates = GetRoomStates();

            foreach (Room room in _config.Rooms)
            {
                LightState storedState = _storedStates[room];
                LightState currentState = roomStates[room];

                // If any states are different override the stored state
                if (storedState.On != currentState.On ||
                    Math.Abs(storedState.Brightness - currentState.Brightness) > 5 ||
                    Math.Abs(storedState.Hue - currentState.Hue) > 5 ||
                    Math.Abs(storedState.Saturation - currentState.Saturation) > 5)
                {
                    _storedStates = roomStates;

                    return true;
                }
            }

            return false;
        }

        private LightState GetSelectedState()
        {
            var activeLights = new List<LightState>();

            foreach (Room room in _selectedRooms)
            {
                if (_storedStates[room].On)
                {
                    activeLights.Add(_storedStates[room]);
                }
            }

            // No lights were on or found in the selected room
            if (activeLights.Count == 0)
            {
                return LightState.LightsOff;
            }

            // Otherwise average the hue, saturation, and brightness of all lights
            int hueSum = 0;
            int saturationSum = 0;
            int brightnessSum = 0;

            foreach (LightState state in activeLights)
            {
                hueSum += state.Hue;
                saturationSum += state.Saturation;
                brightnessSum += state.Brightness;
            }

            return new LightState
            {
                On = true,
                Hue = hueSum / activeLights.Count,
                Saturation = saturationSum / activeLights.Count,
                Brightness = brightnessSum / activeLights.Count
            };
        }

        private void AsyncUpdate()
        {
            _updateRequired = true;

            _smartHub.Initialize();
            _storedStates = GetRoomStates();

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Input, new Action(() =>
            {
                Slider.IsEnabled = true;
                OffButton.IsEnabled = true;
                OnButton.IsEnabled = true;
                NightButton.IsEnabled = true;
            }));

            while (_isActive)
            {
                // Update the state if something triggered a change
                if (_updateRequired)
                {
                    if (_updateDelay == 0)
                    {
                        LightState state = GetSelectedState();

                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Input, new Action(() =>
                        {
                            _refreshUpdate = true;

                            Slider.Value = Math.Round(state.Brightness * (100.0 / 255.0), MidpointRounding.AwayFromZero);
                        }));

                        foreach (Room room in _selectedRooms)
                        {
                            if (room.Lights != null)
                            {
                                _smartHub.UpdateLightsInRooms(new [] { room.Id }, _storedStates[room]);
                            }
                        }

                        _updateRequired = false;
                    }

                    _updateDelay--;
                }

                // Every second check the current state in case an outside system changed it
                //if (_refreshTimer == 100)
                //{
                //    if (RoomStatesChanged())
                //    {
                //        _updateDelay = 0;
                //        _updateRequired = true;
                //    }

                //    _refreshTimer = 0;
                //}

                _refreshTimer++;

                Thread.Sleep(10);
            }
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _isActive = false;

            if (_workerThread != null)
            {
                _workerThread.Join(1000);

                _workerThread = null;
            }
        }

        private void OnTurnOff(object sender, RoutedEventArgs e)
        {
            foreach (Room room in _selectedRooms)
            {
                _storedStates[room] = LightState.LightsOff;
            }

            _updateDelay = 0;
            _refreshTimer = 0;
            _updateRequired = true;
        }

        private void OnTurnOn(object sender, RoutedEventArgs e)
        {
            foreach (Room room in _selectedRooms)
            {
                _storedStates[room] = LightState.LightsOn;
            }

            _updateDelay = 0;
            _refreshTimer = 0;
            _updateRequired = true;
        }

        private void OnNightMode(object sender, RoutedEventArgs e)
        {
            foreach (Room room in _selectedRooms)
            {
                _storedStates[room] = new LightState
                {
                    On = true,
                    Hue = 0,
                    Brightness = 102,
                    Saturation = 255,
                    TransitionTime = 6
                };
            }

            _updateDelay = 0;
            _refreshTimer = 0;
            _updateRequired = true;
        }

        private void OnSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Return early when this data is getting auto refreshed
            if (_refreshUpdate)
            {
                _refreshUpdate = false;
                return;
            }

            foreach (Room room in _selectedRooms)
            {
                _storedStates[room].On = Slider.Value > 0;
                _storedStates[room].Brightness = (int)(Slider.Value * 2.55);
            }

            // Adds a 100ms delay so that many inputs don't trigger it
            _updateDelay = 10;
            _refreshTimer = 0;
            _updateRequired = true;
        }

        private void OnChangeRoom(object sender, SelectionChangedEventArgs e)
        {
            if (RoomBox.SelectedIndex == 0)
            {
                _selectedRooms = _config.Rooms;
            }
            else
            {
                _selectedRooms = new[] { _config.Rooms[RoomBox.SelectedIndex - 1] };
            }

            _updateDelay = 0;
            _refreshTimer = 0;
            _updateRequired = true;
        }
    }
}
