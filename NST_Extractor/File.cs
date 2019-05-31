using System;
using System.IO;

namespace NST_Extractor
{
	// if you can read this and your name is Neo_Kesha then
    // Thanks for the code!
	public class File
	{
		public void SetChunkOffsets(short[] chunk_offsets)
		{
			this.chunk_offsets = chunk_offsets;
		}

		public void SetID(int ID)
		{
			this.ID = ID;
		}

		public void SetOffset(int offset)
		{
			this.offset = offset;
		}

		public void SetOrdinal(int ordinal)
		{
			this.ordinal = ordinal;
		}

		public void SetSize(int size)
		{
			this.size = size;
		}

		public void SetCompression(int compression)
		{
			this.compression = compression;
		}

		public void SetCompression(Compression compression)
		{
			switch (compression)
			{
			case Compression.NONE:
				this.compression = -1;
				return;
			case Compression.LZMA:
				this.compression = 536870912;
				return;
			case Compression.DEFLATE:
				this.compression = 0;
				return;
			default:
				return;
			}
		}

		public void SetRelName(string rel_name)
		{
			this.rel_name = rel_name;
		}

		public void SetFullName(string full_name)
		{
			this.full_name = full_name;
		}

		public void SetSource(string file_path)
		{
			this.file_path = file_path;
		}

		public void SetSourceOffset(int source_offset)
		{
			this.source_offset = source_offset;
		}

		public short[] GetChunkOffsets()
		{
			return this.chunk_offsets;
		}

		public int GetID()
		{
			return this.ID;
		}

		public int GetOffset()
		{
			return this.offset;
		}

		public int GetOrdinal()
		{
			return this.ordinal;
		}

		public int GetSize()
		{
			return this.size;
		}

		public int GetCompressionInt()
		{
			return this.compression;
		}

		public Compression GetCompression()
		{
			if (this.compression == -1)
			{
				return Compression.NONE;
			}
			if ((this.compression & 536870912) != 0)
			{
				return Compression.LZMA;
			}
			return Compression.DEFLATE;
		}

		public string GetRelName()
		{
			return this.rel_name;
		}

		public string GetFullName()
		{
			return this.full_name;
		}

		public string GetSource()
		{
			return this.file_path;
		}

		public int GetSourceOffset()
		{
			return this.source_offset;
		}

		private int ID;

		private int offset;

		private int ordinal;

		private int size;

		private int compression;

		private string full_name;

		private string rel_name;

		private short[] chunk_offsets;

		private string file_path;

		private int source_offset;

		public MemoryStream stream;
	}
}
