var dimensionNavItemHtml = `
    <li class="nav-item">
        <a class="nav-link" data-bs-toggle="tab" href="#{{form_id}}" data-form-id="{{form_id}}" data-form-name="{{form_name}}">
            <span class="d-flex flex-center">
                 <span class="material-icons" style="color:{{dimension_color}}">{{dimension_icon}}</span>
            </span>{{display_name}}
		</a>
    </li>`;

var dimensionNavContentHtml = `
    <div class="tab-pane fade" id="{{form_id}}">
        <div class="d-flex align-items-center mb-4 fs-4">

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
              
                <button href="#" class="btn btn-icon btn-icon-success dimensionOpen" data-dimension="{{form_name}}" data-form-id="{{form_id}}" data-bs-toggle="modal" data-bs-target="#dimensionModal"  onclick="openDimension(this);">
                    <i class="fa fa-plus-circle fs-2"></i>
                </button>
                <button class="btn btn-icon dimensionSearchIcon" data-dimension="{{form_name}}" onclick="showDimensionSearch(this);">
                    <i class="fa fa-search fs-2"></i>
                </button>
            </div>
        </div>
        <div class="menu menu-column mt-4 px-6 dimensionDataSection" id="items_{{form_id}}">
            
        </div>
    </div>`;

var dimensionItemHtml = `
    <div class="menu-item my-1 border-bottom dimensionItems freezed {{freezed}}" data-dimension="{{form_name}}" data-form-id="{{form_id}}" data-recordid="{{source_recordid}}" data-bs-toggle="modal" data-bs-target="#dimensionModal" onclick="openDimension(this);">
        <a href="#" class="menu-link px-0 text-gray-800 text-hover-primary">
            <span class="menu-title"><i class="fa fa-circle fs-10 me-4"></i><span draggable="true" ondragstart="setDragData(event, this);" data-drag='{{row}}'>{{source_name}}</span></span>
        </a>
    </div>`;

var treeNodeHTML = `
    <div class='node-border' onclick="openDimension(this);" data-form-id="{{form_id}}" data-recordid="{{source_recordid}}" style='border-left:4px solid {{dimension_color}}; float: left;'>
        <i style='color:{{dimension_color}}' class='material-icons icone'>{{dimension_icon}}</i>
        <p class='title'><span class="node-short-code">{{display_code}}</span>{{source_name}}</p>
    </div>
    <div style="margin-left: auto;padding-top: 10px;">
        <i class="nodeDelete fa fa-remove d-none fs-2" onclick="removeNode(); return true;" style="display: inline;font-size: 1rem!important;padding: 15px;"></i>
        <i class="fa fa-users fs-2 viewEmployees" data-bs-toggle="tooltip" data-bs-placement="right" data-bs-original-title="View Employees" data-recordid="{{global_config_intermediate_treecoreid}}" onclick="openEmployeeReportFromTree(this); return true;" style="display: inline;font-size: 1rem!important;padding: 15px;"></i>
    </div>
    `;

var rootNodeHTML = `
    <div class='node-border' onclick="openDimension(this);" data-form-id="{{form_id}}" data-recordid="{{source_recordid}}" style='border-left:4px solid #c63d4f; float: left;'>
        <b><p class='title1'>{{source_name}}</p></b>
    </div>
    <i class="fa fa-users fs-2 viewEmployees" data-recordid="{{global_config_intermediate_treecoreid}}" onclick="openEmployeeReportFromTree(this); return true;" style="margin-left: auto;float: right;display: inline;font-size: 1rem!important;padding: 15px;"></i>`;

var dimensionProperties = {};
var treeDimensionWiseNodes = {};
var treeData = [];
var activeNode = {};
var deletedNodes = [];
var loadedDimensionData = []

$(document).ready(function () {
    $("#treeCaption").text(treeCaption);
    getTreeData(companyCode, hierarchyCode);
    getDimensionProps(companyCode, hierarchyCode);

    bindDimensionSearch();
    setResizableDivs();
});

