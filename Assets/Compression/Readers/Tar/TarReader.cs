﻿using System;
using System.Collections.Generic;
using System.IO;
using SharpCompress.Archives.GZip;
using SharpCompress.Archives.Tar;
using SharpCompress.Common;
using SharpCompress.Common.Tar;
using SharpCompress.Compressors;
//using SharpCompress.Compressors.BZip2;
using SharpCompress.Compressors.Deflate;
using SharpCompress.IO;
//using SharpCompress.Compressors.LZMA;
//using SharpCompress.Compressors.Xz;

namespace SharpCompress.Readers.Tar
{
    public class TarReader : AbstractReader<TarEntry, TarVolume>
    {
        private readonly CompressionType compressionType;

        internal TarReader(Stream stream, ReaderOptions options, CompressionType compressionType)
            : base(options, ArchiveType.Tar)
        {
            this.compressionType = compressionType;
            Volume = new TarVolume(stream, options);
        }

        public override TarVolume Volume { get; }

        protected override Stream RequestInitialStream()
        {
            var stream = base.RequestInitialStream();
            switch (compressionType)
            {
               
                case CompressionType.GZip:
                {
                    return new GZipStream(stream, CompressionMode.Decompress);
                }
               
                case CompressionType.None:
                {
                    return stream;
                }
                default:
                {
                    throw new NotSupportedException("Invalid compression type: " + compressionType);
                }
            }
        }

        #region Open

        /// <summary>
        /// Opens a TarReader for Non-seeking usage with a single volume
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static TarReader Open(Stream stream, ReaderOptions options = null)
        {
            stream.CheckNotNull(nameof(stream));
            options = options !=null ? options: new ReaderOptions();
            RewindableStream rewindableStream = new RewindableStream(stream);
            rewindableStream.StartRecording();
            if (GZipArchive.IsGZipFile(rewindableStream))
            {
                rewindableStream.Rewind(false);
                GZipStream testStream = new GZipStream(rewindableStream, CompressionMode.Decompress);
                if (TarArchive.IsTarFile(testStream))
                {
                    rewindableStream.Rewind(true);
                    return new TarReader(rewindableStream, options, CompressionType.GZip);
                }
                throw new InvalidFormatException("Not a tar file.");
            }

            rewindableStream.Rewind(false);
              rewindableStream.Rewind(true);
            return new TarReader(rewindableStream, options, CompressionType.None);
        }

        #endregion Open

        protected override IEnumerable<TarEntry> GetEntries(Stream stream)
        {
            return TarEntry.GetEntries(StreamingMode.Streaming, stream, compressionType, Options.ArchiveEncoding);
        }
    }
}
