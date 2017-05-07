﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Console.NETCore.Test.Model;
using ISimpleHttpServer.Service;
using ISSDP.UPnP.PCL.Enum;
using ISSDP.UPnP.PCL.Interfaces.Service;
using SSDP.UPnP.Netstandard.Helper;
using SSDP.UPnP.PCL.Service;

class Program
{
    private static IHttpListener _httpListener;

    private static IControlPoint _controlPoint;
    private static IDevice _device;
    private static string _controlPointLocalIp = "192.168.0.36";
    //private static string _remoteDeviceIp = "10.10.2.170";


    // For this test to work you most likely need to stop the SSDP Discovery service on Windows
    // If you don't stop the SSDP Windows Service, the service will intercept the UPnP multicasts and consequently nothing will show up in the console. 

    static void Main(string[] args)
    {
        StartAsync();

        System.Console.ReadKey();
    }

    private static async void StartAsync()
    {


        _httpListener = await Initializer.GetHttpListener(
            _controlPointLocalIp);

        //StartDeviceListening();

        await StartControlPointListeningAsync();
    }

    private static async Task ListenToNotify()
    {

        var counter = 0;

        var notifyObs = await _controlPoint.CreateNotifyObservable(Initializer.TcpRequestListenerPort);

        var subscription = notifyObs
            //.Where(req => req.HostIp == "192.168.0.20")
            //.SubscribeOn(Scheduler.CurrentThread)
            //.Where(n => n.NTS == NTS.Alive || n.NTS == NTS.ByeBye || n.NTS == NTS.Update)
            .Subscribe(
                n =>
                {
                    counter++;
                    System.Console.BackgroundColor = ConsoleColor.DarkBlue;
                    System.Console.ForegroundColor = ConsoleColor.White;
                    System.Console.WriteLine($"---### Control Point Received a NOTIFY - #{counter} ###---");
                    System.Console.ResetColor();
                    System.Console.WriteLine($"{n.NotifyCastMethod.ToString()}");
                    System.Console.WriteLine($"From: {n.HostIp}:{n.HostPort}");
                    System.Console.WriteLine($"Location: {n?.Location?.AbsoluteUri}");
                    System.Console.WriteLine($"Cache-Control: max-age = {n.CacheControl}");
                    System.Console.WriteLine($"Server: " +
                                             $"{n.Server.OperatingSystem}/{n.Server.OperatingSystemVersion} " +
                                             $"UPNP/" +
                                             $"{n.Server.UpnpMajorVersion}.{n.Server.UpnpMinorVersion}" +
                                             $" " +
                                             $"{n.Server.ProductName}/{n.Server.ProductVersion}" +
                                             $" - ({n.Server.FullString})");
                    System.Console.WriteLine($"NT: {n.NT}");
                    System.Console.WriteLine($"NTS: {n.NTS}");
                    System.Console.WriteLine($"USN: {n.USN}");
                    System.Console.WriteLine($"BOOTID: {n.BOOTID}");
                    System.Console.WriteLine($"CONFIGID: {n.CONFIGID}");
                    System.Console.WriteLine($"NEXTBOOTID: {n.NEXTBOOTID}");
                    System.Console.WriteLine($"SEARCHPORT: {n.SEARCHPORT}");
                    System.Console.WriteLine($"SECURELOCATION: {n.SECURELOCATION}");

                    if (n.Headers.Any())
                    {
                        System.Console.ForegroundColor = ConsoleColor.DarkYellow;
                        System.Console.WriteLine($"Additional Headers: {n.Headers.Count}");
                        foreach (var header in n.Headers)
                        {
                            System.Console.WriteLine($"{header.Key}: {header.Value}; ");
                        }
                        System.Console.ResetColor();
                    }

                    System.Console.WriteLine();
                });
    }

