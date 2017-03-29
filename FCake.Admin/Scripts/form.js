var pharos = pharos || {};
(function (para) {
    para.formHelper = {
        commonSave: function (formId, url, data, callback) {
            $.ajax({
                url: url,
                type: 'post',
                data: data,
                dataType: 'json',
                success: function (data, status) {
                    if (data.validate == true) {
                        if (callback) {
                            callback(data);
                        }
                        //close
                    }
                    else {
                        alert(data.msg);
                    }
                }
            })
        },
        isNESetGuid: function (objId) {
            var dataId = $("#" + objId).val();
            if (String.isNE(dataId))
                $("#" + objId).val(pharos.g.newGuid());
            return dataId;
        }
    }
    var me = para.formHelper;
})(pharos);