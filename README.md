# LAB.DataScanner project
DataScanner project includes the solution and project files for Visual Studio 2019.
DataScanner consists of three parts:
- Application Types as Console applications
- Configuration database API
- UI dashboard for configuration database API

# Console applications
Every application implemented using .NET Core 3.1 and requires RabbitMQ locally or in cloud.
Applications communicating using queues through RabbitMQ (AMQP). 
Applications configured using appsettings.json file inside every project folder.

Connection to RabbitMQ local or cloud server configuration example:
```json
"RmqConnectionSettings": {
    "UserName": "guest",
    "Password": "guest",
    "HostName": "localhost",
    "Port": 5672,
    "VirtualHost": "/"
  }
```
Default local RabbitMQ connection settings can be set also through changing the code in Program.cs by calling UsingDefaultConnectionSetting, then connection settings set in appsettings.json file are ignored.
```
services.AddSingleton(new RmqPublisherBuilder()
                        .UsingExchange(bindingSettings.SenderExchange)
                        .UsingDefaultConnectionSetting()
                        .Build());
services.AddSingleton(new RmqConsumerBuilder()
    .UsingExchange(bindingSettings.ReceiverExchange)
    .UsingQueue(bindingSettings.ReceiverQueue)
    .UsingRoutingKeys(bindingSettings.ReceiverRoutingKeys)
    .UsingDefaultConnectionSetting()
    .Build());
```

## UrlsGenerator application
Example of application specific settings:
```json
"Application": {
    "UrlTemplate": "https://www.hanselman.com/blog/archive/{0}",
    "Sequences": "['2003..2020']"
  }
```
Example of RabbitMQ queue binding settings:
```json
"Binding": {
    "SenderExchange": "WebPageDownloaderExchange",
    "SenderRoutingKeys": "['#']"
  }
```
UrlsGenerator will send message with URLs generated to RabbitMQ queue.
