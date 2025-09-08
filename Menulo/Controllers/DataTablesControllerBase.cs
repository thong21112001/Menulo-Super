using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Exceptions;
using System.Linq.Expressions;
using static Menulo.Application.Common.Contracts.DataTables.DataTablesModels;

namespace Menulo.Controllers
{
    /// <summary>
    /// Một Controller base dùng chung cho tất cả các chức năng DataTables.
    /// Nó xử lý việc phân trang, tìm kiếm, sắp xếp một cách tự động.
    /// </summary>
    public abstract class DataTablesControllerBase : ControllerBase
    {
        private readonly IMapper _mapper;

        protected DataTablesControllerBase(IMapper mapper)
        {
            _mapper = mapper;
        }

        /// <summary>
        /// Xử lý request từ DataTables và trả về kết quả.
        /// </summary>
        protected virtual IActionResult GetDataTableResult<TEntity, TDto>(
            IQueryable<TEntity> source,
            DataTablesRequest request,
            Expression<Func<TEntity, bool>>? globalSearchPredicate = null)
        {
            int recordsTotal = source.Count();
            var filteredData = source;

            // 1. Filtering (Áp dụng điều kiện tìm kiếm toàn cục nếu có)
            if (globalSearchPredicate != null)
            {
                filteredData = filteredData.Where(globalSearchPredicate);
            }

            int recordsFiltered = filteredData.Count();

            // 2. Sorting (Sử dụng Linq.Dynamic.Core để sắp xếp động)
            if (request.Order != null && request.Order.Any())
            {
                var order = request.Order.First();
                if (order.Column >= 0 && order.Column < request.Columns?.Count)
                {
                    var column = request.Columns[order.Column];
                    // Dùng thuộc tính 'Name' từ DataTables để mapping an toàn với tên property của TEntity.
                    // Đây là cách làm tốt nhất để tránh lỗi khi tên cột thay đổi.
                    var sortColumnName = !string.IsNullOrWhiteSpace(column.Name) ? column.Name : column.Data;

                    if (!string.IsNullOrWhiteSpace(sortColumnName))
                    {
                        try
                        {
                            var direction = order.Dir?.ToLower() == "desc" ? "descending" : "ascending";
                            filteredData = filteredData.OrderBy($"{sortColumnName} {direction}");
                        }
                        catch (ParseException)
                        {
                        }
                    }
                }
            }

            // 3. Paging (Bỏ qua và lấy các record cho trang hiện tại)
            var pagedData = filteredData.Skip(request.Start).Take(request.Length);

            // 4. Projection (Sử dụng AutoMapper để chuyển đổi sang DTO)
            var projectedData = pagedData
                .ProjectTo<TDto>(_mapper.ConfigurationProvider)
                .ToList();

            // 5. Create Response (Tạo đối tượng trả về cho DataTables)
            var response = new DataTablesResponse<TDto>
            {
                Draw = request.Draw,
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsFiltered,
                Data = projectedData
            };

            return Ok(response);
        }
    }
}
