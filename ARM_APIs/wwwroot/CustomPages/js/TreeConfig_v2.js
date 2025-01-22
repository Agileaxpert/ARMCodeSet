var dimensionNavItemHtml = `
    <li class="nav-item">
        <a class="nav-link" onclick="attributesDeselectAll()"  data-bs-toggle="tab" href="#{{form_id}}" data-form-id="{{form_id}}" data-form-name="{{form_name}}">
            <span class="d-flex flex-center">
                 <span class="material-icons" style="color:{{dimension_color}}">{{dimension_icon}}</span>
            </span>{{display_name}}
		</a>
    </li>`;

var dimensionNavContentHtml = `
    <div class="tab-pane fade" id="{{form_id}}">
        <div class="d-flex align-items-center mb-4 fs-4 DimensionAttrHeader">
            <input type="checkbox" class="attributesSelection dimensionCheckBox" title="Select all" id="attributesSelection_{{form_id}}" onchange="doSelection(this, '{{form_id}}');"/>
            <div class="symbol symbol-50px symbol-circle me-4">
                <span class="symbol-label text-primary fw-boldest" style="background:{{dimension_color}}">
                    <span class="material-icons fs-2 text-white">{{dimension_icon}}</span>
                </span>
                    
            </div>
            {{display_name}}

            <div class=" ms-auto ">
               

                <div class="d-flex">
                <div  class="d-none w-250px menu menu-sub menu-sub-dropdown menu-column dimensionSearchTxt   show searchBoxChildContainer dimensionSearch initialized" data-dimension="{{form_name}}">
                  <div class="icon">
                      <span id="idsearch" class="material-icons material-icons-style position-absolute       ms-3">search
                      </span>
                      <input type="search" placeholder="Search records..." class="dimensionSearchTxtBox form-control form-control-flush ps-13" data-dimension="{{form_name}}" onblur="hideDimensionSearch(this);"">
                  </div>
              </div></div>                 
                <button href="#" class="btn btn-icon btn-icon-success dimensionOpen" data-dimension="{{form_name}}" data-form-id="{{form_id}}" data-bs-toggle="modal" data-bs-target="#dimensionModal"  onclick="openDimension(this);" title="Add">
                    <i class="fa fa-plus-circle fs-2"></i>
                </button>
                <button class="btn btn-icon dimensionSearchIcon" data-dimension="{{form_name}}" onclick="showDimensionSearch(this);" title="Search">
                    <i class="fa fa-search fs-2"></i>
                </button>
            </div>
        </div>
        <div class="menu menu-column mt-4 dimensionDataSection" id="items_{{form_id}}">
            
        </div>
    </div>`;

var dimensionItemHtml = `
    <div draggable="true" ondragstart="setDragData(event, this);" class="d-flex menu-item my-1 border-bottom dimensionItems freezed {{freezed}} used{{used_in_tree}} attributeItems noSelect" data-drag='{{row}}' data-sourceid="{{source_recordid}}" data-dimension="{{form_name}}" data-form-id="{{form_id}}" data-recordid="{{source_recordid}}" data-bs-toggle="modal" data-bs-target="#dimensionModal" >
        <a href="#" class="dimensionItems-title menu-link px-0 text-gray-800 text-hover-primary"  >
            <span class="menu-title">
            <input type="checkbox" class="attributesSelection" title="Select" data-sourceid="{{source_recordid}}" onchange="onSelectionChange(this);"/>
                <span>{{source_name}}</span>               
            </span>
        </a>
        <a href="#" data-sourceid="{{source_recordid}}" data-dimension="{{form_name}}" data-form-id="{{form_id}}" data-recordid="{{source_recordid}}" onclick="openDimension(this);" class="ms-4 Dimension-Atr-edit bg-light-info p-1 text-info openDimensions "  >
            <span title="Edit Attribute" class="material-icons ">edit</span>
        </a>
    </div>`;

var treeNodeHTML = `
    <div class='node-border nodeTitle toolTipTitle' title="" data-bs-toggle="tooltip" data-bs-placement="right" data-bs-dismiss="click" data-form-id="{{form_id}}" data-recordid="{{source_recordid}}" style='border-left:4px solid {{dimension_color}}; float: left;'>
        <i style='color:{{dimension_color}}' class='material-icons icone'>{{dimension_icon}}</i>
        <p class='title'><span class="node-short-code">{{display_code}}</span>{{source_name}}</p>
    </div>
    <div style="margin-left: auto;padding-top: 10px;">
        <i class="nodeDelete fa fa-remove d-none fs-2" title="Delete" onclick="removeNode(); return true;" style="display: inline;font-size: 1rem!important;padding: 15px;"></i>
        <i class="fa fa-users fs-2 viewEmployees" data-bs-toggle="tooltip" data-bs-placement="right" data-bs-original-title="View Employees" title="View Employees" data-recordid="{{id}}" onclick="openEmployeeReportFromTree(this); return true;" style="display: inline;font-size: 1rem!important;padding: 15px;"></i>
        <span class="empCount badge">{{employee_count}}</span>
    </div>
    `;

