var pageParams = new URLSearchParams(window.location.search);
var employeeCode = pageParams.get('employee_code');
var targetStruct;

var accordionHtml = `
    <div><div class="specialRow" data-group="{{atype}}">{{atype}}</div></div>`;

if (pageParams.has('target')) {
    targetStruct = pageParams.get('target');
}

function openCustomTstruct(elem, transId, recordId) {
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
    sqlName: "Organization_attributes",
    customBinding: true,
    sqlParams: '',
    bindElem: $("#Emp-profile-container"),
    onSuccess: function (dataObj) {
        var groupArray = [];
        dataObj.forEach(data => {
            if (groupArray.indexOf(data.atype) == -1) {
                groupArray.push(data.atype);
                $("#Emp-profile-container").append(Handlebars.compile(accordionHtml)(data));
            }
        })

        dataObj.forEach(data => {
            data.description = data.description.replaceAll("\\r", "").replaceAll("\\n", "").replaceAll("\\t", "");
            $("[data-group='" + data.atype + "']").parent().append(data.description);
        })

        bindCustomEvents();

        if ($(".customHyperLink").length > 0) {
            $(".customHyperLink").eq(0).click();
        }

    }
}

$(document).ready(function () {
    getSqlData(empInfoOptions);
});

function openCustomListView(target, trasnid, caption) {
    let url = "../../aspx/iview.aspx?ivname=".concat(trasnid, "&tstcaption=").concat(caption);
    $("#rightIframe").attr("src", url);
    return true;
}

function bindCustomEvents() {
    $('.Emp-org-attributes').on("click", function () {
        var elems = document.getElementsByClassName("Emp-Attribute-active");
        [].forEach.call(elems, function (el) {
            el.classList.remove("Emp-Attribute-active");
        });

        this.classList.add("Emp-Attribute-active");
    });

    $('.switch input[type=checkbox]').on("change", function () {
        var $elem = $(this);
        var attributeId = $elem.attr("data-attributeid");
        updateAttrFlag(attributeId);
    });
}

function updateAttrFlag(attributeId) {
    $.ajax({
        type: "POST",
        url: "../aspx/TreeConfig_v2.aspx/UpdateOrgAttributeFlag",
        data: JSON.stringify({ attributeId: attributeId }),
        cache: false,
        async: true,
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (data) {
            console.log(data);
        }
    });
}