function setResizableDivs() {
    var div_width = parseFloat($('#overalldiv').width() - 80) / 100;
    var minwidth = parseFloat(div_width) * 30;
    var maxwidth = parseFloat(div_width) * 70;

    $("#corporatediv").resizable({
        handles: "e, w",
        //  autoHide: true,
        minWidth: minwidth,
        maxWidth: maxwidth,
    });

    $('#corporatediv').resize(function () {
        //console.log($("#overalldiv").width() + "||" + $("#corporatediv").width() + "||" + ($("#overalldiv").width() - $("#corporatediv").width() - 80).toString());
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
        //if ($("#regex").is(":checked")) {
        //    // Pass function to perform match
        //    n = filterFunc.call(tree, function (node) {
        //        return new RegExp(match, "i").test(node.title);
        //    }, opts);
        //} else {

            // Pass a string to perform case insensitive matching
        n = filterFunc.call(tree, match, opts);
        //}
        //$("button#btnResetSearch").attr("disabled", false);
        //$("span#matches").text("(" + n + " matches)");
        //$('.title').css({
        //    'color': 'red',
        //    'font-weight': 'bold',
        //});
    }).focus();
}

function getTreeData(companyCode, hierarchyCode) {
    $.ajax({
        type: "POST",
        url: "TreeConfig.aspx/GetTreeData",
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

            for (var i = 0; i < data.length; i++) {
                let treeNode = getTreeNode(data[i]);//new TreeNode(data[i]);
                if (typeof treeDimensionWiseNodes[treeNode.parentId] == "undefined") {
                    treeDimensionWiseNodes[treeNode.parentId] = [];
                }
                treeDimensionWiseNodes[treeNode.parentId].push(treeNode);
            }

            treeData.push(getChildren("0", 0)); //To get the root node.
            //for (var i = 0; i < treeData.length; i++) {
            //    treeData[i].children.push(getChildNodes(treeData[i]));
            //}
            treeData[0].children = [];
            getChildNodes(treeData[0]);
            //treeData[0].children.push(getChildNodes(treeData[0]));
            constructTree(treeData);
            expandTree();
            resetRemovableNodes();
        }
    });
}


function getChildren(id, index) {
    return JSON.parse(JSON.stringify(treeDimensionWiseNodes[id][index]));
}


