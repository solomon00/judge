using log4net;
using log4net.Config;
using Ninject;
using Solomon.Domain.Abstract;
using Solomon.Domain.Entities;
using Solomon.TypesExtensions;
using Solomon.WebUI.Helpers;
using Solomon.WebUI.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Timers;
using System.Web;

namespace Solomon.WebUI.Testers
{
    /// <summary>
    /// Class holding information and buffers for the Client socket connection
    /// </summary>
    internal class SocketClient
    {
        private static StandardKernel _kernel = new StandardKernel(new FullNinjectModule());
        private IRepository _repository;
        private readonly ILog _logger = LogManager.GetLogger(typeof(SocketClient));

        #region Client information
        /// <summary>
        /// Client connection
        /// </summary>
        protected Socket _socket;
        protected EndPoint _endPoint;
        protected Boolean _isConnected;
        protected Int32 _clientVProcessorsCount;
        protected Int32 _clientFreeThreadsCount;
        protected Int32 _clientCPULoad;
        /// <summary>
        /// List of compilers available on the client
        /// </summary>
        protected List<ProgrammingLanguages> _clientCompilers = new List<ProgrammingLanguages>();
        /// <summary>
        /// List of problems available on the client
        /// </summary>
        protected List<ProblemInfo> _problems = new List<ProblemInfo>();
        #endregion

        private AutoResetEvent _clientReadyForReceivingProblem = new AutoResetEvent(false);
        private AutoResetEvent _clientReceivedProblem = new AutoResetEvent(false);
        private AutoResetEvent _clientReadyForReceivingCompilerOptions = new AutoResetEvent(false);
        private AutoResetEvent _clientReceivedCompilerOptions = new AutoResetEvent(false);
        private AutoResetEvent _clientDeletedProblem = new AutoResetEvent(false);
        private AutoResetEvent _clientSendProblemsInfo = new AutoResetEvent(false);
        private ManualResetEventSlim _canSendRequest = new ManualResetEventSlim(true);
        private AutoResetEvent _solutionChecked = new AutoResetEvent(true);

        private byte[] _buff = new byte[1024];	// Receive data buffer
        private byte[] _receivedData;            // All receive message buffer
        private Int32 _bytesToBeReceived = 0;
        private Int32 _bytesActuallyReceived = 0;
        private Int32 _bytesReceived = 0;

        /// <summary>
        /// Timer for sending request for cpu usage
        /// </summary>
        private System.Timers.Timer _timer;

        #region Fields
        public Int32 ClientVirtualProcessorsCount
        {
            get { return _clientVProcessorsCount; }
            protected set { _clientVProcessorsCount = value; }
        }
        public Int32 ClientFreeThreadsCount
        {
            get { return _clientFreeThreadsCount; }
            protected set { _clientFreeThreadsCount = value; }
        }
        public Int32 ClientCPULoad
        {
            get { return _clientCPULoad; }
            protected set { _clientCPULoad = value; }
        }
        public List<ProgrammingLanguages> ClientCompilers
        {
            get { return _clientCompilers; }
            protected set { _clientCompilers = value; }
        }
        public List<ProblemInfo> Problems
        {
            get { return _problems; }
            protected set { _problems = value; }
        }
        public EndPoint Address
        {
            get { return _endPoint; }
            protected set { _endPoint = value; }
        }
        public Boolean IsConnected
        {
            get { return _isConnected && _socket.IsConnected(); }
            protected set { _isConnected = value; }
        }
        public AutoResetEvent SolutionChecked { get { return _solutionChecked; } }
        #endregion

