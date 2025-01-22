$(document).ready(function () {
    bindPageData();
});

function generateDynamicHtml(dataObj, html) {
    var tempHtml = html;
    var finalHtml = "";
    for (var i = 0; i < dataObj.length; i++) {
        if (tempHtml.indexOf("{{row}}") > -1) {
            tempHtml = tempHtml.replaceAll("{{row}}", JSON.stringify(dataObj[i]));
        }

        for (var key in dataObj[i]) {
            if (tempHtml.indexOf("{{") == -1) {
                break;
            }

            if (dataObj[i].hasOwnProperty(key) && tempHtml.indexOf("{{" + key + "}}") > -1) {
                var val = dataObj[i][key];
                tempHtml = tempHtml.replaceAll("{{" + key + "}}", val).replaceAll("<axdata>" + key + "</axdata>", val);
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

function getSqlData(sqlName, $elem) {
    $.ajax({
        type: "POST",
        url: "../../CustomPages/aspx/TreeConfig_v2.aspx/GetSqlData",
        cache: false,
        async: true,
        contentType: "application/json;charset=utf-8",
        data: JSON.stringify({ sqlName: sqlName }),
        dataType: "json",
        success: function (data) {
            try {
                data = dataConvert(data);
            }
            catch (ex) { }
            debugger;
            $elem.append(generateDynamicHtml(data, $elem.html()));
        }
    });
}


function bindPageData() {
    $(".ax-data").each(function () {
        debugger;
        let $elem = $(this);
        let dataSource = $elem.attr("data-ax-sqlname");
        getSqlData(dataSource, $elem);
    });
}
