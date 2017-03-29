(function ($) {
    $.extend($.fn, {
        mypage: function (options) {
            var mypaging = $.data(this[0], "mypaging");
            mypaging = new $.mypaging(options, this[0]);
        }
    });
    $.mypaging = function (options, btnElement) {
        this.currentBtn = btnElement;
        this.settings = $.extend(true, {}, $.mypaging.defaults, options);
        this.init();
    };
    $.extend($.mypaging, {
        defaults:
          {
              page: 1,
              pagecount: 0,
              url: "",
              querystring: {}
          },
        prototype: {
            init: function () {
                this.settings.pagecount = parseInt($(this.currentBtn).attr("data-pagecount")) || 0;
                this.showOrhide();
                this.bindEvent();
            },
            bindEvent: function () {
                this.bindClick();
                this.bindScrolEnd();
            },
            bindClick: function () {
                var self = this;
                $(this.currentBtn).bind("touchend", { paging: this }, function (event) {
                    self.clickOrTouch(event);
                    return false;
                });
                $(this.currentBtn).bind("click", { paging: this }, function (event) {
                    self.clickOrTouch(event);
                });
            },
            clickOrTouch: function (event) {
                var pging = event.data.paging;
                pging.settings.page = pging.settings.page + 1;
                $.post(pging.settings.url, $.extend(pging.settings.querystring, { page: pging.settings.page, pagesize: pging.settings.pagesize }), function (data) {
                    $(pging.currentBtn).parentsUntil(".control-databox").parent().find(".control-datagrid").find("tbody").first().append(data);
                })
                pging.showOrhide();
            },
            bindScrolEnd: function () {
                var self = this;
                $(document).bind("ScrollEnd", function () {
                    if (!self.isLastPage()) {
                        $(self.currentBtn).trigger("touchend");
                    }
                });
            },
            showOrhide: function () {
                var disValue = "block";
                if (this.settings.page >= this.settings.pagecount) {
                    disValue = "none";
                }
                $(this.currentBtn).parent().css("display", disValue);
            },
            isLastPage: function () {
                if (this.settings.page >= this.settings.pagecount)
                    return true;
                else
                    return false;
            }
        }
    });
})(jQuery);


/*==========event==========*/

/*
 * 描述：设置屏幕最底端拖动处理
 */
(function () {
    $(document).ready(function () {
        //返回角度
        function GetSlideAngle(dx, dy) {
            return Math.atan2(dy, dx) * 180 / Math.PI;
        }

        //根据起点和终点返回方向 1：向上，2：向下，3：向左，4：向右,0：未滑动
        function GetSlideDirection(startX, startY, endX, endY) {
            var dy = startY - endY;
            var dx = endX - startX;
            var result = 0;

            //如果滑动距离太短
            if (Math.abs(dx) < 2 && Math.abs(dy) < 2) {
                return result;
            }

            var angle = GetSlideAngle(dx, dy);
            if (angle >= -45 && angle < 45) {
                result = 4;
            } else if (angle >= 45 && angle < 135) {
                result = 1;
            } else if (angle >= -135 && angle < -45) {
                result = 2;
            }
            else if ((angle >= 135 && angle <= 180) || (angle >= -180 && angle < -135)) {
                result = 3;
            }

            return result;
        }

        //滑动处理
        var startX, startY;
        document.addEventListener('touchstart', function (ev) {
            startX = ev.touches[0].pageX;
            startY = ev.touches[0].pageY;
        }, false);
        document.addEventListener('touchend', function (ev) {
            var endX, endY;
            endX = ev.changedTouches[0].pageX;
            endY = ev.changedTouches[0].pageY;
            var direction = GetSlideDirection(startX, startY, endX, endY);
            switch (direction) {
                case 0://没滑动
                    $(document).trigger('Tap', { X: endX - startX, Y: endY - startY });
                    break;
                case 1://向上
                case 2://向下
                case 3://向左
                case 4://向右
                    $(document).trigger('TouchMove', { X: endX - startX, Y: endY - startY });
                    break;
                default:
            }
        }, false);
        $(document).bind('TouchMove', function (event, data) {
            console.log(event); console.log(data);
            var touch = data;
            // 把元素放在手指所在的位置
            var clientHeight = $(window).height();
            var docHeight = $(document).height();
            var bodyHeight = $('body').height();
            var bodyScrollTop = $('body').scrollTop();
            if (touch.Y < 0 && (clientHeight == docHeight && clientHeight > bodyHeight) || (clientHeight < docHeight && bodyScrollTop + clientHeight >= bodyHeight)) {
                $(document).trigger('ScrollEnd');
            } else if (touch.Y > 0 && bodyScrollTop == 0) {
                $(document).trigger('ScrollStart');
            }
        });

        ////test
        //$(document).bind("ScrollEnd", function () {
        //});
        //$(document).bind("ScrollStart", function () {
        //    console.log("ScrollStart");
        //});
    });
})();

(function () {
    var clientHeight = $(window).height();

    $('body').append("<div id='fixtouchelement'style='width:100%;height:" + clientHeight + "px;Z-index:-1;position:fixed;top:0'><div>");

    $(window).resize(function () {
        $('#fixtouchelement').height($(window).height());
    });
})();

/*================================================================*/