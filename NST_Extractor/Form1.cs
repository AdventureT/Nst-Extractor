using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace NST_Extractor
{
    public partial class NST_Extractor : Form
    {
        private string _path;
        private IGA iga;
        public NST_Extractor()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Pak files (*.pak)|*.pak"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _path = ofd.FileName;
                label2.Text = "";
                label2.Text = _path;
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_path))
            {
                MessageBox.Show("Please choose your .pak file first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                FileStream fs = new FileStream(_path, FileMode.Open, FileAccess.Read);
                if (fs.Length == 80)
                {
                    MessageBox.Show($"The File ({fs.Length} Bytes) is too small!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    fs.Close();
                    return;
                }
                System.IO.File.Copy(_path, _path + ".bak");
                fs.Close();
                iga = new IGA(_path);
                iga.Repack(_path + ".old", progressBar1);
                ReduceFile();
                QuickBMS();
                MessageBox.Show("Extracting Successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                progressBar1.Value = 0;
            }
        }
        private void ReduceFile()
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(_path);
            BinaryWriter bw = new BinaryWriter(new FileStream(_path, FileMode.Create));
            for (int i = 0; i < 80; i++)
            {
                bw.Write(fileBytes[i]);
            }
            bw.Close();
        }

        private void QuickBMS()
        {
            string outputFolder = _path;
            outputFolder = outputFolder.Remove(outputFolder.LastIndexOf("\\"));
            outputFolder = outputFolder.Remove(outputFolder.LastIndexOf("\\"));
            string command = "DontTouch\\quickbms.exe -k \"DontTouch\\marvel_ultimate_alliance_2.bms\" \"" + _path + ".old\"" + " \"" + outputFolder + "\"";
            StreamWriter sw = new StreamWriter("DontTouch\\quickbms.bat");
            sw.Write(command);
            sw.Close();
            Process quick = new Process();
            quick.StartInfo.FileName = "DontTouch\\quickbms.bat";
            quick.Start();
            quick.WaitForExit();
        }
        /* Extracts Everything
        private void Button3_Click(object sender, EventArgs e)
        {
            string outputFolder = _path;
            outputFolder = outputFolder.Remove(outputFolder.LastIndexOf("\\"));
            string[] allFiles = Directory.GetFiles(outputFolder);
            for (int i = 0; i < allFiles.Length; i++)
            {
                FileStream fs = new FileStream(allFiles[i], FileMode.Open, FileAccess.Read);
                if (!allFiles[i].EndsWith(".old") && fs.Length > 96)
                {
                    iga = new IGA(_path);
                    iga.Repack(allFiles[i] + ".old", progressBar1);
                    ReduceFile();
                    QuickBMS();
                    MessageBox.Show("Extracting Successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    progressBar1.Value = 0;
                }
                fs.Close();
            }
        }
        */
    }
}
