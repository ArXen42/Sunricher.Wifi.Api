﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sunricher.Wifi.Api;

// ReSharper disable AccessToDisposedClosure

namespace Sunricher.Wifi.CommandLine
{
	internal static class Program
	{
		private static void Main(String[] args)
		{
			var messagesGenerator = new MessagesGenerator();
			var messagesProvider = new MessagesProvider(messagesGenerator);
			var random = new Random();

			//Just an example of how to use API, pretty bad example
			using (var client = new SunricherTcpClient("192.168.12.194", ApiConstants.DefaultTcpPort))
			{
				client.MessageSent += (s, e) =>
				{
					String messageStr = String.Join("-",
						e.Message.Select(b => Convert.ToString(b, 16).PadLeft(2, '0')));

					Console.WriteLine($"Message sent: {messageStr}");
				};

				var cts = new CancellationTokenSource();
				var ct = cts.Token;
				var task = Task.Run(() =>
				{
					try
					{
						client.SendMessageAsync(messagesProvider.PowerOn(), ct).Wait(ct);
						while (true)
						{
							if (ct.IsCancellationRequested)
								break;

							client.SendMessageAsync(messagesProvider.SetRed((Byte) random.Next(0, 100)), ct).Wait();
							client.SendMessageAsync(messagesProvider.SetGreen((Byte) random.Next(0, 100)), ct).Wait();
							client.SendMessageAsync(messagesProvider.SetBlue((Byte) random.Next(0, 100)), ct).Wait();
							client.SendMessageAsync(messagesProvider.SetWhite((Byte) random.Next(0, 100)), ct).Wait();
							client.SendMessageAsync(messagesProvider.SetBrightness((Byte) random.Next(0, 255)), ct).Wait();
							Thread.Sleep(1000);
						}
					}
					catch (Exception)
					{
						cts.Cancel();
						throw;
					}
				}, ct);

				Console.ReadKey();
				cts.Cancel();
				task.Wait(TimeSpan.FromSeconds(10));
			}
		}
	}
}