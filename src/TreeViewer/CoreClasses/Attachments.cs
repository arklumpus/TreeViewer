/*
    TreeViewer - Cross-platform software to draw phylogenetic trees
    Copyright (C) 2021  Giorgio Bianchini
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.IO;

namespace TreeViewer
{
    public class Attachment : IDisposable
    {
        public Stream Stream { get; }
        public bool CacheResults { get; }
        public bool StoreInMemory { get; }
        public string Name { get; }
        public long StreamStart { get; }
        public int StreamLength { get; }

        private string TempFileName { get; }
        private bool DeleteTempFileOnDispose { get; }

        public Attachment(string name, bool cacheResults, bool storeInMemory, string fileName)
        {
            this.Name = name;
            this.CacheResults = cacheResults;
            this.StoreInMemory = storeInMemory;

            FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);

            this.StreamStart = 0;
            this.StreamLength = (int)stream.Length;

            if (!storeInMemory)
            {
                this.Stream = stream;
            }
            else
            {
                MemoryStream ms = new MemoryStream(this.StreamLength);
                stream.CopyTo(ms);
                stream.Dispose();
                this.Stream = ms;
            }
        }

        public Attachment(string name, bool cacheResults, string fileName, long streamStart, int streamLength)
        {
            this.Name = name;
            this.CacheResults = cacheResults;
            this.StoreInMemory = false;

            FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);

            this.StreamStart = streamStart;
            this.StreamLength = streamLength;

            stream.Seek(this.StreamStart, SeekOrigin.Begin);

            this.Stream = stream;
        }

        public Attachment(string name, bool cacheResults, bool storeInMemory, string fileName, bool deleteOnDispose) : this(name, cacheResults, storeInMemory, fileName)
        {
            if (deleteOnDispose)
            {
                this.TempFileName = fileName;
                this.DeleteTempFileOnDispose = true;
            }
        }

        public Attachment(string name, bool cacheResults, bool storeInMemory, ref Stream stream)
        {
            this.Name = name;
            this.CacheResults = cacheResults;
            this.StoreInMemory = storeInMemory;

            this.StreamStart = 0;
            this.StreamLength = (int)stream.Length;

            this.Stream = stream;
        }

        private string[] linesCache = null;
        public string[] GetLines()
        {
            if (linesCache != null)
            {
                return linesCache;
            }

            List<string> tbr = new List<string>();

            Stream.Seek(StreamStart, SeekOrigin.Begin);

            using (StreamReader reader = new StreamReader(Stream, leaveOpen: true))
            {
                string line = reader.ReadLine();

                while (line != null)
                {
                    tbr.Add(line);
                    line = reader.ReadLine();
                }
            }

            if (!CacheResults)
            {
                return tbr.ToArray();
            }
            else
            {
                linesCache = tbr.ToArray();
                return linesCache;
            }
        }


        private byte[] byteCache = null;
        private bool disposedValue;

        public byte[] GetBytes()
        {
            if (byteCache != null)
            {
                return byteCache;
            }

            Stream.Seek(StreamStart, SeekOrigin.Begin);

            byteCache = new byte[StreamLength];

            int readBytes = 0;

            while (readBytes < StreamLength)
            {
                readBytes += Stream.Read(byteCache, readBytes, StreamLength - readBytes);
            }

            if (CacheResults)
            {
                return byteCache;
            }
            else
            {
                byte[] tbr = byteCache;
                byteCache = null;

                return tbr;
            }
        }

        public void WriteToStream(Stream outputStream)
        {
            this.Stream.Seek(this.StreamStart, SeekOrigin.Begin);

            byte[] buffer = new byte[8192];

            int bytesWritten = 0;

            while (bytesWritten < this.StreamLength)
            {
                int read = this.Stream.Read(buffer, 0, Math.Min(8192, this.StreamLength - bytesWritten));

                outputStream.Write(buffer, 0, read);

                bytesWritten += read;
            }
        }

        public void WriteBase64Encoded(StreamWriter writer)
        {
            this.Stream.Seek(this.StreamStart, SeekOrigin.Begin);

            byte[] buffer = new byte[8193];

            int bytesWritten = 0;
            int bufferShift = 0;

            while (bytesWritten < this.StreamLength)
            {
                int read = this.Stream.Read(buffer, bufferShift, Math.Min(8193 - bufferShift, this.StreamLength - bytesWritten)) + bufferShift;

                byte[] readBytes;

                if (read == 8193)
                {
                    readBytes = buffer;
                    bufferShift = 0;
                }
                else
                {
                    if (read - bufferShift > 0)
                    {
                        readBytes = new byte[(read / 3) * 3];

                        for (int i = 0; i < readBytes.Length; i++)
                        {
                            readBytes[i] = buffer[i];
                        }

                        if (readBytes.Length < read)
                        {
                            byte[] tempBuffer = new byte[read - readBytes.Length];

                            for (int i = 0; i < tempBuffer.Length; i++)
                            {
                                tempBuffer[i] = buffer[i + readBytes.Length];
                            }

                            for (int i = 0; i < tempBuffer.Length; i++)
                            {
                                buffer[i] = tempBuffer[i];
                            }

                            bufferShift = tempBuffer.Length;
                        }
                        else
                        {
                            bufferShift = 0;
                        }
                    }
                    else
                    {
                        readBytes = new byte[bufferShift];
                        for (int i = 0; i < readBytes.Length; i++)
                        {
                            readBytes[i] = buffer[i];
                        }
                        bufferShift = 0;
                    }
                }


                string encoded = Convert.ToBase64String(readBytes);

                writer.Write(encoded);

                bytesWritten += readBytes.Length;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.Stream.Dispose();
                }

                if (this.DeleteTempFileOnDispose)
                {
                    try
                    {
                        File.Delete(this.TempFileName);
                    }
                    catch { }
                }

                disposedValue = true;
            }
        }

        ~Attachment()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
