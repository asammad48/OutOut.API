using ClosedXML.Excel;
using MongoDB.Driver;
using System.ComponentModel;
using System.Data;
using System.Reflection;

namespace OutOut.Core.Utils
{
    public static class ExcelUtils
    {
        private static DataTable ToDataTable<T,Y>(IAsyncCursor<T> items, Func<T, Y> converter)
        {
            DataTable dataTable = new DataTable(typeof(Y).Name);

            PropertyInfo[] Props = typeof(Y).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);

                var description = prop.GetCustomAttribute<DescriptionAttribute>()?.Description;
                dataTable.Columns.Add(description ?? prop.Name, type);
            }

            while (items.MoveNext())
            {
                var batch = items.Current.ToList();
                var batchConverting = batch.Select(item => converter(item));
                foreach (var item in batchConverting)
                {
                    var values = new object[Props.Length];
                    for (int i = 0; i < Props.Length; i++)
                    {
                        values[i] = Props[i].GetValue(item, null);
                    }
                    dataTable.Rows.Add(values);
                }
            }

            return dataTable;
        }

        public static MemoryStream ExportToExcel<T,Y>(IAsyncCursor<T> data, Func<T,Y> converter ,string header = null, string sheetName = "Sheet1")
        {
            DataTable dtt = ToDataTable(data, converter);
            using XLWorkbook wb = new XLWorkbook();

            var sheet = wb.Worksheets.Add(dtt, sheetName);
            if (header != null)
            {
                sheet.Row(1).InsertRowsAbove(1);
                sheet.Range(sheet.Cell(1, 1), sheet.Cell(1, dtt.Columns.Count)).Merge().SetValue(header).Style.Font.SetBold().Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }
            foreach (var table in sheet.Tables)
                table.Theme = XLTableTheme.None;

            var stream = new MemoryStream();
            wb.SaveAs(stream);
            wb.Dispose();

            stream.Position = 0;
            return stream;
        } 

        private static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);

                var description = prop.GetCustomAttribute<DescriptionAttribute>()?.Description;
                //Setting column names as Description or Property names
                dataTable.Columns.Add(description ?? prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        public static MemoryStream ExportToExcel<T>(List<T> data, string header = null, string sheetName = "Sheet1")
        {
            DataTable dtt = ToDataTable(data);
            using XLWorkbook wb = new XLWorkbook();

            var sheet = wb.Worksheets.Add(dtt, sheetName);
            if (header != null)
            {
                sheet.Row(1).InsertRowsAbove(1);
                sheet.Range(sheet.Cell(1, 1), sheet.Cell(1, dtt.Columns.Count)).Merge().SetValue(header).Style.Font.SetBold().Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }
            foreach (var table in sheet.Tables)
                table.Theme = XLTableTheme.None;

            var stream = new MemoryStream();
            wb.SaveAs(stream);
            wb.Dispose();

            stream.Position = 0;
            return stream;
        }
    }
}