function saveTreeData(treeSaveArray, deletedNodes) {
    //getTreeSaveData();
    //let treeSaveArray = getTreeSaveArray();
    ////console.log(treeSaveArray);
    ////console.log(deletedNodes);

    //return;
    $.ajax({
        type: "POST",
        url: "TreeConfig.aspx/SaveTree",
        cache: false,
        async: true,
        contentType: "application/json;charset=utf-8",
        data: JSON.stringify({ treeData: treeSaveArray, deletedNodes: deletedNodes }),
        dataType: "json",
        success: function (data) {
            ShowDimmer(false);
            if (data.d == "success") {
                //parent.showAlertDialog("success", "Corporate Treeview Saved successfully.");
                reloadTreePage();
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
        //childNode.children.push(getChildNodes(childNode));
        getChildNodes(childNode)

        //if (childNode.children == []) {
        //    delete childNode.children;
        //}
    }

    //return parentNode;//JSON.parse(JSON.stringify(childNodes));
}


function getDimensionData(formId, formName) {
    if (loadedDimensionData.indexOf(formId) > -1)
        return;

    $.ajax({
        type: "POST",
        url: "TreeConfig.aspx/GetDimensionsData",
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

            loadedDimensionData.push(formId);
        }
    });
}

function getDimensionProps(companyCode, hierarchyCode) {
    $.ajax({
        type: "POST",
        url: "TreeConfig.aspx/GetDimensionsProperties",
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
        $parent.find(selector).click(function () {
            let formId = $(this).attr("data-form-id");
            let formName = $(this).attr("data-form-name");
            targetDimension = formId;
            getDimensionData(formId, formName);
        });
    }
}

function constructTree(treeSource) {
    //var sampleSource = [
    //    {
    //        title: "<div class='node-border' style='border-left:4px solid #c63d4f; '><p  class='title1'>Quess Corporation(Company)</p></div>", "expanded": true, "children": [
    //            {
    //                title: "<div class='node-border' style='border-left:4px solid #4f96f7; '><i style='color:#4f96f7' class='material-icons icone' >groups_2</i><p class='title'>Allsec Technologies(Entity)</p></div>", "children": [
    //                    {
    //                        title: "<div class='node-border' style='border-left:4px solid #e95a92; '><i style='color:#e95a92' class='material-icons icone'>keyboard_double_arrow_down</i><p class='title'>Technology Vertical</p> </div>", "children": [
    //                            { title: "<div class='node-border' style='border-left:4px solid #f98036; '><i style='color:#f98036'  class='material-icons icone' >lan</i><p class='title'>Software & Services(Department)</p></div>", },
    //                            {
    //                                title: "<div class='node-border' style='border-left:4px solid #f98036; '><i  style='color:#f98036' class='material-icons icone'>lan</i><p class='title'>Infra & Hardware(Department)</p></div>", "children": [
    //                                    { title: "<div class='node-border' style='border-left:4px solid #15c2a3; '><i style='color:#15c2a3' class='material-icons icone'>meeting_room</i><p class='title'>Chennai</p></div>", }
    //                                ]
    //                            },
    //                        ]
    //                    },
    //                    {
    //                        title: "<div class='node-border' style='border-left:4px solid #e95a92; '><i style='color:#e95a92' class='material-icons icone'>keyboard_double_arrow_down</i><p class='title'>Services (Vertical)</p></div>", "children": [
    //                            { title: "<div class='node-border' style='border-left:4px solid #f98036; '><i style='color:#f98036' class='material-icons icone' >lan</i><p class='title'>Payroll</p></div>", },
    //                            { title: "<div class='node-border' style='border-left:4px solid #f98036; '><i style='color:#f98036' class='material-icons icone'>lan</i><p class='title'>HCM</p></div>", }
    //                        ]
    //                    },
    //                    { title: "<div class='node-border' style='border-left:4px solid #e95a92; '><i style='color:#e95a92' class='material-icons icone'>keyboard_double_arrow_down</i><p class='title'>Vertical 3</p></div>", }
    //                ]
    //            },

    //            { title: "<div class='node-border' style='border-left:4px solid #4f96f7; '><i style='color:#4f96f7' class='material-icons icone'>groups_2</i><p class='title'>Entity 2</p></div>", },
    //            { title: "<div class='node-border' style='border-left:4px solid #4f96f7; '><i style='color:#4f96f7' class='material-icons icone'>groups_2</i><p class='title'>Entity 3</p></div>", },
    //        ]
    //    },
    //];

    $("#tree").fancytree({
        extensions: ["dnd5", "glyph", "filter"],
        filter: {
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
        //activate: function (event, data) {
        //    $("#statusLine").text("Active node: " + data.node + ". attribute : " + data.node.data.attribute);
        //},
        click: function (event, data) {
            //debugger;
            //console.log(event.target);
            ////console.log(data.node);
            //var nodeData = (data.node.data ? data.node.data.rowData : data.node.rowData);
            //openDimensionFromTree(nodeData.form_id, nodeData.source_recordid);
            activeNode = data.node;
            try {
                if ($(event.originalEvent.target).hasClass('viewEmployees') || $(event.originalEvent.target).hasClass('nodeDelete'))
                    return false;
            }
            catch (e) {
            }
            return true;
        },
        //edit: {
        //    triggerStart: ["clickActive", "dblclick", "f2", "mac+enter", "shift+click"],
        //    beforeEdit: function (event, data) {
        //    },
        //    edit: function (event, data) {
        //    },
        //    beforeClose: function (event, data) {
        //        //console.log(event.type, event, data);
        //        if (data.originalEvent.type === "mousedown") {
        //        }
        //    },
        //},
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

                data.effectAllowed = "all";
                data.dropEffect = data.dropEffectSuggested;
                return true;
            },
            // dragEnd: function(node, data) {
            //   node.warn( "T2: dragEnd: " + "data: " + data.dropEffect + "/" + data.effectAllowed +
            //     ", dataTransfer: " + data.dataTransfer.dropEffect + "/" + data.dataTransfer.effectAllowed, data );
            //     parent.showAlertDialog("T2: dragEnd")
            // },
            // --- Drop-support:
            dragEnter: function (node, data) {
                ////console.log("Enter: " + node.getLevel() + " " +  data.hitMode);
                return true;
            },
            dragOver: function (node, data) {
                //console.log("Over: " + node.getLevel() + " " +  data.hitMode);
                data.dropEffect = data.dropEffectSuggested;
            },
            dragDrop: function (node, data) {
                //console.log("Drop: " + node.getLevel() + " " +  data.hitMode);
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


                //if (data.hitMode === "after") {
                //    // If node are inserted directly after tagrget node one-by-one,
                //    // this would reverse them. So we compensate:
                //    sourceNodes.reverse();
                //}
                //  parent.showAlertDialog(data.otherNode);
                if (data.otherNode) {
                    if (isValidDrop(data.otherNode, node)) {
                        // parent.showAlertDialog(data.hitMode);
                        if (data.hitMode == 'over') {
                            var sameTree = data.otherNode.tree === data.tree;
                            data.otherNode.moveTo(node, data.hitMode);
                            addDimensionNode(data.otherNode, node);
                        }
                        else if (mode === "move") {
                            data.otherNode.moveTo(node, data.hitMode);
                            addDimensionNode(data.otherNode, node);
                        }
                    }
                    // parent.showAlertDialog("test1");
                    // Drop another Fancytree node from same frame
                    // (maybe from another tree however)                    


                    //else {
                    //    newNode = data.otherNode.copyTo(node, data.hitMode);
                    //    if (mode === "link") {
                    //        newNode.setTitle("Link to " + newNode.title);
                    //    } else {
                    //        newNode.setTitle("Copy of " + newNode.title);
                    //    }
                    //}
                } else if (data.otherNodeData) {                    
                    // Drop Fancytree node from different frame or window, so we only have
                    // JSON representation available
                    if (isValidDrop(data.otherNode, node)) {
                        node.addChild(data.otherNodeData, data.hitMode);
                    }
                } else {
                    if (data.hitMode == "over" || data.hitMode == "after") {
                        // Drop a non-node
                        let dragDataObj = JSON.parse(transfer.getData("text"));
                        dragDataObj.dimension_type = dragDataObj.dimension_name;
                        var dragData = [];
                        dragData.push(dragDataObj);
                        let nodeHtml = generateDynamicHtml(dragData, treeNodeHTML);
                        let props = getProps(dragDataObj.form_name);
                        nodeHtml = generateDynamicHtml(props, nodeHtml);
                        var parentId = node.data.id;
                        var newNode = {
                            title: nodeHtml,
                            rowData: dragDataObj,
                            id: "0",
                            parentId: parentId
                        }
                        if (isValidDrop(newNode, node)) {
                            node.addNode(newNode, data.hitMode);
                            addDimensionNode(newNode, node);
                        }                        
                    }


                }
                node.setExpanded();
                resetRemovableNodes();
                //e.Cancel;
            },
        }
    });
}

function setDragData(event, elem) {
    let $elem = $(elem);
    let dragData = $elem.attr("data-drag");
    event.dataTransfer.setData('text/plain', dragData);
}

function getProps(formName) {
    return dimensionProperties.filter(function (el) {
        return el.form_name == formName
    });
}

function getTreeNode(rowData) {
    return {
        title: getNodeTitle(rowData),
        rowData: rowData,
        expanded: false,
        id: rowData.global_config_intermediate_treecoreid,
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

var saveDataArray = [];
function getTreeSaveData() {
    var tree = $("#tree").fancytree("getTree");
    var saveData = tree.toDict(true);
    saveDataArray = [];
    for (var i = 0; i < saveData.children.length; i++) {
        getSaveChildNodes(saveData.children[i]);
    }
}

function getSaveChildData(childNode) {
    saveDataArray.push(JSON.parse(JSON.stringify(childNode.data)) || undefined);
}

function getSaveChildNodes(parentNode) {

    getSaveChildData(parentNode);
    if (typeof parentNode.children != "undefined") {
        for (var i = 0; i < parentNode.children.length; i++) {
            var childNode = parentNode.children[i];
            getSaveChildNodes(childNode);

        }
    }


    //getSaveChildren(parentNode.children);
    //for (var i = 0; i < parentNode.children.length; i++) {
    //    var childNode = parentNode.children[i];
    //    getSaveChildren(childNode.children);
    //}
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

function getAllLeafNodes() {
    var tree = $.ui.fancytree.getTree();
    var leafNodes = [];
    tree.visit(function (node) {
        if (!node.hasChildren()) {
            leafNodes.push(node);
        }
    });

    return leafNodes;
}

function resetRemovableNodes() {
    var tree = $.ui.fancytree.getTree();
    tree.visit(function (node) {
        var title = node.title.toString().replace("d-none", "").replace("fa-remove", "fa-remove d-none");

        if (!node.hasChildren()) {
            title = title.toString().replace("d-none", "");
        }

        node.setTitle(title);
    });
    //var leafNodes = getAllLeafNodes();
}

function removeNode() {
    //let activeNode = $("#tree").fancytree("getTree").getActiveNode();
    //deletedNodes.push(activeNode.data.id);
    var nodeId = activeNode.data.id;
    activeNode.remove();
    resetRemovableNodes();
    removeDimensionNode(nodeId);
}

function getTreeSaveArray() {
    let treeSaveArray = [];
    for (var i = 0; i < saveDataArray.length; i++) {
        var dimensionData = saveDataArray[i].dimensionData;
        var rowData = {};
        rowData["hierarchy_code"] = dimensionData.hierarchy_code;
        rowData["company_code"] = dimensionData.company_code;
        rowData["form_name"] = dimensionData.form_name;
        rowData["form_id"] = dimensionData.form_id;
        rowData["source_recordid"] = dimensionData.source_recordid;
        rowData["source_code"] = dimensionData.source_code;
        rowData["parent_form"] = saveDataArray[i].parentForm || '';
        rowData["parentid"] = saveDataArray[i].parentId || '0';
        rowData["parent_groupcode"] = saveDataArray[i].parentGroupCode || '';
        rowData["parent_groupcode"] = (rowData["parent_groupcode"].toString().startsWith('-') ? rowData["parent_groupcode"].toString().substr(1) : rowData["parent_groupcode"]);
        rowData["recordid"] = dimensionData.global_config_treecoreid || '0';
        treeSaveArray.push(rowData);
    }
    return treeSaveArray;
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

    $("#dimensionIframe").attr("src", url);
}

function openDimension(elem) {
    ShowDimmer(true);
    setTimeout(function () {
        ShowDimmer(false);
    }, 2000);

    let $dimensionCard = $("#dimensionCard");
    let $dimensionIframe = $("#dimensionIframe");

    let transId = $(elem).attr("data-form-id");
    let url = "../../aspx/tstruct.aspx?transid=" + transId;
    let recordId = $(elem).attr("data-recordid");
    if (typeof recordId != "undefined")
        url += "&act=load&recordid=" + recordId;
    $dimensionCard.addClass("d-none");
    $dimensionIframe.attr("src", "");
    $dimensionIframe.attr("src", url);
    $dimensionIframe.removeClass("d-none");
    $("#dimensionCaption").html("<i class='fa fa-arrow-left btn-text-primary'></i>&nbsp;Dimensions")
}

function openDimensionFromTree(transId, recordId) {
    ShowDimmer(true);
    setTimeout(function () {
        ShowDimmer(false);
    }, 2000);
    let $dimensionCard = $("#dimensionCard");
    let $dimensionIframe = $("#dimensionIframe");

    let url = "../../aspx/tstruct.aspx?transid=" + transId;
    if (typeof recordId != "undefined")
        url += "&act=load&recordid=" + recordId;
    $dimensionCard.addClass("d-none");
    $dimensionIframe.attr("src", "");
    $dimensionIframe.attr("src", url);
    $dimensionIframe.removeClass("d-none");
    $("#dimensionCaption").html("<i class='fa fa-arrow-left btn-text-primary'></i>&nbsp;Dimensions")
}

function openEmployeeReportFromTree(elem) {
    ShowDimmer(true);
    setTimeout(function () {
        ShowDimmer(false);
    }, 2000);
    let $dimensionCard = $("#dimensionCard");
    let $dimensionIframe = $("#dimensionIframe");

    let url = "../../aspx/ivtoivload.aspx?ivname=dimcodev";
    let recordId = $(elem).attr("data-recordid");
    if (typeof recordId != "undefined")
        url += "&precordid=" + recordId;
    $dimensionCard.addClass("d-none");
    $dimensionIframe.attr("src", "");
    $dimensionIframe.attr("src", url);
    $dimensionIframe.removeClass("d-none");
    $("#dimensionCaption").html("<i class='fa fa-arrow-left btn-text-primary'></i>&nbsp;Dimensions")
}

function closeDimension() {
    let $dimensionCard = $("#dimensionCard");
    let $dimensionIframe = $("#dimensionIframe");
    if ($dimensionCard.hasClass("d-none")) {
        $dimensionIframe.addClass("d-none");
        $dimensionCard.removeClass("d-none");
        $dimensionIframe.removeAttr("src");
        let idx = loadedDimensionData.indexOf(targetDimension);
        loadedDimensionData.splice(idx, 1);
        loadDimension();
    }

    $("#dimensionCaption").html("Dimensions");
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

function addDimensionNode(node, parentNode) {
    let treeSaveArray = [];
    let deletedNodeArrray = [];
    let rowData = getSaveDataObj(node, parentNode);
    treeSaveArray.push(rowData);
    ShowDimmer(true);
    //console.log(treeSaveArray[0]);
    saveTreeData(treeSaveArray, deletedNodeArrray)
}

function removeDimensionNode(nodeId) {
    let treeSaveArray = [];
    let deletedNodeArrray = [];
    deletedNodeArrray.push(nodeId);
    ShowDimmer(true);
    //console.log(deletedNodeArrray[0]);
    saveTreeData(treeSaveArray, deletedNodeArrray)
}

function getSaveDataObj(node, parentNode) {
    var rowData = {};
    var nodeData = (node.data ? node.data.rowData : node.rowData);
    var parentNodeData = (parentNode.data ? parentNode.data.rowData : parentNode.rowData);

    var nodeId = (node.data ? node.data.id : node.id);
    var parentId = (parentNode.data ? parentNode.data.id : parentNode.id);

    rowData["hierarchy_code"] = nodeData.hierarchy_code;
    rowData["company_code"] = nodeData.company_code;
    rowData["form_name"] = nodeData.form_name;
    rowData["form_id"] = nodeData.form_id;
    rowData["source_recordid"] = nodeData.source_recordid;
    rowData["source_code"] = nodeData.source_code;
    rowData["parent_form"] = parentNodeData.form_name || '';
    rowData["parentid"] = parentId|| '0';
    rowData["recordid"] = nodeId || '0';
    return rowData;
}

function publishTreeData() {
    ShowDimmer(true);

    $.ajax({
        type: "POST",
        url: "TreeConfig.aspx/PublishTreeChanges",
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
        url: "TreeConfig.aspx/DiscardTreeChanges",
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
    let parentDimensions = getParentForms(parentNode);
    let nodeData = (currentNode.data ? currentNode.data.rowData : currentNode.rowData);
    if (parentDimensions.indexOf(nodeData.form_id) > -1){
        parent.showAlertDialog("warning", "Same dimension is already a parent to this node.");
        isValid = false;
    }

    return isValid;

}

function getParentForms(node) {
    let parents = [];
    let tempNode = node;
    while (tempNode.getLevel() > 0) {
        let nodeData = (tempNode.data ? tempNode.data.rowData : tempNode.rowData);
        parents.push(nodeData.form_id);
        tempNode = tempNode.parent;
    }
    return parents;
}

function reloadTreePage() {
    window.location.href = 'TreeConfig.aspx?cc=' + companyCode + '&hc=' + hierarchyCode + '&caption=' + treeCaption + '&target=' + targetDimension;
}

function loadFrame() {
    parent.$.LoadingOverlay("show");
}

function closeFrame() {
    parent.$.LoadingOverlay("hide", true);
}