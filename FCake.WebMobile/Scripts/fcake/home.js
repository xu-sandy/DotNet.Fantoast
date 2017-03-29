var curIndex = 0;
var t = setTimeout("showImg()", 5000);

function showImg() {
    if (curIndex < $(".slide-page span").length - 1) {
        curIndex++;
    }
    else {
        curIndex = 0;
    }
    setDesign();
    t = setTimeout("showImg()", 5000);
}
function setDesign() {
    $('.slide-list').stop().animate({ top: -500 * curIndex });
    $(".slide-page span").removeClass("active");
    $(".slide-page span").eq(curIndex).addClass("active");
}

$(function () {

    $(".slides .prev,.slides .next").hide();
    $(".slide-page span").eq(0).addClass("active");

    $(".slide-page span").each(function (index) {
        $(this).mouseenter(function () {
            t = clearTimeout(t);
            curIndex = index;
            setDesign();
        });
    });
    $(".slide-page span").mouseleave(function () {
        t = setTimeout("showImg()", 5000);
    });
    $(".slides .prev").click(function () {
        curIndex = curIndex > 0 ? (curIndex - 1) : $(".slide-wrap li").length - 1;
        setDesign();
    });
    $(".slides .next").click(function () {
        curIndex = curIndex < $(".slide-wrap li").length - 1 ? (curIndex + 1) : 0;
        setDesign();
    });

    $(".slides").mouseenter(function () {
        $(".slides .prev,.slides .next").show();
    });
    $(".slides").mouseleave(function () {
        $(".slides .prev,.slides .next").hide();
    });
    $(".slides .prev,.slides .next").hover(function () { t = clearTimeout(t); }, function () { t = setTimeout("showImg()", 5000); });
})