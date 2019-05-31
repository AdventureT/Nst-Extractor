using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SevenZip.Sdk.Compression.Lzma;

namespace NST_Extractor
{
    // if you can read this and your name is Neo_Kesha then
    // Thanks for the code!
    public class IGA
	{
        #region Fields
        private readonly int signature;

        private readonly int version;

        private int info_size;

        private int files_count;
        private readonly int chunk_size;

        private readonly int magic_number1;

        private readonly int magic_number2;

        private readonly int zero1;

        private readonly int table1_size;

        private readonly int table2_size;

        private int names_offset;

        private readonly int zero2;

        private readonly int magic_number6;

        private readonly int one;

        public List<File> file = new List<File>();

        public List<ushort> mc_table = new List<ushort>();

        public List<ushort> sc_table = new List<ushort>();
        #endregion
        public int Info_size { get => info_size; set => info_size = value; }
        public IGA(string path)
		{
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fs);
			signature = binaryReader.ReadInt32();
			version = binaryReader.ReadInt32();
			Info_size = binaryReader.ReadInt32();
			files_count = binaryReader.ReadInt32();
			chunk_size = binaryReader.ReadInt32();
			magic_number1 = binaryReader.ReadInt32();
			magic_number2 = binaryReader.ReadInt32();
			zero1 = binaryReader.ReadInt32();
			table1_size = binaryReader.ReadInt32();
			table2_size = binaryReader.ReadInt32();
			names_offset = binaryReader.ReadInt32();
			zero2 = binaryReader.ReadInt32();
			magic_number6 = binaryReader.ReadInt32();
			one = binaryReader.ReadInt32();
			for (int i = 0; i < this.files_count; i++)
			{
				file.Add(new File());
				file[i].SetID(binaryReader.ReadInt32());
				file[i].SetSource(path);
			}
			for (int j = 0; j < this.files_count; j++)
			{
				file[j].SetOffset(binaryReader.ReadInt32());
				file[j].SetOrdinal(binaryReader.ReadInt32());
				file[j].SetSize(binaryReader.ReadInt32());
				file[j].SetCompression(binaryReader.ReadInt32());
				file[j].SetSourceOffset(file[j].GetOffset());
			}
			for (int k = 0; k < table1_size; k++)
			{
				mc_table.Add(binaryReader.ReadUInt16());
			}
			for (int l = 0; l < table2_size / 2; l++)
			{
				sc_table.Add(binaryReader.ReadUInt16());
			}
			binaryReader.BaseStream.Position = (long)names_offset;
			int[] array = new int[files_count];
			for (int m = 0; m < files_count; m++)
			{
				array[m] = binaryReader.ReadInt32();
			}
			for (int n = 0; n < files_count; n++)
			{
				binaryReader.BaseStream.Position = (long)(names_offset + array[n]);
				string fullName = ReadString(binaryReader);
				string relName = ReadString(binaryReader);
				file[n].SetFullName(fullName);
				file[n].SetRelName(relName);
			}
			binaryReader.Close();
		}

		public void Repack(string path, ProgressBar bar)
		{
			for (int i = 0; i < files_count; i++)
			{
				if (path == file[i].GetSource())
				{
					throw new Exception("Can't overwrite source!");
				}
			}
			BinaryWriter binaryWriter = new BinaryWriter(new FileStream(path, FileMode.Create, FileAccess.Write));
			this.Recalculate();
			binaryWriter.Write(signature);
			binaryWriter.Write(version);
			binaryWriter.Write(Info_size);
			binaryWriter.Write(files_count);
			binaryWriter.Write(chunk_size);
			binaryWriter.Write(magic_number1);
			binaryWriter.Write(magic_number2);
			binaryWriter.Write(zero1);
			binaryWriter.Write(table1_size);
			binaryWriter.Write(table2_size);
			binaryWriter.Write(names_offset);
			binaryWriter.Write(zero2);
			binaryWriter.Write(magic_number6);
			binaryWriter.Write(one);
            for (int j = 0; j < files_count; j++)
            {
                binaryWriter.Write(file[j].GetID());
            }
            for (int k = 0; k < files_count; k++)
            {
                binaryWriter.Write(file[k].GetOffset());
                binaryWriter.Write(file[k].GetOrdinal());
                binaryWriter.Write(file[k].GetSize());
                binaryWriter.Write(uint.MaxValue);
            }
            if (bar != null)
            {
                bar.Maximum = files_count;
            }
            for (int l = 0; l < files_count; l++)
            {
                if (bar != null)
                {
                    bar.Value = l;
                }
                Application.DoEvents();
                BinaryReader binaryReader = new BinaryReader(new FileStream(file[l].GetSource(), FileMode.Open, FileAccess.Read));
                binaryReader.BaseStream.Position = (long)file[l].GetSourceOffset();
                binaryWriter.BaseStream.Position = (long)file[l].GetOffset();
                if (this.file[l].GetCompression() == Compression.NONE)
                {
                    binaryWriter.Write(binaryReader.ReadBytes(file[l].GetSize()));
                }
                else
                {
                    Uncomress(binaryReader, binaryWriter, file[l].GetSize());
                    file[l].SetCompression(Compression.NONE);
                }
                binaryReader.Close();
            }
            binaryWriter.BaseStream.Position = (long)names_offset;
			int num = files_count * 4;
			for (int m = 0; m < files_count; m++)
			{
				binaryWriter.Write(num);
				num += file[m].GetFullName().Length + file[m].GetRelName().Length + 6;
			}
			for (int n = 0; n < files_count; n++)
			{
				string fullName = file[n].GetFullName();
				string relName = file[n].GetRelName();
				foreach (char ch in fullName)
				{
					binaryWriter.Write(ch);
				}
				binaryWriter.Write('\0');
				foreach (char ch2 in relName)
				{
					binaryWriter.Write(ch2);
				}
				binaryWriter.Write('\0');
				binaryWriter.Write(0);
			}
			binaryWriter.Close();
		}

