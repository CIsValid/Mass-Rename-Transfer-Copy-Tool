using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using DataGrid = System.Windows.Controls.DataGrid;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Path = System.IO.Path;

namespace RWTC
{

    public partial class MainWindow : Window
    {
        private OpenFileDialog fileDialog = new OpenFileDialog();
        private DataGrid dataGridCache = new DataGrid();
        private MessageBoxResult result;
        private string newFileName;
        private string oldFileName;
        private List<ItemStruct> items = new List<ItemStruct>();
        private int prefixNum = -1;
        private int suffixNum = -1;
        private int fileCount = -1;
        private string customFileName;
        private bool bDoneOnce = false;
        private bool bSetCustomFileName = false;
        private bool bSetCustomPrefixSuffix = false;
        private string customReplacement;
        private bool bSetCustomReplacement = false;

        public struct ItemStruct
        {
            public string Filename { set; get; }
            public string Filepath { set; get; }
            public string Type { set; get; }
            public DateTime Date { set; get; }
        }

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            dataGridCache = dataGrid;

        }


        private void button_Click(object sender, RoutedEventArgs e) // Select Files
        {
            dataGrid = dataGridCache;
            fileDialog.Multiselect = true;
            fileDialog.FileName = "";


            fileDialog.ShowDialog();

            if (fileDialog.FileName != "")
            {
                result = MessageBox.Show(
                    "Enter Selected Files? They May Be Modified!",
                    "Warning",
                    MessageBoxButton.YesNo);
            }



            if (result == MessageBoxResult.Yes)
            {
                for (int i = 0; i < fileDialog.FileNames.Length; i++)
                {
                    var fileName = Path.GetFileNameWithoutExtension(fileDialog.FileNames[i]);
                    var filePath = Path.GetFullPath(fileDialog.FileNames[i]);
                    var dateModified = File.GetLastWriteTime(fileDialog.FileNames[i]);
                    var fileType = Path.GetExtension(Path.GetFileName(fileDialog.FileNames[i]));

                    dataGrid.Items.Add(new ItemStruct { Filename = fileName, Filepath = filePath, Date = dateModified, Type = fileType });
                    items.Add(new ItemStruct { Filename = fileName, Filepath = filePath, Date = dateModified, Type = fileType });

                }

                dataGrid.Items.Refresh();

            }

        }

        private void button0_Click(object sender, RoutedEventArgs e) // Rename
        {
            // Buggy when renaming twice in a row. Have not fully tried re-selecting yet though in theory nothing should really change.
            if (dataGrid.Items.Count < 1) { MessageBox.Show("Nothing to rename..."); return;}

            if (!bSetCustomFileName && !bSetCustomPrefixSuffix && !bSetCustomReplacement) { MessageBox.Show("No Name/Prefix/Suffix Added..."); return; }

            result = MessageBox.Show(
                "Are You Sure? \n\n" + "The Files You've Selected Will Be Renamed.",
                "Warning",
                MessageBoxButton.YesNo);

            if (result == MessageBoxResult.No) return;

            var itemsUpdate = new List<ItemStruct>();

            foreach (var data in items)
            {
                string prefix = comboBox0.Text;
                string suffix = comboBox1.Text;
                var destFolderFull = data.Filepath; // Full Path
                var destFolderPath = destFolderFull.Replace(data.Filename + data.Type, ""); // Removed file name and type, only path now.

                // Set Old Name:
                oldFileName = destFolderFull;

                // Update fileCount to not have overlapping names
                fileCount++;

                if (bSetCustomPrefixSuffix)
                {
                    // Create New File Name:
                    if (bSetCustomFileName)
                    {
                        newFileName = destFolderPath + fileCount + "_" + prefix + customFileName + suffix + data.Type;
                    }
                    else { newFileName = destFolderPath + prefix + data.Filename + suffix + data.Type; }
                }

                else if (!bSetCustomPrefixSuffix && bSetCustomFileName) newFileName = destFolderPath + fileCount + "_" + customFileName + data.Type;

                else if (!bSetCustomPrefixSuffix && !bSetCustomFileName && bSetCustomReplacement) newFileName = destFolderPath + data.Filename + data.Type;
                

                // Replace custom string if valid.
                if(bSetCustomReplacement) newFileName = newFileName.Replace(customReplacement, "");

                if(File.Exists(newFileName)) File.Delete(newFileName); // Ensure the file doesn't already exist.
                File.Move(oldFileName, newFileName); // Make new file.

                // Reference new name:
                var destFileName = Path.GetFileName(newFileName);
                var dateModified = File.GetLastWriteTime(newFileName);
                var fileType = data.Type;

                // Refresh Data Grid
                dataGrid.Items.Remove(data);
                dataGrid.Items.Add(new ItemStruct { Filename = destFileName, Filepath = newFileName, Date = dateModified, Type = fileType });
                dataGrid.Items.Refresh();

                // Add to temp holder of updated items
                itemsUpdate.Add(new ItemStruct { Filename = destFileName, Filepath = newFileName, Date = dateModified, Type = fileType });

            }

            items = itemsUpdate; // Replace old item info with updated info.


        }