        private readonly Object _eventLock = new Object();
        private EventHandler _clientConnected;
        public event EventHandler ClientConnected
        {
            add
            {
                lock (_eventLock) { _clientConnected += value; }
            }
            remove
            {
                lock (_eventLock) { _clientConnected -= value; }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="socket">client socket conneciton this object represents</param>
        public SocketClient(Socket socket, IRepository repository)
        {
            XmlConfigurator.Configure();
            CultureInfo culture = new CultureInfo("ru-RU");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            _repository = repository;

            _socket = socket;
            IsConnected = true;

            Address = _socket.RemoteEndPoint;

            _timer = new System.Timers.Timer(2000);
            _timer.Elapsed += timer_Elapsed;
            _timer.Start();

            _logger.Info("New socket client connected: " + Address);

        }

        /// <summary>
        /// Timer for checking client CPU usage.
        /// Send the key value to client for getting cpu usage.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            SendRequestForCPUUasage();
            _timer.Start();
        }

        /// <summary>
        /// Setup the callback for recieved data and loss of conneciton
        /// </summary>
        /// <param name="app"></param>
        public void SetupReceiveCallback()
        {
            try
            {
                _logger.Debug(Address + ": Recieve callback setup");

                AsyncCallback recieveData = new AsyncCallback(this.OnRecievedData);
                _socket.BeginReceive(_buff, 0, _buff.Length, SocketFlags.None, recieveData, _socket);
            }
            catch (Exception ex)
            {
                //if (IsConnected == false)
                //    TestersSingleton.Instance.DeleteDisconnectedClients();

                _logger.Error(Address + ": Recieve callback setup failed:", ex);
            }
        }

        protected void receiveMainInfo(byte[] data, IRepository repository)
        {
            ClientVirtualProcessorsCount = BitConverter.ToInt32(data, 8);
            ClientFreeThreadsCount = ClientVirtualProcessorsCount * 3 / 2;

            int compilersCount = BitConverter.ToInt32(data, 12);
            ClientCompilers.Clear();
            for (int i = 1; i <= compilersCount; i++)
            {
                ProgrammingLanguages pl = (ProgrammingLanguages)BitConverter.ToInt32(data, 12 + 4 * i);
                ClientCompilers.Add(pl);
                repository.MakeProgrammingLanguageAvailable(pl);

                Compilers.AddLanguage(pl);
            }

            SendRequestForProblemsInfo();
            _clientSendProblemsInfo.WaitOne();
            SendNewProblems();
        }
        protected void receiveProblemsInfo(byte[] data, IRepository repository)
        {
            Int32 fileNameLength = BitConverter.ToInt32(data, 8);
            string fileName = Encoding.ASCII.GetString(data, 12, fileNameLength);

            try
            {
                if (!Directory.Exists(LocalPath.AbsoluteTestersDirectory))
                    Directory.CreateDirectory(LocalPath.AbsoluteTestersDirectory);

                if (File.Exists(LocalPath.AbsoluteTestersDirectory + Address))
                    File.Delete(LocalPath.AbsoluteTestersDirectory + Address);

                using (BinaryWriter bw = new BinaryWriter(
                    File.Open(LocalPath.AbsoluteTestersDirectory + Address.ToString().Replace(':', '.'), FileMode.Create)))
                {
                    bw.Write(data, fileNameLength + 12, data.Length - fileNameLength - 12);
                }

                using (Stream stream = File.Open(
                    LocalPath.AbsoluteTestersDirectory + Address.ToString().Replace(':', '.'), FileMode.Open))
                {
                    BinaryFormatter bformatter = new BinaryFormatter();

                    _logger.Debug("Reading problems info from " + Address);

                    Problems = (List<ProblemInfo>)bformatter.Deserialize(stream);
                }

                _logger.Debug("Problems info from " + Address + " saved");

                EventHandler temp = _clientConnected;
                if (temp != null) temp(this, null);

                _clientSendProblemsInfo.Set();
            }
            catch (Exception ex)
            {
                _logger.Error("Error occurred on problems info from " + Address + " saving: ", ex);
            }
        }
        protected void solutionChecked(byte[] data, IRepository repository)
        {
            ClientFreeThreadsCount++;
            SolutionChecked.Set();

            int solutionID = BitConverter.ToInt32(data, 8);

            _logger.Info("Begin saving solution checked info from " + Address + ", for solution " + solutionID);

            Solution solution = repository.Solutions.FirstOrDefault(s => s.SolutionID == solutionID);

            ProblemTypes pt = (ProblemTypes)BitConverter.ToInt32(data, 12);
            TestResults result = (TestResults)BitConverter.ToInt32(data, 16);
            int score = BitConverter.ToInt32(data, 20);
            int testsCount = BitConverter.ToInt32(data, 24);

            _logger.Debug("Received " + testsCount + " tests results");

            solution.Result = result;
            solution.Score = score;

            if (pt == ProblemTypes.Standart)
            {
                int errorOnTest = 0;
                SolutionTestResult str;
                for (int i = 0; i < testsCount; i++)
                {
                    str = new SolutionTestResult()
                    {
                        SolutionID = solutionID,
                        Time = BitConverter.ToInt64(data, 28 + 20 * i),
                        Memory = BitConverter.ToInt64(data, 36 + 20 * i),
                        Result = (TestResults)BitConverter.ToInt32(data, 44 + 20 * i)
                    };

                    if (str.Result != TestResults.OK && errorOnTest == 0)
                        errorOnTest = i + 1;

                    repository.AddSolutionTestResult(str);
                }

                solution.ErrorOnTest = errorOnTest;
            }

            if (solution.Result == TestResults.OK)
            {
                solution.User.NotSolvedProblems.Remove(solution.Problem);
                solution.User.SolvedProblems.Add(solution.Problem);
            }
            else
            {
                if (!solution.User.SolvedProblems.Contains(solution.Problem))
                {
                    solution.User.NotSolvedProblems.Add(solution.Problem);
                }
            }
            repository.SaveSolution(solution);

            _logger.Info("Solution checked info from " + Address + ", for solution " + solutionID + " saved");
            _logger.Info("Solution " + solutionID + " checked: Result " + result + ", Score " + score);
        }
        /// <summary>
        /// Translate recieved data. Get the key value and the message.
        /// </summary>
        /// <param name="data"></param>
        protected void translateReceivedData(byte[] data)
        {
            IRepository repository = _kernel.Get<IRepository>();

            Int32 code = BitConverter.ToInt32(data, 4);

            switch ((ResponseCodes)code)
            {
                case ResponseCodes.MainInfo:
                    receiveMainInfo(data, repository);
                    break;
                case ResponseCodes.ProblemsInfo:
                    receiveProblemsInfo(data, repository);
                    break;
                case ResponseCodes.CPUUsage:
                    ClientCPULoad = BitConverter.ToInt32(data, 8);
                    break;
                case ResponseCodes.SolutionFileChecked:
                    solutionChecked(data, repository);
                    break;
                case ResponseCodes.ReadyForReceivingProblem:
                    _clientReadyForReceivingProblem.Set();
                    break;
                case ResponseCodes.ProblemFileReceived:
                    _clientReceivedProblem.Set();
                    break;
                case ResponseCodes.ProblemDeleted:
                    _clientDeletedProblem.Set();
                    break;
                case ResponseCodes.ReadyForReceivingCompilerOptions:
                    _clientReadyForReceivingCompilerOptions.Set();
                    break;
                case ResponseCodes.CompilerOptionsFileReceived:
                    _clientReceivedCompilerOptions.Set();
                    break;
            }
        }

        /// <summary>
        /// Get the new data and translate it.
        /// </summary>
        /// <param name="ar"></param>
        protected void OnRecievedData(IAsyncResult ar)
        {
            _logger.Debug(Address + ": New data received");

            Socket client = (Socket)ar.AsyncState;

            // Check if we got any data
            try
            {
                _bytesReceived = client.EndReceive(ar);
            }
            catch (Exception ex)
            {
                _logger.Error(Address + ": Unusual error during Recieve: ", ex);
                _bytesReceived = 0;
            }

            int endIndex = 0;   // End of received data in buff
            int startIndex = 0; // Start of received data in buff

            if (_bytesReceived > 0)
            {
                try
                {
                    endIndex = 0;   // End of received data in buff
                    startIndex = 0; // Start of received data in buff
                    while (_bytesReceived - endIndex > 0)
                    {
                        if (_bytesToBeReceived == 0)
                        {
                            _bytesToBeReceived = BitConverter.ToInt32(_buff, endIndex);
                            _receivedData = new byte[_bytesToBeReceived];
                        }

                        endIndex = _bytesReceived > startIndex + _bytesToBeReceived - _bytesActuallyReceived
                            ? startIndex + _bytesToBeReceived - _bytesActuallyReceived
                            : _bytesReceived;
                        Array.Copy(_buff, startIndex, _receivedData, _bytesActuallyReceived, endIndex - startIndex);
                        _bytesActuallyReceived += endIndex - startIndex;

                        if (_bytesActuallyReceived >= _bytesToBeReceived)
                        {
                            byte[] data = new byte[_receivedData.Length];
                            Array.Copy(_receivedData, data, _receivedData.Length);

                            ThreadPool.QueueUserWorkItem(s => translateReceivedData(data));

                            _bytesToBeReceived = _bytesActuallyReceived = 0;

                            startIndex = endIndex;
                        }
                    }
                }
                catch (Exception ex)
                {
                    //if (IsConnected == false)
                    //    TestersSingleton.Instance.DeleteDisconnectedClients();

                    _logger.Error(Address + ": Unusual error during Recieve: ", ex);

                    _bytesToBeReceived = _bytesActuallyReceived = 0;
                }
                finally
                {
                    // If the connection is still usable restablish the callback
                    SetupReceiveCallback();
                }
            }
            else
            {
                try
                {
                    // If no data was received then the connection is probably dead
                    IsConnected = false;
                    _timer.Stop();
                    _clientCompilers.Each(c =>
                    {
                        Compilers.RemoveLanguage(c);
                        if (!Compilers.AvailableLanguages.Contains(c))
                            _repository.MakeProgrammingLanguageUnavailable(c);
                    });
                    _clientCompilers.Clear();
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();

                    _logger.Info(Address + ": Disconnected");
                    return;
                }
                catch (Exception) { }
            }
        }


        #region Send functions

        /// <summary>
        /// Send request for main info of client
        /// </summary>
        public void SendRequestForMainInfo()
        {
            ThreadPool.QueueUserWorkItem(s =>
            {
                SendCompilerOptions();
            });
        }

        /// <summary>
        /// Send request for cpu usage of client
        /// </summary>
        public void SendRequestForCPUUasage()
        {
            _canSendRequest.Wait();

            byte[] data = new byte[4 + 4];
            BitConverter.GetBytes(4 + 4).CopyTo(data, 0);
            BitConverter.GetBytes((Int32)RequestCodes.CPUUsage).CopyTo(data, 4);

            Send(data, data.Length);

            _logger.Debug(Address + ": Send request for cpu usage");
        }

        /// <summary>
        /// Extract information about solution from database and send it to client.
        /// </summary>
        public void SendSolutionFile(Int32 solutionID)
        {
            Solution solution = _repository
                .Solutions
                .FirstOrDefault(s => s.SolutionID == solutionID);

            if (solution == null)
            {
                throw new KeyNotFoundException(solutionID.ToString());
            }

            SendSolutionFile(
                solution.ProblemID,
                solution.SolutionID,
                solution.ProgrammingLanguage,
                solution.Tournament.Format,
                solution.Problem.Type,
                solution.Path,
                solution.FileName);
        }
        /// <summary>
        /// Send information about solution to client.
        /// </summary>
        public void SendSolutionFile(Int32 problemID, Int32 solutionID,
            ProgrammingLanguages PL, TournamentFormats TF, ProblemTypes PT,
            string relativePathToFile, string fileName)
        {
            _canSendRequest.Wait();
            ClientFreeThreadsCount--;

            string absolutePath = Path.Combine(LocalPath.RootDirectory, relativePathToFile);

            if (!File.Exists(absolutePath))
            {
                throw new FileNotFoundException(absolutePath);
            }

            string extension = Path.GetExtension(fileName);
            if (extension == "")
            {
                switch (PL)
                {
                    case ProgrammingLanguages.C:
                        extension = ".c";
                        break;
                    case ProgrammingLanguages.CPP:
                        extension = ".cpp";
                        break;
                    case ProgrammingLanguages.CS:
                        extension = ".cs";
                        break;
                    case ProgrammingLanguages.Pascal:
                        extension = ".pas";
                        break;
                    case ProgrammingLanguages.Python:
                        extension = ".py";
                        break;
                    case ProgrammingLanguages.VB:
                        extension = ".vb";
                        break;
                    default:
                        _logger.Warn("Send solution " + solutionID + " file without extension");
                        break;
                }
            }


            string fName = PL == ProgrammingLanguages.Java ? fileName : solutionID.ToString() + extension;
            byte[] name = Encoding.ASCII.GetBytes(fName);
            byte[] fileData = File.ReadAllBytes(absolutePath);
            byte[] clientData = new byte[4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + name.Length + fileData.Length];
            byte[] fileNameLen = BitConverter.GetBytes(name.Length);
            BitConverter.GetBytes(clientData.Length).CopyTo(clientData, 0);
            BitConverter.GetBytes((Int32)RequestCodes.SolutionFile).CopyTo(clientData, 4);
            BitConverter.GetBytes(problemID).CopyTo(clientData, 8);
            BitConverter.GetBytes(solutionID).CopyTo(clientData, 12);
            BitConverter.GetBytes((Int32)PL).CopyTo(clientData, 16);
            BitConverter.GetBytes((Int32)TF).CopyTo(clientData, 20);
            BitConverter.GetBytes((Int32)PT).CopyTo(clientData, 24);
            fileNameLen.CopyTo(clientData, 28);
            name.CopyTo(clientData, 32);
            fileData.CopyTo(clientData, 32 + name.Length);

            Send(clientData, clientData.Length);

            _logger.Debug(Address + ": Send solution " + solutionID + " file");
        }

        /// <summary>
        /// Send all problems to client
        /// </summary>
        public void SendAllProblems()
        {
            ThreadPool.QueueUserWorkItem(s =>
            {
                IRepository repository = _kernel.Get<IRepository>();

                _canSendRequest.Wait();
                _canSendRequest.Reset();
                SendRequestForReceivingProblem();
                _clientReadyForReceivingProblem.WaitOne();

                repository.Problems
                    .Each(p =>
                    {
                        try
                        {
                            SendProblemFile(p.ProblemID);
                            _clientReceivedProblem.WaitOne();
                        }
                        catch (Exception ex)
                        {
                            _logger.Error("Error occurred on problem " + p.ProblemID + " sending: ", ex);
                        }
                    });

                SendRequestForEndOfReceivingProblem();
                _canSendRequest.Set();

                SendRequestForProblemsInfo();
            });
        }

        /// <summary>
        /// Send non-existing and updated problems to tester
        /// </summary>
        public void SendNewProblems()
        {
            ThreadPool.QueueUserWorkItem(s =>
            {
                IRepository repository = _kernel.Get<IRepository>();

                _logger.Info("Sending new problems");
                _logger.Debug("Wait for canSendRequest");
                _canSendRequest.Wait();
                _canSendRequest.Reset();
                _logger.Debug("SendRequestForReceivingProblem");
                SendRequestForReceivingProblem();
                _logger.Debug("Wait for clientReadyForReceivingProblem");
                _clientReadyForReceivingProblem.WaitOne();

                repository.Problems.Each(p =>
                {
                    try
                    {
                        ProblemInfo problemInfo = Problems.FirstOrDefault(pi => pi.ProblemID == p.ProblemID);

                        if (problemInfo == null ||
                            DateTime.Compare(
                                problemInfo.LastModifiedTime.Truncate(TimeSpan.FromSeconds(1)),
                                p.LastModifiedTime.Truncate(TimeSpan.FromSeconds(1))) < 0)
                        {
                            SendProblemFile(p.ProblemID);
                            _clientReceivedProblem.WaitOne();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Error occurred on problem " + p.ProblemID + " sending: ", ex);
                    }
                });

                _logger.Debug("SendRequestForEndOfReceivingProblem");
                SendRequestForEndOfReceivingProblem();
                _logger.Debug("canSendRequest");
                _canSendRequest.Set();

                _logger.Debug("SendRequestForProblemsInfo");
                SendRequestForProblemsInfo();
            });
        }

        /// <summary>
        /// Send problem file to client
        /// </summary>
        /// <param name="ProblemID"></param>
        public void SendProblem(Int32 ProblemID)
        {
            ThreadPool.QueueUserWorkItem(s =>
            {
                _canSendRequest.Wait();
                _canSendRequest.Reset();
                SendRequestForReceivingProblem();
                _clientReadyForReceivingProblem.WaitOne();

                try
                {
                    SendProblemFile(ProblemID);
                }
                catch (Exception ex)
                {
                    _logger.Error("Error occurred on problem " + ProblemID + " sending: ", ex);
                }

                _clientReceivedProblem.WaitOne();
                SendRequestForEndOfReceivingProblem();
                _canSendRequest.Set();

                SendRequestForProblemsInfo();
            });
        }

        /// <summary>
        /// Send compiler options file to client
        /// </summary>
        public void SendCompilerOptions()
        {
            ThreadPool.QueueUserWorkItem(s =>
            {
                _canSendRequest.Wait();
                _canSendRequest.Reset();
                SendRequestForReceivingCompilerOptions();
                _clientReadyForReceivingCompilerOptions.WaitOne();

                try
                {
                    SendCompilerOptionsFile();
                }
                catch (Exception ex)
                {
                    _logger.Error("Error occurred on compiler options sending: ", ex);
                }

                _clientReceivedCompilerOptions.WaitOne();
                _canSendRequest.Set();
            });
        }

        public void SendRequestForProblemsInfo()
        {
            _canSendRequest.Wait();

            byte[] data = new byte[4 + 4];
            BitConverter.GetBytes(4 + 4).CopyTo(data, 0);
            BitConverter.GetBytes((Int32)RequestCodes.ProblemsInfo).CopyTo(data, 4);

            Send(data, data.Length);

            _logger.Debug(Address + ": Send request for problems info");
        }

        /// <summary>
        /// Send delete problem command to client
        /// </summary>
        /// <param name="ProblemID"></param>
        public void DeleteProblem(Int32 ProblemID)
        {
            ThreadPool.QueueUserWorkItem(s =>
            {
                _canSendRequest.Wait();
                _canSendRequest.Reset();

                SendRequestForDeletingProblem(ProblemID);
                _clientDeletedProblem.WaitOne();

                _canSendRequest.Set();

                SendRequestForProblemsInfo();
            });
        }

        protected void SendRequestForReceivingProblem()
        {
            byte[] data = new byte[4 + 4];
            BitConverter.GetBytes(4 + 4).CopyTo(data, 0);
            BitConverter.GetBytes((Int32)RequestCodes.ReadyForReceivingProblem).CopyTo(data, 4);

            Send(data, data.Length);

            _logger.Debug(Address + ": Send request for receiving problem");
        }
        protected void SendRequestForEndOfReceivingProblem()
        {
            byte[] data = new byte[4 + 4];
            BitConverter.GetBytes(4 + 4).CopyTo(data, 0);
            BitConverter.GetBytes((Int32)RequestCodes.EndOfReceivingProblem).CopyTo(data, 4);

            Send(data, data.Length);

            _logger.Debug(Address + ": Send request for end of receiving problem");
        }
        protected void SendProblemFile(Int32 ProblemID)
        {
            if (!File.Exists(LocalPath.AbsoluteProblemsDirectory + ProblemID.ToString() + "/" + ProblemID.ToString() + ".zip"))
            {
                throw new FileNotFoundException(LocalPath.AbsoluteProblemsDirectory + ProblemID.ToString() + "/" + ProblemID.ToString() + ".zip");
            }

            byte[] fileName = Encoding.ASCII.GetBytes(ProblemID.ToString());
            byte[] fileData = File.ReadAllBytes(LocalPath.AbsoluteProblemsDirectory + ProblemID.ToString() + "/" + ProblemID.ToString() + ".zip");
            byte[] clientData = new byte[4 + 4 + 4 + fileName.Length + fileData.Length];
            byte[] fileNameLen = BitConverter.GetBytes(fileName.Length);
            BitConverter.GetBytes(clientData.Length).CopyTo(clientData, 0);
            BitConverter.GetBytes((Int32)RequestCodes.ProblemFile).CopyTo(clientData, 4);
            fileNameLen.CopyTo(clientData, 8);
            fileName.CopyTo(clientData, 12);
            fileData.CopyTo(clientData, 12 + fileName.Length);

            Send(clientData, clientData.Length);

            _logger.Debug(Address + ": Send problem " + ProblemID + " file");
        }
        protected void SendRequestForReceivingCompilerOptions()
        {
            byte[] data = new byte[4 + 4];
            BitConverter.GetBytes(4 + 4).CopyTo(data, 0);
            BitConverter.GetBytes((Int32)RequestCodes.ReadyForReceivingCompilerOptions).CopyTo(data, 4);

            Send(data, data.Length);

            _logger.Debug(Address + ": Send request for receiving compiler options");
        }
        protected void SendCompilerOptionsFile()
        {
            if (!File.Exists(Path.Combine(LocalPath.AbsoluteCompilerOptionsDirectory, "options.xml")))
            {
                throw new FileNotFoundException(Path.Combine(LocalPath.AbsoluteCompilerOptionsDirectory, "options.xml"));
            }

            byte[] fileName = Encoding.ASCII.GetBytes("options.xml");
            byte[] fileData = File.ReadAllBytes(Path.Combine(LocalPath.AbsoluteCompilerOptionsDirectory, "options.xml"));
            byte[] clientData = new byte[4 + 4 + 4 + fileName.Length + fileData.Length];
            byte[] fileNameLen = BitConverter.GetBytes(fileName.Length);
            BitConverter.GetBytes(clientData.Length).CopyTo(clientData, 0);
            BitConverter.GetBytes((Int32)RequestCodes.CompilerOptionsFile).CopyTo(clientData, 4);
            fileNameLen.CopyTo(clientData, 8);
            fileName.CopyTo(clientData, 12);
            fileData.CopyTo(clientData, 12 + fileName.Length);

            Send(clientData, clientData.Length);

            _logger.Debug(Address + ": Send compiler options file");
        }
        protected void SendRequestForDeletingProblem(int ProblemID)
        {
            byte[] data = new byte[4 + 4 + 4];
            BitConverter.GetBytes(4 + 4 + 4).CopyTo(data, 0);
            BitConverter.GetBytes((Int32)RequestCodes.DeleteProblem).CopyTo(data, 4);
            BitConverter.GetBytes(ProblemID).CopyTo(data, 8);

            Send(data, data.Length);

            _logger.Debug(Address + ": Send request for deleting problem");
        }


        /// <summary>
        /// Send data asynchronously.
        /// 
        /// Thread safety.
        /// </summary>
        /// <param name="data"></param>
        protected void Send(byte[] Data, int Length)
        {
            try
            {
                // Begin sending the data to the remote client.
                _socket.BeginSend(Data, 0, Length, SocketFlags.None,
                    new AsyncCallback(SendCallback), this);
            }
            catch (SocketException ex)
            {
                _logger.Warn("Error occurred on socket begin send: ", ex);
                if (!_socket.IsConnected())
                {
                    try
                    {
                        IsConnected = false;
                        _timer.Stop();
                        _clientCompilers.Each(c =>
                        {
                            Compilers.RemoveLanguage(c);
                            if (!Compilers.AvailableLanguages.Contains(c))
                                _repository.MakeProgrammingLanguageUnavailable(c);
                        });
                        _clientCompilers.Clear();
                        _socket.Shutdown(SocketShutdown.Both);
                        _socket.Close();

                        _logger.Info(Address + ": Disconnected:", ex);
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
                SocketClient client = (SocketClient)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client._socket.EndSend(ar);
            }
            catch (Exception ex)
            {
                _logger.Error(Address + ": Error occurred on sending data: ", ex);
            }
        }
        #endregion
    }
}