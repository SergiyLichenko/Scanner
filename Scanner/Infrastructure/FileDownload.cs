using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Scanner.Infrastructure;
using DownloadProgressChangedEventArgs = Scanner.Infrastructure.DownloadProgressChangedEventArgs;
using System.Diagnostics;

namespace Scanner.ViewModel
{
    public class FileDownload : IDisposable
    {

        public FileDownload(string source, int chunkSize, CancellationTokenSource cancellToken)
        {
            _source = source;
            _chunkSize = chunkSize;
            _cancellToken = cancellToken;
            BytesDownloaded = 0;


        }

        private const string _userAgent = @"Mozilla/5.0 (Windows NT 6.2; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1667.0 Safari/537.36";

        private readonly string _source;
        private readonly int _chunkSize;
        private readonly CancellationTokenSource _cancellToken;
        private long? _contentLength;

        public volatile DownloadStatus DownloadStatus;

        public string ResultStr { get; set; }
        public int BytesDownloaded { get; private set; }
        public int? ContentLength
        {
            get
            {
                if (_contentLength == null)
                    return null;
                return Convert.ToInt32(_contentLength);
            }
        }

        public event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged;
        public event EventHandler<DownloadCompletedEventArgs> DownloadCompleted;
        public event EventHandler<DownloadErrorEventArgs> DownloadError;

        private async Task<long> GetContentLength()
        {
            var request = (HttpWebRequest)WebRequest.Create(_source);
            request.Method = "HEAD";
            request.UserAgent = _userAgent;


            using (var response = await request.GetResponseAsync(_cancellToken.Token))
                return response.ContentLength;
        }

        private async Task Start(int range, CancellationToken token)
        {
            try
            {
                if (ContentLength == null)
                    _contentLength = await GetContentLength();

                var request = (HttpWebRequest)WebRequest.Create(_source);
                request.Method = "GET";
                request.UserAgent = _userAgent;
                request.AddRange(range);

                List<byte> bytes = new List<byte>();
                using (var response = await request.GetResponseAsync(token))
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        while (DownloadStatus == DownloadStatus.Downloading)
                        {
                            var buffer = new byte[_chunkSize];
                            var bytesRead = await responseStream?.ReadAsync(buffer, 0, buffer.Length, token);
                            BytesDownloaded += bytesRead;
                            OnDownloadProgressChanged();
                            if (bytesRead == 0)
                            {
                                OnDownloadCompleted(bytes);
                                break;
                            }

                            bytes.AddRange(buffer.Take(bytesRead).ToArray());

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OnDownloadError(ex.Message);
            }
        }
       
      
        private void OnDownloadError(string message)
        {
            if (DownloadStatus != DownloadStatus.Cancelled)
                DownloadStatus = DownloadStatus.Error;
            DownloadError?.Invoke(this, new DownloadErrorEventArgs(message));
        }

        private void OnDownloadProgressChanged()
        {
            double persentage = 100 * ((double)BytesDownloaded) / ContentLength.Value;
            int rounded = Convert.ToInt32(Math.Round(persentage));

            DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedEventArgs(rounded));
        }

        private void OnDownloadCompleted(List<byte> bytes)
        {
            using (var stream = new MemoryStream(bytes.ToArray()))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    ResultStr = streamReader.ReadToEnd();
                }
            }

            DownloadStatus = DownloadStatus.Downloaded;
            DownloadCompleted?.Invoke(this, new DownloadCompletedEventArgs(ResultStr));
        }

        public Task Start()
        {
            DownloadStatus = DownloadStatus.Downloading;
            return Start(BytesDownloaded, _cancellToken.Token);
        }

        public void Pause()
        {
            DownloadStatus = DownloadStatus.Paused;
        }

        public void Stop()
        {
            DownloadStatus = DownloadStatus.Cancelled;

            _cancellToken?.Cancel();
        }

        public void Dispose()
        {
            _cancellToken.Dispose();
        }
    }
}
