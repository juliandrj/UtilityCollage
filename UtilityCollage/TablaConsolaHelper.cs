using ConsoleTables;
using NLog;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace UtilityCollage
{
    public class TablaConsolaHelper
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public static void ImprimirDataSetConErrores(DataSet dataSet)
        {
            foreach (DataTable dt in dataSet.Tables)
            {
                _log.Info($"[{dt.Namespace}.{dt.TableName}]");
                ImprimirDataTable(dt, true);
            }
        }

        public static void ImprimirDataSet(DataSet dataSet)
        {
            foreach (DataTable dt in dataSet.Tables)
            {
                _log.Info($"[{dt.Namespace}.{dt.TableName}]");
                ImprimirDataTable(dt, false);
            }
        }

        public static void ImprimirDataTable(DataTable dataTable)
        {
            ImprimirDataTable(dataTable, false);
        }

        public static void ImprimirDataTable(DataTable dataTable, bool incluirRowError)
        {
            List<string> columnas = new List<string>();
            if (incluirRowError)
            {
                columnas.Add("ERROR");
            }
            foreach (DataColumn col in dataTable.Columns)
            {
                columnas.Add(col.ColumnName);
            }
            ConsoleTable ct = new ConsoleTable(columnas.ToArray<string>());
            int i = 1;
            foreach (DataRow dr in dataTable.Rows)
            {
                if (i >= 10)
                {
                    break;
                }
                List<object> valores = new List<object>();
                if (incluirRowError)
                {
                    valores.Add(dr.RowError);
                }
                valores.AddRange(dr.ItemArray);
                ct.AddRow(valores.ToArray());
                i++;
            }
            _log.Info($"\n{ct.ToMinimalString()}");
            _log.Info($"Se mostraron {i - 1} filas de {dataTable.Rows.Count}");
            _log.Info("########################################################");
        }
    }
}
