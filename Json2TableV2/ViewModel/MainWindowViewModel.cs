using Json2TableV2.MVVM;
using Json2TableV2.ViewModel.Commands;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Printing;
using System.Text;
using System.Text.Json;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;

namespace Json2TableV2.ViewModel
{
    internal class MainWindowViewModel : ViewModelBase
    {
        private ObservableCollection<dynamic> _itemsObj = new ObservableCollection<dynamic>();
        public ObservableCollection<dynamic> ItemsObj
        {
            get => _itemsObj;
            set
            {
                _itemsObj = value;
                OnPropertyChanged(nameof(ItemsObj));
            }
        }


        private dynamic root;

        private string currentFileName;
        private string content;
        private string currentConvertion;


        #region TextBlocks
        private string _outputText;
        public string OutputText
        {
            get => _outputText;
            set
            {
                _outputText = value;
                OnPropertyChanged(nameof(OutputText));
            }
        }

        private string _convertedText;
        public string ConvertedText
        {
            get => _convertedText;
            set
            {
                _convertedText = value;
                OnPropertyChanged(nameof(ConvertedText));
            }
        }
        #endregion


        #region ICommands

        private ICommand _dbmlCommand;
        public ICommand DBMLCommand
        {
            get
            {
                if (_dbmlCommand == null)
                {
                    _dbmlCommand = new RelayCommand(DBMLConverterClick, CanButtonClick);
                }
                return _dbmlCommand;
            }
        }

        private ICommand _sqlCommand;
        public ICommand SQLCommand
        {
            get
            {
                if (_sqlCommand == null)
                {
                    _sqlCommand = new RelayCommand(SQLConverterClick, CanButtonClick);
                }
                return _sqlCommand;
            }
        }

        private ICommand _beautifiedJsonCommand;
        public ICommand BeautifiedJsonCommand
        {
            get
            {
                if (_beautifiedJsonCommand == null)
                {
                    _beautifiedJsonCommand = new RelayCommand(BeautifiedJsonConverterClick, CanButtonClick);
                }
                return _beautifiedJsonCommand;
            }
        }

        private ICommand _saveAsTxtCommand;
        public ICommand SaveAsTxtCommand
        {
            get
            {
                if (_saveAsTxtCommand == null)
                {
                    _saveAsTxtCommand = new RelayCommand(SaveAsTxtClick, CanButtonClick);
                }
                return _saveAsTxtCommand;
            }
        }

        private ICommand _copyToClipboardCommand;
        public ICommand CopyToClipboardCommand
        {
            get
            {
                if (_copyToClipboardCommand == null)
                {
                    _copyToClipboardCommand = new RelayCommand(CopyToClipboardClick, CanButtonClick);
                }
                return _copyToClipboardCommand;
            }
        }
        #endregion




        public OpenFileCommand OpenJsonCommand { get; private set; }

        public MainWindowViewModel()
        {
            ItemsObj = new ObservableCollection<dynamic>();
            OpenJsonCommand = new OpenFileCommand(OpenJsonFile);
        }



