/*----------------------------------------------------------------------------------+
|                                                                                   |
|           Copyright (c) 2014, Xhakal_Systems. All Rights Reserved.                |
|                                                                                   |
|           Limited permission is hereby granted to reproduce and modify this       |
|           copyrighted material provided that this notice is retained              |
|           in its entirety in any such reproduction or modification.               |
|                                                                                   |
|           Author: mTéllez                                                         |
|           First Version Date: 2014/01/16                                          |
|                                                                                   |
+-----------------------------------------------------------------------------------+
 */

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Net.NetworkInformation;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using System.IO;
using System.Text;
using NetduinoApplication4;


namespace HttpLibrary
{
    /// <summary>
    /// on error event arguments
    /// </summary>
    public class OnErrorEventArgs : EventArgs
    {
       
        private string EVENT_MESSAGE;
        /// <summary>
        /// property containing event message
        /// </summary>
        public string EventMessage
        {
            get { return EVENT_MESSAGE; }
        }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="EVENT_MESSAGE">message</param>
        public OnErrorEventArgs(string EVENT_MESSAGE)
        {
            this.EVENT_MESSAGE = EVENT_MESSAGE;
        }
    }
    /// <summary>
    /// interface used for event handeling
    /// </summary>
    /// <param name="sender">object sender</param>
    /// <param name="e">arguments passed</param>
    public delegate void OnErrorDelegate(object sender, OnErrorEventArgs e);
    /// <summary>
    /// http server class
    /// </summary>
    public class HttpServer
    {
        private Thread SERVER_THREAD;
        private Socket LISTEN_SOCKET;
        private Socket ACCEPTED_SOCKET;
        private string LISTEN_IP;
        private int LISTEN_PORT;
        private bool IS_SERVER_RUNNING;
        private string STORAGE_PATH;
        private FileStream FILE_STREAM;
        private StreamReader FILE_READER;
        private StreamWriter FILE_WRITER;
        private byte[] RECEIVE_BUFFER;
        private byte[] SEND_BUFFER;
        public Program program;
        /// <summary>
        /// property returns true if server is running and listening
        /// </summary>
        public bool IsServerRunning
        {
            get { return IS_SERVER_RUNNING; }
        }
        /// <summary>
        /// returns the ip address obtained from the dhcp server
        /// </summary>
        public string ObtainedIp
        {
            get { return LISTEN_IP; }
        }
        /// <summary>
        /// returns the running thread, use this if you want to set server thread priority
        /// </summary>
        public Thread RunningThread
        {
            get { return SERVER_THREAD; }
        }
        private enum FileType { JPEG = 1, GIF = 2, Html = 3 };

