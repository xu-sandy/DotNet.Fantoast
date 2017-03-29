if (!window.isNivo) {
    $.getScript('/scripts/slider.js',
    function () {
        $(".slide-list").nivoSlider({
            pauseTime: 5000,
            animSpeed: 1000,
            controlNav: true
        });
        window.isNivo = true;
    });
} else {
    $(function () {
        $(".slide-list").nivoSlider({
            pauseTime: 5000,
            animSpeed: 1000,
            controlNav: true
        });
    });
}

$(function () {
    $(".prev").click(function () {
        $(".nivo-prevNav").click();
    });
    $(".next").click(function () {
        $(".nivo-nextNav").click();
    });
})
