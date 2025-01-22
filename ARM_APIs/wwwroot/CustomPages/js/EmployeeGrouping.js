var saveValuesArray = [];
var recordId = "0";
$(document).ready(function () {
    $("#groupCode").html("Group code: " + groupCode);
    $("#groupName").html("Group description: " + groupName);

    getAttributes();
    setResizableDivs();
    // start code by Rakesh
    $('#attributediv').on('change', '.attributesSelection:checkbox:not(.changeall)', function () {
        let form_id = $(this).attr("data-form");
        let field_name = $(this).attr("data-field");
        let field_value = $(this).attr("data-value");
        let formidvalue = form_id + "_" + field_name;
        let fieldcaption = $('#attributeTabNav [data-form-id="' + formidvalue + '"]').text().trim();
        if ($(this).is(':checked')) {
            if ($("#groupviewDiv #itm_" + formidvalue).length == 0) {
                let groupHeaderHtml = ` <div class="accordion-item" id="itm_${formidvalue}">
                    <h2 class="accordion-header" id="header-${formidvalue}" >
                        <button class="accordion-button" type="button" data-bs-toggle="collapse" data-bs-target="#${formidvalue}-collapse" aria-expanded="true" aria-controls="${formidvalue}-collapse" data-form-id="${formidvalue}" data-form-name="${field_name}">
                        ${fieldcaption}
                        </button>
                    </h2>
                    <div id="${formidvalue}-collapse" class="accordion-collapse collapse show" aria-labelledby="header-${formidvalue}">
                            <ul class="accordion-body attributeList">
                            </ul>
                    </div>
                 </div>`;
                $("#groupviewDiv #tree").append(groupHeaderHtml);
            }
            var selectedattributeHtml = `<li><span class="text-hover-primary">${field_value}</span>
                 <a href='#' class="deleteattr" data-value='${field_value}' data-field="${field_name}" data-form="${form_id}">
                 <span title="Delete" class="material-icons removeIcon bg-light-danger text-danger">close</span>
                 </a></li>`;
            $("#groupviewDiv #itm_" + formidvalue + " .accordion-body").append(selectedattributeHtml);
            $(this).parents(".menu.attributeDataSection").find('input[type=checkbox]:not(:checked)').length == 0 && $("#attributesSelection_" + formidvalue).prop("checked", true);

        } else {
            $("#groupviewDiv #itm_" + formidvalue + " [data-value='" + field_value + "'] ").parent('li').remove();
            $("#groupviewDiv #itm_" + formidvalue + " li").length == 0 && $("#groupviewDiv #itm_" + formidvalue).remove();
            $(this).parents(".menu.attributeDataSection").find('input[type=checkbox]:checked').length == 0 && $("#attributesSelection_" + formidvalue).prop("checked", false);
        }
    });
    $("#groupviewDiv").on('click', '.deleteattr', function () {
        $(this).parent('li').remove();
        $("#attributeTabContent input[data-value='" + $(this).attr('data-value') + "']." + $(this).attr('data-form') + "." + $(this).attr('data-field')).prop('checked', false);
    })
    // end code by Rakesh
});

function openAdvancedGrouping() {
    let url = "tstruct.aspx?transid=empgr";
    if (typeof recordId != "undefined" && (recordId.trim() != "" && recordId.trim() != "0")) {
        url += "&act=load&recordid=" + recordId;
        parent.LoadIframe(url)
    }
}