        /// <summary>
        /// event fired when an error occurs
        /// </summary>
        public event OnErrorDelegate OnServerError;
        string HtmlPageHeader = "HTTP/1.0 200 OK\r\nContent-Type: ";
        /// <summary>
        /// event fire function
        /// </summary>
        /// <param name="e">event passed</param>
        protected virtual void OnServerErrorFunction(OnErrorEventArgs e)
        {
            OnServerError(this, e);
        }
        private void FragmentateAndSend(string FILE_NAME, FileType Type)
        {
            byte[] HEADER;
            long FILE_LENGTH;
            FILE_STREAM = new FileStream(FILE_NAME, FileMode.Open, FileAccess.Read);
            FILE_READER = new StreamReader(FILE_STREAM);
            FILE_LENGTH = FILE_STREAM.Length;

            switch (Type)
            {
                case FileType.Html:
                    HEADER = UTF8Encoding.UTF8.GetBytes(HtmlPageHeader + "text/html" + "; charset=utf-8\r\nContent-Length: " + FILE_LENGTH.ToString() + "\r\n\r\n");
                    break;
                case FileType.GIF:
                    HEADER = UTF8Encoding.UTF8.GetBytes(HtmlPageHeader + "image/gif" + "; charset=utf-8\r\nContent-Length: " + FILE_LENGTH.ToString() + "\r\n\r\n");
                    break;
                case FileType.JPEG:
                    HEADER = UTF8Encoding.UTF8.GetBytes(HtmlPageHeader + "image/jpeg" + "; charset=utf-8\r\nContent-Length: " + FILE_LENGTH.ToString() + "\r\n\r\n");
                    break;
                default:
                    HEADER = UTF8Encoding.UTF8.GetBytes(HtmlPageHeader + "text/html" + "; charset=utf-8\r\nContent-Length: " + FILE_LENGTH.ToString() + "\r\n\r\n");
                    break;
            }

            ACCEPTED_SOCKET.Send(HEADER, 0, HEADER.Length, SocketFlags.None);

            while (FILE_LENGTH > SEND_BUFFER.Length)
            {
                FILE_STREAM.Read(SEND_BUFFER, 0, SEND_BUFFER.Length);
                ACCEPTED_SOCKET.Send(SEND_BUFFER, 0, SEND_BUFFER.Length, SocketFlags.None);
                FILE_LENGTH -= SEND_BUFFER.Length;
            }
            FILE_STREAM.Read(SEND_BUFFER, 0, (int)FILE_LENGTH);
            ACCEPTED_SOCKET.Send(SEND_BUFFER, 0, (int)FILE_LENGTH, SocketFlags.None);

            FILE_READER.Close();
            FILE_STREAM.Close();
        }
        private string GetFileName(string RequestStr)
        {
            Debug.Print("FILE :" + RequestStr);
            RequestStr = RequestStr.Substring(RequestStr.IndexOf("GET /") + 5);

            RequestStr = RequestStr.Substring(0, RequestStr.IndexOf("HTTP"));
            return RequestStr.Trim();
        }
        private string GetRequest(string RequestStr)
        {
            string operation="";
           
            RequestStr = RequestStr.Substring(RequestStr.IndexOf("GET /") + 5);
          
            RequestStr = RequestStr.Substring(0, RequestStr.IndexOf("HTTP"));
            operation = RequestStr;
           
            if (operation.Length >=5 && (operation.Substring(0,5)) == "OPERA")
            {
                return operation;

            }
            else {
                return "NO_OP"; 
            }
        }
        private bool RequestContains(string Request, string Str)
        {
            return (Request.IndexOf(Str) >= 0);
        }
        private void BuildFileList(string[] FILES)
        {
            FILE_STREAM = new FileStream(STORAGE_PATH + "\\index.txt", FileMode.Create, FileAccess.Write);
            FILE_WRITER = new StreamWriter(FILE_STREAM);
            FILE_WRITER.WriteLine("<html>");
            FILE_WRITER.WriteLine("<head>");
            FILE_WRITER.WriteLine("<title>");
            FILE_WRITER.WriteLine("Index Page");
            FILE_WRITER.WriteLine("</title>");
            FILE_WRITER.WriteLine("<body>");
            FILE_WRITER.WriteLine("<h1 align=center>");
            FILE_WRITER.WriteLine("FILE LIST");
            FILE_WRITER.WriteLine("</h1>");
            FILE_WRITER.WriteLine("<h1 align=center>");
            FILE_WRITER.WriteLine((FILES.Length - 2).ToString() + " FILES");
            FILE_WRITER.WriteLine("</h1>");
            foreach (string F in FILES)
            {
                if (!RequestContains(F, "index") && !RequestContains(F, "NotFound"))
                {
                    FILE_WRITER.WriteLine("<h1 align=center><a href=\"");
                    FILE_WRITER.WriteLine("/" + F.Substring(F.LastIndexOf("\\") + 1).ToLower() + "\">");
                    FILE_WRITER.WriteLine(F.Substring(F.LastIndexOf("\\") + 1).ToLower());
                    FILE_WRITER.WriteLine("</a>");
                }
            }
            FILE_WRITER.WriteLine("</body>");
            FILE_WRITER.WriteLine("</html>");
            FILE_WRITER.Close();
            FILE_STREAM.Close();
        }
        private bool IsFileFound(ref string FILE_NAME, string[] FILES)
        {
            foreach (string F in FILES)
            {
                if (RequestContains(F.ToLower(), FILE_NAME.ToLower()))
                {
                    FILE_NAME = F;
                    return true;
                }
            }
            return false;
        }