        #region Button Clicks
        private void DBMLConverterClick(object parameter)
        {
            // Action to perform when the button is clicked
            if (root == null)
            {
                MessageBox.Show("Unable to Convert, no table found.", "Convert error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                ConvertedText = ConvertJsonToDbml(root); // Set the DBML content
            }
        }

        private void SQLConverterClick(object parameter)
        {
            // Action to perform when the button is clicked
            if (root == null)
            {
                MessageBox.Show("Unable to Convert, no table found.", "Convert error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                ConvertedText = ConvertJsonToSql(root); // Set the DBML content
            }
        }

        private void BeautifiedJsonConverterClick(object parameter)
        {
            // Action to perform when the button is clicked
            if (root == null)
            {
                MessageBox.Show("Unable to Convert, no table found.", "Convert error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                ConvertedText = BeautifiedJson(root); // Set the DBML content
            }
        }



        private void SaveAsTxtClick(object parameter)
        {
            // Action to perform when the button is clicked
            if (ConvertedText == null || ConvertedText == "Can't find table")
            {
                MessageBox.Show("Unable to save file, no table found.", "Save error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                SaveFileDialog dialog = new SaveFileDialog()
                {
                    FileName = currentFileName + " as " + currentConvertion,
                    Filter = "Text Files(*.txt)|*.txt|All(*.*)|*"
                };

                if (dialog.ShowDialog() == true)
                {
                    File.WriteAllText(dialog.FileName, ConvertedText);
                }
            }
        }

        private void CopyToClipboardClick(object parameter)
        {
            // Action to perform when the button is clicked
            if (ConvertedText == null || ConvertedText == "Can't find table")
            {
                MessageBox.Show("Unable to Copy text, no table found.", "Copy error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                Clipboard.SetText(ConvertedText);
            }
        }


        private bool CanButtonClick(object parameter)
        {
            // Logic to enable or disable the button
            return true;
        }
        #endregion

        private void OpenJsonFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Open JSON File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string path = openFileDialog.FileName;

                currentFileName = openFileDialog.SafeFileName;

                try
                {
                    // Read the file
                    content = File.ReadAllText(path);
                }
                catch (Exception e)
                {
                    OutputText = e.Message;
                }
            }

            ItemsObj.Clear();
            ItemsObj = new ObservableCollection<dynamic>();
            ItemsObj = ParseFileToJson(content);
        }

        public ObservableCollection<dynamic> ParseFileToJson(string content)
        {
            // Clear previous items
            //ItemsObj.Clear();
            ObservableCollection<dynamic> items = new ObservableCollection<dynamic>();
            //ItemsObj = new ObservableCollection<dynamic>();


            try
            {

                // Parse the JSON
                root = JsonConvert.DeserializeObject<JToken>(content);


                // Check if the content is an array or object
                if (root is JArray jsonArray)
                {
                    // Add each item in the array to the ObservableCollection
                    foreach (var item in jsonArray)
                    {
                        if (item is JObject obj)
                        {
                            items.Add(obj);
                        }
                    }
                }
                else if (root is JObject jsonObject)
                {
                    // Handles Seperate Objects
                    foreach (var item in jsonObject)
                    {
                        foreach (var item2 in jsonObject)
                        {
                            items.Add(item2);
                        }
                    }
                }
                else
                {
                    OutputText = "The JSON content is not an object or array.";
                }
            }
            catch (JsonReaderException ex)
            {
                OutputText = "JSON Error: \n" + ex + "\n";
                return new ObservableCollection<dynamic>();
            }
            catch (IOException ex)
            {
                OutputText = "I/O Error: \n" + ex + "\n";
                return new ObservableCollection<dynamic>();
            }

            return items;

        }

        #region Table Converters
        public string ConvertJsonToDbml(dynamic json)
        {
            string dbml = "";
            currentConvertion = "DBML";

            if (json is JArray rootArray)
            {
                foreach (var item in rootArray)
                {
                    dbml += $"Table Root {{\n";
                    if (item is JObject nestedObject)
                    {
                        // Handle primitive Objects
                        if (item is JObject nested)
                        {
                            foreach (var columnProperty in nested.Properties())
                            {
                                string columnName = columnProperty.Name;
                                string columnType = MapJsonToDBMLType(columnProperty.Value);
                                dbml += $"  {columnName} {columnType}\n";
                            }

                            dbml += "}\n\n";
                            return dbml;
                        }
                    }
                    else if (item is JValue nestedValue)
                    {
                        // Handle primitive values
                        if (item is JObject nested)
                        {
                            foreach (var columnProperty in nested.Properties())
                            {
                                string columnName = columnProperty.Name;
                                string columnType = MapJsonToDBMLType(columnProperty.Value);
                                dbml += $"  {columnName} {columnType}\n";
                            }

                            dbml += "}\n\n";
                            return dbml;
                        }
                    }
                }
            }
            else if (json is JObject rootObject)
            {
                foreach (var tableProperty in json.Properties())
                {
                    string tableName = tableProperty.Name;
                    JToken tableData = tableProperty.Value;

                    dbml += $"Table {tableName} {{\n";

                    if (tableData is JArray dataArray && dataArray.Count > 0)
                    {
                        foreach (var item in dataArray)
                        {
                            if (item is JObject nested)
                            {
                                foreach (var columnProperty in nested.Properties())
                                {
                                    string columnName = columnProperty.Name;
                                    string columnType = MapJsonToDBMLType(columnProperty.Value);
                                    dbml += $"  {columnName} {columnType}\n";
                                }
                            }

                            // Break so it doesn't fill "dbml" multiple times
                            break;
                        }
                    }
                    else if (tableData is JObject obj)
                    {
                        foreach (var columnProperty in obj.Properties())
                        {
                            string columnName = columnProperty.Name;
                            string columnType = MapJsonToDBMLType(columnProperty.Value);

                            dbml += $"  {columnName} {columnType}\n";
                        }
                    }
                    else if (tableData is JArray emptyArray && emptyArray.Count == 0)
                    {
                        dbml += "  // Empty table\n";
                    }
                    else
                    {
                        dbml += "  // No valid data found for this table\n";
                    }

                    dbml += "}\n\n";
                }
            }
            else
            {
                dbml += "// Unsupported JSON format\n";
            }

            return dbml;
        }


        public string ConvertJsonToSql(dynamic json)
        {
            string sqlString = "";
            currentConvertion = "MySQL";

            if (json is JArray rootArray)
            {
                foreach (var item in rootArray)
                {
                    sqlString += $"CREATE TABLE Root (\n";

                    if (item is JObject nestedObject)
                    {
                        // Handle primitive Objects
                        if (item is JObject nested)
                        {
                            foreach (var columnProperty in nested.Properties())
                            {
                                string columnName = columnProperty.Name;
                                string columnType = MapJsonTypeToSqlType(columnProperty.Value);
                                sqlString += $"    {columnName} {columnType},\n";
                            }

                            // Goes 1 line back up and removes the last comma
                            sqlString = sqlString.Remove(sqlString.Length - 1);
                            sqlString = sqlString.Remove(sqlString.Length - 1);

                            sqlString += "\n);\n\n";
                            return sqlString;
                        }

                    }
                }
            }
            else if (json is JObject rootObject)
            {
                foreach (var tableProperty in json.Properties())
                {
                    string tableName = tableProperty.Name;
                    JToken tableData = tableProperty.Value;

                    sqlString += $"CREATE TABLE {tableName} (\n";

                    if (tableData is JArray dataArray && dataArray.Count > 0)
                    {
                        foreach (var item in dataArray)
                        {
                            if (item is JObject nested)
                            {
                                foreach (var columnProperty in nested.Properties())
                                {
                                    string columnName = columnProperty.Name;
                                    string columnType = MapJsonTypeToSqlType(columnProperty.Value);
                                    sqlString += $"    {columnName} {columnType},\n";

                                }
                                // Break so it doesn't fill "dbml" multiple times

                                // Goes 1 line back up and removes the last comma
                                sqlString = sqlString.Remove(sqlString.Length - 1);
                                sqlString = sqlString.Remove(sqlString.Length - 1);

                                sqlString += "\n);\n\n";
                                break;

                            }

                        }
                    }
                    else if (tableData is JObject obj)
                    {
                        foreach (var columnProperty in obj.Properties())
                        {
                            string columnName = columnProperty.Name;
                            string columnType = MapJsonTypeToSqlType(columnProperty.Value);

                            sqlString += $"    {columnName} {columnType},\n";
                        }
                        // Goes 1 line back up and removes the last comma
                        sqlString = sqlString.Remove(sqlString.Length - 1);
                        sqlString = sqlString.Remove(sqlString.Length - 1);

                        sqlString += "\n);\n\n";
                        return sqlString;
                    }
                    else if (tableData is JArray emptyArray && emptyArray.Count == 0)
                    {
                        sqlString += "  // Empty table\n";
                    }
                    else
                    {
                        sqlString += "  // No valid data found for this table\n";
                    }
                }

            }
            return sqlString;
        }


        public string BeautifiedJson(dynamic json)
        {
            currentConvertion = "BeautifiedJson";

            var options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };

            return JsonConvert.SerializeObject(json, Formatting.Indented);
        }

        #endregion

        // Change Types

        static string MapJsonTypeToSqlType(JToken value)
        {
            return value.Type switch
            {
                JTokenType.Integer => "INT",
                JTokenType.Float => "FLOAT",
                JTokenType.String => "VARCHAR(255)",
                JTokenType.Boolean => "BIT",
                JTokenType.Date => "DATETIME",
                JTokenType.Null => "TEXT",         // Default type for null values
                JTokenType.Object => "JSON",      // For nested structures
                JTokenType.Array => "JSON",       // For arrays (commonly stored as JSON in SQL)
                _ => "TEXT" // Default type for unrecognized or complex types
            };
        }

        static string MapJsonToDBMLType(JToken value)
        {
            return value.Type switch
            {
                JTokenType.Integer => "int",
                JTokenType.Float => "float",
                JTokenType.String => "string",
                JTokenType.Boolean => "bool",
                JTokenType.Date => "datetime",
                JTokenType.Null => "nullable",
                JTokenType.Object => "object", // For nested structures
                JTokenType.Array => "array",   // For arrays
                _ => "string" // Default type for unrecognized or complex types
            };
        }
    }
}
