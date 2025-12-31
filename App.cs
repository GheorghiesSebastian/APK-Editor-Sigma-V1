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
using axmlprinter;
using System.Xml;
using System.Diagnostics;

namespace APK_editor_Sigma
{
    public partial class App : Form
    {
        string appDirectory = AppDomain.CurrentDomain.BaseDirectory;

        public static string TXTposlocation = string.Empty;
        string apkPath;
        string extract;
        string repack;
        string newxt;
        bool firstvalue = false;

        public static string manifest;


        string TXTpos;

        int counter;

        bool inCommentBlock = false;

        public static string version;
        static string appBaseDir = AppDomain.CurrentDomain.BaseDirectory;
        string apksignerJar = Path.Combine(appBaseDir, "Tools", "apksigner.jar");
        string settingstxt = Path.Combine(appBaseDir, "Settings.txt");


        public App()
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
                counter = splitter.Length;

                label2.Text = apkPath;
                label4.Text = splitter[counter - 1];

                extract = Path.Combine(Path.GetDirectoryName(apkPath),
                                       Path.GetFileNameWithoutExtension(apkPath) + "_extracted");

                try
                {
                    using (var zip = ZipFile.OpenRead(apkPath))
                    {
                        var iconEntry = zip.Entries
                            .FirstOrDefault(x => (x.FullName.StartsWith("res/drawable") ||
                                                  x.FullName.StartsWith("res/mipmap")) &&
                                                 (x.FullName.EndsWith("ic_launcher.png") ||
                                                  x.FullName.EndsWith("icon.png")));

                        if (iconEntry != null)
                        {
                            using (var stream = iconEntry.Open())
                            {
                                pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
                                pictureBox3.Image = System.Drawing.Image.FromStream(stream);
                            }
                        }
                        else
                        {
                            pictureBox3.Image = null;
                            MessageBox.Show("No PNG launcher icon found in the APK.");
                        }
                    }
                }
                catch
                {
                    pictureBox3.Image = null;
                }
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(apkPath) || !File.Exists(apkPath))
            {
                MessageBox.Show("No APK selected.");
                return;
            }

