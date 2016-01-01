using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace url2wiz.NET
{
    public class Logger:IDisposable
    {
        private static readonly string FileName = "FailedList.txt";
        private static readonly string FailedUrlFormat = "{0}\t{1}";
        private StreamWriter streamWrite;
        private FileStream fileStream;

        public Logger(string path)
        {
            fileStream = new FileStream(Path.Combine(path, FileName), FileMode.Append);
            streamWrite = new StreamWriter(fileStream);
        }

        public void Log(string message)
        {
            streamWrite.WriteLine(message);
        }
        public void Log(string wizFolder, string url)
        {
            streamWrite.WriteLine(FailedUrlFormat, wizFolder, url);
        }
        public void Flush()
        {
            streamWrite.Flush();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    
                    streamWrite.Dispose();
                    fileStream.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Logger() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