function getSelectedAttributeFieldValues() {

    $.ajax({
        type: "POST",
        url: "TreeConfig_v2.aspx/GetSelectedAttributeFieldValues",
        data: JSON.stringify({ groupCode: groupCode }),
        cache: false,
        async: true,
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (data) {
            try {
                data = dataConvert(data);
            }
            catch (ex) { }

            recordId = data[0].global_config_employeegroupingid;

            if (data[0].selected_attributes_value != "") {
                var rowdata = $.parseJSON(data[0].selected_attributes_value);
                $.each(rowdata, function (index, getdata) {
                    var field_value = getdata.field_value;
                    var form_id = getdata.form_id;
                    var field_name = getdata.field_name;
                    let formidvalue = form_id + "_" + field_name; //code by Rakesh
                    if (field_value != "") {
                        // start code by Rakesh
                        let fieldcaption = $('#attributeTabNav [data-form-id="' + formidvalue + '"]').text().trim();
                        var groupHeaderHtml = ` <div class="accordion-item" id="itm_${formidvalue}">
                        <h2 class="accordion-header" id="header-${formidvalue}" >
                            <button class="accordion-button" type="button" data-bs-toggle="collapse" data-bs-target="#${formidvalue}-collapse" aria-expanded="true" aria-controls="${formidvalue}-collapse" data-form-id="${formidvalue}" data-form-name="${field_name}">
                            ${fieldcaption}
                            </button>
                        </h2>
                        <div id="${formidvalue}-collapse" class="accordion-collapse collapse show" aria-labelledby="header-${formidvalue}">
                                <ul class="accordion-body attributeList">
                                </ul>
                        </div>
                  </div>`;
                        $("#groupviewDiv #tree").append(groupHeaderHtml);
                        //     var corporateAtrributeContainer =` <div id="${formidvalue}-collapse" class="accordion-collapse collapse show" aria-labelledby="header-${formidvalue}">
                        //             <ul class="accordion-body">
                        //             </ul>
                        //           </div>`
                        //           $("#groupviewDiv #itm_" + formidvalue).append(corporateAtrributeContainer);
                        // end code by Rakesh
                        let field_values = field_value.split('~');
                        $.each(field_values, function (index, field_value) {
                            if (field_value != "") {
                                $("#attributeTabContent input[data-value='" + field_value + "']." + form_id + "." + field_name).prop('checked', true);
                                // start code by Rakesh
                                var selectedattributeHtml = `<li><span class="text-hover-primary">${field_value}</span>
                            <a href='#' class="deleteattr" data-value='${field_value}' data-field="${field_name}" data-form="${form_id}">
                            <span title="Delete" class="material-icons removeIcon bg-light-danger text-danger">close</span>
                            </a></li>`;
                                // `<li><a href='#' data-value='${field_value}' data-field="${field_name}" data-form="${form_id}">
                                // ${field_value}
                                // </a></li>`;
                                $("#groupviewDiv #itm_" + formidvalue + " .accordion-body").append(selectedattributeHtml);
                                // start code by Rakesh

                            }
                        })
                    }
                })
            }
            else {
                setTimeout(function () {
                    $("#Select_Emp_attribute").trigger("click");
                }, 100);
            }
        }
    });
}

function getAttributeFieldValues() {

    $.ajax({
        type: "POST",
        url: "TreeConfig_v2.aspx/GetAttributeFieldValues",
        data: JSON.stringify({ groupCode: groupCode }),
        cache: false,
        async: true,
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (data) {
            try {
                data = dataConvert(data);
            }
            catch (ex) { }

            for (let i = 0, len = data.length; i < len; i++) {
                let groupingdata = data[i];
                let fieldcaption = groupingdata['fieldcaption'];
                let field_name = groupingdata['field_name'];
                let form_id = groupingdata['form_id'];
                let field_value = groupingdata['field_value'];
                let data_type = groupingdata['data_type'];

                let formidvalues = form_id + "_" + field_name;

                var attributeItemHtml = `
                <div class="d-flex menu-item my-1 border-bottom attributeItems used{{used_in_tree}} attributeItems noSelect" >
                    <a href="#" class="attributeItems-title menu-link px-0 text-gray-800 text-hover-primary"  >
                        <span class="menu-title">
                            <input type="checkbox" class="attributesSelection ${form_id} ${field_name}" data-value="${field_value}" data-field="${field_name}" data-form="${form_id}" data-type="${data_type}" title="Select" name="selectedvalues">
                            <span>${field_value}</span>               
                        </span>
                           
                    </a>
                </div>`;
                $("#items_" + formidvalues).append(attributeItemHtml);

            }
            getSelectedAttributeFieldValues();
        }
    });
}

