using System;

namespace LAB.DataScanner.Components.Services.MessageBroker
{
    public abstract class RmqBuilder<T> where T : class
    {
        protected string _userName;
        protected string _password;
        protected string _hostName;
        protected int _port;
        protected string _virtualHost;

        public RmqBuilder<T> UsingDefaultConnectionSetting()
        {
            _userName = "guest";
            _password = "guest";
            _hostName = "localhost";
            _port = 5672;
            _virtualHost = "/";
            return this;
        }

        public RmqBuilder<T> UsingConfigConnectionSettings(RmqBuilderConnSettings settings)
        {
            if(settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            _userName = string.IsNullOrEmpty(settings.UserName) ? throw new ArgumentNullException(nameof(settings.UserName)) : settings.UserName;
            _password = string.IsNullOrEmpty(settings.Password) ? throw new ArgumentNullException(nameof(settings.Password)) : settings.Password;
            _hostName = string.IsNullOrEmpty(settings.HostName) ? throw new ArgumentNullException(nameof(settings.HostName)) : settings.HostName;
            _virtualHost = string.IsNullOrEmpty(settings.VirtualHost) ? throw new ArgumentNullException(nameof(settings.VirtualHost)) : settings.VirtualHost;
            _port = (settings.Port < 0 || settings.Port > 65535) ? throw new ArgumentNullException(nameof(settings.Port)) : settings.Port;
            return this;
        }

        public RmqBuilder<T> UsingCustomHost(string hostName)
        {
            if(string.IsNullOrEmpty(hostName))
            {
                throw new ArgumentNullException(nameof(hostName));
            }
            _hostName = hostName;
            return this;
        }

        public RmqBuilder<T> UsingCustomCredentials(string userName, string userPassword)
        {
            if(string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userPassword))
            {
                throw new ArgumentNullException();
            }
            _userName = userName;
            _password = userPassword;
            return this;
        }

        public abstract T Build();
    }
}
