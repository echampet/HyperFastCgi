﻿using System;
using Mono.WebServer.HyperFastCgi.Interfaces;
using Mono.WebServer.HyperFastCgi.Transport;
using Mono.WebServer.HyperFastCgi.Logging;
using Mono.WebServer.HyperFastCgi.Configuration;

namespace Mono.WebServer.HyperFastCgi.AppHosts.AspNet
{
	[Config(typeof(AspNetHostConfig))]
	public class AspNetApplicationHost : AppHostBase
	{
		#region IApplicationHost implementation

		public override IWebRequest CreateRequest (ulong requestId, int requestNumber, object arg)
		{
			return new AspNetNativeWebRequest (requestId, requestNumber, this, base.AppHostTransport, AddTrailingSlash);
		}

		public override IWebResponse GetResponse (IWebRequest request, object arg)
		{
			return (IWebResponse)request;
		}

		public override void ProcessRequest (IWebRequest request)
		{
			throw new NotImplementedException ();
		}

		public override void Configure (IApplicationServer server, 
			IListenerTransport listenerTransport, 
			Type appHostTransportType, object transportConfig,
			object appHostConfig)
		{
			AspNetHostConfig config = appHostConfig as AspNetHostConfig;

			if (config != null) {
				LogLevel = config.Log.Level;
				LogToConsole = config.Log.WriteToConsole;
				AddTrailingSlash = config.AddTrailingSlash;
			}

			base.Configure (server, listenerTransport, appHostTransportType, transportConfig, appHostConfig);
		}
		#endregion

		public LogLevel LogLevel {
			get { return Logger.Level; }
			set { Logger.Level = value; }
		}

		public bool LogToConsole {
			get { return Logger.WriteToConsole; }
			set { Logger.WriteToConsole = value; }
		}

		public bool AddTrailingSlash { get; set;}

	}
}