function generateSaveArray() {
    var formFieldMapping = [];
    var filtered = [];
    saveValuesArray = [];
    $('input[name="selectedvalues"]:checked').each(function () {
        let $elem = $(this);
        formFieldMapping.push({ "form_id": $elem.attr("data-form"), "field_name": $elem.attr("data-field"), "data_type": $elem.attr("data-type"), "field_value": $elem.attr("data-value") })
    })
    keys = ['form_id', 'field_name'],
        filtered = formFieldMapping.filter(
            (s => o =>
                (k => !s.has(k) && s.add(k))
                    (keys.map(k => o[k]).join('|'))
            )
                (new Set)
        );
    $.each(filtered, function (index, filter) {
        var field_values = '';
        $.each(formFieldMapping, function (index, FormFieldMap) {
            if (FormFieldMap.form_id == filter.form_id && FormFieldMap.field_name == filter.field_name) {
                if (field_values != '') {
                    field_values = field_values + '~' + FormFieldMap.field_value;
                } else {
                    field_values = FormFieldMap.field_value;
                }
            }
        })
        saveValuesArray.push({ "form_id": filter.form_id, "field_name": filter.field_name, "data_type": filter.data_type, "field_value": field_values });

    })
}

function saveGroupingData() {
    generateSaveArray();
    setSelectedAttributeFieldValues();
}

function setSelectedAttributeFieldValues() {
    $.ajax({
        type: "POST",
        url: "TreeConfig_v2.aspx/SetSelectedAttributeFieldValues",
        data: JSON.stringify({ groupCode: groupCode, data: saveValuesArray }),
        cache: false,
        async: true,
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (data) {
            if (data.d == "done") {
                parent.showAlertDialog("success", "Saved successfully.");
                window.location.reload();
            }
            else {
                parent.showAlertDialog("error", "Error in Employee grouping Save. Please try again.")
            }
        }
    });

}

function doSelection(elem, id) {
    let $elem = $(elem);
    if ($elem.prop('checked') == true) {
        $elem.attr("title", "Deselect all");
        attributesSelectAll(id);
    }
    else {
        $elem.attr("title", "Select all");
        attributesDeselectAll(id);
    }
}

function attributesSelectAll(id) {
    $("#" + id + " .attributeItems").each(function () {
        $(this).find('input[type=checkbox]:not(:checked)').prop("checked", true).trigger('change'); //code change by rakesh
    })
}

function attributesDeselectAll(id) {
    $("#" + id + " .attributeItems").each(function () {
        $(this).find('input[type=checkbox]:checked').prop("checked", false).trigger('change');//code change by rakesh
    })
}

function setResizableDivs() {
    var div_width = parseFloat($('#overalldiv').width() - 80) / 100;
    var minwidth = parseFloat(div_width) * 30;
    var maxwidth = parseFloat(div_width) * 70;

    $("#groupviewDiv").resizable({
        handles: "e, w",
        minWidth: minwidth,
        maxWidth: maxwidth,
    });

    $('#groupviewDiv').resize(function () {
        $('#attributediv').width($("#overalldiv").width() - $("#groupviewDiv").width() - 80);
    });
}

