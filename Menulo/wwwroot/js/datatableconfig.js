(function ($) {
    'use strict';

    $.fn.DataTable.ext.pager.numbers_length = 5;

    function buildColumnsFromThead(tableSelector) {
        const columns = [];
        const $table = $(tableSelector);
        const basePath = $table.data('base-path');

        $table.find('thead th').each(function () {
            const $th = $(this);
            const type = $th.data('type');
            const data = $th.data('data');
            const name = $th.data('name') || data;
            const isNoSort = $th.hasClass('no-sort');

            let colDef = {
                data: data,
                name: name,
                orderable: !isNoSort,
                searchable: true,
            };

            if (type === 'index') {
                colDef.data = null;
                colDef.orderable = false;
                colDef.searchable = false;
                colDef.defaultContent = '';
            } else if (type === 'actions') {
                colDef.data = data;
                colDef.orderable = false;
                colDef.searchable = false;

                if (basePath) {
                    colDef.render = function (id, type, row) {
                        if (!id) return '';
                        return `
                            <div class="d-flex justify-content-center gap-1">
                                <a href="${basePath}/Details?id=${id}" class="btn btn-sm btn-info" title="Xem"><i class="bi bi-info-square"></i></a>
                                <a href="${basePath}/Edit?id=${id}" class="btn btn-sm btn-primary" title="Sửa"><i class="bi bi-pencil-square"></i></a>
                                <a href="${basePath}/Delete?id=${id}" class="btn btn-sm btn-danger btn-delete" title="Xóa"><i class="bi bi-trash"></i></a>
                            </div>
                        `;
                    };
                }
            }
            columns.push(colDef);
        });
        return columns;
    }

    function autoNumberingDrawCallback(tableApi, indexCol) {
        const pageInfo = tableApi.page.info();
        tableApi.column(indexCol, { search: 'applied', order: 'applied' })
            .nodes()
            .each((cell, i) => {
                cell.innerHTML = pageInfo.start + i + 1;
            });
    }

    window.initDataTable = function (tableSelector, options = {}) {
        const columns = buildColumnsFromThead(tableSelector);

        const defaultConfig = {
            responsive: false,
            autoWidth: false,
            processing: true,
            scrollX: true,
            scrollCollapse: true,
            dom: `<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>><"row"<"col-sm-12"tr>><"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>`,
            language: {
                url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/vi.json',
                paginate: {
                    first: '<i class="bi bi-chevron-double-left"></i>',
                    previous: '<i class="bi bi-chevron-left"></i>',
                    next: '<i class="bi bi-chevron-right"></i>',
                    last: '<i class="bi bi-chevron-double-right"></i>'
                }
            },
            pagingType: "full_numbers",
            pageLength: 10,
            lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "Tất cả"]],
            order: [],
            columns: columns,
            drawCallback: function (settings) {
                const api = this.api();
                const indexCol = $(tableSelector).find('thead th[data-type="index"]').index();
                if (indexCol > -1) {
                    autoNumberingDrawCallback(api, indexCol);
                }
                api.columns.adjust();
            },
            initComplete: function () {
                const api = this.api();
                // sau khi DOM xong, adjust lại (tránh lúc container chưa đo được width)
                setTimeout(() => api.columns.adjust().draw(false), 50);
            }
        };

        if (options.ajaxUrl) {
            Object.assign(defaultConfig, {
                serverSide: true,
                ajax: {
                    url: options.ajaxUrl,
                    type: 'POST',
                    headers: {
                        'RequestVerificationToken': document.querySelector('meta[name="xsrf-token"]')?.content ?? ''
                    },
                    error: xhr => {
                        if (window.Swal) {
                            Swal.fire({
                                icon: 'error',
                                title: 'Không tải được danh sách',
                                text: `Lỗi ${xhr?.status || 'không xác định'}`
                            });
                        }
                    }
                }
            });
            delete options.ajaxUrl;
        }

        const finalConfig = $.extend(true, {}, defaultConfig, options);
        finalConfig.destroy = true;

        return $(tableSelector).DataTable(finalConfig);
    };

})(jQuery);