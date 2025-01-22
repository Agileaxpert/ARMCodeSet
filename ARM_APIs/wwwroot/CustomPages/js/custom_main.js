alertsTimeout = '1';
//var customGlobalVarsObj = {};

//$(document).ready(function(){
//	setTimeout(function(){
//		/*let customGlobalVars = AxGetGlobalVar();

//		let customGlobalVars.globalVars.forEach(function(item){
//		Object.keys(item).forEach(function eachKey(key) {
//			test[key] = item[key];
//			});
//		});*/

//		setCompanyLogo();
//	}, 1)
//});


function AxOnLoadMiddleIframe(src) {
    if (src.toLowerCase().startsWith("employeegrouping.aspx")) {
        document.getElementById('middle1').src = "../CustomPages/aspx/" + src;
        return true;
    }
}

function setCompanyDetails(isUpdate){
    let compLogoHtml = `
	<img id="customCompanyLogo" alt="Logo" src="" class="mh-45px " />
	<h3 class="d-flex mh-50px m-2 text-dark fw-bold fs-1 align-items-center divTitle">{{company_name}}</h3>`;

    if (isUpdate) {
        $("#customCompanyLogo +  .divTitle").remove();
        $("#customCompanyLogo").remove();
    }

    var compOptions = {
        sqlName: "Company_Logo",
        sqlParams: '',
        bindElem: $("#customLogoContainer"),
        contentHtml: compLogoHtml,
        url: "../CustomPages/aspx/TreeConfig_v2.aspx/GetSqlData",
        onSuccess: function (data) {
            try {
                let filePath = data[0].file_path;
                let recordid = data[0].recordid;
                let displayBrandLogo = data[0].display_brand_logo;
                if (displayBrandLogo && displayBrandLogo == "F") {
                    $("#customLogoContainer img").eq(0).hide();
                }
                else {
                    $("#customLogoContainer img").eq(0).show();
                }

                $.ajax({
                    type: "POST",
                    url: "../CustomPages/aspx/TreeConfig_v2.aspx/GetFile",
                    cache: false,
                    async: true,
                    contentType: "application/json;charset=utf-8",
                    data: JSON.stringify({ filePath: filePath, recordId: recordid }),
                    dataType: "json",
                    success: function (data) {
                        try {
                            $(customCompanyLogo).attr("src", data.d);
                        }
                        catch (ex) { }
                    }
                });
            }
            catch (e) { }
        }
    }
	
    getSqlData(compOptions);
}

function reSetPersonalInfoGlobalVar() {
    $.ajax({
        type: "POST",
        url: "../CustomPages/aspx/TreeConfig_v2.aspx/ReSetPersonalInfoGlobalVar",
        cache: false,
        async: true,
        contentType: "application/json;charset=utf-8",
        dataType: "json"
    });
}