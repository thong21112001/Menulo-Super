namespace Menulo.WebAPI.Api.Contracts.DataTables
{
    public class DataTablesModels
    {
        public sealed class DataTablesRequest
        {
            public int draw { get; set; }
            public int start { get; set; }
            public int length { get; set; }
            public SearchDto search { get; set; } = new();
            public List<ColumnDto> columns { get; set; } = new();
            public List<OrderDto> order { get; set; } = new();

            public sealed class SearchDto { public string? value { get; set; } }
            public sealed class OrderDto { public int column { get; set; } public string dir { get; set; } = "asc"; }
            public sealed class ColumnDto { public string data { get; set; } = ""; public string name { get; set; } = ""; }
        }

        public sealed class DataTablesResponse<T>
        {
            public int draw { get; set; }
            public int recordsTotal { get; set; }
            public int recordsFiltered { get; set; }
            public IEnumerable<T> data { get; set; } = Enumerable.Empty<T>();
        }
    }
}