var rootNodeHTML = `
    <div class='node-border toolTipTitle' title="" data-bs-toggle="tooltip" data-bs-placement="right" data-bs-dismiss="click" data-form-id="{{form_id}}" data-recordid="{{source_recordid}}" style='border-left:4px solid #c63d4f; float: left;'>
        <b><p class='title1'>{{source_name}}</p></b>
    </div>
    <i class="fa fa-users fs-2 viewEmployees" data-recordid="{{id}}" onclick="openEmployeeReportFromTree(this); return true;" title="View Employees" style="margin-left: auto;float: right;display: inline;font-size: 1rem!important;padding: 15px;"></i>
    <span class="empCount badge">{{employee_count}}</span>`;

var dimensionProperties = {};
var treeDimensionWiseNodes = {};
var treeData = [];
var activeNode = {};
var deletedNodes = [];
var loadedDimensionData = [];
var saveDataGlobalArray = [];
var allowedParents = [];
var deletedRecords = [];
var selectedAttributesArray = [];

$(document).ready(function () {
    $("#btnPublish").addClass("disabled");
    $("#btnDiscard").addClass("disabled");

    $("#treeCaption").text(treeCaption);
    getTreeData(companyCode, hierarchyCode);    
    getDimensionProps(companyCode, hierarchyCode);

    bindDimensionSearch();
    setResizableDivs();
});

function doSelection(elem, id) {
    let $elem = $(elem);
    if ($elem.prop('checked') == true) {
        $elem.attr("title", "Deselect all");
        attributesSelectAll(id);
    }
    else {
        $elem.attr("title", "Select all");
        attributesDeselectAll();
    }
}

function attributesSelectAll(id) {
    selectedAttributesArray = [];
    $("#" + id + " .attributeItems").each(function () {
        if ($(this).is('.freezed.NO')) {
            $(this).addClass('selected');
            $(this).find('input[type=checkbox]').prop("checked", true);
            selectedAttributesArray.push($(this).attr('data-drag'));
        }
    })
}

function attributesDeselectAll() {
    selectedAttributesArray = [];
    var elems = document.getElementsByClassName("attributeItems");

    [].forEach.call(elems, function (el) {
        el.classList.remove("selected");
        $(el).find('input[type=checkbox]').prop("checked", false);
    });
}


var clearSelectionElem = document.getElementById('dimensionTabNav');
document.addEventListener('click', function (event) {
    var isClickInsideElement = clearSelectionElem.contains(event.target);
    if (isClickInsideElement) {
        $(".dimensionCheckBox").removeClass("dimensionCheckBox");
        attributesDeselectAll();
    }
});

function onDimensionMouseUp() {
    if (this.classList.contains('selected')) {
        if (event.target.classList.contains('attributesSelection')) {
            return;
        }
        else {
            $(this).find('input[type=checkbox]').prop('checked', false).trigger('change');
        }
    } else {
        if (event.target.classList.contains('attributesSelection')) {
            return;
        }
        else {
            $(this).find('input[type=checkbox]').prop('checked', true).trigger('change');
        }
    }
    event.stopPropagation();
    event.preventDefault();
    //return;

    //if (event.ctrlKey == false) {
    //    if (this.classList.contains('selected')) {
    //        if (document.getElementsByClassName("attributeItems selected").length == 1) {
    //            attributesDeselectAll();
    //        }
    //        else {
    //            attributesDeselectAll();
    //            this.classList.add('selected');
    //            selectedAttributesArray.push(this.getAttribute('data-drag'));
    //        }
    //    }
    //    else {
    //        attributesDeselectAll();
    //        this.classList.add('selected');
    //        selectedAttributesArray.push(this.getAttribute('data-drag'));
    //    }
    //} else {
    //    if (this.classList.contains('selected')) {
    //        this.classList.remove('selected');
    //        $(this).find('input[type=checkbox]').prop("checked", false);
    //        selectedAttributesArray = selectedAttributesArray.filter(function (el) {
    //            return el != this.getAttribute('data-drag');
    //        });
    //    } else {
    //        this.classList.add('selected');
    //        $(this).find('input[type=checkbox]').prop("checked", true);
    //        selectedAttributesArray.push(this.getAttribute('data-drag'));
    //    }
    //}
}

