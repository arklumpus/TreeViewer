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

using PhyloTree;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace TreeViewer
{
    public class Attachment : IDisposable
    {
        public string Id { get; } = Guid.NewGuid().ToString();
        public Stream Stream { get; }
        public bool CacheResults { get; }
        public bool StoreInMemory { get; }
        public string Name { get; }
        public long StreamStart { get; }
        public int StreamLength { get; }
        private string TempFileName { get; }
        private bool DeleteTempFileOnDispose { get; }

        private AttachmentFontFamily fontFamilyCache = null;

        private ImmutableList<TreeNode> treesCache = null;

        public AttachmentFontFamily GetFontFamily()
        {
            if (CacheResults)
            {
                if (fontFamilyCache == null)
                {
                    this.Stream.Seek(this.StreamStart, SeekOrigin.Begin);

                    try
                    {
                        fontFamilyCache = AttachmentFontFamily.Create(this.Name, this.Stream);
                    }
                    catch
                    {
                        fontFamilyCache = null;
                    }
                }

                return fontFamilyCache;
            }
            else
            {
                this.Stream.Seek(this.StreamStart, SeekOrigin.Begin);

                try
                {
                    return AttachmentFontFamily.Create(this.Name, this.Stream);
                }
                catch
                {
                    return null;
                }
            }
        }

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

        private string textCache = null;
        public string GetText()
        {
            if (textCache != null)
            {
                return textCache;
            }

            Stream.Seek(StreamStart, SeekOrigin.Begin);

            string tbr;

            using (StreamReader reader = new StreamReader(Stream, leaveOpen: true))
            {
                tbr = reader.ReadToEnd();
            }

            if (!CacheResults)
            {
                return tbr;
            }
            else
            {
                textCache = tbr;
                return textCache;
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

        private TreeNode[] ReadTrees()
        {
            string fileName = Path.GetTempFileName();

            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    this.WriteToStream(fs);
                }

                double maxResult = 0;
                int maxIndex = -1;

                for (int i = 0; i < Modules.FileTypeModules.Count; i++)
                {
                    try
                    {
                        double priority = Modules.FileTypeModules[i].IsSupported(fileName);
                        if (priority > maxResult)
                        {
                            maxResult = priority;
                            maxIndex = i;
                        }
                    }
                    catch { }
                }

                if (maxIndex >= 0)
                {
                    try
                    {
                        List<(string, Dictionary<string, object>)> moduleSuggestions = new List<(string, Dictionary<string, object>)>()
                                {
                                    ("32914d41-b182-461e-b7c6-5f0263cc1ccd", new Dictionary<string, object>()),
                                    ("68e25ec6-5911-4741-8547-317597e1b792", new Dictionary<string, object>()),
                                };

                        Action<double> openerProgressAction = (_) => { };

                        bool askForCodePermission(RSAParameters? publicKey)
                        {
                            return false;
                        };

                        IEnumerable<TreeNode> loader = Modules.FileTypeModules[maxIndex].OpenFile(fileName, moduleSuggestions, (val) => { openerProgressAction(val); }, askForCodePermission);

                        return loader.ToArray();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("An error has occurred while opening the file!\n" + ex.Message, ex);
                    }
                }
                else
                {
                    throw new Exception("The file type is not supported by any of the currently installed modules!");
                }
            }
            finally
            {
                try
                {
                    File.Delete(fileName);
                }
                catch { }
            }
        }

        public IReadOnlyList<TreeNode> GetTrees()
        {
            if (CacheResults)
            {
                if (treesCache == null)
                {
                    treesCache = ImmutableList.Create<TreeNode>(this.ReadTrees());
                }

                return treesCache;
            }
            else
            {
                return this.ReadTrees();
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

    public class AttachmentFontFamily : VectSharp.FontFamily
    {
        public string AttachmentName { get; }

        private AttachmentFontFamily(string attachmentName, Stream ttfStream) : base(ttfStream)
        {
            this.AttachmentName = attachmentName;
        }

        public static AttachmentFontFamily Create(string attachmentName, Stream ttfStream)
        {
            AttachmentFontFamily tbr = new AttachmentFontFamily(attachmentName, ttfStream);

            return tbr;
        }
    }

}
