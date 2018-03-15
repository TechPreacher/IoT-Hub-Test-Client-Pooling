using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace IoT_Hub_Test_Client
{
    class Program
    {
        static Sensor _sensor;
        static DateTime _startTime;
        static int _count = 50;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting to generate data. Press \'q\' to quit the Simulator.\n");

            _startTime = System.DateTime.Now;
            _sensor = new Sensor();

            // Initialize Device Client GLOBALLY
            var devices = new Device[3];
            devices[0] = new Device { Id = "TestSensor1" };
            devices[1] = new Device { Id = "TestSensor2" };
            devices[2] = new Device { Id = "TestSensor3" };

            var deviceClients = new DeviceClient[3];

            for (int i = 0; i < 3; i++)
            {
                // Authenticate using TOKEN
                /*
                var auth = new DeviceAuthenticationWithToken(
                    devices[i].Id,
                    "[token]");
                */

                // Authenticate using KEY
                var auth = new DeviceAuthenticationWithSharedAccessPolicyKey(
                    devices[i].Id, 
                    "[shared access policy name]",
                    "[shared access policy key]");

                deviceClients[i] = DeviceClient.Create(
                    "[iothubname].azure-devices.net",
                    auth,
                    new ITransportSettings[]
                    {
                        new AmqpTransportSettings(TransportType.Amqp_Tcp_Only)
                        {
                            AmqpConnectionPoolSettings = new AmqpConnectionPoolSettings()
                            {
                                Pooling = true,
                                MaxPoolSize = 1,
                                ConnectionIdleTimeout = TimeSpan.FromMilliseconds(10000)
                            }
                        }
                    }
                );

                deviceClients[i].OpenAsync();

                Console.WriteLine(devices[i].Id + ": Opening Connection");

                devices[i].DC = deviceClients[i];
            }

            // Do Sync processing.

            Thread myThread1 = new Thread(new ParameterizedThreadStart(SendMessagesSync));
            myThread1.Start(devices[0]);

            Thread myThread2 = new Thread(new ParameterizedThreadStart(SendMessagesSync));
            myThread2.Start(devices[1]);

            Thread myThread3 = new Thread(new ParameterizedThreadStart(SendMessagesSync));
            myThread3.Start(devices[2]);

            // Done.
            while (Console.Read() != 'q') ;
        }

        private async static void SendMessagesSync(object deviceObject)
        {
            Device device = (Device)deviceObject;
            DeviceClient deviceClient = device.DC;
            int iMsgSuccess = 0;
            int iMsgFailed = 0;

            for (int _i2 = 0; _i2 < _count; _i2++)
            {
                _sensor.SensorName = device.Id;
                _sensor.NoiseLevel = _sensor.GetNoiseLevel();
                _sensor.AirPressure = _sensor.GetAirPressure();
                _sensor.AirQuality = _sensor.GetAirQuality();
                _sensor.Timecreated = DateTime.Now;
                string _sensorJson = JsonConvert.SerializeObject(_sensor);

                Debug.WriteLine(device.Id + ": Generating Message No " + _i2 + ":\n {0}", _sensorJson);

                try
                {
                    // Send message Async
                    await deviceClient.SendEventAsync(new Message(Encoding.ASCII.GetBytes(_sensorJson)));
                    Debug.WriteLine("\t" + device.Id + ": Send Message Success.");
                    iMsgSuccess++;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(device.Id + ": Send Message Failed: " + ex.Message.ToString());
                    iMsgFailed++;
                }
            }

            Console.WriteLine(device.Id + ": " + _count + " messages sent. Timer stopped.");
            Console.WriteLine("Succeeded: " + iMsgSuccess + ", Failed: " + iMsgFailed);
            TimeSpan _elapsed = DateTime.Now - _startTime;
            Console.WriteLine(device.Id + ": Time elapsed: " + (int)_elapsed.TotalMilliseconds);
        }
    }

    internal class Device
    {
        public string Id { get; set; }
        public DeviceClient DC { get; set; }
    }
}
