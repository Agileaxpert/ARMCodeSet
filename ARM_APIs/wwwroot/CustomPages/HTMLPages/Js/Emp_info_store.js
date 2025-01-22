var pageParams = new URLSearchParams(window.location.search);
var employeeCode = pageParams.get('employee_code');
var targetStruct;

var accordionHtml = `
    <div class="accordion accordion-icon-toggle" id="{{group_code}}">
        <div class="mb-5">
            <div class="accordion-header py-3 d-flex" data-bs-toggle="collapse" data-bs-target="#{{group_code}}_item" aria-expanded="false">
                <span class="accordion-icon">
                    <span class="material-icons material-icons-style material-icons-2">chevron_right</span>
                </span>
                <h3 class="fs-5 fw-bold mb-0 ms-4">{{group_name}}</h3>
            </div>
            <div id="{{group_code}}_item" class="fs-6 ps-10 collapse show accordion-body" data-bs-parent="#{{group_code}}">
                {{stepsHtml}}
            </div>
        </div>
    </div>`;

var stepsHtml = `
    <div class="step" data-target="{{form_id}}" onclick="openStep(this,'{{form_id}}','{{recordid}}'); return false;">
        <div>
            <div class="circle {{status}}">
                <i class="fa fa-check"></i>
                <span class="Emp-steps-counts">{{slno}}</span>
            </div>
        </div>
        <div class="title">
            <a href="#" onclick="return false;">{{form_name}}</a>
        </div>
    </div>`;

var profileCompletionHtml = `
    <div class="d-flex justify-content-between w-100 mt-auto mb-2">
        <span class="  text-gray-800">Profile Completion</span>
        <span class="fw-bolder ">{{profile_completion}}%</span>
    </div>
    <div class="h-5px mx-3 w-100 bg-light mb-3 profile-progress-bar">
        <div class="bg-success rounded h-5px" role="progressbar" style="width: {{profile_completion}}%;" aria-valuenow="{{profile_completion}}" aria-valuemin="0" aria-valuemax="100"></div>
    </div>
`;

var empProfileDetailsHtml = `    
    <div class="d-flex flex-column">
        <div class="text-gray-800  mb-1 Emp-name">{{first_name}} {{last_name}}</div>
        <span class="Emp-ID">{{employee_code}}</span>
    </div>`;

if (pageParams.has('target')) {
    targetStruct = pageParams.get('target');
}

function openStep(elem, transId, recordId) {
    let tempVal = "a";
    if (transId == "basei" || transId == "baseio") {        
        (transId == "basei") ? tempVal = "p" : tempVal = "o";
        transId = "basei";

        $.ajax({
            type: "POST",
            url: "../aspx/TreeConfig_v2.aspx/SetPersonalInfoGlobalVar",
            cache: false,
            async: false,
            contentType: "application/json;charset=utf-8",
            data: JSON.stringify({ val: tempVal }),
            dataType: "json"
        });
    }

    targetStruct = transId;
    $(".step-active").removeClass('step-active');
    $(elem).addClass('step-active');

    $("#rightPageTitle").text("");

    let $rightIframe = $("#rightIframe");
    let url = "../../aspx/tstruct.aspx?transid=" + transId;
    if (typeof recordId != "undefined" && (recordId.trim() != "" && recordId.trim() != "0"))
        url += "&act=load&recordid=" + recordId;
    else {
        url += "&act=open&employee_code=" + employeeCode;
    }

    $rightIframe.attr("src", "");
    $rightIframe.attr("src", url);
}

function closeParentFrame() {
    try {
        eval(callParent('closeFrame()', 'function'));
    } catch (ex) {
        //console.log("Error in CloseParentFrame -" + ex.message);
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

var empInfoOptions = {
    sqlName: "Employee_Info",
    customBinding: true,
    sqlParams: 'empcodeinfostore$:$' + employeeCode + '',
    bindElem: $("#employeeInfoSteps"),
    contentHtml: stepsHtml,
    onSuccess: function (dataObj) {
        var groupArray = [];
        dataObj.forEach(data => {
            if (groupArray.indexOf(data.group_code) == -1) { 
                groupArray.push(data.group_code);
                $("#employeeInfoSteps").append(Handlebars.compile(accordionHtml)(data));
            }
        })

        dataObj.forEach(data => {
            groupArray.push(data.group_code);
            $("#" + data.group_code + "_item").append(Handlebars.compile(stepsHtml)(data));            
        })

        if (typeof targetStruct == "undefined") {
            $("#employeeInfoSteps .step").eq(0).click();
        }
        else {
            $("#employeeInfoSteps .step[data-target='" + targetStruct + "']").eq(0).click();
        }
    }
}

var profileCompletionOptions = {
    sqlName: "Employee_Info_Profile_Completion",
    sqlParams: 'empcodeinfostore$:$' + employeeCode + '',
    bindElem: $("#profileCompletion"),
    contentHtml: profileCompletionHtml
}

var profileDetailsOptions = {
    sqlName: "Employee_Info_Profile",
    sqlParams: 'empcodeinfostore$:$' + employeeCode + '',
    bindElem: $("#empProfileDetails"),
    contentHtml: empProfileDetailsHtml,
    onSuccess: function (data) {

        let filePath = data[0].profile_image_path;
        let recordid = data[0].recordid;

        $.ajax({
            type: "POST",
            url: "../../CustomPages/aspx/TreeConfig_v2.aspx/GetFile",
            cache: false,
            async: true,
            contentType: "application/json;charset=utf-8",
            data: JSON.stringify({ filePath: filePath, recordId: recordid }),
            dataType: "json",
            success: function (data) {
                try {
                    $("#profilePic").attr("src", data.d);
                }
                catch (ex) { }
            }
        });

    }
}

window.addEventListener('beforeunload', (event) => {
    parent.reSetPersonalInfoGlobalVar();
});

//sqlName, sqlParams, bindElem, contentHtml, successCallBack, errorCallBack
$(document).ready(function () {    

    getSqlData(empInfoOptions);
    getSqlData(profileCompletionOptions);
    getSqlData(profileDetailsOptions);
});
