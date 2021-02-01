

$(".indir").click(function () {

    var videoIDIndir = $("#videoID").val();
    var convertTypeIndir = $(this).val();

    if (videoIDIndir == "") {
        alert("Video linki zorunludur.");
        return;
    }

    convert(videoIDIndir, convertTypeIndir);

});

function convert(videoID, convertType) {

    $("#yukleniyor").show();

    var start = videoID.indexOf("?v=") + 3;
    if (start > 3)
        videoID = videoID.substring(start, start + 11);

    start = videoID.indexOf("https://youtu.be/") + 17;
    if (start >= 17)
        videoID = videoID.substring(start, start + 11);


    $.ajax({
        url: "https://www.yt-download.org/api/button/" + convertType + "/" + videoID,
        type: 'get',
        success:function(string) {
            var convertIDs = "";
            var html = $.parseHTML(string);
            for (var i = 0; i < $(html).find("a").length; i++) {
                convertIDs += (i != 0 ? "," : "") + $(html).find("a").get(i);
            }

            if (convertIDs == "") {
                $("#yukleniyor").hide();
                alert("Link hatalı olabilir.");
                return;
            }



            

            var Veri = { videoID: videoID, convertIDs: convertIDs };
            $.ajax(
                {
                    url: '/youtube',
                    type: 'POST',
                    dataType: 'json',
                    data: Veri,
                    success: function (data) {

                        if (data.length == 1) {
                            $("#yukleniyor").hide();
                            alert(data);
                        }
                        if (data.length == 3) {

                            var title = "<h3>" + data[0] + "</h3>";
                            var thumb = "<img src='" + data[1] + "' class='img-fluid' >";
                            var buttons = data[2];
                            $("#youtubeThumb").html(thumb);
                            $("#modalBaslik").html(title);
                            $("#youtubeButonlar").html(buttons);
                            $("#yukleniyor").hide();
                            $('#youtubeModal').modal('show');
                        }
                    },
                    error: function () {
                        $("#yukleniyor").hide();
                        alert("Bir hata oluştu.");
                    }
                });




        },
        error: function () {
            $("#yukleniyor").hide();
            alert("Bir hata oluştu.");
        }
    });
}