        /*private void button1_Click(object sender, RoutedEventArgs e) // Rename And Copy NEW!!
        {
            if (dataGrid.Items.Count < 1) { MessageBox.Show("Nothing to rename..."); return; }

            if (!bSetCustomFileName && !bSetCustomPrefixSuffix && !bSetCustomReplacement) { MessageBox.Show("No Name/Prefix/Suffix Added..."); return; }

            var fd = new OpenFileDialog();
            fd.ShowDialog();
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialog.ShowDialog();
            var destFolder = folderDialog.SelectedPath + "\\";

            if (folderDialog.SelectedPath != "")
            {
                result = MessageBox.Show(
                    "Are You Sure?" + " You've Selected:\n\n" + destFolder + "\n\nAs Destination To Send The Copy",
                    "Warning",
                    MessageBoxButton.YesNo);
            }
            else { return; }

            var itemsUpdate = new List<ItemStruct>();

            foreach (var data in items)
            {
                string prefix = comboBox0.Text;
                string suffix = comboBox1.Text;
                var destFolderFull = fd.folder; // Full Path
                var destFolderPath = destFolderFull.Replace(data.Filename + data.Type, ""); // Removed file name and type, only path now.

                // Set Old Name:
                oldFileName = data.Filepath;

                // Update fileCount to not have overlapping names
                fileCount++;

                if (bSetCustomPrefixSuffix)
                {
                    // Create New File Name:
                    if (bSetCustomFileName)
                    {
                        newFileName = destFolder + fileCount + "_" + prefix + customFileName + suffix + data.Type;
                    }
                    else { newFileName = destFolder + prefix + data.Filename + suffix + data.Type; }
                }

                else if (!bSetCustomPrefixSuffix && bSetCustomFileName) newFileName = destFolder + fileCount + "_" + customFileName + data.Type;

                else if (!bSetCustomPrefixSuffix && !bSetCustomFileName && bSetCustomReplacement) newFileName = destFolder + data.Filename + data.Type;


                // Replace custom string if valid.
                if (bSetCustomReplacement) newFileName = newFileName.Replace(customReplacement, "");

                if (File.Exists(newFileName)) File.Delete(newFileName); // Ensure the file doesn't already exist.
                File.Copy(oldFileName, newFileName); // Make new file.

                // Reference new name:
                var destFileName = Path.GetFileName(newFileName);
                var dateModified = File.GetLastWriteTime(newFileName);
                var fileType = data.Type;

                // Refresh Data Grid
                dataGrid.Items.Remove(data);
                dataGrid.Items.Add(new ItemStruct { Filename = destFileName, Filepath = newFileName, Date = dateModified, Type = fileType });
                dataGrid.Items.Refresh();

                // Add to temp holder of updated items
                itemsUpdate.Add(new ItemStruct { Filename = destFileName, Filepath = newFileName, Date = dateModified, Type = fileType });

            }

            items = itemsUpdate; // Replace old item info with updated info.
        }*/

        private void button1_Click(object sender, RoutedEventArgs e) // Rename And Copy OLD
        {
            if (dataGrid.Items.Count < 1) { MessageBox.Show("Nothing to rename..."); return; }

            if (!bSetCustomFileName && !bSetCustomPrefixSuffix && !bSetCustomReplacement) { MessageBox.Show("No Name/Prefix/Suffix Added..."); return; }

            var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialog.ShowDialog();
            var destFolder = folderDialog.SelectedPath + "\\";

            if (folderDialog.SelectedPath != "")
            {
                result = MessageBox.Show(
                    "Are You Sure?" + " You've Selected:\n\n" + destFolder + "\n\nAs Destination To Send The Copy",
                    "Warning",
                    MessageBoxButton.YesNo);
            }
            else { return;}

            var itemsUpdate = new List<ItemStruct>();

            foreach (var data in items)
            {
                string prefix = comboBox0.Text;
                string suffix = comboBox1.Text;

                // Set Old Name:
                oldFileName = data.Filepath;

                // Update fileCount to not have overlapping names
                fileCount++;

                if (bSetCustomPrefixSuffix)
                {
                    // Create New File Name:
                    if (bSetCustomFileName)
                    {
                        newFileName = destFolder + fileCount + "_" + prefix + customFileName + suffix + data.Type;
                    }
                    else { newFileName = destFolder + prefix + data.Filename + suffix + data.Type; }
                }

                else if (!bSetCustomPrefixSuffix && bSetCustomFileName) newFileName = destFolder + fileCount + "_" + customFileName + data.Type;

                else if (!bSetCustomPrefixSuffix && !bSetCustomFileName && bSetCustomReplacement) newFileName = destFolder + data.Filename + data.Type;


                // Replace custom string if valid.
                if (bSetCustomReplacement) newFileName = newFileName.Replace(customReplacement, "");

                if (File.Exists(newFileName)) File.Delete(newFileName); // Ensure the file doesn't already exist.
                File.Copy(oldFileName, newFileName); // Make new file.

                // Reference new name:
                var destFileName = Path.GetFileName(newFileName);
                var dateModified = File.GetLastWriteTime(newFileName);
                var fileType = data.Type;

                // Refresh Data Grid
                dataGrid.Items.Remove(data);
                dataGrid.Items.Add(new ItemStruct { Filename = destFileName, Filepath = newFileName, Date = dateModified, Type = fileType });
                dataGrid.Items.Refresh();

                // Add to temp holder of updated items
                itemsUpdate.Add(new ItemStruct { Filename = destFileName, Filepath = newFileName, Date = dateModified, Type = fileType });

            }

            items = itemsUpdate; // Replace old item info with updated info.
        }

