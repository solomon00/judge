using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Solomon.TypesExtensions;
using NLog;
using System.Globalization;

namespace Solomon.Tester
{
    public class SocketConnection
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static CultureInfo culture = new CultureInfo(ConfigurationManager.AppSettings["DefaultCulture"]);

        #region Events
        public class ReceivedDataEventArgs : EventArgs
        {
            private readonly byte[] receivedData;

            public ReceivedDataEventArgs(byte[] ReceivedData)
            {
                receivedData = ReceivedData;
            }

            public byte[] ReceivedData { get { return receivedData; } }
        }
        public event EventHandler<ReceivedDataEventArgs> ReceivedData;
        #endregion

        private Socket socket;					// Server connection
        private IPEndPoint epServer;
        private byte[] buff = new byte[1024];	// Received data buffer
        private byte[] receivedData;
        private Int32 bytesToBeReceived = 0;
        private Int32 bytesActuallyReceived = 0;
        private Int32 bytesReceived = 0;

        public System.Timers.Timer CheckConnectionTimer;
        public bool WasReceivedRequest { get; set; }

        public SocketConnection()
        {
            //CultureInfo culture = new CultureInfo(ConfigurationManager.AppSettings["DefaultCulture"]);
            //Thread.CurrentThread.CurrentCulture = culture;
            //Thread.CurrentThread.CurrentUICulture = culture;

            logger.Info("Begin socket initialization");
            Console.WriteLine(DateTime.Now.ToString(culture) + " - Begin socket initialization");

            IPAddress serverAddress;
            IPAddress.TryParse(ConfigurationManager.AppSettings["Socket.ServerAddress"], out serverAddress);

            Int32 port;
            Int32.TryParse(ConfigurationManager.AppSettings["Socket.Port"], out port);

            // Create the socket object
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Define the Server address and port
            epServer = new IPEndPoint(serverAddress, port);

            //socket.Bind();

            // Connect to server non-Blocking method
            socket.Blocking = false;
            CheckConnectionTimer = new System.Timers.Timer(5000);
            CheckConnectionTimer.Elapsed += CheckConnectionTimer_Elapsed;
            SetupConnectCallback();
        }

        private void CheckConnectionTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            CheckConnectionTimer.Stop();

            if (logger.IsTraceEnabled)
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Check connection timer");
            if (!WasReceivedRequest)
            {
                Console.WriteLine(DateTime.Now.ToString(culture) + " - No request: Request not received in timer interval");
                logger.Warn("No request: Request not received in timer interval");
                if (!(socket.Connected && socket.IsConnected()))
                {
                    Console.WriteLine(DateTime.Now.ToString(culture) + " - Socket connection aborted: Setup connect callback");
                    logger.Warn("Socket connection aborted: Setup connect callback");
                    socket.Disconnect(true);
                    SetupConnectCallback();
                    return;
                }
            }
            WasReceivedRequest = false;