		public void Replace(int index, string source, int offset, int size)
		{
			int size2 = file[index].GetSize();
			file[index].SetSize(size);
			file[index].SetSource(source);
			file[index].SetSourceOffset(offset);
			int size3 = file[index].GetSize();
			int num = Round(size3) - Round(size2);
			names_offset += num;
			for (int i = 0; i < files_count; i++)
			{
				if (file[i].GetOffset() > file[index].GetOffset())
				{
					file[i].SetOffset(file[i].GetOffset() + num);
				}
			}
		}

		public void Add(File file)
		{
			for (int i = 0; i < files_count; i++)
			{
				if (this.file[i].GetID() == file.GetID())
				{
					Replace(i, file.GetSource(), file.GetSourceOffset(), file.GetSize());
					return;
				}
			}
			int num = 0;
			int num2 = 0;
			for (int j = 0; j < files_count; j++)
			{
				if (this.file[j].GetOffset() + this.file[j].GetSize() > num)
				{
					num = this.file[j].GetOffset() + this.file[j].GetSize();
				}
				if (this.file[j].GetOrdinal() > num2)
				{
					num2 = this.file[j].GetOrdinal();
				}
			}
			file.SetOffset(Round(num));
			file.SetOrdinal(num2 + 1);
			files_count++;
			this.file.Add(file);
			Recalculate();
		}

		private void Recalculate()
		{
			Info_size = files_count * 20;
			int num = Round(Info_size);
			for (int i = 0; i < files_count; i++)
			{
				file[i].SetOffset(num);
				num += Round(file[i].GetSize());
			}
			names_offset = num;
		}

		public void Normalize(string path)
		{
			path = path.Replace('\\', '/');
			if (path.Last<char>() != '/')
			{
				path += "/";
			}
			Info_size = files_count * 20;
			int num = this.Round(Info_size + 56);
			for (int i = 0; i < files_count; i++)
			{
				Replace(i, path + file[i].GetFullName(), 0, file[i].GetSize());
				file[i].SetCompression(Compression.NONE);
				file[i].SetOffset(num);
				num += Round(file[i].GetSize());
			}
			names_offset = num;
		}

		public void Uncomress(BinaryReader reader, BinaryWriter writer, int size)
		{
			Decoder decoder = new Decoder();
			long position = writer.BaseStream.Position;
			while (writer.BaseStream.Position - position < (long)size)
			{
				short num = reader.ReadInt16();
				byte[] array = reader.ReadBytes(5);
				if (array[0] != 93 || BitConverter.ToInt32(array, 1) != 32768)
				{
					reader.BaseStream.Position -= 7L;
					writer.Write(reader.ReadBytes(32768));
				}
				else
				{
					decoder.SetDecoderProperties(array);
					decoder.Code(reader.BaseStream, writer.BaseStream, (long)num, Math.Min(32768L, (long)size - (writer.BaseStream.Position - position)), null);
					reader.BaseStream.Position = (long)this.Round((int)reader.BaseStream.Position);
				}
			}
		}

		public void Extract(int index, string path)
		{
			BinaryReader binaryReader = new BinaryReader(new FileStream(this.file[index].GetSource(), FileMode.Open, FileAccess.Read));
			BinaryWriter binaryWriter = new BinaryWriter(new FileStream(path, FileMode.Create, FileAccess.Write));
			binaryReader.BaseStream.Position = (long)this.file[index].GetSourceOffset();
			if (this.file[index].GetCompression() != Compression.NONE)
			{
				this.Uncomress(binaryReader, binaryWriter, this.file[index].GetSize());
			}
			else
			{
				binaryWriter.Write(binaryReader.ReadBytes(this.file[index].GetSize()));
			}
			binaryWriter.Close();
		}

		public int GetTableOffset(File f)
		{
			return f.GetCompressionInt() & -536870913;
		}

		public List<ushort> GetChunks(File f)
		{
			if (f.GetCompression() == Compression.NONE)
			{
				return null;
			}
			List<ushort> list = new List<ushort>();
			int num = this.GetTableOffset(f);
			if (f.GetSize() < 32768)
			{
				list.Add(this.sc_table[num / 2]);
			}
			else
			{
				for (;;)
				{
					list.Add(this.mc_table[num]);
					if (num >= this.mc_table.Count || this.mc_table[num] > this.mc_table[num + 1])
					{
						break;
					}
					num++;
				}
			}
			return list;
		}

		private int Round(int num)
		{
			return ((num - 1) / this.chunk_size + 1) * this.chunk_size;
		}

		private bool IsNormalized()
		{
			using (List<File>.Enumerator enumerator = this.file.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.GetCompression() != Compression.NONE)
					{
						return false;
					}
				}
			}
			return true;
		}

		private string ReadString(BinaryReader reader)
		{
			string text = "";
			char c;
			do
			{
				c = reader.ReadChar();
				if (c != '\0')
				{
					text += c.ToString();
				}
			}
			while (c != '\0');
			return text;
		}


    }
}