        private string GetFileExtention(string FILE_NAME)
        {
            string x = FILE_NAME;
            x = x.Substring(x.LastIndexOf('.') + 1);
            return x;
        }
        private void ProcessRequest()
        {

            string[] FILES;
            string REQUEST = "";
            string FILE_NAME = "";
            string FILE_EXTENTION = "";
            string OPERATION;
            float tiempoMin = 0f;
            float tiempoMax = 0f;
            int interval = 50;
            program = new Program();
            ACCEPTED_SOCKET.Receive(RECEIVE_BUFFER);
            REQUEST = new string(UTF8Encoding.UTF8.GetChars(RECEIVE_BUFFER));
            FILES = Directory.GetFiles(STORAGE_PATH);
            OPERATION = GetRequest(REQUEST);
            FILE_NAME = GetFileName(REQUEST);

            

           if (OPERATION.Length>=7 && OPERATION.Substring(0,7) =="OPERA/Y") {
               program.testWarm();
            }
           else if (OPERATION.Length >= 7 && OPERATION.Substring(0, 7) == "OPERA/Z")
           {
               program.testCold();

           }
         
            
            
           else  if (OPERATION.Length == 25 && OPERATION.Substring(0, 6) == "OPERA/")
           {
               
               int tiempoCombate = Convert.ToInt32(OPERATION.Substring(OPERATION.IndexOf("m") + 1,3 ));
              
               if (OPERATION.Substring(OPERATION.IndexOf("M") + 4, 1) == "3" || OPERATION.Substring(OPERATION.IndexOf("M") + 4, 1) == "4"
                   || OPERATION.Substring(OPERATION.IndexOf("M") + 4, 1) == "8" || OPERATION.Substring(OPERATION.IndexOf("M") + 4, 1) == "9")
               {
                   tiempoMin = (float)Convert.ToDouble(OPERATION.Substring(OPERATION.IndexOf("M") + 1, 4));
                   tiempoMin += 0.01f;
                  
               }
               else
               {
                   tiempoMin = (float)Convert.ToDouble(OPERATION.Substring(OPERATION.IndexOf("M") + 1, 4));
               }

               
               if (OPERATION.Substring(OPERATION.IndexOf("/") + 4, 1) == "3" || OPERATION.Substring(OPERATION.IndexOf("/") + 4, 1) == "4"
                   || OPERATION.Substring(OPERATION.IndexOf("/") + 4, 1) == "8" || OPERATION.Substring(OPERATION.IndexOf("/") + 4, 1) == "9")
               {
                   tiempoMax = (float)Convert.ToDouble(OPERATION.Substring(OPERATION.IndexOf("/") + 1, 4));
                   tiempoMax += 0.01f;
               }
               else
               {
                   tiempoMax = (float)Convert.ToDouble(OPERATION.Substring(OPERATION.IndexOf("/") + 1, 4));
               }

               
               interval = Convert.ToInt32(OPERATION.Substring(OPERATION.IndexOf("t") + 1, 4));
               
               String response =program.controlador(tiempoCombate, tiempoMin, tiempoMax, interval);

               Debug.Print(response);

               FILE_STREAM = new FileStream(STORAGE_PATH + "\\FileResponse.html", FileMode.Create, FileAccess.Write);
               FILE_WRITER = new StreamWriter(FILE_STREAM);
               FILE_WRITER.WriteLine("<html>");

               FILE_WRITER.WriteLine("<head>");

               FILE_WRITER.WriteLine("<title>");
               FILE_WRITER.WriteLine("Resultados del combate");
               FILE_WRITER.WriteLine("</title>");
              
               FILE_WRITER.WriteLine("<style>body{margin:auto;width:1050px;font-family: sans-serif;}");
               FILE_WRITER.WriteLine("header{width:1050px;height:250px;margin-bottom:10px;}");
	           FILE_WRITER.WriteLine("#cabecera{width:1050px;height:250px;margin-bottom:10px;float:left;}");
               FILE_WRITER.WriteLine("#contentTable{width:900px;margin-left:30px;}");
               FILE_WRITER.WriteLine("#contentTable table{width:900px;margin:auto;color:#3F3F3F;}");
               FILE_WRITER.WriteLine("#contentTable table td{text-align: center;}");
               FILE_WRITER.WriteLine("#contentTable table caption{padding-bottom: 50px;text-align: center;}");
               FILE_WRITER.WriteLine("#botones{margin-top:30px;width:900px;float:left;clear:both;margin-left:30px;background:#2E64FE;border-radius:5px;text-align: center;padding-top:2px;padding-bottom:2px;}");
               FILE_WRITER.WriteLine("#botones a{text-decoration: none;color:white;font-weight: bold;}");
               FILE_WRITER.WriteLine("</style>");
               
               FILE_WRITER.WriteLine("</head>");

               FILE_WRITER.WriteLine("<body>");
               FILE_WRITER.WriteLine("<header><img src='./logo.png' alt='Logo' width='950' height='250'>");

               FILE_WRITER.WriteLine("</header>");
               if (response.IndexOf("W") == -1)
               {
                   FILE_WRITER.WriteLine("<div id='contentTable'><table><caption style='font-size:30px;'>Resultados satisfactorios de la prueba</caption>");  
               }
               else
               {
                   FILE_WRITER.WriteLine("<div id='contentTable'><table><caption style='font-size:30px;'>Abortado por exceso de temperatura</caption>");
               }
               FILE_WRITER.WriteLine("<thead><tr>");
               FILE_WRITER.WriteLine("<th>Temperatura m&iacute;nima</th>");
               FILE_WRITER.WriteLine("<th>temperatura m&aacute;xima</th>");
               FILE_WRITER.WriteLine("<th>Tiempo en rango</th></tr>");
               FILE_WRITER.WriteLine("<tr></thead><tbody>");
             //Mirar dependiendo del numero devuelve uno u otro (21 en vez de 21.0)

               FILE_WRITER.WriteLine("<td id='tempMin'>" + response.Substring(response.IndexOf("m") + 1, 4) + " &#176;C</td>");

               FILE_WRITER.WriteLine("<td id='tempMax'>" + response.Substring(response.IndexOf("M") + 1, 4) + " &#176;C</td>");
               
               FILE_WRITER.WriteLine("<td id='timeRang'>" + response.Substring(response.IndexOf("R") + 1, 3) + " \"</td></tr></tbody></table></div>");
               FILE_WRITER.WriteLine("<div id='botones'><a href='http://192.168.2.100/'>Volver al panel de configuraci&oacute;n</a></div>");
               FILE_WRITER.WriteLine("</body>"); 
               FILE_WRITER.WriteLine("</html>");
               FILE_WRITER.Close();
               FILE_STREAM.Close();


           }


          

            else {
            if (FILE_NAME == "" || RequestContains(FILE_NAME, "index"))
            {
                BuildFileList(FILES);
                FragmentateAndSend(STORAGE_PATH + "\\index.html", FileType.Html);
            }
            else
            {
                if (IsFileFound(ref FILE_NAME, FILES))
                {
                    FILE_EXTENTION = GetFileExtention(FILE_NAME.ToLower());
                    switch (FILE_EXTENTION)
                    {
                        case "gif":
                            FragmentateAndSend(FILE_NAME, FileType.GIF);
                            break;
                        case "txt":
                            FragmentateAndSend(FILE_NAME, FileType.Html);
                            break;
                        case "jpg":
                            FragmentateAndSend(FILE_NAME, FileType.JPEG);
                            break;
                        case "jpeg":
                            FragmentateAndSend(FILE_NAME, FileType.JPEG);
                            break;
                        case "htm":
                            FragmentateAndSend(FILE_NAME, FileType.Html);
                            break;
                        case "html":
                            FragmentateAndSend(FILE_NAME, FileType.Html);
                            break;
                        default:
                            FragmentateAndSend(FILE_NAME, FileType.Html);
                            break;
                    }
                }
                else
                {
                    FragmentateAndSend(STORAGE_PATH + "\\NotFound.txt", FileType.Html);
                }

            }

             }
        }

