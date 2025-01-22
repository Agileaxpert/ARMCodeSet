function generateDynamicHtml(dataObj, html) {
    var tempHtml = html;
    var finalHtml = "";
    for (var i = 0; i < dataObj.length; i++) {
        for (var key in dataObj[i]) {
            if (tempHtml.indexOf("{{") == -1) {
                break;
            }

            if (dataObj[i].hasOwnProperty(key) && tempHtml.indexOf("{{" + key + "}}") > -1) {
                var val = dataObj[i][key];
                tempHtml = tempHtml.replaceAll("{{" + key + "}}", val);
            }
        }

        finalHtml += tempHtml;
        tempHtml = html;
    }

    return finalHtml;
}

function dataConvert(data) {
    try {
        data = JSON.parse(data.d);
        if (typeof data.result[0].result.row != "undefined") {
            return data.result[0].result.row;
        }
    }
    catch (ex) {
        //console.log(ex);
    };

    try {
        if (typeof data.result[0].result != "undefined") {
            return data.result[0].result;
        }
    }
    catch (ex) {
        //console.log(ex);
    };

    return data;
}

function getSqlData(dataOptions) {
    $.ajax({
        type: "POST",
        url: (dataOptions.url || "../../CustomPages/aspx/TreeConfig_v2.aspx/GetSqlData"),
        cache: false,
        async: true,
        contentType: "application/json;charset=utf-8",
        data: JSON.stringify({ sqlName: dataOptions.sqlName, sqlParams: dataOptions.sqlParams }),
        dataType: "json",
        success: function (data) {
            try {
                data = dataConvert(data);
            }
            catch (ex) { }            

            if (!dataOptions.customBinding) {
                if (dataOptions.replaceHtml) {
                    dataOptions.bindElem.html("");
                }

                dataOptions.bindElem.append(generateDynamicHtml(data, dataOptions.contentHtml));
            }

            if (typeof dataOptions.onSuccess != "undefined")
                dataOptions.onSuccess(data);
        },
        error: function (e) {
            if (typeof dataOptions.onError != "undefined")
                dataOptions.onError(e);
        }
    });
}