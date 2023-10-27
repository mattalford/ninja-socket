#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Code;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;

using System.Net.WebSockets;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Protechy
{
	public class NinjaSocket : Indicator, IDisposable
	{
		private ClientWebSocket client = new ClientWebSocket();
		private Uri serverUri;
		private const int MaxRetries = 5;
		private CancellationTokenSource cancellationTokenSource; 

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				cancellationTokenSource = new CancellationTokenSource(); 
				Description = @"A WebSocket client for Ninjatrader 8.";
				Name = "NinjaSocket";
				Calculate = Calculate.OnBarClose;
				IsOverlay = true;
				DisplayInDataBox = true;
				DrawOnPricePanel = true;
				DrawHorizontalGridLines = true;
				DrawVerticalGridLines = true;
				PaintPriceMarkers = true;
				ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive = true;
				ServerAddress = "ws://localhost:8000/ws/client1";
			}
			else if (State == State.Configure)    
			{
				cancellationTokenSource = new CancellationTokenSource();  
			    Uri uriResult;  
			    bool isValidUri = Uri.TryCreate(ServerAddress, UriKind.Absolute, out uriResult)  
			                      && (uriResult.Scheme == "ws" || uriResult.Scheme == "wss");  
			  
			    if (!isValidUri) {    
					Output.Process("Invalid URI", PrintTo.OutputTab1);
			        return;  
			    }  
			    serverUri = uriResult;  
			} 
			else if (State == State.DataLoaded)
			{
				ListenToWebSocketAsync();
			}
			else if (State == State.Terminated)
			{
				cancellationTokenSource.Cancel();
				Dispose();
			}
		}

		public async Task ConnectAsync()
		{
			if (client != null && serverUri != null)
			{
				await client.ConnectAsync(serverUri, CancellationToken.None).ConfigureAwait(false);
			}
		}
		
		public async Task ListenToWebSocketAsync()  
		{  
		    int retryDelay = 3000;  
		    int consecutiveFailures = 0;  
		    bool isConnected = false;  
		  
		    while (!cancellationTokenSource.Token.IsCancellationRequested && !isConnected)  
		    {  
		        int retryCount = 0;  
		        bool shouldRetry = false;  
		        do  
		        {  
		            try  
		            {  
		                if (client == null || client.State != WebSocketState.Open)  
		                {  
		                    client = new ClientWebSocket();  
		                    await ConnectAsync();  
		                    isConnected = true;  
							
		                }  
		  
		                shouldRetry = false;  
		                consecutiveFailures = 0;  // Reset the count of consecutive failures  
						
						if (client.State == WebSocketState.Open)    
						{    
							Output.Process("Connected to the server.", PrintTo.OutputTab1);      
						    ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[8192]);    
						    WebSocketReceiveResult result;
							retryCount = 0;
						     
						    while (client.State == WebSocketState.Open)    
						    {    
						        result = await client.ReceiveAsync(buffer, CancellationToken.None);    
								if (buffer.Array != null)
								{
									string receivedData = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
									Output.Process(receivedData, PrintTo.OutputTab1);
									
									ParseAndInvoke(receivedData); 
								}  
						    }   
						} 
		            }  
		            catch (OperationCanceledException)  
		            {  
						Output.Process("The token has been cancelled", PrintTo.OutputTab1);
		                return;  // Exit from method  
		            }  
		            catch (WebSocketException wex)  
		            {  
		                shouldRetry = true;  
		                retryCount++;  
		                Output.Process("Connection closed. Retry attempt: " + retryCount, PrintTo.OutputTab1);  
		                cancellationTokenSource.Token.ThrowIfCancellationRequested();  
		                Task.Delay(retryDelay).Wait();  
		            }  
		            catch (Exception ex)  
		            {  
		                if (++retryCount < MaxRetries)  
		                {  
		                    shouldRetry = true;  
		                    Output.Process("Connection failed.  " + retryCount + "  attempt(s)", PrintTo.OutputTab1);  
		                    cancellationTokenSource.Token.ThrowIfCancellationRequested();  
		                    Task.Delay(retryDelay).Wait();  
		                }  
		                else  
		                {  
		                    Output.Process("Failed to connect to WebSocket after " + retryCount + " attempts.", PrintTo.OutputTab1);  
		                    shouldRetry = false;  
		                    consecutiveFailures++;
		                }  
		                Output.Process("Connection error: " + ex.Message, PrintTo.OutputTab1);  
		            }  
		        } while (shouldRetry);  
		    }  
		}		

		public void Dispose()
		{
			if (client != null)
			{
				client.Abort();
				client.Dispose();
				client = null;
			}
		}
		
		public async Task SendDataAsync(string data)
		{
			if (client != null && client.State == WebSocketState.Open)
			{
				ArraySegment<byte> buffer = new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(data));
				await client.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None).ConfigureAwait(false);
			}
		}
		
		private void ParseAndInvoke(string receivedData)  
		{  
			Output.Process("Data: " + receivedData, PrintTo.OutputTab1);
		}
		
		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.  
		}

		[NinjaScriptProperty]
		[Display(Name = "Server Address", Description = "WebSocket server address to connect to", Order = 1, GroupName = "Parameters")]
		public string ServerAddress { get; set; }
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Protechy.NinjaSocket[] cacheNinjaSocket;
		public Protechy.NinjaSocket NinjaSocket(string serverAddress)
		{
			return NinjaSocket(Input, serverAddress);
		}

		public Protechy.NinjaSocket NinjaSocket(ISeries<double> input, string serverAddress)
		{
			if (cacheNinjaSocket != null)
				for (int idx = 0; idx < cacheNinjaSocket.Length; idx++)
					if (cacheNinjaSocket[idx] != null && cacheNinjaSocket[idx].ServerAddress == serverAddress && cacheNinjaSocket[idx].EqualsInput(input))
						return cacheNinjaSocket[idx];
			return CacheIndicator<Protechy.NinjaSocket>(new Protechy.NinjaSocket(){ ServerAddress = serverAddress }, input, ref cacheNinjaSocket);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Protechy.NinjaSocket NinjaSocket(string serverAddress)
		{
			return indicator.NinjaSocket(Input, serverAddress);
		}

		public Indicators.Protechy.NinjaSocket NinjaSocket(ISeries<double> input , string serverAddress)
		{
			return indicator.NinjaSocket(input, serverAddress);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Protechy.NinjaSocket NinjaSocket(string serverAddress)
		{
			return indicator.NinjaSocket(Input, serverAddress);
		}

		public Indicators.Protechy.NinjaSocket NinjaSocket(ISeries<double> input , string serverAddress)
		{
			return indicator.NinjaSocket(input, serverAddress);
		}
	}
}

#endregion
