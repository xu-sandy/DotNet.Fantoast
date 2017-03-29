$(function () {
    pharos.permission.load();
    pharos.permission.startcheck();

    //设置全局下拉样式
    $(".easyui-combobox").combobox({ height: 34 });
    $(".Wdate").css({ border: "1px solid #D3D3D3" });
    $(".easyui-numberbox").numberbox({ height: 32 });

    //toolbar样式 
    $(".datagrid-toolbar .l-btn").each(function () {
        if ($(this).hasClass("linkbtn") == false) {
            $(this).addClass("cus1").addClass("_linkbtn");
        }
    });
});