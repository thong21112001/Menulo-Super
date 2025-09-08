namespace Menulo.Application.Common.Contracts.DataTables
{
    public class DataTablesModels
    {
        public class DataTablesRequest
        {
            public int Draw { get; set; }
            public int Start { get; set; }
            public int Length { get; set; }
            public Search? Search { get; set; }
            public List<Order>? Order { get; set; }
            public List<Column>? Columns { get; set; }
        }

        public class Search { public string? Value { get; set; } }

        public class Order { public int Column { get; set; } public string? Dir { get; set; } }

        public class Column { public string? Data { get; set; } public string? Name { get; set; } }

        public class DataTablesResponse<T>
        {
            public int Draw { get; set; }
            public int RecordsTotal { get; set; }
            public int RecordsFiltered { get; set; }
            public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
        }
    }
}
