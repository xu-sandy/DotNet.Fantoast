$(function () {
    $("#layout").layout();
    //$('#list').accordion({
    //    animate: true,
    //    fit: true,
    //    border: false,
    //    selected: 0
    //});

    //tabs事件绑定
    $("#tabs").delegate(".tabs-inner", "dblclick", function () {
        var tab = $('#tabs').tabs('getSelected');
        var url = $(tab.panel('options').content).attr('src');
        if (url == null || url == '' || url == 'undefined')
            return;
        $('#tabs').tabs('update', { tab: tab, options: { content: createFrame(url) } });
    })

    $("#tabs").tabs({
        onSelect: function (title, index) {
            var tab = $('#tabs').tabs('getSelected');
            $("#list .hover").removeClass("hover");
            $("#_" + tab.panel('options').id).addClass("hover");
        }
    });
})
//创建frame
function createFrame(url) {
    //<iframe scrolling="auto" frameborder="0" src="' + url + '" style="width:100%;height:99%;"></iframe>
    return '<iframe class="tabIframe" scrolling="auto" frameborder="0" src="' + url + '" ></iframe>';
}
//跳转
//function jump(node) {
//    if ($("#tabs").tabs("exists", node.text) == false) {
//        addnode(node);
//    }
//    else {
//        $('#tabs').tabs('select', node.text)
//    }
//}
function jump(text, url, id) {
    if ($("#tabs").tabs("exists", text) == false) {
        addnode(text, url, id);
    }
    else {
        $('#tabs').tabs('select', text)
    }
}
//添加node
//function addnode(node) {
//    $("#tabs").tabs('add', {
//        title: node.text,
//        closable: true,
//        content: createFrame(node.attributes.url)
//    });
//}
function addnode(text, url, id) {
    $("#tabs").tabs('add', {
        title: text,
        closable: true,
        id: id,
        bodyCls: 'tabContent',
        content: createFrame(url)
    });
}
$(window).bind("resize", function () {
    $("#layout").layout("resize");
});