            CheckConnectionTimer.Start();
        }

        public void OnReceivedData(IAsyncResult ar)
        {
            logger.Trace("New data received");
            if (logger.IsTraceEnabled)
                Console.WriteLine(DateTime.Now.ToString(culture) + " - New data received");

            // Socket was the passed in object
            Socket sock = (Socket)ar.AsyncState;

            bytesReceived = 0;
            // Check if we got any data
            try
            {
                bytesReceived = sock.EndReceive(ar);
            }
            catch (SocketException ex)
            {
                logger.Warn("Error occurred on socket end receive: {0}", ex.Message);
            }

            int endIndex = 0;   // End of received data in buff
            int startIndex = 0; // Start ot received data in buff

            if (bytesReceived > 0)
            {
                try
                {
                    WasReceivedRequest = true;

                    endIndex = 0;   // End of received data in buff
                    startIndex = 0; // Start ot received data in buff
                    while (bytesReceived - endIndex > 0)
                    {
                        if (bytesToBeReceived == 0)
                        {
                            bytesToBeReceived = BitConverter.ToInt32(buff, endIndex);
                            receivedData = new byte[bytesToBeReceived];
                        }

                        // If received more then one request in one session
                        endIndex = bytesReceived > startIndex + bytesToBeReceived - bytesActuallyReceived
                            ? startIndex + bytesToBeReceived - bytesActuallyReceived
                            : bytesReceived;
                        Array.Copy(buff, startIndex, receivedData, bytesActuallyReceived, endIndex - startIndex);
                        bytesActuallyReceived += endIndex - startIndex;

                        //bytesReceived -= endIndex - startIndex;

                        if (bytesActuallyReceived >= bytesToBeReceived)
                        {
                            EventHandler<ReceivedDataEventArgs> temp = ReceivedData;
                            if (temp != null)
                                temp(this, new ReceivedDataEventArgs(receivedData));

                            bytesToBeReceived = bytesActuallyReceived = 0;

                            startIndex = endIndex;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error occurred on received data: {0}", ex.Message);
                    Console.WriteLine(DateTime.Now.ToString(culture) + " - Error occurred on received data: {0}", ex.Message);

                    bytesToBeReceived = bytesActuallyReceived = 0;
                }
                finally
                {
                    // If the connection is still usable restablish the callback
                    SetupRecieveCallback();
                }
            }
            else
            {
                // If no data was received then the connection is probably dead
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Socket connection aborted: Setup connect callback");
                logger.Warn("Socket connection aborted: Setup connect callback");
                sock.Disconnect(true);
                SetupConnectCallback();
                return;
            }
            
        }

        public void SetupRecieveCallback()
        {
            try
            {
                logger.Trace("Recieve callback setup");
                if (logger.IsTraceEnabled)
                    Console.WriteLine(DateTime.Now.ToString(culture) + " - Recieve callback setup");

                AsyncCallback receivedata = new AsyncCallback(OnReceivedData);
                socket.BeginReceive(buff, 0, buff.Length, SocketFlags.None, receivedata, socket);
            }
            catch (Exception ex)
            {
                logger.Error("Recieve callback setup failed: {0}", ex.Message);
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Recieve callback setup failed: {0}", ex.Message);
            }
        }

        public void SetupConnectCallback()
        {
            try
            {
                logger.Trace("Connect callback setup");
                if (logger.IsTraceEnabled)
                    Console.WriteLine(DateTime.Now.ToString(culture) + " - Connect callback setup");

                AsyncCallback onConnect = new AsyncCallback(OnConnect);
                socket.BeginConnect(epServer, onConnect, socket);
            }
            catch (Exception ex)
            {
                logger.Error("Connect callback setup failed: {0}", ex.Message);
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Connect callback setup failed: {0}", ex.Message);
            }
        }

        public void OnConnect(IAsyncResult ar)
        {
            // Socket was the passed in object
            Socket sock = (Socket)ar.AsyncState;

            // Check if we were sucessfull
            try
            {
                if (sock.Connected)
                {
                    SetupRecieveCallback();

                    Console.WriteLine(DateTime.Now.ToString(culture) + " - Connected to remote machine");
                    logger.Info("Connected to remote machine");
                    WasReceivedRequest = true;
                    CheckConnectionTimer.Start();
                }
                else
                {
                    Console.WriteLine(DateTime.Now.ToString(culture) + " - Unable to connect to remote machine");
                    Thread.Sleep(5000);     // Pause...
                    SetupConnectCallback();
                }
            }
            catch (Exception ex)
            {
                logger.Error("Unusual error during connect: {0}", ex.Message);
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Unusual error during connect: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Send data asynchronously.
        /// 
        /// Thread safety.
        /// </summary>
        /// <param name="data"></param>
        public void Send(byte[] Data, int Length)
        {
            try
            {
                // Begin sending the data to the remote device.
                socket.BeginSend(Data, 0, Length, SocketFlags.None,
                    new AsyncCallback(SendCallback), this);
            }
            catch (SocketException ex)
            {
                logger.Warn("Error occurred on socket begin send: {0}", ex.Message);
                if (!socket.IsConnected())
                {
                    try
                    {
                        Console.WriteLine(DateTime.Now.ToString(culture) + " - Socket connection aborted: {0}", ex.Message);
                        logger.Warn("Socket connection aborted: {0}", ex.Message);
                        socket.Disconnect(true);
                        SetupConnectCallback();
                    }
                    catch (Exception) { }
                }
            }
        }
        protected void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                SocketConnection sc = (SocketConnection)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = sc.socket.EndSend(ar);
                //Console.WriteLine(DateTime.Now.ToString(culture) + " - Sent {0} bytes to client.", bytesSent);
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred on sending data: {0}", ex.Message);
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Error occurred on sending data: {0}", ex.Message);
            }
        }
    }
}
