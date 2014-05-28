﻿using System;
using Mono.WebServer.HyperFastCgi.Interfaces;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Mono.WebServer.HyperFastCgi.AppHosts.AspNet;
using System.Threading;
using System.Text;
#if !NET_2_0
using System.Threading.Tasks;
#endif 

namespace Mono.WebServer.HyperFastCgi.Transport
{
	public class CombinedAppHostTransport : MarshalByRefObject, IApplicationHostTransport
	{
		Dictionary<ulong, IWebRequest> requests = new Dictionary<ulong, IWebRequest> ();
		object requestsLock = new object ();
		IApplicationHost appHost;

		public IApplicationHost AppHost {
			get { return appHost;}
		}

		#region INativeTransport implementation

		public void Configure (IApplicationHost host, object config)
		{
			this.appHost = host;
			RegisterHost (host.VPath, host.Path);
		}

		public void CreateRequest (ulong requestId, int requestNumber)
		{
			IWebRequest req=AppHost.CreateRequest (requestId, requestNumber, null);

			lock (requestsLock) {
				requests.Add (requestId, req);
			}

			//			Console.WriteLine ("CreateRequest hash={0}, reqN={1}", requestId, requestNumber);
		}

		public void AddServerVariable (ulong requestId, int requestNumber, string name, string value)
		{
			IWebRequest request;
			lock (requestsLock)
			{
				requests.TryGetValue (requestId, out request);
			}

			try
			{
				if (request != null 
					&& request.RequestNumber == requestNumber) {
					request.AddServerVariable (name, value);
				}
			} catch (Exception ex) {
				Console.WriteLine ("ex={0}", ex.ToString ());
			}
		}

		public void AddHeader (ulong requestId, int requestNumber, string name, string value)
		{
			IWebRequest request;
			lock (requestsLock)
			{
				requests.TryGetValue (requestId, out request);
			}

			if (request != null
				&& request.RequestNumber == requestNumber) {
				request.AddHeader (name, value);
			}
		}

		public void HeadersSent (ulong requestId, int requestNumber)
		{

		}

		public void AddBodyPart (ulong requestId, int requestNumber, byte[] body, bool final)
		{
			IWebRequest request;
			lock (requestsLock)
			{
				requests.TryGetValue (requestId, out request);
			}

			if (request != null
				&& request.RequestNumber == requestNumber) {

				if (final) {
					lock (requestsLock) {
						requests.Remove (requestId);
					}
					request.Process ((IWebResponse)request);
				} else {
					request.AddBodyPart (body);
				}
			}

		}

		public void Process (ulong requestId, int requestNumber)
		{
			//			Console.WriteLine ("Remove ReqId={0}", requestId);
			lock (requestsLock) {
				requests.Remove (requestId);
			}
		}
		#endregion

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern void RegisterHost (string virtualPath, string path);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern void SendOutput (ulong requestId, int requestNumber, byte[] data, int len);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern void EndRequest (ulong requestId, int requestNumber, int appStatus);

	}
}