        private void button2_Click(object sender, RoutedEventArgs e) // Rename And Transfer
        {
            if (dataGrid.Items.Count < 1) { MessageBox.Show("Nothing to rename..."); return; }

            if (!bSetCustomFileName && !bSetCustomPrefixSuffix && !bSetCustomReplacement) { MessageBox.Show("No Name/Prefix/Suffix Added..."); return; }

            var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialog.ShowDialog();
            var destFolder = folderDialog.SelectedPath + "\\";

            if (folderDialog.SelectedPath != "")
            {
                result = MessageBox.Show(
                    "Are You Sure?" + " You've Selected:\n\n" + destFolder + "\n\nAs Destination as Transfer Location",
                    "Warning",
                    MessageBoxButton.YesNo);
            }
            else { return; }

            var itemsUpdate = new List<ItemStruct>();

            foreach (var data in items)
            {
                string prefix = comboBox0.Text;
                string suffix = comboBox1.Text;

                // Set Old Name:
                oldFileName = data.Filepath;

                // Update fileCount to not have overlapping names
                fileCount++;

                if (bSetCustomPrefixSuffix)
                {
                    // Create New File Name:
                    if (bSetCustomFileName)
                    {
                        newFileName = destFolder + fileCount + "_" + prefix + customFileName + suffix + data.Type;
                    }
                    else { newFileName = destFolder + prefix + data.Filename + suffix + data.Type; }
                }

                else if (!bSetCustomPrefixSuffix && bSetCustomFileName) newFileName = destFolder + fileCount + "_" + customFileName + data.Type;

                else if (!bSetCustomPrefixSuffix && !bSetCustomFileName && bSetCustomReplacement) newFileName = destFolder + data.Filename + data.Type;


                // Replace custom string if valid.
                if (bSetCustomReplacement) newFileName = newFileName.Replace(customReplacement, "");

                if (File.Exists(newFileName)) File.Delete(newFileName); // Ensure the file doesn't already exist.
                File.Move(oldFileName, newFileName); // Make new file.

                // Reference new name:
                var destFileName = Path.GetFileName(newFileName);
                var dateModified = File.GetLastWriteTime(newFileName);
                var fileType = data.Type;

                // Refresh Data Grid
                dataGrid.Items.Remove(data);
                dataGrid.Items.Add(new ItemStruct { Filename = destFileName, Filepath = newFileName, Date = dateModified, Type = fileType });
                dataGrid.Items.Refresh();

                // Add to temp holder of updated items
                itemsUpdate.Add(new ItemStruct { Filename = destFileName, Filepath = newFileName, Date = dateModified, Type = fileType });

            }

            items = itemsUpdate; // Replace old item info with updated info.
        }

        // Text Boxes

        private void CustomPrefixBox_TextChanged(object sender, TextChangedEventArgs e) // Custom Prefix
        {
            CustomPrefixBox.IsKeyboardFocusedChanged += CustomPrefixBox_IsKeyboardFocusedChanged;
        }

        private void CustomPrefixBox_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            e.NewValue.Equals(false);
            if (CustomPrefixBox.Text != "Custom Prefix")
            {
                var MakePreset = MessageBox.Show(
                    "Make Prefix Preset?",
                    "Warning",
                    MessageBoxButton.YesNo);
                if (MakePreset == MessageBoxResult.Yes)
                {
                    comboBox0.Items.Add(newItem: CustomPrefixBox.Text);
                    CustomPrefixBox.Text = "Custom Prefix";
                    prefixNum++;
                    comboBox0.SelectedIndex = prefixNum;
                }
                if (MakePreset == MessageBoxResult.No)
                {
                    CustomPrefixBox.Text = "Custom Prefix";
                }
            }

