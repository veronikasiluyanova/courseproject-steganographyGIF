using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Steganography_GIF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string gifPath ="", gifPath_new = "", gifPath_B = "";
        string filePath = "", filePath_B = "";
        string text = "";
        bool LSB, LSB_B, palExt, palExt_B;

        public MainWindow()
        {
            InitializeComponent();
            CommandBinding btnLoadGifCommandBinding = new CommandBinding();
            btnLoadGifCommandBinding.Command = ApplicationCommands.Open;
            btnLoadGifCommandBinding.Executed += BtnLoadGifCommandBinding_Executed;
            btnLoadGif.CommandBindings.Add(btnLoadGifCommandBinding);

            CommandBinding btnLoadGifCommandBinding_B = new CommandBinding();
            btnLoadGifCommandBinding_B.Command = ApplicationCommands.Open;
            btnLoadGifCommandBinding_B.Executed += BtnLoadGifCommandBinding_B_Executed;
            btnLoadGif_B.CommandBindings.Add(btnLoadGifCommandBinding_B);

            CommandBinding btnLoadFileCommandBinding = new CommandBinding();
            btnLoadFileCommandBinding.Command = ApplicationCommands.Open;
            btnLoadFileCommandBinding.Executed += BtnLoadFileCommandBinding_Executed; 
            btnLoadFile.CommandBindings.Add(btnLoadFileCommandBinding);

            CommandBinding saveButtonCommandBinding = new CommandBinding();
            saveButtonCommandBinding.Command = ApplicationCommands.Save;
            saveButtonCommandBinding.Executed += SaveButtonCommandBinding_Executed;
            btnSave.CommandBindings.Add(saveButtonCommandBinding);

            CommandBinding saveButtonCommandBinding_B = new CommandBinding();
            saveButtonCommandBinding_B.Command = ApplicationCommands.Save;
            saveButtonCommandBinding_B.Executed += SaveButtonCommandBinding_B_Executed;
            btnSave_B.CommandBindings.Add(saveButtonCommandBinding_B);

            CommandBinding exitButtonCommandBinding = new CommandBinding();
            exitButtonCommandBinding.Command = ApplicationCommands.Close;
            exitButtonCommandBinding.Executed += ExitButtonCommandBinding_Executed;
            btnExit.CommandBindings.Add(exitButtonCommandBinding);
            btnExit_B.CommandBindings.Add(exitButtonCommandBinding);
        }

        private void BtnLoadGifCommandBinding_B_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "GIF (*.gif)|*.gif|All files (*.*)|*.*";
            if (Convert.ToBoolean(openFile.ShowDialog()))
            {
                gifPath_B = "";

                FileStream fileStream = new FileStream(openFile.FileName, FileMode.Open);
                gifPath_B = openFile.FileName;
                fileStream.Close();

                tbLoadGif_B.Text = gifPath_B;

                gifPreview_B.Source = new Uri(gifPath_B);
                gifPreview_B.Position = new TimeSpan(0, 0, 1);
                gifPreview_B.Play();
            }
        }

        private void BtnLoadGifCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "GIF (*.gif)|*.gif|All files (*.*)|*.*";
            if (Convert.ToBoolean(openFile.ShowDialog()))
            {
                gifPath = "";

                FileStream fileStream = new FileStream(openFile.FileName, FileMode.Open);
                gifPath = openFile.FileName;
                fileStream.Close();

                tbLoadGif.Text = gifPath;

                tblSize.Text = "GIF size: " + new FileInfo(gifPath).Length + " bytes";
                tblPossibleLength.Text = "Possible text length: " + Encryptor.GetPossibleTextLength(gifPath);
                tblPaletteSize.Text = "Palette size: " + Encryptor.GetPaletteSize(gifPath);

                gifPreview.Source = new Uri(gifPath);
                gifPreview.Position = new TimeSpan(0, 0, 1);
                gifPreview.Play();
            }
        }

        private void BtnLoadFileCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Text File (*.txt)|*.txt|All files (*.*)|*.*";
            if (Convert.ToBoolean(openFile.ShowDialog()))
            {
                filePath = "";
                StreamReader fileStream = new StreamReader(openFile.FileName);
                text = fileStream.ReadToEnd();
                fileStream.Close();
                filePath = openFile.FileName;

                tbLoadFile.Text = filePath;
                tbInputText.Text = text;

                tblTextLength.Text = "Text length: " + text.Length;
            }
        }

        private void SaveButtonCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "GIF (*.gif)|*.gif|All files (*.*)|*.*";
            if (Convert.ToBoolean(saveFile.ShowDialog()))
            {
                FileStream fileStream = new FileStream(saveFile.FileName, FileMode.Create);
                gifPath_new = saveFile.FileName;
                tbSaveTo.Text = gifPath_new;
                fileStream.Close();

                tbSaveTo.Text = filePath;
            }
        }

        private void SaveButtonCommandBinding_B_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "Text File (*.txt)|*.txt|All files (*.*)|*.*";
            if (Convert.ToBoolean(saveFile.ShowDialog()))
            {
                FileStream fileStream = new FileStream(saveFile.FileName, FileMode.Create);
                filePath_B = saveFile.FileName;
                tbSaveTo_B.Text = filePath_B;
                fileStream.Close();

            }
        }

        private void ExitButtonCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void btnEmbed_Click(object sender, RoutedEventArgs e)
        {
            if (gifPath.Length != 0)
            {
                if (text.Length != 0)
                {
                    if (gifPath_new.Length != 0)
                    {
                        if (LSB)
                        {
                            File.WriteAllBytes(gifPath_new, GIFEncryptorByLSBMethod.Encrypt(gifPath, text));
                            tblResult.Text = "Complete successfully!";
                        }
                        else if (palExt)
                        {
                            File.WriteAllBytes(gifPath_new, GIFEncryptorByPaletteExtensionMethod.Encrypt(gifPath, text));
                            tblResult.Text = "Complete successfully!";
                        }
                        else
                        {
                            tblResult.Text = "No method selected!";
                        }
                    }
                    else
                    {
                        tblResult.Text = "No path to save file!";
                    }
                }
                else
                {
                    tblResult.Text = tblResult.Text = "No message to embed!";
                }
            }
            else
            {
                tblResult.Text = "No GIF container selected!";
            }
        }
        private void btnExtract_Click(object sender, RoutedEventArgs e)
        {
            if (gifPath_B.Length != 0)
            {
                if (filePath_B.Length != 0)
                {
                    if (LSB_B)
                    {
                        File.WriteAllBytes(filePath_B, GIFEncryptorByLSBMethod.Decrypt(gifPath_B));
                        tblResult_B.Text = "Complete successfully!";
                    }
                    else if (palExt_B)
                    {
                        File.WriteAllBytes(filePath_B, GIFEncryptorByPaletteExtensionMethod.Decrypt(gifPath_B));
                        tblResult_B.Text = "Complete successfully!";
                    }
                    else
                    {
                        tblResult_B.Text = "No method selected!";
                    }
                }
                else
                {
                    tblResult_B.Text = "No path to save message!";
                }
            }
            else
            {
                tblResult_B.Text = "No GIF container selected!";
            }
        }

        private void rbtnLSB_Checked(object sender, RoutedEventArgs e)
        {
            LSB = true;
            palExt = false;

            if (text.Length == 0)
            {
                if (tbInputText.Text.Length != 0)
                {
                    text = tbInputText.Text;
                    tblTextLength.Text = "Text length: " + text.Length;
                }
                else
                {
                    tblResult.Text = tblResult.Text = "No message to embed!";
                }
            }
        }

        private void rbtnPalExt_Checked(object sender, RoutedEventArgs e)
        {
            LSB = false;
            palExt = true;

            if (text.Length == 0)
            {
                if (tbInputText.Text.Length != 0)
                {
                    text = tbInputText.Text;
                    tblTextLength.Text = "Text length: " + text.Length;
                }
                else
                {
                    tblResult.Text = tblResult.Text = "No message to embed!";
                }
            }
        }

        private void rbtnLSB_B_Checked(object sender, RoutedEventArgs e)
        {
            LSB_B = true;
            palExt_B = false;
        }

        private void rbtnPalExt_B_Checked(object sender, RoutedEventArgs e)
        {
            LSB_B = false;
            palExt_B = true;
        }

        private void gifPreview_MediaEnded(object sender, RoutedEventArgs e)
        {
            gifPreview.Position = new TimeSpan(0, 0, 1);
            gifPreview.Play();
        }
        private void gifPreview_B_MediaEnded(object sender, RoutedEventArgs e)
        {
            gifPreview_B.Position = new TimeSpan(0, 0, 1);
            gifPreview_B.Play();
        }

    }
}
