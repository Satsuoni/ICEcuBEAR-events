using System;
using System.IO;
using SharpCompress.Archives.GZip;
using SharpCompress.Archives.Tar;
using SharpCompress.Common;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;
using SharpCompress.IO;
using SharpCompress.Readers.GZip;
//using SharpCompress.Readers.Rar;
using SharpCompress.Readers.Tar;

namespace SharpCompress.Readers
{
    public static class ReaderFactory
    {
        /// <summary>
        /// Opens a Reader for Non-seeking usage
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IReader Open(Stream stream, ReaderOptions options = null)
        {
            stream.CheckNotNull(nameof(stream));
            options = options!=null ?options: new ReaderOptions()
                                 {
                                     LeaveStreamOpen = false
                                 };
            RewindableStream rewindableStream = new RewindableStream(stream);
            rewindableStream.StartRecording();
               rewindableStream.Rewind(false);
            if (GZipArchive.IsGZipFile(rewindableStream))
            {
                rewindableStream.Rewind(false);
                GZipStream testStream = new GZipStream(rewindableStream, CompressionMode.Decompress);
                if (TarArchive.IsTarFile(testStream))
                {
                    rewindableStream.Rewind(true);
                    return new TarReader(rewindableStream, options, CompressionType.GZip);
                }
                rewindableStream.Rewind(true);
                return GZipReader.Open(rewindableStream, options);
            }

            rewindableStream.Rewind(false);
      
                     if (TarArchive.IsTarFile(rewindableStream))
            {
                rewindableStream.Rewind(true);
                return TarReader.Open(rewindableStream, options);
            }
            rewindableStream.Rewind(false);
            
            throw new InvalidOperationException("Cannot determine compressed stream type.  Supported Reader Formats: Zip, GZip, BZip2, Tar, Rar, LZip, XZ");
        }
    }
}
