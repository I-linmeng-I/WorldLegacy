using System.IO;

namespace Ionic.Zlib
{
	public class ZlibStream : Stream
	{
		internal ZlibBaseStream _baseStream;

		private bool _disposed;

		public virtual FlushType FlushMode
		{
			get
			{
				return default(FlushType);
			}
			set
			{
			}
		}

		public int BufferSize
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public virtual long TotalIn => 0L;

		public virtual long TotalOut => 0L;

		public override bool CanRead => false;

		public override bool CanSeek => false;

		public override bool CanWrite => false;

		public override long Length => 0L;

		public override long Position
		{
			get
			{
				return 0L;
			}
			set
			{
			}
		}

		public ZlibStream(Stream stream, CompressionMode mode)
		{
		}

		public ZlibStream(Stream stream, CompressionMode mode, CompressionLevel level)
		{
		}

		public ZlibStream(Stream stream, CompressionMode mode, bool leaveOpen)
		{
		}

		public ZlibStream(Stream stream, CompressionMode mode, CompressionLevel level, bool leaveOpen)
		{
		}

		protected override void Dispose(bool disposing)
		{
		}

		public override void Flush()
		{
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return 0;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return 0L;
		}

		public override void SetLength(long value)
		{
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
		}

		public static byte[] CompressBuffer(byte[] b)
		{
			return null;
		}

		public static byte[] UncompressBuffer(byte[] compressed)
		{
			return null;
		}
	}
}
