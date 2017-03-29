function addCart(id, num, reload, callback) {
    $.ajax({
        aysnc: false,
        url: '/cart/addcart/' + id+"?num="+num,
        type: 'post',
        dataType: 'json',
        success: function (data) {
            //alert(data.msg);
            if (data.validate && typeof callback != undefined) {
                //更改导航显示的购物车数量
                $(".CartCount").text(data.cartnum);
                eval(callback);
            }
            if (data.validate && reload)
                window.location.reload();
        },
        error: function () {
            alert("网络异常，添加失败，请刷新后重试");
        }
    });
}

//订单明细中添加餐具等进购物车，将添加后的CartId放置cookie中，以保证添加后在订单明细中实时显示
function settlementAddOtherToCart(id, num, reload, callback) {
    $.ajax({
        aysnc: false,
        url: '/cart/SettlementAddOtherToCart/' + id + "?num=" + num,
        type: 'post',
        dataType: 'json',
        async: false,
        success: function (data) {
            //alert(data.msg);
            if (data.validate && typeof callback != undefined) {
                //更改导航显示的购物车数量
                $(".CartCount").text(data.cartnum);
                callback();
            }
            if (data.validate && reload)
                window.location.reload();
        },
        error: function () {
            alert("网络异常，添加失败，请刷新后重试");
        }
    });
}

function removeCart(id, reload, callback) {
    if (confirm("确定删除该商品吗?")) {
        $.ajax({
            aysnc: false,
            url: '/cart/removeCart/' + id,
            type: 'post',
            dataType: 'json',
            success: function (data) {
                //alert(data.msg);
                if (data.validate && typeof callback != undefined)
                    eval(callback);
                if (data.validate && reload)
                    window.location.reload();
            },
            error: function () {
                alert("网络异常，删除失败，请刷新后重试");
            }
        });
    }
}
function BuyNow(id, num) {
    //$("<form></form>").attr("method", "get").attr("action", "/cart/buynow?id=" + id + "&num=" + num).submit();
    window.location = "/cart/buynow?id=" + id + "&num=" + num;


    //$.ajax({
    //    aysnc: false,
    //    url: '/Cart/BuyNow?id=' + id + '&num=' + num,
    //    type: 'post',
    //    dataType: 'json',
    //    error: function () { alert("网络异常，请重试！"); }
    //});
}