            using (var folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    extract = Path.Combine(folderDialog.SelectedPath,
                                           Path.GetFileNameWithoutExtension(apkPath) + "_extracted");

                    if (!Directory.Exists(extract))
                    {
                        Directory.CreateDirectory(extract);
                    }

                    try
                    {
                        ZipFile.ExtractToDirectory(apkPath, extract);
                        MessageBox.Show("APK extracted to: " + extract);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error extracting APK: " + ex.Message);
                    }
                }
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
            if (!string.IsNullOrEmpty(repack))
            {
                string repackedFilePath = Path.Combine(repack, "Unsigned_apkfile.apk");
                ZipFile.CreateFromDirectory(extract, repackedFilePath);
            }
            else
            {
                MessageBox.Show("Invalid repack destination.");
            }


        }

        private void button6_Click(object sender, EventArgs e)
        {
            OpenFileDialog File2 = new OpenFileDialog();
            File2.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";

            if (File2.ShowDialog() == DialogResult.OK)
            {
                TXTpos = File2.FileName;
                TXTposlocation = TXTpos;

                SettingsManager sm = new SettingsManager();
                sm.ChangeTXTPath(TXTpos);

                MessageBox.Show("TXT path saved to Settings.txt");
            }
        }



        async private void Ticked(object sender, EventArgs e)
        {
            await Task.Delay(100);
            Timer timer = sender as Timer;

            string origin;
            string destination;

            if (timer.Tag is Tuple<string, string> data)
            {
                origin = Path.Combine(newxt, data.Item1);
                destination = Path.Combine(data.Item2, data.Item1);
                if (File.Exists(origin))
                {
                    File.Move(origin, destination);
                    timer.Enabled = false;
                }
            }
            else
            {
                //nothing lol 
            }


        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                if (TXTpos != string.Empty)
                {
                    newxt = TXTpos;
                }
                else
                {
                    string[] lines2 = File.ReadAllLines(settingstxt);
                    string line1 = lines2[0];
                    string[] splitter = line1.Split(':');
                    string path = splitter[1].ToString().Trim();
                    newxt = path;
                }
                string[] lines = File.ReadAllLines(TXTpos);

                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();

                    if (trimmedLine == "//")
                    {
                        inCommentBlock = !inCommentBlock;
                        continue;
                    }

                    if (inCommentBlock)
                        continue;

                    int commentIndex = line.IndexOf("//");
                    string processedLine = commentIndex >= 0
                        ? line.Substring(0, commentIndex)
                        : line;

                    if (string.IsNullOrWhiteSpace(processedLine))
                        continue;

                    string[] splitter = processedLine.Split(']');

                    for (int i = 0; i < splitter.Length; i++)
                    {
                        string token = splitter[i].Trim();

                        if (token == "continue" && i + 1 < splitter.Length)
                        {
                            newxt = Path.Combine(newxt, splitter[++i].Trim());
                        }
                        else if (token == "switch" && i + 1 < splitter.Length)
                        {
                            newxt = splitter[++i].Trim();
                        }
                        else if (token == "move" && i + 2 < splitter.Length)
                        {
                            string sourceFile = Path.Combine(newxt, splitter[i + 1].Trim());
                            string destinationPath = splitter[i + 2].Trim();
                            i += 2;
                        }
                        else if (token == "copy" && i + 2 < splitter.Length)
                        {
                            string sourceFile = Path.Combine(newxt, splitter[i + 1].Trim());
                            string destinationPath = splitter[i + 2].Trim();
                            i += 2;
                        }
                        else if (token == "rename" && i + 3 < splitter.Length)
                        {
                            string target = splitter[i + 1].Trim();
                            string newName = splitter[i + 2].Trim();
                            string mode = splitter[i + 3].Trim();
                            i += 3;

                            string fullPath;

                            if (mode == "0")
                            {
                                fullPath = target;
                            }
                            else
                            {
                                fullPath = Path.Combine(newxt, target);
                            }

                            if (File.Exists(fullPath))
                            {
                                string newPath = Path.Combine(Path.GetDirectoryName(fullPath), newName);
                                File.Move(fullPath, newPath);
                                MessageBox.Show($"Renamed {fullPath} to {newPath}");
                            }
                            else
                            {
                                MessageBox.Show($"File does not exist: {fullPath}");
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }




        private void Form1_Load(object sender, EventArgs e)
        {
            SettingsManager lol = new SettingsManager();
            lol.Instantiate();

            pictureBox4.Visible = SettingsManager.Anime;
            pictureBox5.Visible = SettingsManager.Anime;
            pictureBox6.Visible = SettingsManager.Anime;


        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(apkPath) || !File.Exists(apkPath))
            {
                MessageBox.Show("No APK selected.");
                return;
            }

            string tempDir = Path.Combine(Path.GetTempPath(), "APK_editor_Sigma_" + Guid.NewGuid());
            Directory.CreateDirectory(tempDir);

            try
            {
                ZipFile.ExtractToDirectory(apkPath, tempDir);

                string manifestPath = Directory
                    .GetFiles(tempDir, "AndroidManifest.xml", SearchOption.AllDirectories)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(manifestPath))
                {
                    MessageBox.Show("AndroidManifest.xml not found.");
                    return;
                }

                using (var fs = File.OpenRead(manifestPath))
                {
                    string xmlContent = AXMLPrinter.getXmlFromStream(fs);
                    XDocument xmlDocument = XDocument.Parse(xmlContent);
                    string formattedXml = xmlDocument.ToString();

                    using (var folderDialog = new FolderBrowserDialog())
                    {
                        if (folderDialog.ShowDialog() == DialogResult.OK)
                        {
                            string outputPath = Path.Combine(folderDialog.SelectedPath, "AndroidManifest.xml.txt");
                            File.WriteAllText(outputPath, formattedXml, Encoding.UTF8);
                            MessageBox.Show("AndroidManifest.xml extracted successfully.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error extracting AndroidManifest: " + ex.Message);
            }
            finally
            {
                try
                {
                    if (Directory.Exists(tempDir))
                        Directory.Delete(tempDir, true);
                }
                catch { }
            }
        }


        private void button7_Click(object sender, EventArgs e)
        {
            /*using (var fileDialog = new OpenFileDialog())
            {
                fileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string xmlContent = File.ReadAllText(fileDialog.FileName, Encoding.UTF8);

                        // Convert readable XML back into binary AXML (needs a writer library)
                        byte[] binaryManifest = XmlWriter.Create

                        string manifestPath = Path.Combine(extract, "AndroidManifest.xml");
                        File.WriteAllBytes(manifestPath, binaryManifest);

                        MessageBox.Show("Successfully repacked manifest into: " + manifestPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error repacking AndroidManifest: " + ex.Message);
                    }
                }
            }
            */
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(apkPath) || !File.Exists(apkPath))
            {
                MessageBox.Show("No APK selected.");
                return;
            }

            string tempDir = Path.Combine(Path.GetTempPath(), "APK_editor_Sigma_" + Guid.NewGuid());
            Directory.CreateDirectory(tempDir);

            try
            {
                ZipFile.ExtractToDirectory(apkPath, tempDir);

                string manifestPath = Directory
                    .GetFiles(tempDir, "AndroidManifest.xml", SearchOption.AllDirectories)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(manifestPath))
                {
                    MessageBox.Show("AndroidManifest.xml not found.");
                    Directory.Delete(tempDir, true);
                    return;
                }

                using (var fs = File.OpenRead(manifestPath))
                {
                    string xmlContent = AXMLPrinter.getXmlFromStream(fs);
                    XDocument xmlDocument = XDocument.Parse(xmlContent);
                    manifest = xmlDocument.ToString();
                }

                Manifest man = new Manifest();

                man.FormClosed += (s, args) =>
                {
                    try
                    {
                        if (Directory.Exists(tempDir))
                            Directory.Delete(tempDir, true);
                    }
                    catch { }
                };

                man.Show();
            }
            catch (Exception ex)
            {
                try
                {
                    if (Directory.Exists(tempDir))
                        Directory.Delete(tempDir, true);
                }
                catch { }

                MessageBox.Show("Error: " + ex.Message);
            }
        }



        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void button7_Click_1(object sender, EventArgs e)
        {
            string apkToSign;

            if (apkPath == null || apkPath == string.Empty)
            {
                OpenFileDialog openFile = new OpenFileDialog();
                openFile.Filter = "APK Files (*.apk)|*.apk|All Files (*.*)|*.*";
                if (openFile.ShowDialog() != DialogResult.OK) return;
                apkToSign = openFile.FileName;
            }
            else
            {
                apkToSign = apkPath;
            }

            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "APK Files (*.apk)|*.apk";
            saveFile.FileName = Path.GetFileNameWithoutExtension(apkToSign) + "_signed.apk";
            if (saveFile.ShowDialog() != DialogResult.OK) return;
            string signedApk = saveFile.FileName;

            string tempSignedApk = Path.Combine(Path.GetTempPath(), Path.GetFileName(apkToSign));
            File.Copy(apkToSign, tempSignedApk, true);

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "java",
                Arguments = $"-jar \"{apksignerJar}\" sign \"{tempSignedApk}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(psi))
                process.WaitForExit();

            File.Copy(tempSignedApk, signedApk, true);
            MessageBox.Show("APK successfully signed and saved to: " + signedApk);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Settings f = new Settings();
            f.Show();
            this.Hide();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
