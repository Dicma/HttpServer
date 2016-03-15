﻿#region Licence
/*
   Copyright 2016 Miha Strehar

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
#endregion

using Feri.MS.Http;
using Feri.MS.Http.Json;
using Feri.MS.Http.Template;
using Feri.MS.Parts.I2C.PortExpander;
using System;
using System.Diagnostics;

namespace WebServerDemo
{
    /// <summary>
    /// Demo of timer function in HttpServer. Allso provides page to enable and disable the timer.
    /// It blinks LED connected to  the port one of PCF8574 port extender connected to i2c.
    /// There is sample code for GPIO in comments of the code.
    /// </summary>
    class TimerDemo : IDisposable
    {
        HttpServer _ws;
        string _privatePath = "AppHtml";

        SimpleTemplate _timerControl = new SimpleTemplate();
        PCF8574 _ports = PCF8574.Create();

        SimpleJsonListener _json;
        SimpleTemplate _templateDemo;

        bool enableBlink = false;
        string state = "Unspecified";

        bool _debug = false;

        public void Start(HttpServer server, SimpleJsonListener json, SimpleTemplate templateDemo)
        {
            _ws = server;
            _json = json;
            _templateDemo = templateDemo;

            _ws.AddPath("/demoTimer.html", ProcessTimer);

            _timerControl.LoadString(_ws.HttpRootManager.ReadToByte(_privatePath + "/templateTimer.html"));
            _timerControl["timerOn"] = new TemplateAction() { Pattern = "TIMERON" };
            _timerControl["timerOff"] = new TemplateAction() { Pattern = "TIMEROFF" };

            _templateDemo["led"] = new TemplateAction() { Pattern = "LED", Data = "Off" };
            _templateDemo["timer"] = new TemplateAction() { Pattern = "TIMER", Data = "Off" };

            _json.AddData("Timer", "Off");
            _json.AddData("Led", "Off");

            _ws.HttpRootManager.AddExtensionTemplateData("shtml", "led", new TemplateAction() { Pattern = "LED", Data = "Off" });
            _ws.HttpRootManager.AddExtensionTemplateData("shtml", "timer", new TemplateAction() { Pattern = "TIMER", Data = "Off" });

            _ws.AddTimer("TestTimer", 10000, TimerEvent);

            //_ports._debug = true;
        }

        public void TimerEvent()
        {
            try
            {
                if (enableBlink)
                {
                    if (state.Equals("on", StringComparison.OrdinalIgnoreCase))
                    {
                        _json.UpdateData("Led", "Off");
                        _templateDemo["led"].Data = "Off";
                        state = "Off";
                        _ws.HttpRootManager.UpdateExtensionTemplateData("shtml", "led", new TemplateAction() { Pattern = "LED", Data = "Off" });
                        //pin.Write(GpioPinValue.Low);
                        //_ports.WritePin(PortNumber.PORT_ONE, false);    // Uncomment for sensors
                    }
                    else
                    {
                        _json.UpdateData("Led", "On");
                        _templateDemo["led"].Data = "On";
                        state = "On";
                        _ws.HttpRootManager.UpdateExtensionTemplateData("shtml", "led", new TemplateAction() { Pattern = "LED", Data = "On" });
                        //pin.Write(GpioPinValue.High);
                        //_ports.WritePin(PortNumber.PORT_ONE, true);    // Uncomment for sensors
                    }
                    Debug.WriteLineIf(_debug, "State changed to: " + state);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        private void ProcessTimer(HttpRequest request, HttpResponse response)
        {
            try
            {
                if (request.Parameters.ContainsKey("state"))
                {
                    if (request.Parameters["state"].Equals("On", StringComparison.OrdinalIgnoreCase))
                    {
                        enableBlink = true;
                        Debug.WriteLineIf(_debug, "Timer state changed to: On");
                    }
                    else if (request.Parameters["state"].Equals("Off", StringComparison.OrdinalIgnoreCase))
                    {
                        enableBlink = false;
                        Debug.WriteLineIf(_debug, "Timer state changed to: Off");
                    }
                }

                if (enableBlink)
                {
                    _timerControl["timerOn"].Data = "checked";
                    _timerControl["timerOff"].Data = string.Empty;
                    _json.UpdateData("Timer", "On");
                    _templateDemo["timer"].Data = "On";
                    _ws.HttpRootManager.UpdateExtensionTemplateData("shtml", "timer", new TemplateAction() { Pattern = "TIMER", Data = "On" });
                }
                else
                {
                    _timerControl["timerOn"].Data = string.Empty;
                    _timerControl["timerOff"].Data = "checked";
                    _json.UpdateData("Timer", "Off");
                    _templateDemo["timer"].Data = "Off";
                    _ws.HttpRootManager.UpdateExtensionTemplateData("shtml", "timer", new TemplateAction() { Pattern = "TIMER", Data = "Off" });
                }

                _timerControl.ProcessAction();
                response.Write(_timerControl.GetByte(), _ws.GetMimeType.GetMimeFromFile("/templateTimer.html"));

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                response.Write(e);
            }
        }

        #region IDisposable Support
        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            _ws.RemoveTimer("TestTimer");
            _ws.RemovePath("/demoTimer.html");
            _ports.Dispose();
        }
        #endregion

        //private void InitGPIO()
        //{
        //    gpio = GpioController.GetDefault();
        //    if (gpio == null)
        //        return; // GPIO ni na voljo na tem sistemu
        //    pin = gpio.OpenPin(4);
        //    pin.Write(GpioPinValue.Low);
        //    pin.SetDriveMode(GpioPinDriveMode.Output);
        //}
    }
}
