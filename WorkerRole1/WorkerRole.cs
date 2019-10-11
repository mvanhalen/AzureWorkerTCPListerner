using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        Int32 port = 8000;
        IPAddress localAddr;
        TcpListener server = null;
        TcpClient client;
        public override void Run()
        {
            Trace.TraceInformation("WorkerRole1 is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("WorkerRole1 has been started");

            // Set the TcpListener on port 25.
            
            // IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            // TcpListener server = new TcpListener(port);

            localAddr = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["Endpoint1"].IPEndpoint.Address;
            server = new TcpListener(localAddr, port);

            // Start listening for client requests.
            server.Start();

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole1 is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("WorkerRole1 has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
          
            try
            {
               

                // Buffer for reading data
                

                // Enter the listening loop.
                while (true)
                {
                    Trace.WriteLine("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    client = server.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(ThreadProc, client);

                 
                }
            }
            catch (SocketException e)
            {
                Trace.WriteLine("SocketException: {0}", e.ToString());
            }
            finally
            {
                if (server != null)
                {
                    // Stop listening for new clients.
                    server.Stop();
                }
            }
        }
        private static void ThreadProc(object obj)
        {
            Byte[] bytes = new Byte[256];
            String data = null;
            var client = (TcpClient)obj;
            // Do your work here
            Trace.WriteLine("Connected!");

            data = null;

            // Get a stream object for reading and writing
            NetworkStream stream = client.GetStream();

            int i;

            // Loop to receive all the data sent by the client.
            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                // Translate data bytes to a ASCII string.
                data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                Trace.WriteLine("Received: " + data);

                // Process the data sent by the client.
                data = data.ToUpper();

                byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                // Send back a response.
                stream.Write(msg, 0, msg.Length);
                Trace.WriteLine("Sent: " + data);
            }

            // Shutdown and end connection
            client.Close();
        }
    }
}