function onSelectionChange(elem) {    
    let $item = $(elem).parents('.attributeItems');
    if ($(elem).prop("checked")) {        
        $item.addClass('selected');
        selectedAttributesArray.push($item.attr('data-drag'));
    } else {
        $item.removeClass('selected');
        selectedAttributesArray = selectedAttributesArray.filter(function (el) {
            return el != $item.attr('data-drag');
        });
    }
    event.stopPropagation();
    event.preventDefault();
}

function bindSelectionEvents(event, elem) {
    var attributeItems = document.getElementsByClassName("attributeItems");
    for (var i = 0; i < attributeItems.length; i++) {
        attributeItems[i].removeEventListener("mouseup", onDimensionMouseUp, true);
        attributeItems[i].addEventListener("mouseup", onDimensionMouseUp, true);
    }
}

function setResizableDivs() {
    var div_width = parseFloat($('#overalldiv').width() - 80) / 100;
    var minwidth = parseFloat(div_width) * 30;
    var maxwidth = parseFloat(div_width) * 70;

    $("#corporatediv").resizable({
        handles: "e, w",
        minWidth: minwidth,
        maxWidth: maxwidth,
    });

    $('#corporatediv').resize(function () {
        $('#dimesiondiv').width($("#overalldiv").width() - $("#corporatediv").width() - 80);
    });
}

function bindDimensionSearch() {
    $("input[id=treeSearch]").on('change keyup', function (e) {
        var n,
            tree = $.ui.fancytree.getTree(),
            args = "autoApply autoExpand fuzzy hideExpanders highlight leavesOnly nodata".split(" "),
            opts = {},
            filterFunc = tree.filterBranches,
            match = $(this).val();

        expandTree();

        $.each(args, function (i, o) {
            opts[o] = $("#" + o).is(":checked");
        });
        opts.mode = $("#hideMode").is(":checked") ? "hide" : "dimm";

        if (e && e.which === $.ui.keyCode.ESCAPE || $.trim(match) === "") {
            tree.clearFilter();
            return;
        }
        n = filterFunc.call(tree, match, opts);
    }).focus();
}

function getTreeData(companyCode, hierarchyCode) {
    $.ajax({
        type: "POST",
        url: "TreeConfig_v2.aspx/GetTreeData",
        cache: false,
        async: true,
        contentType: "application/json;charset=utf-8",
        data: JSON.stringify({ companyCode: companyCode, hierarchyCode: hierarchyCode }),
        dataType: "json",
        success: function (data) {
            try {
                data = dataConvert(data);
            }
            catch (ex) { }

            if (typeof data[0]["is_published"] != "undefined" && data[0]["is_published"] == "F") {
                $("#btnPublish").removeClass("disabled");
                $("#btnDiscard").removeClass("disabled");
            }

            for (var i = 0; i < data.length; i++) {
                let treeNode = getTreeNode(data[i]);//new TreeNode(data[i]);
                if (typeof treeDimensionWiseNodes[treeNode.parentId] == "undefined") {
                    treeDimensionWiseNodes[treeNode.parentId] = [];
                }
                treeDimensionWiseNodes[treeNode.parentId].push(treeNode);
            }

            treeData.push(getChildren("0", 0)); //To get the root node.

            treeData[0].children = [];
            getChildNodes(treeData[0]);
            constructTree(treeData);
            expandTree();
            resetNodeProperties();

        }
    });
}

function getChildren(id, index) {
    return JSON.parse(JSON.stringify(treeDimensionWiseNodes[id][index]));
}

function saveTreeData() {

    if (saveDataGlobalArray.length == 0) {
        parent.showAlertDialog("warning", "No changes made.");
        return;
    }

    $.ajax({
        type: "POST",
        url: "TreeConfig_v2.aspx/SaveTree",
        cache: false,
        async: true,
        contentType: "application/json;charset=utf-8",
        data: JSON.stringify({ hierarchyCode: hierarchyCode, companyCode: companyCode, saveJson: saveDataGlobalArray }),
        dataType: "json",
        success: function (data) {
            ShowDimmer(false);
            if (data.d == "done") {
                saveDataGlobalArray = [];
                parent.showAlertDialog("success", "Saved successfully.");
                reloadTreePage();

                $("#btnPublish").removeClass("disabled");
                $("#btnDiscard").removeClass("disabled");
            }
            else {
                parent.showAlertDialog("error", "Error in " + treeCaption + " Save. Please try again.")
            }

        }
    });
}

