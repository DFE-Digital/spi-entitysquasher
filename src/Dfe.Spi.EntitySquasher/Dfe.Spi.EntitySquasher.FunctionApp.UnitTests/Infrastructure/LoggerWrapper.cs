namespace Dfe.Spi.EntitySquasher.FunctionApp.Infrastructure
{
    using System;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Dfe.Spi.Common.Logging.Definitions;
    using Microsoft.AspNetCore.Http;

    public class LoggerWrapper : ILoggerWrapper
    {
        private const string ColumnNameTime = "Time";
        private const string ColumnNameCategory = "Category";
        private const string ColumnNameCallingMethod = "CallingMethod";
        private const string ColumnNameMessage = "Message";
        private const string ColumnNameException = "Exception";

        private readonly DataTable dataTable;

        public LoggerWrapper()
        {
            this.dataTable = new DataTable();

            DataColumn[] dataColumns = new DataColumn[]
            {
                new DataColumn(ColumnNameTime, typeof(DateTime)),
                new DataColumn(ColumnNameCategory, typeof(string)),
                new DataColumn(ColumnNameCallingMethod, typeof(string)),
                new DataColumn(ColumnNameMessage, typeof(string)),
                new DataColumn(ColumnNameException, typeof(Exception)),
            };

            foreach (DataColumn dataColumn in dataColumns)
            {
                this.dataTable.Columns.Add(dataColumn);
            }
        }

        public void SetContext(IHeaderDictionary headerDictionary)
        {
            // Do nothing. Not relevant to this test library.
        }

        public void Debug(string message)
        {
            this.AppendToDataTable(nameof(this.Debug), message);
        }

        public void Error(string message, Exception exception)
        {
            this.AppendToDataTable(nameof(this.Error), message, exception);
        }

        public void Info(string message)
        {
            this.AppendToDataTable(nameof(this.Info), message);
        }

        public void Warning(string message)
        {
            this.AppendToDataTable(nameof(this.Warning), message);
        }

        public void Warning(string message, Exception exception)
        {
            this.AppendToDataTable(nameof(this.Warning), message, exception);
        }

        public string ReturnLog()
        {
            string toReturn = null;

            StringBuilder stringBuilder = new StringBuilder();

            int categoryPadding =
                this.GetMaximumStringLength(ColumnNameCategory);
            int callingMethodPadding =
                this.GetMaximumStringLength(ColumnNameCallingMethod);

            string time = null;
            string category = null;
            string callingMethod = null;
            string message = null;
            string exception = null;
            foreach (DataRow dataRow in this.dataTable.Rows)
            {
                time = this.RenderInformationColumn<DateTime?>(
                    dataRow,
                    ColumnNameTime,
                    toStringProvider: x => x.Value.ToString("hh:MM:ss"));

                time = $"[{time}]";

                category = this.RenderInformationColumn<string>(
                    dataRow,
                    ColumnNameCategory);

                category = $"[{category}]".PadRight(categoryPadding + 2);

                callingMethod = this.RenderInformationColumn<string>(
                    dataRow,
                    ColumnNameCallingMethod);

                callingMethod = $"[{callingMethod}]"
                    .PadRight(callingMethodPadding + 2);

                message = this.RenderInformationColumn<string>(
                    dataRow,
                    ColumnNameMessage);

                exception = this.RenderInformationColumn<Exception>(
                    dataRow,
                    ColumnNameException,
                    toStringProvider: x =>
                    {
                        string exceptionStr = null;

                        exceptionStr =
                            $"{x.Message}{Environment.NewLine}" +
                            $"{x.StackTrace}{Environment.NewLine}";

                        return exceptionStr;
                    });

                stringBuilder.Append(
                    $"{time} " +
                    $"{category} " +
                    $"{callingMethod} " +
                    $"{message}");

                if (exception != null)
                {
                    stringBuilder.Append($"{exception}");
                }

                stringBuilder.Append($"{Environment.NewLine}");
            }

            toReturn = stringBuilder.ToString();

            return toReturn;
        }

        private int GetMaximumStringLength(string columnName)
        {
            int toReturn = this.dataTable
                .Rows
                .Cast<DataRow>()
                .Select(x => ((string)x[columnName]).Length)
                .Max();

            return toReturn;
        }

        private string RenderInformationColumn<TColumn>(
            DataRow dataRow,
            string columnName,
            Func<TColumn, string> toStringProvider = null)
        {
            string toReturn = null;

            if (dataRow[columnName] != DBNull.Value)
            {
                TColumn column = (TColumn)dataRow[columnName];

                toReturn =
                    toStringProvider == null
                        ?
                    column.ToString()
                        :
                    toStringProvider(column);
            }

            return toReturn;
        }

        private void AppendToDataTable(
            string category,
            string message,
            Exception exception = null)
        {
            StackTrace stackTrace = new StackTrace();

            StackFrame stackFrame = stackTrace.GetFrame(2);
            MethodBase methodBase = stackFrame.GetMethod();

            string callingMethod =
                $"{methodBase.DeclaringType.Name}.{methodBase.Name}";

            DataRow dataRow = this.dataTable.NewRow();

            dataRow[ColumnNameTime] = DateTime.Now;
            dataRow[ColumnNameCategory] = category.ToUpper();
            dataRow[ColumnNameCallingMethod] = callingMethod;
            dataRow[ColumnNameMessage] = message;
            dataRow[ColumnNameException] = exception;

            this.dataTable.Rows.Add(dataRow);
        }
    }
}