function getSelectedAttributes() {

    $.ajax({
        type: "POST",
        url: "TreeConfig_v2.aspx/GetSelectedAttributes",
        data: JSON.stringify({ groupCode: groupCode }),
        cache: false,
        async: true,
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (data) {
            try {
                data = dataConvert(data);
            }
            catch (ex) { }
            for (let i = 0, len = data.length; i < len; i++) {
                let groupingdata = data[i];
                let fieldcaption = groupingdata['fieldcaption'];
                let field_name = groupingdata['field_name'];
                let form_id = groupingdata['form_id'];
                let formidvalue = form_id + "_" + field_name;

                $("input[value=" + field_name + "]").prop('checked', true);
                var attributeNavItemHtml = `<li class="nav-item">
                <a class="nav-link" data-bs-toggle="tab" href="#${formidvalue}" data-form-id="${formidvalue}" data-form-name="${field_name}">
                    <span class="d-flex flex-center">
                         <span class="material-icons"></span>
                    </span>${fieldcaption}
                </a>
                 </li>`;
                $('#attributeTabNav').append(attributeNavItemHtml);

                var attributeNavContentHtml = `
    <div class="tab-pane fade" id="${formidvalue}">
        <div class="d-flex align-items-center mb-4 fs-4 AttributeAttrHeader">
            <input type="checkbox" class="attributesSelection changeall" title="Select all" id="attributesSelection_${formidvalue}" onchange="doSelection(this, '${formidvalue}');"/>
            ${fieldcaption}

            <div class="ms-auto">   
                <div class="d-flex">
                <div  class="d-none w-250px menu menu-sub menu-sub-dropdown menu-column attributeSearchTxt   show searchBoxChildContainer attributeSearch initialized" data-attribute=" ${fieldcaption}">
              </div></div>                 
            </div>
        </div>
        <div class="menu menu-column mt-4 attributeDataSection" id="items_${formidvalue}">
            
        </div>
    </div>`;
                $('#attributeTabContent').append(attributeNavContentHtml);

                // start code by Rakesh
                //     var groupHeaderHtml=` <div class="accordion-item" id="itm_${formidvalue}">
                //     <h2 class="accordion-header" id="header-${formidvalue}" >
                //       <button class="accordion-button" type="button" data-bs-toggle="collapse" data-bs-target="#${formidvalue}-collapse" aria-expanded="true" aria-controls="${formidvalue}-collapse" data-form-id="${formidvalue}" data-form-name="${field_name}">
                //       ${fieldcaption}
                //       </button>
                //     </h2>

                //   </div>`;
                //   $("#groupviewDiv #tree").append(groupHeaderHtml);
                // end code by Rakesh

            }

            $("#attributeTabNav .nav-link").eq(0).tab("show");

            getAttributeFieldValues();
        }

    });
}

function getAttributes() {
    $.ajax({
        type: "POST",
        url: "TreeConfig_v2.aspx/GetAttributes",
        cache: false,
        async: true,
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (data) {
            try {
                data = dataConvert(data);
            }
            catch (ex) { }

            for (let i = 0, len = data.length; i < len; i++) {
                let groupingdata = data[i];
                let fieldcaption = groupingdata['fieldcaption'];
                let field_name = groupingdata['field_name'];
                let form_id = groupingdata['form_id'];
                var groupdata = `<div class='col-3'>
                                    <div class="form-check">
                                    <input class="form-check-input" type="checkbox" value="${field_name}" name="groupingname" id="${form_id}.${field_name}">
                                    <label class="form-check-label"
                                        for="${form_id}.${field_name}">${fieldcaption}</label>
                                    </div>
                                    </div>`;
                $('#appendGroup').append(groupdata);
            }
            getSelectedAttributes();
        }
    });
}

function groupingSubmit() {

    var groupingData = [];
    $('input[name="groupingname"]:checked').each(function () {
        groupingData.push(this.id);
    });
    let data = groupingData.join(",");

    generateSaveArray();
    let tempSaveValuesArray = [];
    saveValuesArray.forEach(function (item) {
        let attribute = item.form_id + "." + item.field_name;
        if (groupingData.indexOf(attribute) > -1) {
            tempSaveValuesArray.push(item);
        }
    });

    $.ajax({
        type: "POST",
        url: "TreeConfig_v2.aspx/SetAttributes",
        data: JSON.stringify({ groupCode: groupCode, selectedAttributes: data, selectedValues: tempSaveValuesArray }),
        cache: false,
        async: true,
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (data) {
            try {
                data = dataConvert(data);
            }
            catch (e) { }

            if (data.d == "done") {
                parent.showAlertDialog("success", "Saved successfully.");
                window.location.reload();
            }
            else {
                parent.showAlertDialog("error", "Error in Employee grouping Save. Please try again.")
            }

        }
    });
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
                tempHtml = tempHtml.replaceAll("{{" + key + "}}", val);
            }
        }

        finalHtml += tempHtml;
        tempHtml = html;
    }

    return finalHtml;
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

function newGuid() {
    var dt = new Date().getTime();
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = (dt + Math.random() * 16) % 16 | 0;
        dt = Math.floor(dt / 16);
        return (c == 'x' ? r : (r & 0x2 | 0x4)).toString(16);
    });
}
