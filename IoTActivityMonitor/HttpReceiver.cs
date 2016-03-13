using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace IoTActivityMonitor
{

    public sealed class HttpReceiver
    {
        private class RequestPayload
        {
            public string Origin { get; set; }
            public string Data { get; set; }
        }

        public event TypedEventHandler<HttpReceiver, string> DataReceived;

        private uint _bufferSize = 1024;
        private int _port;
        private StreamSocketListener _listener;

        public HttpReceiver(int port)
        {
            _port = port;

            _listener = new StreamSocketListener();
            _listener.Control.KeepAlive = true;
            _listener.Control.NoDelay = true;

            _listener.ConnectionReceived += async (s, e) => { await processRequestAsync(e.Socket); };
        }

        public void Start()
        {
            Task.Run(async () => {
                await _listener.BindServiceNameAsync(_port.ToString());
            });
        }

        private async Task processRequestAsync(StreamSocket socket)
        {
            string data = await readSocket(socket);
            RequestPayload payload = extractPayload(data);

            if (DataReceived != null)
            {
                DataReceived(this, payload.Data);
            }

            writeResponse("OK", payload.Origin, socket);
        }

        private RequestPayload extractPayload(string httpRequestData)
        {
            var lines = httpRequestData.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            var origin = lines.Where(o => o.Contains("Origin:")).Select(o => o.Substring(8)).FirstOrDefault();
            var result = lines.Last().Replace("\0","");

            return new RequestPayload() { Origin=origin, Data=result };
        }

        private async Task<string> readSocket(StreamSocket socket)
        {
            StringBuilder request = new StringBuilder();
            byte[] data = new byte[_bufferSize];
            IBuffer buffer = data.AsBuffer();
            uint dataRead = _bufferSize;
            using (IInputStream input = socket.InputStream)
            {
                while (dataRead == _bufferSize)
                {
                    await input.ReadAsync(buffer, _bufferSize, InputStreamOptions.Partial);
                    request.Append(Encoding.UTF8.GetString(data, 0, data.Length));
                    dataRead = buffer.Length;
                }
            }

            return request.ToString();
        }

        private void writeResponse(string html, string origin, StreamSocket socket)
        {
            byte[] bodyArray = Encoding.UTF8.GetBytes(html);
            
            using (var outputStream = socket.OutputStream)
            {
                using (Stream response = outputStream.AsStreamForWrite())
                {
                    using (MemoryStream stream = new MemoryStream(bodyArray))
                    {
                        string header = String.Format(
                            "HTTP/1.1 200 OK\r\n" + 
                            "Access-Control-Allow-Origin: {0}\r\n" + 
                            "Content-Length: {1}\r\n" + 
                            "Connection: close\r\n" + 
                            "\r\n", 
                            origin, stream.Length);
                        byte[] headerArray = Encoding.UTF8.GetBytes(header);
                        response.Write(headerArray, 0, headerArray.Length);
                        stream.CopyTo(response);
                        response.Flush();
                    }
                }
            }
        }
    }

}