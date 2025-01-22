
var groupHtml = `
    <div class="accordion accordion-icon-toggle" id="{{groupname}}">
        <div class="mb-5">
            <div class="accordion-header py-3 d-flex" data-bs-toggle="collapse" data-bs-target="#{{groupname}}_item" aria-expanded="false">
                <span class="accordion-icon">
                    <span class="material-icons material-icons-style material-icons-2">chevron_right</span>
                </span>
                <h3 class="fs-5 fw-bold mb-0 ms-4">{{groupname}}</h3>
            </div>
            <div id="{{groupname}}_item" class="fs-6 ps-10 collapse show accordion-body" data-bs-parent="#{{groupname}}">
                {{subGroupHtml}}
            </div>
        </div>
    </div>`;

var subGroupHtml = `
    <div class="step" onclick="openConfig(this,'{{groupid}}','{{subgroupid}}'); return false;">
        <div>
            <div class="circle">
                <i class="fa fa-check"></i>
                <span class="steps-counts">{{sno}}</span>
            </div>
        </div>
        <div class="title">
            <a href="#" onclick="return false;">{{subgroupname}}</a>
        </div>
    </div>`;

function openConfig(elem, groupId, subGroupId) {
    $(".step-active").removeClass('step-active');
    $(elem).addClass('step-active');

    $.ajax({
        type: "POST",
        url: "../aspx/ConfigParams.aspx/GetConfigParams",
        cache: false,
        async: true,
        contentType: "application/json;charset=utf-8",
        data: JSON.stringify({ groupId: groupId, subGroupId: subGroupId }),
        dataType: "json",
        success: function (data) {
            try {
                data = JSON.parse(data.d);
                if (typeof data.error != "undefined") {
                    showAlertDialog("error", groupsJson.error.msg);
                }
                else
                    data = data.result.row;
            }
            catch (error) {
                showAlertDialog("error", error.message);
            }

            const container = document.getElementById("configParamsContainer");
            container.innerHTML = '';
            container.scrollTop = 0;

            data.forEach(data => {
                let input = data.input_text;
                let jsonData = (data.json == '') ? undefined : JSON.parse(data.json.replace(/(?:\r\n|\r|\n)/g, ''));
                axHtmlObj.parse({ name: data.rule, input: input, data: jsonData, container: "#configParamsContainer"});
            })

        }
    });
}

function closeParentFrame() {
    try {
        eval(callParent('closeFrame()', 'function'));
    } catch (ex) {

    }
}

function ShowDimmer(status) {

    var dv = $("#waitDiv");

    if (dv.length > 0 && dv != undefined) {
        if (status == true) {
            closeParentFrame();
            $("body").addClass("page-loading");
            document.onkeydown = function EatKeyPress() {
                return false;
            }
        } else {
            $("body").removeClass("page-loading");
            document.onkeydown = function EatKeyPress() {
                return true;
            }
        }
    } else {

        //TODO:Needs to be tested
        if (window.opener != undefined) {

            dv = $("#waitDiv", window.opener.document);
            if (dv.length > 0) {
                if (status == true) {
                    $("body", window.opener.document).addClass("page-loading");
                } else {
                    $("body", window.opener.document).removeClass("page-loading");
                }
            }
        }
    }
}

function loadFrame() {
    parent.$.LoadingOverlay("show");
}

function closeFrame() {
    parent.$.LoadingOverlay("hide", true);
}



function dataConvert(data) {
    try {
        data = JSON.parse(data.d);
        if (typeof data.result[0].result.row != "undefined") {
            return data.result[0].result.row;
        }
    }
    catch (ex) {
    };

    try {
        if (typeof data.result[0].result != "undefined") {
            return data.result[0].result;
        }
    }
    catch (ex) {
    };

    return data;
}


var axHtmlObj;
$(document).ready(function () {
    axHtmlObj = new AxHTML();

    try {
        groupsJson = JSON.parse(groupsJson);

        if (typeof groupsJson.error != "undefined") {
            showAlertDialog("error", groupsJson.error.msg);
        }
        else
            groupsJson = groupsJson.result.row;
    }
    catch (error){
        showAlertDialog("error", error.message);
    }

    var groupArray = [];
    groupsJson.forEach(data => {
        if (groupArray.indexOf(data.groupname) == -1) {
            groupArray.push(data.groupname);
            $("#configParamGroups").append(Handlebars.compile(groupHtml)(data));
        }
    });

    groupsJson.forEach(data => {
        groupArray.push(data.groupname);
        $("#" + data.groupname + "_item").append(Handlebars.compile(subGroupHtml)(data));
    });

    $(".step").eq(0).click();

});

function saveConfig() {
    let configData = axHtmlObj.getData();
    
    $.ajax({
        type: "POST",
        url: "../aspx/ConfigParams.aspx/SetConfigParams",
        cache: false,
        async: true,
        contentType: "application/json;charset=utf-8",
        data: JSON.stringify({ json: JSON.stringify(configData) }),
        dataType: "json",
        success: function (data) {
            if (data.d == "SUCCESS") {
                showAlertDialog("success", "Configuration Parameters saved successfully.");
            }
            else {
                showAlertDialog("error", "Error occurred.");
            }
        }
    });
}