        private void ProcessRequestOperation()
        {

            
            string REQUEST = "";
            string REQUEST_OPERATION = "";
            ACCEPTED_SOCKET.Receive(RECEIVE_BUFFER);
            REQUEST = new string(UTF8Encoding.UTF8.GetChars(RECEIVE_BUFFER));
            REQUEST_OPERATION = GetRequest(REQUEST);
            
           
        }

        private void RunServer()
        {

            try
            {
                LISTEN_SOCKET = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint BindingAddress = new IPEndPoint(IPAddress.Any, LISTEN_PORT);
                LISTEN_SOCKET.Bind(BindingAddress);
                LISTEN_SOCKET.Listen(10);
                IS_SERVER_RUNNING = true;
                while (true)
                {
                    ACCEPTED_SOCKET = LISTEN_SOCKET.Accept();
                    ProcessRequest();
                    ACCEPTED_SOCKET.Close();
                }
            }
            catch (Exception)
            {
                IS_SERVER_RUNNING = false;
                ACCEPTED_SOCKET.Close();
                LISTEN_SOCKET.Close();
                OnServerErrorFunction(new OnErrorEventArgs("Server Error\r\nCheck Connection Parameters"));
            }
        }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="port">listening port</param>
        /// <param name="ReceiveBufferSize">receive buffer size must be > 50 to handle the http request correctly</param>
        /// <param name="SendBufferSize">send Buffer size, data is fragmented hence the fragment size depends on this</param>
        /// <param name="pages_folder">the directory where pages are placed, if using sd card fill this with @"\SD"</param>
        public HttpServer(int port, int ReceiveBufferSize, int SendBufferSize, string pages_folder)
        {
            SERVER_THREAD = null;
            LISTEN_SOCKET = null;
            ACCEPTED_SOCKET = null;
            IS_SERVER_RUNNING = false;
            LISTEN_PORT = port;
            STORAGE_PATH = pages_folder;
            RECEIVE_BUFFER = new byte[ReceiveBufferSize];
            SEND_BUFFER = new byte[SendBufferSize];
            LISTEN_IP = "No Ip Address";
            SERVER_THREAD = new Thread(new ThreadStart(RunServer));
           
            
            if (!File.Exists(STORAGE_PATH + "\\index.txt"))
            {
                FILE_STREAM = new FileStream(STORAGE_PATH + "\\index.txt", FileMode.Create, FileAccess.Write);
                FILE_WRITER = new StreamWriter(FILE_STREAM);
                FILE_WRITER.WriteLine("<html>");
                FILE_WRITER.WriteLine("<head>");
                FILE_WRITER.WriteLine("<title>");
                FILE_WRITER.WriteLine("Index Page");
                FILE_WRITER.WriteLine("</title>");
                FILE_WRITER.WriteLine("<body>");
                FILE_WRITER.WriteLine("<h1 align=center>");
                FILE_WRITER.WriteLine("FILE LIST");
                FILE_WRITER.WriteLine("</h1>");
                FILE_WRITER.WriteLine("</body>");
                FILE_WRITER.WriteLine("</html>");
                FILE_WRITER.Close();
                FILE_STREAM.Close();
            }
            if (!File.Exists(STORAGE_PATH + "\\NotFound.txt"))
            {
                FILE_STREAM = new FileStream(STORAGE_PATH + "\\NotFound.txt", FileMode.Create, FileAccess.Write);
                FILE_WRITER = new StreamWriter(FILE_STREAM);
                FILE_WRITER.WriteLine("<html>");
                FILE_WRITER.WriteLine("<head>");
                FILE_WRITER.WriteLine("<title>");
                FILE_WRITER.WriteLine("Page Not Found");
                FILE_WRITER.WriteLine("</title>");
                FILE_WRITER.WriteLine("<body>");
                FILE_WRITER.WriteLine("<h1 align=center>");
                FILE_WRITER.WriteLine("Page Not Found");
                FILE_WRITER.WriteLine("</h1>");
                FILE_WRITER.WriteLine("</body>");
                FILE_WRITER.WriteLine("</html>");
                FILE_WRITER.Close();
                FILE_STREAM.Close();
            }
            Thread.Sleep(5000);
            NetworkInterface networkInterface = NetworkInterface.GetAllNetworkInterfaces()[0];
            LISTEN_IP = networkInterface.IPAddress;
        }
        /// <summary>
        /// starts the server and listens to connections
        /// </summary>
        public void Start()
        {
            SERVER_THREAD.Start();
        }
        /// <summary>
        /// stops the server activity 
        /// </summary>
        public void Stop()
        {
            LISTEN_SOCKET.Close();
        }
    }
}
