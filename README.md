##HTTP Server library for windows IoT devices and Universal apps.

There is limited HTTP server support functionality in Windows IoT, but some projects could really benifit from HTTP Server embedded in the app. It specially makes it easy to display data to the clients or control the IoT device itself.

This HTTP server library can be embedded in any project. User does not need to know anything about HTTP protocol or what is going on behind the scenes. It is all done with few calls to functions of HttpServer class.

For example, to embed simple HelloWorld style web page in your application, you would add following code:

```
using feri.MS.Http;

...

HttpServer server = new HttpServer();
server.start();
```

This will start http server on port 8000 and display "It works!" page to user.

To add some functionality to the server, you could either write "Liquid" or "Simple" templates or write and register listener function. There are two templating engines, "DotLiquidCore", that is [DotLiquid](http://dotliquidmarkup.org/) templating engine ported to Windows Universal Apps (.net core) and "SimpleTemplate" that is just string substitutions within document.
Templates can served by default listener and are passed request, response and and other objets that are provided to them or can be used within custom listener to format output from it.
All scripts are cached to avoid slow recompilations.

When registring listener, you need to provide URL that will tell the server when to trigger event.

Code could look something like this:

```
using feri.MS.Http;

...

HttpServer server = new HttpServer();
server.AddPath("/HelloWorld.html", HelloWorldListener);
server.start();

...

public void HelloWorldListener(HttpRequest request, HttpResponse response) {
    // Do some work here
}
```

The listener method is provided HttpRequest and HttpResponse objects.

HttpRequest contains all information about user request (from where connection came, what method it was, what was url, what were parameters, what were http headers, cookies, session related to the user, what was data, ...). HttpResponse takes care of sending data back to user (sending data, headers, cookies).

If you wish to limit access to server to some users, server supports Basic HTTP authentication. 

```
using feri.MS.Http;

...

HttpServer server = new HttpServer();
server.UserManager.AddUser("user","password");
server.AuthenticationRequired = true;
server.AddPath("/HelloWorld.html", HelloWorldListener);
server.start();

...
```

If user needs different authentication provider, they can replace default UserManager class at runtime, by custom class that implements IUserManager interface.

```
server.UserManager = new CustomUserManager();
```

When HttpServer class calls registred UserManager class it will first call Start() method and when UserManager gets destroyed for whatever reason, it will call Stop() method, helping with object lifecycle management.

Sometimes you need periodic data, for example reading temperature from sensors or updating display. To do this you could register new timer, that will get called periodically.

```
using feri.MS.Http;

...

HttpServer server = new HttpServer();
...
server.AddTimer("TimerName", 10000, TimerListener);

...

public void TimerListener() {
    // Do some work
}
```

Timers are not passed any information, since they are not related to any HTTP request or response.

If you wish to limit access to the server for certain IP subnet, you can use IP Blacklist or Whitelist functionality. To use ip blacklist you need to enable filtering and add ip's with network mask to the correct list. Network mask is provided in CIDR notation (for example 32)

```
using feri.MS.Http;

...

HttpServer server = new HttpServer();
server.IPFilterEnabled = true;
server.AddBlackList(new IPAddress(new byte[] { 192, 168, 1, 64 }), 24);
server.AddWhiteList(new IPAddress(new byte[] { 192, 168, 2, 64 }), 32);

...

server.start();
```

To avoid accidentally leaking pages to blocked ip addresses, you should set the filter before calling start() method.

If user needs different IP filter provider, they can replace default one at runtime by implementing implementing IIPFilter interface and registring it with server class.

```
server.IPFilter = new CustomIPFilter();
```

When HttpServer class calls registred IPFilter class it will first call Start() method and when IPFilter gets destroyed for whatever reason, it will call Stop() method, helping with object lifecycle management.

Redirecting request to different address can be done from HttpResponse class. Class supports temporary and permanent redirects. Example of temporary redirect would be:

```
using feri.MS.Http;

...

HttpServer server = new HttpServer();
server.AddPath("/addressThatWillRedirect.html", ProcessDemoRedirect);

...

private void ProcessDemoRedirect(HttpRequest request, HttpResponse response)
{
   response.Redirect("/targetWeWillRedirectTo.html");
}
```

For more information and example of use, please view the included demo project. It shows how to use tings like simple templating engine, JSON, classes for some electronic parts, ability to define server root path to serve static content, using JavaScript to display JSON data, ...