function getChildNodes(parentNode) {
    if (typeof treeDimensionWiseNodes[parentNode.id] != "undefined") {
        for (var i = 0; i < treeDimensionWiseNodes[parentNode.id].length; i++) {
            parentNode.children.push(getChildren(parentNode.id, i));
        }
    }
    for (var i = 0; i < parentNode.children.length; i++) {
        var childNode = parentNode.children[i];
        childNode.children = [];
        getChildNodes(childNode)
    }
}

function getDimensionData(formId, formName) {
    if (loadedDimensionData.indexOf(formId) > -1)
        return;

    $.ajax({
        type: "POST",
        url: "TreeConfig_v2.aspx/GetDimensionsData",
        cache: false,
        async: true,
        contentType: "application/json;charset=utf-8",
        data: JSON.stringify({ formName: formName, companyCode: companyCode, hierarchyCode: hierarchyCode }),
        dataType: "json",
        success: function (data) {
            try {
                data = dataConvert(data);
            }
            catch (ex) { }
            $("#dimensionTabContent #items_" + formId).html("");
            $("#dimensionTabContent #items_" + formId).append(generateDynamicHtml(data, dimensionItemHtml));
            bindSelectionEvents();
            loadedDimensionData.push(formId);
            deletedRecords.forEach(function (source_recordid) {
                let props = getProps(formId)[0];
                if (props.freeze_after_selection == "YES") {
                    $('[data-sourceid=' + source_recordid + ']').toggleClass('YES NO');
                }
            });
        }
    });
}

function getDimensionProps(companyCode, hierarchyCode) {
    $.ajax({
        type: "POST",
        url: "TreeConfig_v2.aspx/GetDimensionsProperties",
        cache: false,
        async: true,
        contentType: "application/json;charset=utf-8",
        data: JSON.stringify({ companyCode: companyCode, hierarchyCode: hierarchyCode }),
        dataType: "json",
        success: function (data) {
            try {
                data = dataConvert(data);
            }
            catch (ex) { }
            dimensionProperties = data;

            $("#dimensionTabNav").append(generateDynamicHtml(data, dimensionNavItemHtml));
            $("#dimensionTabContent").append(generateDynamicHtml(data, dimensionNavContentHtml));

            bindEvents({ parent: $("#dimensionTabNav"), selector: ".nav-link" });

            loadDimension();

            $(".dimensionSearchTxtBox").off("change keyup").on('change keyup ', function () {
                let dimension = $(this).attr("data-dimension");
                let searchTxt = $(this).val().toLowerCase().trim();
                if (searchTxt == "") {
                    $(".dimensionItems[data-dimension='" + dimension + "']").removeClass('d-none');
                }
                else {
                    $(".dimensionItems[data-dimension='" + dimension + "']").each(function () {
                        if ($(this).text().toLowerCase().indexOf(searchTxt) > -1) {
                            $(this).removeClass('d-none');
                        }
                        else {
                            $(this).addClass('d-none');
                        }
                    })
                }
            });
        }
    });
}

