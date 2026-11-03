using System.Numerics;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SpreadsheetEngine
{
    public class Table
    {
        public int RowCount { get; set; }
        public int ColCount { get; set; }

        Dictionary<string, Cell> cells = new Dictionary<string, Cell>();
        HashSet<string> calculated = new HashSet<string>();
        HashSet<string> inProgress = new HashSet<string>();

        public Table(int rows, int cols)
        {
            RowCount = rows;
            ColCount = cols;
        }

        public static string ColumnName(int col)
        {
            string result = "";
            int n = col + 1;
            while (n > 0)
            {
                result = (char)('A' + (n - 1) % 26) + result;
                n = (n - 1) / 26;
            }
            return result;
        }

        public static string CellName(int row, int col)
        {
            return ColumnName(col) + (row + 1);
        }

        public Cell GetCell(string name)
        {
            name = name.ToUpper();
            if (!cells.ContainsKey(name))
            {
                cells[name] = new Cell();
            }
            return cells[name];
        }

        public void SetExpression(string name, string expression)
        {
            GetCell(name).Expression = expression;
            Recalculate();
        }

        public void InsertRow(int index)
        {
            Shift(true, index, 1);
            RowCount++;
            Recalculate();
        }

        public void DeleteRow(int index)
        {
            if (RowCount > 1)
            {
                Shift(true, index, -1);
                RowCount--;
                Recalculate();
            }
        }

        public void InsertColumn(int index)
        {
            Shift(false, index, 1);
            ColCount++;
            Recalculate();
        }

        public void DeleteColumn(int index)
        {
            if (ColCount > 1)
            {
                Shift(false, index, -1);
                ColCount--;
                Recalculate();
            }
        }

        private void Shift(bool isRow, int index, int delta)
        {
            Dictionary<string, Cell> newCells = new Dictionary<string, Cell>();

            foreach (var pair in cells)
            {
                int row, col;
                ParseName(pair.Key, out row, out col);

                if (delta < 0 && (isRow ? row : col) == index)
                {
                    continue;
                }

                if (isRow && row >= index)
                {
                    row += delta;
                }
                if (!isRow && col >= index)
                {
                    col += delta;
                }

                pair.Value.Expression = ShiftReferences(pair.Value.Expression, isRow, index, delta);
                newCells[CellName(row, col)] = pair.Value;
            }

            cells = newCells;
        }

        private string ShiftReferences(string expression, bool isRow, int index, int delta)
        {
            return Regex.Replace(expression, "[A-Za-z]+[0-9]+", match =>
            {
                int row, col;
                if (!ParseName(match.Value, out row, out col))
                {
                    return match.Value;
                }

                int position = isRow ? row : col;
                if (delta < 0 && position == index)
                {
                    return "#REF!";
                }

                if (position >= index)
                {
                    position += delta;
                }

                if (isRow)
                {
                    return CellName(position, col);
                }
                else
                {
                    return CellName(row, position);
                }
            });
        }

        public static bool ParseName(string name, out int row, out int col)
        {
            name = name.ToUpper();
            row = -1;
            col = -1;

            int i = 0;
            int c = 0;
            while (i < name.Length && name[i] >= 'A' && name[i] <= 'Z')
            {
                c = c * 26 + (name[i] - 'A' + 1);
                i++;
            }

            if (i == 0 || i == name.Length)
            {
                return false;
            }

            int r;
            if (!int.TryParse(name.Substring(i), out r) || r < 1)
            {
                return false;
            }

            row = r - 1;
            col = c - 1;
            return true;
        }

        private bool IsInTable(string name)
        {
            int row, col;
            if (!ParseName(name, out row, out col))
            {
                return false;
            }
            return row < RowCount && col < ColCount;
        }

        public void Recalculate()
        {
            calculated.Clear();
            inProgress.Clear();
            foreach (string name in cells.Keys.ToList())
            {
                Calculate(name);
            }
        }

        private void Calculate(string name)
        {
            if (calculated.Contains(name))
            {
                return;
            }
            if (inProgress.Contains(name))
            {
                throw new Exception("Циклічне посилання на клітинку " + name);
            }

            Cell cell = GetCell(name);
            cell.Error = "";
            cell.Value = 0;

            if (cell.Expression.Contains("#REF!"))
            {
                cell.Error = "Посилання на видалену клітинку";
            }
            else if (cell.Expression.Trim() != "")
            {
                inProgress.Add(name);
                try
                {
                    cell.Value = Calculator.Evaluate(cell.Expression, this);
                }
                catch (Exception ex)
                {
                    cell.Error = ex.Message;
                }
                inProgress.Remove(name);
            }

            calculated.Add(name);
        }

        public BigInteger GetCellValue(string name)
        {
            if (!IsInTable(name))
            {
                throw new Exception("Клітинка " + name + " поза межами таблиці");
            }

            Calculate(name);
            Cell cell = GetCell(name);

            if (cell.Error != "")
            {
                throw new Exception("Помилка в клітинці " + name);
            }
            return cell.Value;
        }

        public void Save(string path)
        {
            XElement root = new XElement("spreadsheet",
                new XAttribute("rows", RowCount),
                new XAttribute("columns", ColCount));

            for (int r = 0; r < RowCount; r++)
            {
                for (int c = 0; c < ColCount; c++)
                {
                    string name = CellName(r, c);
                    if (cells.ContainsKey(name) && cells[name].Expression != "")
                    {
                        root.Add(new XElement("cell", new XAttribute("name", name), cells[name].Expression));
                    }
                }
            }

            XDocument doc = new XDocument(root);
            doc.Save(path);
        }

        public void Load(string path)
        {
            XDocument doc = XDocument.Load(path);
            XElement root = doc.Root;

            if (root == null || root.Name != "spreadsheet")
            {
                throw new Exception("Невірний формат файлу");
            }

            int rows = int.Parse(root.Attribute("rows").Value);
            int cols = int.Parse(root.Attribute("columns").Value);
            if (rows < 1 || cols < 1)
            {
                throw new Exception("Невірний розмір таблиці");
            }

            RowCount = rows;
            ColCount = cols;
            cells.Clear();

            foreach (XElement element in root.Elements("cell"))
            {
                string name = element.Attribute("name").Value.ToUpper();
                if (IsInTable(name))
                {
                    GetCell(name).Expression = element.Value;
                }
            }

            Recalculate();
        }
    }
}
