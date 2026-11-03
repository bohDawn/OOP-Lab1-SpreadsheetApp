using SpreadsheetEngine;
using Cell = SpreadsheetEngine.Cell;

namespace SpreadsheetApp
{
    public partial class MainPage : ContentPage
    {
        Table table = new Table(10, 8);
        Dictionary<string, Entry> entries = new Dictionary<string, Entry>();
        bool showExpressions = false;
        string selectedCell = null;
        string filePath = null;

        public MainPage()
        {
            InitializeComponent();
            CreateGrid();
        }

        private void CreateGrid()
        {
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();
            entries.Clear();

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 50 });
            for (int c = 0; c < table.ColCount; c++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 110 });
            }

            for (int r = 0; r <= table.RowCount; r++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = 35 });
            }

            for (int c = 0; c < table.ColCount; c++)
            {
                grid.Add(CreateHeader(Table.ColumnName(c)), c + 1, 0);
            }

            for (int r = 0; r < table.RowCount; r++)
            {
                grid.Add(CreateHeader((r + 1).ToString()), 0, r + 1);

                for (int c = 0; c < table.ColCount; c++)
                {
                    string name = Table.CellName(r, c);
                    Entry entry = new Entry();
                    entry.Focused += (s, e) => Cell_Focused(name);
                    entry.Unfocused += (s, e) => Cell_Unfocused(name);
                    entry.TextChanged += (s, e) => Cell_TextChanged(name);
                    entry.Completed += (s, e) => entry.Unfocus();

                    entries[name] = entry;
                    grid.Add(entry, c + 1, r + 1);
                }
            }

            if (selectedCell != null && !entries.ContainsKey(selectedCell))
            {
                SelectCell(null);
            }
            else
            {
                SelectCell(selectedCell);
            }

            UpdateCells();
        }

        private Label CreateHeader(string text)
        {
            Label label = new Label();
            label.Text = text;
            label.FontAttributes = FontAttributes.Bold;
            label.HorizontalTextAlignment = TextAlignment.Center;
            label.VerticalTextAlignment = TextAlignment.Center;
            return label;
        }

        private void UpdateCells()
        {
            foreach (var pair in entries)
            {
                Cell cell = table.GetCell(pair.Key);
                Entry entry = pair.Value;

                if (!entry.IsFocused)
                {
                    if (showExpressions)
                    {
                        entry.Text = cell.Expression;
                    }
                    else if (cell.Expression.Trim() == "")
                    {
                        entry.Text = "";
                    }
                    else if (cell.Error != "")
                    {
                        entry.Text = "#ПОМИЛКА";
                    }
                    else
                    {
                        entry.Text = cell.Value.ToString();
                    }
                }

                if (cell.Error != "")
                {
                    entry.BackgroundColor = Colors.Red;
                }
                else
                {
                    entry.BackgroundColor = Colors.Transparent;
                }
            }
        }

        private void SelectCell(string name)
        {
            selectedCell = name;
            if (name == null)
            {
                cellNameLabel.Text = "";
                formulaEntry.Text = "";
            }
            else
            {
                cellNameLabel.Text = name;
                formulaEntry.Text = table.GetCell(name).Expression;
            }
        }

        private void ShowCellStatus(string name)
        {
            Cell cell = table.GetCell(name);
            if (cell.Error != "")
            {
                statusLabel.Text = "Помилка в " + name + ": " + cell.Error;
            }
            else
            {
                statusLabel.Text = "Клітинка " + name;
            }
        }

        private void Cell_Focused(string name)
        {
            entries[name].Text = table.GetCell(name).Expression;
            SelectCell(name);
            ShowCellStatus(name);
        }

        private void Cell_TextChanged(string name)
        {
            if (entries[name].IsFocused)
            {
                formulaEntry.Text = entries[name].Text;
            }
        }

        private void Cell_Unfocused(string name)
        {
            string text = entries[name].Text ?? "";
            if (text != table.GetCell(name).Expression)
            {
                table.SetExpression(name, text);
            }
            UpdateCells();
            ShowCellStatus(name);
        }

        private void FormulaEntry_Completed(object sender, EventArgs e)
        {
            if (selectedCell == null)
            {
                return;
            }

            table.SetExpression(selectedCell, formulaEntry.Text ?? "");
            UpdateCells();
            ShowCellStatus(selectedCell);
        }

        private void CalculateButton_Clicked(object sender, EventArgs e)
        {
            table.Recalculate();
            UpdateCells();
            statusLabel.Text = "Таблицю перераховано";
        }

        private void ModeButton_Clicked(object sender, EventArgs e)
        {
            showExpressions = !showExpressions;
            if (showExpressions)
            {
                modeButton.Text = "Режим: ВИРАЗ";
            }
            else
            {
                modeButton.Text = "Режим: ЗНАЧЕННЯ";
            }
            UpdateCells();
        }

        private async Task<bool> CheckSelected()
        {
            if (selectedCell == null)
            {
                await DisplayAlert("Помилка", "Спочатку виберіть клітинку", "OK");
                return false;
            }
            return true;
        }

        private async void InsertRowButton_Clicked(object sender, EventArgs e)
        {
            if (!await CheckSelected())
            {
                return;
            }

            int row, col;
            Table.ParseName(selectedCell, out row, out col);
            table.InsertRow(row);
            CreateGrid();
            statusLabel.Text = "Вставлено рядок " + (row + 1);
        }

        private async void DeleteRowButton_Clicked(object sender, EventArgs e)
        {
            if (!await CheckSelected())
            {
                return;
            }

            int row, col;
            Table.ParseName(selectedCell, out row, out col);
            table.DeleteRow(row);
            CreateGrid();
            statusLabel.Text = "Видалено рядок " + (row + 1);
        }

        private async void InsertColumnButton_Clicked(object sender, EventArgs e)
        {
            if (!await CheckSelected())
            {
                return;
            }

            int row, col;
            Table.ParseName(selectedCell, out row, out col);
            table.InsertColumn(col);
            CreateGrid();
            statusLabel.Text = "Вставлено стовпець " + Table.ColumnName(col);
        }

        private async void DeleteColumnButton_Clicked(object sender, EventArgs e)
        {
            if (!await CheckSelected())
            {
                return;
            }

            int row, col;
            Table.ParseName(selectedCell, out row, out col);
            table.DeleteColumn(col);
            CreateGrid();
            statusLabel.Text = "Видалено стовпець " + Table.ColumnName(col);
        }

        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            if (filePath == null)
            {
                await SaveAs();
                return;
            }

            try
            {
                table.Save(filePath);
                statusLabel.Text = "Збережено у файл " + filePath;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", "Не вдалося зберегти файл: " + ex.Message, "OK");
            }
        }

        private async void SaveAsButton_Clicked(object sender, EventArgs e)
        {
            await SaveAs();
        }

        private async Task SaveAs()
        {
            try
            {
                var picker = new Windows.Storage.Pickers.FileSavePicker();
                picker.FileTypeChoices.Add("XML файл", new List<string> { ".xml" });
                picker.SuggestedFileName = "таблиця";

                var window = (MauiWinUIWindow)Application.Current.Windows[0].Handler.PlatformView;
                WinRT.Interop.InitializeWithWindow.Initialize(picker, window.WindowHandle);

                var file = await picker.PickSaveFileAsync();
                if (file == null)
                {
                    return;
                }

                table.Save(file.Path);
                filePath = file.Path;
                Title = "Електронна таблиця - " + file.Name;
                statusLabel.Text = "Збережено у файл " + file.Path;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", "Не вдалося зберегти файл: " + ex.Message, "OK");
            }
        }

        private async void OpenButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.Default.PickAsync();
                if (result == null)
                {
                    return;
                }

                table.Load(result.FullPath);
                filePath = result.FullPath;
                Title = "Електронна таблиця - " + result.FileName;
                selectedCell = null;
                CreateGrid();
                statusLabel.Text = "Таблицю прочитано з файлу " + result.FileName;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", "Не вдалося прочитати файл: " + ex.Message, "OK");
            }
        }

        private async void ExitButton_Clicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Вихід", "Ви дійсно хочете вийти?", "Так", "Ні");
            if (answer)
            {
                Application.Current.Quit();
            }
        }

        private async void HelpButton_Clicked(object sender, EventArgs e)
        {
            await DisplayAlert("Довідка",
                "Лабораторна робота №1, варіант 9\n" +
                "Студент: Грабовський Богдан\n\n" +
                "Операції: +, -, *, / (цілочисельне), унарні + та -, ^, max(x, y), min(x, y)\n" +
                "Можна використовувати дужки та посилання на клітинки, наприклад A1\n\n" +
                "Приклад: max(A1, 5) + 2\n\n" +
                "Вираз можна вводити в клітинку або в рядок формул.\n" +
                "Кнопка Режим перемикає показ виразів або значень.\n" +
                "Вставити/видалити рядок чи стовпець - на місці вибраної клітинки.\n" +
                "Зберегти - у поточний файл (перший раз запитує, куди зберегти),\n" +
                "Зберегти як - завжди запитує папку та ім'я файлу.",
                "OK");
        }
    }
}