function loadDimension() {
    if (targetDimension == "0") {
        $("#dimensionTabNav .nav-link").eq(0).addClass('active');
        $("#dimensionTabContent .tab-pane").eq(0).addClass('active').addClass('show');
        $("#dimensionTabNav .nav-link").eq(0).click();
    }
    else {
        $("#dimensionTabNav .nav-link[data-form-id='" + targetDimension + "']").addClass('active');
        $("#dimensionTabContent .tab-pane[id='" + targetDimension + "']").addClass('active').addClass('show');
        $("#dimensionTabNav .nav-link[data-form-id='" + targetDimension + "']").click();
    }
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

function bindEvents(bindOptions) {
    let $parent = undefined;
    let selector = undefined;

    if (typeof bindOptions != "undefined") {
        $parent = bindOptions.parent;
        selector = bindOptions.selector;
    }

    if (typeof $parent == "undefined") {
        $parent = $(document); //Bind events to existing fields on pageload.
    }

    if (typeof selector == "undefined" || selector.indexOf(".nav-link") > -1) {
        $parent.find(selector).off("click").on("click", function () {
            let formId = $(this).attr("data-form-id");
            let formName = $(this).attr("data-form-name");
            targetDimension = formId;
            getDimensionData(formId, formName);
        });
    }
}

function constructTree(treeSource) {
    $("#tree").fancytree({
        extensions: ["dnd5", "glyph", "filter"],
        filter: {
            //selectMode: 3,
            autoApply: true,   // Re-apply last filter if lazy data is loaded
            autoExpand: true, // Expand all branches that contain matches while filtered
            counter: true,     // Show a badge with number of matching child nodes near parent icons
            fuzzy: false,      // Match single characters in order, e.g. 'fb' will match 'FooBar'
            hideExpandedCounter: true,  // Hide counter badge if parent is expanded
            hideExpanders: true,       // Hide expanders if all child nodes are hidden by filter
            highlight: true,   // Highlight matches by wrapping inside <mark> tags
            leavesOnly: false, // Match end nodes only
            nodata: true,      // Display a 'no data' status node if result is empty
            mode: "hide"      // Grayout unmatched nodes (pass "hide" to remove unmatched node instead)
        },
        selectMode: 1,
        source: treeSource,
        icon: false,
        click: function (event, data) {
            attributesDeselectAll();
            activeNode = data.node;
            try {
                if ($(event.originalEvent.target).hasClass('viewEmployees') || $(event.originalEvent.target).hasClass('nodeDelete') || $(event.originalEvent.target).hasClass('nodeTitle'))
                    return false;
            }
            catch (e) {
            }
            return true;
        },

        dnd5: {
            smartRevert: true,
            autoExpandMS: 200,
            focusOnClick: true,
            preventVoidMoves: true, // Prevent dropping nodes 'before self', etc.
            preventRecursiveMoves: true, // Prevent dropping nodes on own descendants
            effectAllowed: "all",
            dropEffectDefault: "move",
            // --- Drag-support:
            dragStart: function (node, data) {
                let form_id = data.node.data.rowData.form_id;
                allowedParents = getProps(form_id)[0].allowed_parents.split(',');
                attributesDeselectAll();
                return true;
            },

            dragEnter: function (node, data) {
                let parent = node.data.rowData.form_name;
                if (allowedParents.indexOf(parent) > -1) {
                    return true;
                }
                else {
                    return false;
                }
            },
            dragOver: function (node, data) {
                data.dropEffect = data.dropEffectSuggested;
            },
            dragDrop: function (node, data) {
                let level = node.getLevel();
                if (level == 0) {
                    parent.showAlertDialog("warning", "Can't move to root node");
                    return false;
                }
                var newNode,
                    transfer = data.dataTransfer,
                    sourceNodes = data.otherNodeList,
                    mode = data.dropEffect;

                data.originalEvent.preventDefault();

                if (data.otherNode && selectedAttributesArray.length == 0) {
                    if (isValidDrop(data.otherNode, node)) {
                        if (data.hitMode == 'over') {
                            var sameTree = data.otherNode.tree === data.tree;
                            data.otherNode.moveTo(node, data.hitMode);
                            addDimensionNode(data.otherNode, node, "u");
                        }
                        else if (mode === "move") {
                            data.otherNode.moveTo(node, data.hitMode);
                            addDimensionNode(data.otherNode, node, "u");
                        }
                    }

                } else if (data.otherNodeData) {
                    if (isValidDrop(data.otherNode, node)) {
                        node.addChild(data.otherNodeData, data.hitMode);
                    }
                } else {
                    if (level == 1 && data.hitMode == "after")
                        return;

                    if (data.hitMode == "over" || data.hitMode == "after") {
                        selectedAttributesArray.forEach(function (item, idx) {
                            let dragDataObj = JSON.parse(item);
                            tempsourceid = dragDataObj.source_recordid;
                            dragDataObj.dimension_type = dragDataObj.dimension_name;
                            dragDataObj.employee_count = "0";
                            var dragData = [];
                            dragData.push(dragDataObj);

                            let nodeHtml = generateDynamicHtml(dragData, treeNodeHTML);
                            let props = getProps(dragDataObj.form_id);
                            nodeHtml = generateDynamicHtml(props, nodeHtml);
                            var parentId = node.data.rowData.id;
                            dragDataObj.id = newGuid();
                            dragDataObj.parentId = parentId;
                            dragDataObj.action = 'i';

                            var newNode = {
                                title: nodeHtml,
                                rowData: dragDataObj,
                                id: dragDataObj.id,
                                parentId: dragDataObj.parentId,
                                action: "i"
                            }

                            if (isValidDrop(newNode, node)) {
                                if (props[0].freeze_after_selection == "YES") {
                                    $('[data-sourceid=' + tempsourceid + ']').toggleClass('YES NO');
                                }

                                $('[data-sourceid=' + tempsourceid + ']').addClass('usedYES');

                                attributesDeselectAll();
                                $(".attributesSelection").prop("checked", false);
                                node.addNode(newNode, data.hitMode);
                                addDimensionNode(newNode, node, "i");
                            }
                        });
                    }


                }
                node.setExpanded();
                resetNodeProperties();
            },
        }
    });
}

function setDragData(event, elem) {
    let $elem = $(elem);
    let dragData = JSON.parse($elem.attr("data-drag"));
    allowedParents = getProps(dragData.form_id)[0].allowed_parents.split(',');
}

function getProps(formId) {
    return dimensionProperties.filter(function (el) {
        return el.form_id == formId;
    });
}

function getTreeNode(rowData) {
    return {
        title: getNodeTitle(rowData),
        rowData: rowData,
        expanded: false,
        id: rowData.id,
        parentId: (rowData.parentid ? rowData.parentid.toString() : "0")
    }
}

function getNodeTitle(rowData) {
    let tempData = [];
    tempData.push(rowData);

    if (rowData.parentid == "0") {
        return generateDynamicHtml(tempData, rootNodeHTML)
    }
    else {
        return generateDynamicHtml(tempData, treeNodeHTML);
    }
}

function expandTree() {
    $("#tree").fancytree("getRootNode").visit(function (node) {
        node.setExpanded(true);
        $('#collapseTree').show();
        $('#expandTree').hide();
    });

}

function collapseTree() {
    $("#tree").fancytree("getRootNode").visit(function (node) {
        node.setExpanded(false);
        $('#collapseTree').hide();
        $('#expandTree').show();
    });

}

function maximizeTree() {
    $("#dimesiondiv").hide();
    setTimeout(function () {
        $('#dimesiondiv').fadeOut();
    }, 100);

    $("#corporatediv, #dimesiondiv").css("width", "").switchClass("col-xl-6", "col-xl-12").switchClass("col-md-6", "col-md-12");

    $("#maximizeTree").hide();
    $("#minimizeTree").show();
}

function minimizeTree() {
    $("#corporatediv, #dimesiondiv").css("width", "").switchClass("col-xl-12", "col-xl-6").switchClass("col-md-12", "col-md-6");
    setTimeout(function () {
        $('#dimesiondiv').fadeIn();
    }, 100);

    $("#maximizeTree").show();
    $("#minimizeTree").hide();
}

//function getAllLeafNodes() {
//    var tree = $.ui.fancytree.getTree();
//    var leafNodes = [];
//    tree.visit(function (node) {
//        if (!node.hasChildren()) {
//            leafNodes.push(node);
//        }
//    });
//    return leafNodes;
//}

function resetNodeProperties() {
    var tree = $.ui.fancytree.getTree();
    tree.visit(function (node) {
        var title = node.title.toString().replace("d-none", "").replace("fa-remove", "fa-remove d-none");

        if (!node.hasChildren() && (typeof node.data.rowData.employee_count == "undefined" || node.data.rowData.employee_count == "0")) {
            title = title.toString().replace("d-none", "");
        }

        var $nodeTitle = $("<div>" + title + "</div>");
        if (node.children != null && node.children.length) {
            $nodeTitle.find(".toolTipTitle").attr("title", "Child Attributes : " + node.children.length);
        }
        else {
            $nodeTitle.find(".toolTipTitle").attr("title", "");
        }
        title = $nodeTitle.html();
        node.setTitle(title);
    });

    $(".toolTipTitle").tooltip();
}

function isNodeAvailable(source_recordid) {
    var tree = $.ui.fancytree.getTree();
    var result = false;
    tree.visit(function (node) {
        if (typeof node.data.rowData.source_recordid != "undefined" && node.data.rowData.source_recordid == source_recordid) {
            result = true;
        }
    });
    return result;
}

function removeNode() {
    var rowData = activeNode.data.rowData;
    rowData["id"] = activeNode.data.id;
    rowData["parentid"] = activeNode.data.parentId;
    rowData["action"] = activeNode.data.action;

    if (typeof rowData["action"] != "undefined" && rowData["action"] == "i") {
        saveDataGlobalArray = saveDataGlobalArray.filter(function (el) {
            return el.id != rowData.id;
        });
    }
    else {
        rowData["action"] = "d";
        saveDataGlobalArray.push(rowData);
    }

    if (saveDataGlobalArray.length > 0) {
        $("#btnPublish").addClass("disabled");
        $("#btnDiscard").addClass("disabled");
    }

    let props = getProps(rowData.form_id)[0];
    if (props.freeze_after_selection == "YES") {
        $('[data-sourceid=' + rowData.source_recordid + ']').toggleClass('YES NO');
    }    

    deletedRecords.push(rowData.source_recordid);
    activeNode.remove();
    resetNodeProperties();

    if (!isNodeAvailable(rowData.source_recordid)) {
        $('[data-sourceid=' + rowData.source_recordid + ']').removeClass('usedYES');
    }
}

function openDimensionPopup(elem) {
    var div_width = parseFloat($('#dimesiondiv').width());
    var actualwidth = parseInt(div_width);
    $(".modal-dialog").css({
        'position': 'fixed',
        'margin': 'auto',
        'width': actualwidth,
        'height': '50%',
        'right': '10%',
        'top': '12%',
    });

    let transId = $(elem).attr("data-form-id");
    let url = "../../aspx/tstruct.aspx?transid=" + transId;
    let recordId = $(elem).attr("data-recordid");
    if (typeof recordId != "undefined")
        url += "&act=load&recordid=" + recordId;

    $("#rightIframe").attr("src", url);
}

function openDimension(elem) {
    ShowDimmer(true);
    setTimeout(function () {
        ShowDimmer(false);
    }, 2000);

    let $dimensionCard = $("#dimensionCard");
    let $rightIframe = $("#rightIframe");

    let transId = $(elem).attr("data-form-id");
    let url = "../../aspx/tstruct.aspx?transid=" + transId;
    let recordId = $(elem).attr("data-recordid");
    if (typeof recordId != "undefined")
        url += "&act=load&recordid=" + recordId;
    $dimensionCard.addClass("d-none");
    $rightIframe.attr("src", "");
    $rightIframe.attr("src", url);
    $rightIframe.removeClass("d-none");
    $("#dimensionCaption").html("<i class='fa fa-arrow-left btn-text-primary'></i>&nbsp;&nbsp;Back to Attributes")
}

function openDimensionFromTree(transId, recordId) {
    ShowDimmer(true);
    setTimeout(function () {
        ShowDimmer(false);
    }, 2000);
    let $dimensionCard = $("#dimensionCard");
    let $rightIframe = $("#rightIframe");

    let url = "../../aspx/tstruct.aspx?transid=" + transId;
    if (typeof recordId != "undefined")
        url += "&act=load&recordid=" + recordId;
    $dimensionCard.addClass("d-none");
    $rightIframe.attr("src", "");
    $rightIframe.attr("src", url);
    $rightIframe.removeClass("d-none");
    $("#dimensionCaption").html("<i class='fa fa-arrow-left btn-text-primary'></i>&nbsp;&nbsp;Back to Attributes")
}

function openEmployeeReportFromTree(elem) {
    ShowDimmer(true);
    setTimeout(function () {
        ShowDimmer(false);
    }, 2000);
    let $dimensionCard = $("#dimensionCard");
    let $rightIframe = $("#rightIframe");

    let url = "../../aspx/ivtoivload.aspx?ivname=dimcodev";
    let recordId = $(elem).attr("data-recordid");
    if (typeof recordId != "undefined")
        url += "&precordid=" + recordId;
    $dimensionCard.addClass("d-none");
    $rightIframe.attr("src", "");
    $rightIframe.attr("src", url);
    $rightIframe.removeClass("d-none");
    $("#dimensionCaption").html("<i class='fa fa-arrow-left btn-text-primary'></i>&nbsp;&nbsp;Back to Attributes")
}

function closeDimension() {
    let $dimensionCard = $("#dimensionCard");
    let $rightIframe = $("#rightIframe");
    if ($dimensionCard.hasClass("d-none")) {
        $rightIframe.addClass("d-none");
        $dimensionCard.removeClass("d-none");
        $rightIframe.removeAttr("src");
        let idx = loadedDimensionData.indexOf(targetDimension);
        loadedDimensionData.splice(idx, 1);
        loadDimension();
    }

    $("#dimensionCaption").html("Attributes");
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

function showDimensionSearch(elem) {
    let dimension = $(elem).attr("data-dimension");
    $(".searchBoxChildContainer.dimensionSearch[data-dimension='" + dimension + "']").removeClass("d-none");
    $("input[data-dimension='" + dimension + "']").focus();
    $(".dimensionOpen[data-dimension='" + dimension + "']").hide();
    $(".dimensionSearchIcon[data-dimension='" + dimension + "']").hide();
}

function hideDimensionSearch(elem) {
    let dimension = $(elem).attr("data-dimension");
    $(".searchBoxChildContainer.dimensionSearch[data-dimension='" + dimension + "']").addClass("d-none");
    $(".dimensionOpen[data-dimension='" + dimension + "']").show();
    $(".dimensionSearchIcon[data-dimension='" + dimension + "']").show();
}

function addDimensionNode(node, parentNode, action) {
    let treeSaveArray = [];

    let rowData = getRowDataObj(node, parentNode);
    treeSaveArray.push(rowData);
    if (typeof rowData["action"] == 'undefined')
        rowData["action"] = action;

    saveDataGlobalArray = saveDataGlobalArray.filter(function (el) {
        return el.id != rowData.id;
    });

    saveDataGlobalArray.push(rowData);
    deletedRecords.splice(deletedRecords.indexOf(rowData.source_id), 1);

    if (saveDataGlobalArray.length > 0) {
        $("#btnPublish").addClass("disabled");
        $("#btnDiscard").addClass("disabled");
    }
}

function getRowDataObj(node, parentNode) {
    var rowData = {};
    var nodeData = (node.data ? node.data.rowData : node.rowData);
    var parentNodeData = (parentNode.data ? parentNode.data.rowData : parentNode.rowData);

    var nodeId = (nodeData.id);
    var parentId = (parentNodeData.id);

    rowData["hierarchy_code"] = nodeData.hierarchy_code;
    rowData["company_code"] = nodeData.company_code;
    rowData["form_name"] = nodeData.form_name;
    rowData["form_id"] = nodeData.form_id;
    rowData["source_recordid"] = nodeData.source_recordid;
    rowData["source_code"] = nodeData.source_code;
    rowData["parent_form"] = parentNodeData.form_name || '';
    rowData["parentid"] = parentId || '';
    rowData["recordid"] = nodeId || '0';
    rowData["id"] = nodeId || '0';
    rowData["action"] = nodeData.action;
    return rowData;
}

function publishTreeData() {
    ShowDimmer(true);

    $.ajax({
        type: "POST",
        url: "TreeConfig_v2.aspx/PublishTreeChanges",
        cache: false,
        async: true,
        contentType: "application/json;charset=utf-8",
        data: JSON.stringify({ companyCode: companyCode, hierarchyCode: hierarchyCode }),
        dataType: "json",
        success: function (data) {
            ShowDimmer(false);
            if (data.d == "success") {
                parent.showAlertDialog("success", "Published successfully.");
                reloadTreePage();

                $("#btnPublish").addClass("disabled");
                $("#btnDiscard").addClass("disabled");
            }
            else {
                parent.showAlertDialog("error", "Error: " + data.d);
            }

        }
    });
}

function discardTreeData() {
    ShowDimmer(true);

    $.ajax({
        type: "POST",
        url: "TreeConfig_v2.aspx/DiscardTreeChanges",
        cache: false,
        async: true,
        contentType: "application/json;charset=utf-8",
        data: JSON.stringify({ companyCode: companyCode, hierarchyCode: hierarchyCode }),
        dataType: "json",
        success: function (data) {
            ShowDimmer(false);
            if (data.d == "success") {
                parent.showAlertDialog("success", "Changes discarded.");
                reloadTreePage();
            }
            else {
                parent.showAlertDialog("error", "Error: " + data.d);
            }

        }
    });
}

function isValidDrop(currentNode, parentNode) {
    let isValid = true;
    let parentNodeData = (parentNode.data ? parentNode.data.rowData : parentNode.rowData);
    let parentRecId = parentNodeData.source_recordid;
    //let parentDimensions = getParentAttributes(parentNode);
    let nodeData = (currentNode.data ? currentNode.data.rowData : currentNode.rowData);
    //if (parentDimensions.indexOf(nodeData.source_recordid) > -1) {
    if(parentRecId == nodeData.source_recordid) {
        parent.showAlertDialog("warning", "Same attribute is already added as a  parent to this node.");
        isValid = false;
    }

    let tempSiblings = [];
    if (parentNode.children != null && (typeof parentNode.children != "undefined" && parentNode.children.length > 0)) {
        parentNode.children.forEach(function (item) {
            let nodeData = (item.data ? item.data.rowData : item.rowData);
            tempSiblings.push(nodeData.source_recordid);
        });

        let currentSourceId = nodeData.source_recordid;

        if (tempSiblings.indexOf(currentSourceId) > -1) {
            parent.showAlertDialog("warning", "Same attribute is already added to this node.");
            isValid = false;
        }
    }

    return isValid;

}

function getParentAttributes(node) {
    let parents = [];
    let tempNode = node;
    while (tempNode.getLevel() > 0) {
        let nodeData = (tempNode.data ? tempNode.data.rowData : tempNode.rowData);
        parents.push(nodeData.source_recordid);
        tempNode = tempNode.parent;
    }
    return parents;
}

function reloadTreePage() {
    window.location.href = 'TreeConfig_v2.aspx?cc=' + companyCode + '&hc=' + hierarchyCode + '&caption=' + treeCaption + '&target=' + targetDimension;
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
