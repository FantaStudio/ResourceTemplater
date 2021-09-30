using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace ResourceTemplater
{
    public partial class MainWindow : Window
    {
        OpenFileDialog _fileDialog = new OpenFileDialog() { Multiselect = true };
        OpenFileDialog _templateDialog = new OpenFileDialog() { Multiselect = false, Filter = "Resource template | *.rt", InitialDirectory = Directory.GetCurrentDirectory()};

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Header_Bar_MouseDown(object sender, MouseButtonEventArgs e)
            => DragMove();

        private void Minimize_Button_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private static string CreateMetaParametrs(params (string,string)[] args)
        {
            string result = "";
            foreach(var parametr in args)
            {
                if (!string.IsNullOrWhiteSpace(parametr.Item1) && !string.IsNullOrWhiteSpace(parametr.Item2))
                {
                    result += $"{parametr.Item1}=\"{parametr.Item2}\"{(parametr == args.Last() ? "":" ")}";
                }
            }
            return result;
        }

        static string[] fontsType = new string[] { ".ttf", ".otf" };
        static string[] imgsType = new string[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };
        static string[] soundsType = new string[] { ".mp3", ".wav", ".ogg", ".riff", ".mod", ".xm", ".it", ".pls" };
        static string[] shadersType = new string[] { ".fx" };
        static string[] databasesType = new string[] { ".db", ".sql" };

        static public string getFolderByFileType(string fileType)
        {
            if(fontsType.Where(x => x == fileType).Any())
                return "fonts";
            if (imgsType.Where(x => x == fileType).Any())
                return "imgs";
            if (soundsType.Where(x => x == fileType).Any())
                return "sounds";
            if (shadersType.Where(x => x == fileType).Any())
                return "shaders";
            if (databasesType.Where(x => x == fileType).Any())
                return "db";
            return "";
        }

        private string getFileNameIfRepeat(string path)
        {
            int counter = 0;
            string path2 = path;
            while (File.Exists(path2))
            {
                counter++;
                path2 = System.IO.Path.GetDirectoryName(path) + "/" + System.IO.Path.GetFileNameWithoutExtension(path) + "(" + counter + ")" + System.IO.Path.GetExtension(path);
            }
            return path2;
        }

        private void Create_Resource_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Client_Checkbox.IsChecked && !Server_Checkbox.IsChecked && !Shared_Checkbox.IsChecked)
                {
                    throw new Exception("Выберите хотя-бы один элемент из основы");
                }
                if (string.IsNullOrWhiteSpace(Resource_Name_Box.Text))
                {
                    throw new Exception("Введите название ресурса");
                }
                if(!Regex.IsMatch(Resource_Name_Box.Text, "^[a-zA-Z0-9()\\[\\]-_]*$"))
                {
                    throw new Exception("Название ресурса не может содержать специальные символы, допускаются только буквы, цифры и символы: ()[]-_");
                }
                if (!Regex.IsMatch(Version_Box.Text, "^[a-zA-Z0-9()\\[\\].-]*$"))
                {
                    throw new Exception("Версия может содержать только буквы, цифры и символы: ()[].-_");
                }

                var mainDirectory = Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/resources");
                if (Directory.Exists(mainDirectory.FullName + "/" + Resource_Name_Box.Text))
                    Directory.Delete(mainDirectory.FullName + "/" + Resource_Name_Box.Text,true);
                var currentDirectory = Directory.CreateDirectory(mainDirectory.FullName + "/" + Resource_Name_Box.Text);
                var metaFilePath = currentDirectory.FullName + "/meta.xml";

                // Информация
                string fullMetaData = "<meta>";
                fullMetaData += $"\n<info {CreateMetaParametrs(("author", Author_Box.Text), ("version", Version_Box.Text), ("name", Resource_Name_Box.Text))} />";

                // Основа
                fullMetaData += "\n<!--- Основа -->";
                if (Client_Checkbox.IsChecked)
                {
                    var clientName = (bool)Full_Name_Checkbox.IsChecked ? "client.lua" : "c.lua";
                    fullMetaData += $"\n<script {CreateMetaParametrs(("src", clientName), ("type", "client"), ("cache", (bool)Cache_Checkbox.IsChecked ? "true" : "false"))} />";
                    File.WriteAllText($"{currentDirectory.FullName}/{clientName}","");
                }
                if (Server_Checkbox.IsChecked)
                {
                    var serverName = (bool)Full_Name_Checkbox.IsChecked ? "server.lua" : "s.lua";
                    fullMetaData += $"\n<script {CreateMetaParametrs(("src", serverName), ("type", "server"), ("cache", (bool)Cache_Checkbox.IsChecked ? "true" : "false"))} />";
                    File.WriteAllText($"{currentDirectory.FullName}/{serverName}","");
                }
                if (Shared_Checkbox.IsChecked)
                {
                    fullMetaData += $"\n<script {CreateMetaParametrs(("src", "shared.lua"), ("type", "shared"))} />";
                    File.WriteAllText($"{currentDirectory.FullName}/shared.lua","");
                }

                // Файлы
                if (File_List_Box.Items.Count > 0)
                {
                    fullMetaData += "\n<!--- Файлы -->";
                    var filesDir = Directory.CreateDirectory(currentDirectory.FullName + "/files");
                    foreach (string item in File_List_Box.Items)
                    {
                        string fileFolder = getFolderByFileType(System.IO.Path.GetExtension(item));
                        if (string.IsNullOrEmpty(fileFolder))
                        {
                            string fileName = getFileNameIfRepeat(filesDir.FullName + "/" + System.IO.Path.GetFileName(item));
                            File.Copy(item, fileName);
                            fileName = System.IO.Path.GetFileName(fileName);
                            fullMetaData += $"\n<file {CreateMetaParametrs(("src", "files/" + fileName), ("download", (bool)Download_Checkbox.IsChecked ? "true" : "false"))} />";
                        }
                        else
                        {
                            if (!Directory.Exists(filesDir.FullName + "/" + fileFolder))
                                Directory.CreateDirectory(filesDir.FullName + "/" + fileFolder);
                            string fileName = getFileNameIfRepeat(filesDir.FullName + "/" + fileFolder + "/" + System.IO.Path.GetFileName(item));
                            File.Copy(item, fileName);
                            fileName = System.IO.Path.GetFileName(fileName);
                            fullMetaData += $"\n<file {CreateMetaParametrs(("src", "files/" + fileFolder + "/" + fileName), ("download", (bool)Download_Checkbox.IsChecked ? "true" : "false"))} />";
                        }
                    }
                }


                // Дополнительно
                if ((bool)OOP_Checkbox.IsChecked || !string.IsNullOrWhiteSpace(Minimal_Version_Box.Text))
                {
                    fullMetaData += "\n<!--- Дополнительно -->";
                    if ((bool)OOP_Checkbox.IsChecked)
                        fullMetaData += "\n<oop>true</oop>";
                    else
                         fullMetaData += $"\n<min_mta_version {CreateMetaParametrs(("client", Minimal_Version_Box.Text), ("server", Minimal_Version_Box.Text))} />";
                }
                fullMetaData += "\n</meta>";

                File.WriteAllText(metaFilePath,fullMetaData);
                MessageBox.Show("Шаблон ресурса успешно создан","Круто :)",MessageBoxButton.OK,MessageBoxImage.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message,"Ошибка",MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }
        
        private void Choose_Files_Button_Click(object sender, RoutedEventArgs e)
        {
            if(_fileDialog.ShowDialog() == true)
            {
                File_List_Box.ItemsSource = _fileDialog.FileNames;
            }
        }

        private void Clear_Files_Button_Click(object sender, RoutedEventArgs e)
        {
            File_List_Box.Items.Clear();
        }

        private void Save_Template_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Client_Checkbox.IsChecked && !Server_Checkbox.IsChecked && !Shared_Checkbox.IsChecked)
                {
                    throw new Exception("Выберите хотя-бы один элемент из основы");
                }
                if (string.IsNullOrWhiteSpace(Resource_Name_Box.Text))
                {
                    throw new Exception("Введите название ресурса");
                }

                var dir = Directory.CreateDirectory("templates");
                string temlateData = "";
                temlateData += "Client = "+Client_Checkbox.IsChecked.ToString();
                temlateData += "\nServer = " + Server_Checkbox.IsChecked.ToString();
                temlateData += "\nShared = " + Shared_Checkbox.IsChecked.ToString();
                temlateData += "\nFullName = " + Full_Name_Checkbox.IsChecked.ToString();
                temlateData += "\nCache = " + Cache_Checkbox.IsChecked.ToString();
                temlateData += "\nOOP = " + OOP_Checkbox.IsChecked.ToString();
                temlateData += "\nAuthor = " + Author_Box.Text;
                temlateData += "\nVersion = " + Version_Box.Text;
                temlateData += "\nName = " + Resource_Name_Box.Text;
                temlateData += "\nMinVersion = " + Minimal_Version_Box.Text;

                File.WriteAllText(dir.FullName + "/" + Resource_Name_Box.Text + ".rt", temlateData);
                MessageBox.Show("Шаблон ресурса сохранён", "Круто :)", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch(Exception exception)
            {
                MessageBox.Show(exception.Message,"Ошибка",MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }

        private void Load_Template_Button_Click(object sender, RoutedEventArgs e)
        {
            if (_templateDialog.ShowDialog() == true)
            {
                var template = File.OpenRead(_templateDialog.FileName);
                var sr = new StreamReader(template);
                string client = parseKeyValue(sr.ReadLine());
                string server = parseKeyValue(sr.ReadLine());
                string shared = parseKeyValue(sr.ReadLine());
                string fullname = parseKeyValue(sr.ReadLine());
                string cache = parseKeyValue(sr.ReadLine());
                string oop = parseKeyValue(sr.ReadLine());
                string author = parseKeyValue(sr.ReadLine());
                string version = parseKeyValue(sr.ReadLine());
                string name = parseKeyValue(sr.ReadLine());
                string minvers = parseKeyValue(sr.ReadLine());
                Client_Checkbox.IsChecked = bool.Parse(client);
                Server_Checkbox.IsChecked = bool.Parse(server);
                Shared_Checkbox.IsChecked = bool.Parse(shared);
                Full_Name_Checkbox.IsChecked = bool.Parse(fullname);
                Cache_Checkbox.IsChecked = bool.Parse(cache);
                OOP_Checkbox.IsChecked = bool.Parse(oop);
                Author_Box.Text = author;
                Version_Box.Text = version;
                Resource_Name_Box.Text = name;
                Minimal_Version_Box.Text = minvers;
            }
        }

        private string parseKeyValue(string text)
        {
            int equalIndex = text.IndexOf("=");
            text = text.Substring(equalIndex + 1);
            text = text.TrimStart();
            text = text.TrimEnd();
            return text;
        }
    }
}