            CustomPrefixBox.IsKeyboardFocusedChanged -= CustomPrefixBox_IsKeyboardFocusedChanged;

        }

        private void CustomSuffixBox_TextChanged(object sender, TextChangedEventArgs e) // Custom Suffix
        {
            CustomSuffixBox.IsKeyboardFocusedChanged += CustomSuffixBox_IsKeyboardFocusedChanged;

        }

        private void CustomSuffixBox_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            e.NewValue.Equals(false);
            if (CustomSuffixBox.Text != "Custom Suffix")
            {
                var MakePreset = MessageBox.Show(
                    "Make Suffix Preset?",
                    "Warning",
                    MessageBoxButton.YesNo);
                if (MakePreset == MessageBoxResult.Yes)
                {
                    comboBox1.Items.Add(newItem: CustomSuffixBox.Text);
                    CustomSuffixBox.Text = "Custom Suffix";
                    suffixNum++;
                    comboBox1.SelectedIndex = suffixNum;

                }

                if (MakePreset == MessageBoxResult.No)
                {
                    CustomSuffixBox.Text = "Custom Suffix";
                }
            }

            CustomSuffixBox.IsKeyboardFocusedChanged -= CustomSuffixBox_IsKeyboardFocusedChanged;

        }

        private void CustomName_TextChanged(object sender, TextChangedEventArgs e)
        {
            bDoneOnce = false;
            CustomName.IsKeyboardFocusedChanged += CustomName_IsKeyboardFocusedChanged;
        }

        private void CustomName_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            
            if (e.NewValue.Equals(false))
            {
                if (!bDoneOnce)
                {
                    var MakePreset = MessageBox.Show(
                        "Set Custom Name",
                        "Warning",
                        MessageBoxButton.YesNo);
                    if (MakePreset == MessageBoxResult.Yes)
                    {
                        customFileName = CustomName.Text;
                        bSetCustomFileName = true;
                    }

                    bDoneOnce = true;
                    CustomName.IsKeyboardFocusedChanged -= CustomName_IsKeyboardFocusedChanged;

                }

            }

        }

        private void ComboBox1_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox1 != null)
            {
                bSetCustomPrefixSuffix = true;
            }
        }

        private void ComboBox0_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox0 != null)
            {
                bSetCustomPrefixSuffix = true;
            }
        }

        private void CustomNameReplace_TextChanged(object sender, TextChangedEventArgs e)
        {
            bDoneOnce = false;
            CustomNameReplace.IsKeyboardFocusedChanged += CustomNameReplace_IsKeyboardFocusedChanged;
        }

        private void CustomNameReplace_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue.Equals(false))
            {
                if (!bDoneOnce)
                {
                    var MakePreset = MessageBox.Show(
                        "Replace/Remove From Name",
                        "Warning",
                        MessageBoxButton.YesNo);
                    if (MakePreset == MessageBoxResult.Yes)
                    {
                        customReplacement = CustomNameReplace.Text;
                        bSetCustomReplacement = true;
                    }

                    bDoneOnce = true;
                    CustomNameReplace.IsKeyboardFocusedChanged -= CustomNameReplace_IsKeyboardFocusedChanged;

                }

            }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            comboBox0.SelectedIndex = -1;
            if(comboBox0 == null && comboBox1 == null) bSetCustomPrefixSuffix = false;
        }

        private void button3_Copy_Click(object sender, RoutedEventArgs e)
        {
            comboBox1.SelectedIndex = -1;
            if (comboBox0 == null && comboBox1 == null) bSetCustomPrefixSuffix = false;
        }

        private void button3_Copy1_Click(object sender, RoutedEventArgs e)
        {
            CustomName.Text = "";
            customFileName = "";
            bSetCustomFileName = false;
        }

        private void button3_Copy2_Click(object sender, RoutedEventArgs e)
        {
            CustomNameReplace.Text = "";
            customReplacement = "";
            bSetCustomReplacement = false;
        }

        private void button3_Copy3_Click(object sender, RoutedEventArgs e)
        {
            CustomPrefixBox.Text = "";
        }

        private void button3_Copy4_Click(object sender, RoutedEventArgs e)
        {
            CustomSuffixBox.Text = "";
        }

        private void buttonCopy_Click(object sender, RoutedEventArgs e)
        {
            // Refresh Data Grid
            dataGrid.Items.Clear();
            dataGrid.Items.Refresh();
        }

        private void EMouseUP(object sender, MouseButtonEventArgs e)
        {
            // Remove clicked item
            dataGrid.Items.Remove(dataGrid.SelectedItem);
            dataGrid.Items.Refresh();
        }
    }
}
