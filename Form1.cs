using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Compression;
using System.IO;
using System.Xml.Linq;

namespace APK_editor_Sigma
{
    public partial class Form1 : Form
    {
        string apkPath;
        string extract;
        string repack;
        string newxt;
        bool firstvalue = false;
        

        string TXTpos;

        int counter;

        public Form1()
        {
            InitializeComponent();
        }

        private void buttonAPK_Click(object sender, EventArgs e)
        {
            OpenFileDialog File = new OpenFileDialog();
            File.Filter = "APK Files (*.apk)|*.apk|All Files (*.*)|*.*";

            if (File.ShowDialog() == DialogResult.OK)
            {
                apkPath = File.FileName;

                string[] splitter = apkPath.Split('\\');
                foreach (var item in splitter)
                {
                    ++counter;
                }

                label2.Text = apkPath;
                label4.Text = splitter[counter - 1];
            }


        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(extract) && Directory.Exists(extract)) // Ensure extract path is valid
            {
                ZipFile.ExtractToDirectory(apkPath, extract);
            }
            else
            {
                MessageBox.Show("Invalid extract path.");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                extract = folderDialog.SelectedPath;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(repack)) // Ensure repack path is valid
            {
                string repackedFilePath = Path.Combine(repack, "Unsigned_apkfile.apk");
                ZipFile.CreateFromDirectory(extract, repackedFilePath);
            }
            else
            {
                MessageBox.Show("Invalid repack destination.");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                repack = folderDialog.SelectedPath;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            OpenFileDialog File = new OpenFileDialog();

            if (File.ShowDialog() == DialogResult.OK)
            {
                TXTpos = File.FileName;
            }
        }

        async private void Ticked(object sender, EventArgs e)
        {
            await Task.Delay(100);
            Timer timer = sender as Timer;

            string origin;
            string destination;

            if (timer.Tag is Tuple<string, string> data )
            {
                origin = Path.Combine(newxt, data.Item1);
                destination = Path.Combine(data.Item2, data.Item1);
                if (File.Exists(origin))
                {
                    File.Move(origin, destination);
                    timer.Enabled = false;
                    firstvalue = true;
                }
            }
            else
            {
                
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            newxt = extract;
            string[] lines = File.ReadAllLines(TXTpos);

            foreach (string line in lines)
            {
                string[] splitter = line.Split(']');
                for (int i = 0; i < splitter.Length; i++)
                {
                    if (splitter[i] == "continue")
                    {
                        if (i + 1 < splitter.Length)
                        {
                            newxt = Path.Combine(newxt, splitter[++i]);
                        }
                    }
                    if (splitter[i] == "switch")
                    {
                        newxt = splitter[i+1];
                    }
                    if (splitter[i] == "move")
                    {
                        string sourceFile = Path.Combine(newxt, splitter[i + 1]);
                        string destinationPath = splitter[i + 2];

                        if (File.Exists(sourceFile))
                        {
                            if (Directory.Exists(destinationPath))
                            {
                                destinationPath = Path.Combine(destinationPath, Path.GetFileName(sourceFile));
                            }

                            File.Move(sourceFile, destinationPath);
                            MessageBox.Show($"Moved {sourceFile} to {destinationPath}");
                        }
                        else
                        {
                            MessageBox.Show($"Source file does not exist: {sourceFile}");
                        }
                    }
                    if (splitter[i] == "backto")
                    {
                        string backto = splitter[i + 1];
                        string newxt2 = string.Empty;
                        string[] splitback = newxt.Split('\\');

                        for (int i2 = 0; i2 < newxt.Length; i2++)
                        {
                            if (splitback[i2] == backto)
                            {
                                newxt2 += backto;
                                break;
                            }
                            else
                            {
                                newxt2 += splitback[i2];
                                i2++;
                            }
                        }
                        newxt = newxt2;

                    }
                    if (splitter[i] == "waitmove")
                    {
                        string name = splitter[i + 1];
                        string destination = splitter[i + 2];

                        Timer timer = new Timer();
                        timer.Interval = 1000;
                        timer.Tick += Ticked;
                        timer.Start();

                        timer.Tag = new Tuple<string, string>(name, destination);

                        if (firstvalue == true)
                        {
                            timer.Enabled = false;
                        }

                    }
                    else if (splitter[i] == "copy")
                    {
                        if (i + 2 < splitter.Length)
                        {
                            string sourceFile = Path.Combine(newxt, splitter[i + 1]);
                            string destinationPath = splitter[i + 2];
                            if (File.Exists(sourceFile))
                            {
                                if (Directory.Exists(destinationPath))
                                {
                                    destinationPath = Path.Combine(destinationPath, Path.GetFileName(sourceFile));
                                }

                                File.Copy(sourceFile, destinationPath, overwrite: true);
                                MessageBox.Show($"Copied {sourceFile} to {destinationPath}");
                            }
                            else
                            {
                                MessageBox.Show($"Source file does not exist: {sourceFile}");
                            }
                        }
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            string currentext = extract;
            string manifest = Path.Combine(currentext + "\\AndroidManifest.xml" );

            if (File.Exists(manifest))
            {
                string xmlContent = File.ReadAllText(manifest);

                XDocument xmlDocument = XDocument.Parse(xmlContent);
                string formattedXml = xmlDocument.ToString();

                File.WriteAllText("C:\\Users\\Sebi\\Desktop\\Akaskav\\filetxt.txt", formattedXml, Encoding.UTF8);

                MessageBox.Show("XML successfully converted to TXT.");
            }
        }
    }
}
