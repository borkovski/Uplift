﻿var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/admin/webImage/GetAll",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "name",
                "width": "50%"
            },
            {
                "data": "id",
                "render": function (data) {
                    return `<div class="text-center">
                                <a href="/admin/webImage/Upsert/${data}" class='btn btn-success text-white' style='cursor:pointer; width:100px;'>
                                    <i class='far fa-edit'></i>
                                    Edit
                                </a>
                                &nbsp;
                                <a onclick=Delete("/admin/webImage/Delete/${data}") class='btn btn-danger text-white' style='cursor:pointer; width:100px;'>
                                    <i class='far fa-trash-alt'></i>
                                    Delete
                                </a>
                            </div>`;
                },
                "width": "50%"
            }
        ],
        "language": {
            "emptyTable":"No records found"
        },
        "width":"100%"
    });
}