    private static async Task ListenToMSearchResponse()
    {

        var mSeachResObs = await _controlPoint.CreateMSearchResponseObservable(Initializer.TcpResponseListenerPort);

        var counter = 0;

        var mSearchresponseSubscribe = mSeachResObs
            //.Where(req => req.HostIp == _remoteDeviceIp)
            //.SubscribeOn(Scheduler.CurrentThread)
            .Subscribe(
                res =>
                {
                    counter++;
                    System.Console.BackgroundColor = ConsoleColor.DarkBlue;
                    System.Console.ForegroundColor = ConsoleColor.White;
                    System.Console.WriteLine($"---### Control Point Received a  M-SEARCH RESPONSE #{counter} ###---");
                    System.Console.ResetColor();
                    System.Console.WriteLine($"{res.ResponseCastMethod.ToString()}");
                    System.Console.WriteLine($"From: {res.HostIp}:{res.HostPort}");
                    System.Console.WriteLine($"Status code: {res.StatusCode} {res.ResponseReason}");
                    System.Console.WriteLine($"Location: {res.Location.AbsoluteUri}");
                    System.Console.WriteLine($"Date: {res.Date.ToString(CultureInfo.CurrentCulture)}");
                    System.Console.WriteLine($"Cache-Control: max-age = {res.CacheControl}");
                    System.Console.WriteLine($"Server: " +
                                             $"{res.Server.OperatingSystem}/{res.Server.OperatingSystemVersion} " +
                                             $"UPNP/" +
                                             $"{res.Server.UpnpMajorVersion}.{res.Server.UpnpMinorVersion}" +
                                             $" " +
                                             $"{res.Server.ProductName}/{res.Server.ProductVersion}" +
                                             $" - ({res.Server.FullString})");
                    System.Console.WriteLine($"ST: {res.ST}");
                    System.Console.WriteLine($"USN: {res.USN}");
                    System.Console.WriteLine($"BOOTID.UPNP.ORG: {res.BOOTID}");
                    System.Console.WriteLine($"CONFIGID.UPNP.ORG: {res.CONFIGID}");
                    System.Console.WriteLine($"SEARCHPORT.UPNP.ORG: {res.SEARCHPORT}");
                    System.Console.WriteLine($"SECURELOCATION: {res.SECURELOCATION}");

                    if (res.Headers.Any())
                    {
                        System.Console.ForegroundColor = ConsoleColor.DarkYellow;
                        System.Console.WriteLine($"Additional Headers: {res.Headers.Count}");
                        foreach (var header in res.Headers)
                        {
                            System.Console.WriteLine($"{header.Key}: {header.Value}; ");
                        }
                        System.Console.ResetColor();
                    }

                    System.Console.WriteLine();
                });

        await StartMSearchRequestMulticastAsync();
    }

    private static void StartDeviceListening()
    {
        _device = new Device(_httpListener);
        var mSearchRequestSubscribe = _device.MSearchObservable.Subscribe(
            req =>
            {
                System.Console.BackgroundColor = ConsoleColor.DarkGreen;
                System.Console.ForegroundColor = ConsoleColor.White;
                System.Console.WriteLine($"---### Device Received a M-SEARCH REQUEST ###---");
                System.Console.ResetColor();
                System.Console.WriteLine($"{req.SearchCastMethod.ToString()}");
                System.Console.WriteLine($"HOST: {req.HostIp}:{req.HostPort}");
                System.Console.WriteLine($"MAN: {req.MAN}");
                System.Console.WriteLine($"MX: {req.MX.TotalSeconds}");
                System.Console.WriteLine($"USER-AGENT: " +
                                         $"{req.UserAgent?.OperatingSystem}/{req.UserAgent?.OperatingSystemVersion} " +
                                         $"UPNP/" +
                                         $"{req.UserAgent?.UpnpMajorVersion}.{req.UserAgent?.UpnpMinorVersion}" +
                                         $" " +
                                         $"{req.UserAgent?.ProductName}/{req.UserAgent?.ProductVersion}" +
                                         $" - ({req.UserAgent?.FullString})");

                System.Console.WriteLine($"CPFN: {req.CPFN}");
                System.Console.WriteLine($"CPUUID: {req.CPUUID}");
                System.Console.WriteLine($"TCPPORT: {req.TCPPORT}");

                if (req.Headers.Any())
                {
                    System.Console.ForegroundColor = ConsoleColor.DarkYellow;
                    System.Console.WriteLine($"Additional Headers: {req.Headers.Count}");
                    foreach (var header in req.Headers)
                    {
                        System.Console.WriteLine($"{header.Key}: {header.Value}; ");
                    }
                    System.Console.ResetColor();
                }


                System.Console.WriteLine();
            });
    }

    private static async Task StartControlPointListeningAsync()
    {
        _controlPoint = new ControlPoint(_httpListener);

        await ListenToNotify();
       
        await ListenToMSearchResponse();
    }

    private static async Task StartMSearchRequestMulticastAsync()
    {
        var mSearchMessage = new MSearch
        {
            SearchCastMethod = CastMethod.Multicast,
            CPFN = "TestXamarin",
            HostIp = "239.255.255.250",
            HostPort = 1900,
            MX = TimeSpan.FromSeconds(5),
            TCPPORT = Initializer.TcpResponseListenerPort.ToString(),
            ST = "upnp:rootdevice",
            UserAgent = new UserAgent
            {
                OperatingSystem = "Windows",
                OperatingSystemVersion = "10.0",
                ProductName = "SSDP.UPNP.PCL",
                ProductVersion = "0.9",
                UpnpMajorVersion = "2",
                UpnpMinorVersion = "0",
            }
        };
        await _controlPoint.SendMSearchAsync(mSearchMessage);
    }
}