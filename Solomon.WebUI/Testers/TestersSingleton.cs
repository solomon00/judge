using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using System.Web;
using Solomon.TypesExtensions;
using System.IO;
using Solomon.WebUI.Helpers;
using Solomon.Domain.Abstract;
using Ninject;
using Solomon.WebUI.Infrastructure;
using System.Threading;
using Solomon.Domain.Entities;
using log4net;
using System.Runtime.Serialization.Formatters.Binary;
using log4net.Config;
using System.Globalization;
using Solomon.Domain.Concrete;

namespace Solomon.WebUI.Testers
{
    internal sealed class TestersSingleton
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(TestersSingleton));

        #region Helpers
        private class SolutionObject
        {
            public Int32 ProblemID { get; private set; }
            public Int32 SolutionID { get; private set; }
            public ProgrammingLanguages PL { get; private set; }
            public TournamentFormats TF { get; private set; }
            public ProblemTypes PT { get; private set; }
            public string RelativePathToFile { get; private set; }
            public string FileName { get; private set; }

            public SolutionObject(Int32 ProblemID, Int32 SolutionID, 
                ProgrammingLanguages PL, TournamentFormats TF, ProblemTypes PT,
                string RelativePathToFile, string FileName)
            {
                this.ProblemID = ProblemID;
                this.SolutionID = SolutionID;
                this.PL = PL;
                this.TF = TF;
                this.PT = PT;
                this.RelativePathToFile = RelativePathToFile;
                this.FileName = FileName;
            }
        }
        #endregion

        private static StandardKernel _kernel = new StandardKernel(new FullNinjectModule());
        private WaitHandle[] _emptyThreads;

        private static TestersSingleton _instance;
        private IRepository _repository;
        private Int32 _portListen;
        private IPAddress[] _localAddress = null;
        private String _hostName = "";


        private Socket _listener;
        private List<SocketClient> _clients = new List<SocketClient>();
        private object _lockClients = new object();
        private Queue<SolutionObject> _solutionsQueue = new Queue<SolutionObject>();
        private object _lockSolutionsQueue = new object();
        private AutoResetEvent _solutionAdded = new AutoResetEvent(false);
        private AutoResetEvent _testerConnected = new AutoResetEvent(false);

        private TestersSingleton(IRepository Repository) 
        {
            XmlConfigurator.Configure();
            _repository = Repository;

            foreach (ProgrammingLanguages pl in (ProgrammingLanguages[])Enum.GetValues(typeof(ProgrammingLanguages)))
            {
                _repository.MakeProgrammingLanguageUnavailable(pl);
            }
        }
        ~TestersSingleton()
        {
            if (_listener != null)
                _listener.Close();
        }

        private void _initializeSoket()
        {
            _logger.Debug("Begin socket initialization");

            Int32.TryParse(ConfigurationManager.AppSettings["Socket.PortListen"], out _portListen);

            _logger.Info("Port listen: " + _portListen);

            // Determine the IPAddress of this machine
            try
            {
                // NOTE: DNS lookups are nice and all but quite time consuming.
                _hostName = Dns.GetHostName();
                IPHostEntry ipEntry = Dns.Resolve(_hostName);
                _localAddress = ipEntry.AddressList;
            }
            catch (Exception ex)
            {
                _logger.Error("Error occurred on trying to get local address:", ex);
                throw;
            }

            // Verify we got an IP address
            if (_localAddress == null || _localAddress.Length < 1)
            {
                _logger.Error("Unable to get local address");
                throw new ArgumentNullException("Unable to get local address");
            }

            // Create the listener socket in this machines IP address
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(new IPEndPoint(_localAddress[0], _portListen));
            _logger.Info("Bind to: " + _localAddress[0]);
            //_listener.Bind( new IPEndPoint( IPAddress.Loopback, _portListen ) );	// For use with localhost 127.0.0.1

            Int32 listen;
            Int32.TryParse(ConfigurationManager.AppSettings["Socket.ClientsCount"], out listen);
            if (listen == 0)
            {
                _logger.Info("Bad config setting 'Socket.ClientsCount'. Clients count is set to 10");
                listen = 10;
            }
            _listener.Listen(listen);

            // Setup a callback to be notified of connection requests
            _listener.BeginAccept(new AsyncCallback(_instance._onConnectRequest), _listener);
        }
        public static void Initialize()
        {
            if (_instance == null)
            {
                _instance = new TestersSingleton(_kernel.Get<IRepository>());
                
                Instance._initializeSoket();
                Thread t = new Thread(Instance._solutionsSending);
                t.Start();
            }
        }
        public static TestersSingleton Instance
        {
            get
            {
                if (_instance == null)
                    Initialize();

                return _instance;
            }
        }

        public List<SocketClient> Clients { get { return _clients; } }
        public String HostName { get { return _hostName; } }
        public IPAddress[] LocalAddress { get { return _localAddress; } }
        public int PortListen { get { return _portListen; } }
        public Int32 Count { get { return _clients.Count; } }

        /// <summary>
		/// Callback used when a client requests a connection. 
		/// Accpet the connection, adding it to our list and setup to 
		/// accept more connections.
		/// </summary>
		/// <param name="ar"></param>
		private void _onConnectRequest(IAsyncResult ar)
		{
            _logger.Info("New socket connection requested");

			Socket listener = (Socket)ar.AsyncState;
			_newConnection(listener.EndAccept(ar));
			listener.BeginAccept(new AsyncCallback(_onConnectRequest), listener);
		}

		/// <summary>
		/// Add the given connection to our list of clients
		/// Setup a callback to recieve data
		/// </summary>
		/// <param name="sockClient">Connection to keep</param>
		private void _newConnection(Socket sockClient)
		{
            _logger.Info("Begin new socket connection initialization");

			// Program blocks on Accept() until a client connects.
			//SocketChatClient client = new SocketChatClient( listener.AcceptSocket() );
            SocketClient client = new SocketClient(sockClient, _kernel.Get<IRepository>());
            client.ClientConnected += _clientConnected;

            // Add new client to list of clients and add new free thread for checking solution.
            lock (this._lockClients)
            {
                _clients.Add(client);
                Array.Resize<WaitHandle>(ref _emptyThreads, _emptyThreads.Length + 1);
                _emptyThreads[_emptyThreads.Length - 1] = client.SolutionChecked;
            }

            client.SendRequestForMainInfo();

			client.SetupReceiveCallback();
		}

        private void _clientConnected(Object sender, EventArgs e)
        {
            _testerConnected.Set();
        }

        /// <summary>
        /// Remove clients that lost a connection
        /// </summary>
        public void RemoveDisconnectedClients()
        {
            List<SocketClient> forRemove = new List<SocketClient>();

            Clients.Each(c =>
                {
                    if (c.IsConnected == false)
                        forRemove.Add(c);
                });

            forRemove.Each(c => Clients.Remove(c));
        }

        /// <summary>
        /// Send problem to clients
        /// </summary>
        public void SendProblem(int problemID)
        {
            Clients.Each(c => 
                {
                    if (c.IsConnected)
                    {
                        c.SendProblem(problemID);
                        _logger.Info("Problem " + problemID + " send to tester " + c.Address);
                    }
                });
        }

        /// <summary>
        /// Send compiler options to clients
        /// </summary>
        public void SendCompilerOptions()
        {
            Clients.Each(c =>
            {
                if (c.IsConnected)
                {
                    c.SendCompilerOptions();
                    _logger.Info("CompilerOptions send to tester " + c.Address);
                }
            });
        }

        /// <summary>
        /// Delete problem from clients
        /// </summary>
        public void DeleteProblem(int problemID)
        {
            Clients.Each(c => c.DeleteProblem(problemID));
        }

        /// <summary>
        /// Recheck solution
        /// </summary>
        public void RecheckSolution(int solutionID)
        {
            Solution solution = _repository.Solutions.FirstOrDefault(s => s.SolutionID == solutionID);

            if (solution == null)
            {
                _logger.Warn("Error occurred on solution rechecking: Solution with id = " + solutionID + " not found");
                throw new ArgumentException("Solution with id = " + solutionID + " not found");
            }

            try
            {
                _repository.DeleteSolutionTestResults(solutionID);
            }
            catch (Exception ex)
            {
                _logger.Warn("Exception occurred on DeleteSolutionTestResults with solutionID = " + solutionID, ex);
                throw;
            }

            solution.Result = TestResults.Waiting;
            solution.Score = 0;
            _repository.SaveSolution(solution);

            AddSolutionForChecking(solution.ProblemID, solution.SolutionID,
                solution.ProgrammingLanguage, solution.Tournament.Format, solution.Problem.Type,
                solution.Path, solution.FileName);
        }

        /// <summary>
        /// Add solution for checking
        /// </summary>
        public void AddSolutionForChecking(Int32 problemID, Int32 solutionID, 
            ProgrammingLanguages PL, TournamentFormats TF, ProblemTypes PT,
            string relativePathToFile, string fileName)
        {
            lock (this._lockSolutionsQueue)
            {
                _solutionsQueue.Enqueue(new SolutionObject(problemID, solutionID, PL, TF, PT, relativePathToFile, fileName));
                _solutionAdded.Set();

                _logger.Info("Solution " + solutionID + " for problem " + problemID + " on PL " + PL + " added to queue");
            }
        }

        // Queue processing
        private void _solutionsSending()
        {
            _emptyThreads = new WaitHandle[] { _testerConnected };
            SolutionObject solution;
            int clientsCount = 0;
            while (true)
            {
                solution = null;

                lock (this._lockClients)
                {
                    clientsCount = Clients.Count(c => c.IsConnected == true);
                }

                // Wait for connected clients
                if (clientsCount == 0)
                {
                    _logger.Debug("No connected testers found, wait for connection");
                    _testerConnected.WaitOne();
                    _logger.Debug("Tester connected");
                    continue;
                }

                // Get solution from queue
                lock (this._lockSolutionsQueue)
                {
                    if (_solutionsQueue.Count > 0)
                        solution = _solutionsQueue.Dequeue();
                }

                if (solution == null)
                {
                    _logger.Debug("No solutions found in queue, wait for adding solution");
                    _solutionAdded.WaitOne();
                    _logger.Debug("Solution added");
                    continue;
                }

                // At this section solution != null. Send it to free tester.
                while (solution != null)
                {
                    lock (this._lockClients)
                    {
                        foreach (var client in Clients)
                        {
                            // Check if the client can test solution
                            if (client.IsConnected && 
                                client.ClientCompilers.Contains(solution.PL) && 
                                client.Problems.Contains(pi => pi.ProblemID == solution.ProblemID) &&
                                client.ClientFreeThreadsCount > 0)
                            {
                                client.SendSolutionFile(solution.ProblemID, solution.SolutionID,
                                    solution.PL, solution.TF, solution.PT, solution.RelativePathToFile, solution.FileName);

                                _logger.Info("Solution " + solution.SolutionID + " for problem " + solution.ProblemID + 
                                    " on PL " + solution.PL + " send to tester " + client.Address);

                                solution = null;
                                break;
                            }
                        }
                    }

                    if (solution != null)
                    {
                        _logger.Debug("All testers are busy, wait for free tester");
                        WaitHandle.WaitAny(_emptyThreads);
                        _logger.Debug("Empty thread");
                    }
                }
            }
        }
	}
}