$(document).ready(function () {
    getTreeData(companyCode, hierarchyCode);
    bindDimensionSearch();
});

var treeDimensionWiseNodes = {};
var treeData = [];
var activeTreeNode = {};
var treeNodeHTML, rootNodeHTML, treeCaption;

treeNodeHTML = `
    <div class='node-border w-100 selection_allow {{selection_allow}}' data-form-id="{{form_id}}" data-recordid="{{source_recordid}}" style='border-left:4px solid {{dimension_color}}; '>
        <i style='color:{{dimension_color}}' class='material-icons icone'>{{dimension_icon}}</i>
        <p class='title'><span class="node-short-code">{{display_code}}</span>{{source_name}}</p>
    </div>`;

rootNodeHTML = `
    <div class='node-border w-100 selection_allow headerNode  {{selection_allow}}' data-form-id="{{form_id}}" data-recordid="{{source_recordid}}" style='border-left:4px solid #c63d4f; float: left;'>
        <b><p class='title1'>{{source_name}}</p></b>
    </div>`;

function getTreeData(companyCode, hierarchyCode) {
    $.ajax({
        type: "POST",
        url: "../aspx/TreeConfig_v2.aspx/GetTreeCoreData",
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
                let treeNode = getTreeNode(data[i]);
                if (typeof treeDimensionWiseNodes[treeNode.parentId] == "undefined") {
                    treeDimensionWiseNodes[treeNode.parentId] = [];
                }
                treeDimensionWiseNodes[treeNode.parentId].push(treeNode);
            }

            treeData.push(getChildren("0", 0)); //To get the root node.
            treeData[0].children = [];
            getChildNodes(treeData[0]);
            constructTree(treeData);
            bindSelectedNode();
        }
    });
}

function getChildren(id, index) {
    return JSON.parse(JSON.stringify(treeDimensionWiseNodes[id][index]));
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

var removedItemsArr = [];

function constructTree(treeSource) {
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
        selectMode: 2,
        source: treeSource,
        icon: false,
        checkbox: function (event, data) {
            return $(data.node.title).hasClass("selection_allow YES") ? true : false;
        },
        glyph: {
            preset: "material",
            // map:{}
            map: {
                _addClass: "material-icons",
                checkbox: { text: "check_box_outline_blank" },
                checkboxSelected: { text: "check_box" },
                checkboxUnknown: { text: "indeterminate_check_box" },
                dragHelper: { text: "play_arrow" },
                dropMarker: { text: "arrow-forward" },
                error: { text: "warning" },
                expanderClosed: { text: "chevron_right" },
                expanderLazy: { text: "last_page" },
                expanderOpen: { text: "expand_more" },
                loading: { text: "autorenew", addClass: "fancytree-helper-spin" },
                nodata: { text: "info" },
                noExpander: { text: "" },
                radio: { text: "radio_button_unchecked" },
                radioSelected: { text: "radio_button_checked" },
                // Default node icons.
                // (Use tree.options.icon callback to define custom icons based on node data)
                doc: { text: "web_asset" },
                docOpen: { text: "web_asset" },
                folder: { text: "folder" },
                folderOpen: { text: "folder_open" }
            }
        },
        click: function (event, data) {
            try {

                if (data.targetType == "checkbox") {
                    //check if any node is alredy selected,if yes then check if the current node belongs to same parent or not.

                    if (data.node.selected) {
                        removedItemsArr = removedItemsArr.filter(function (item) {
                            return item !== data.node.data.fill_field + "=";
                        })
                        removedItemsArr.push(data.node.data.fill_field + "=");                        
                        return true;
                    }
                    else {
                        let selectednodes = $.ui.fancytree.getTree("#tree").getSelectedNodes();
                        if (selectednodes.length) {
                            // if (!$(selectednodes[selectednodes.length - 1].parent.li).is($(data.node.parent.li))) {
                            //     return false;
                            // }
                            // else {
                                let displayCodeArr = [];
                                selectednodes.forEach(function (item) {
                                    displayCodeArr.push(item.data.displayCode);
                                })

                                if (displayCodeArr.indexOf(data.node.data.displayCode) > -1)
                                    return false;
                                else {
                                    removedItemsArr = removedItemsArr.filter(function (item) {
                                        return item !== data.node.data.fill_field + "=";
                                    })
                                    return true;
                                }
                            }
                        // }
                        else {
                            removedItemsArr = removedItemsArr.filter(function (item) {
                                return item !== data.node.data.fill_field + "=";
                            })
                            return true;
                        }
                    }

                }

            }
            catch (e) {
            }
        }

    });
}

function getParentAttributes(node) {
    let tempNode = node;
    while (tempNode.getLevel() > 0) {
        let nodeData = tempNode.data;
        //let tempArr = [];
        //tempArr.push(nodeData.fill_field);
        //tempArr.push(nodeData.fill_value);
        selectionArray.push(nodeData.fill_field + "=" + nodeData.fill_value);
        tempNode = tempNode.parent;
    }
}

function getTreeNode(rowData) {
    return {
        title: getNodeTitle(rowData),
        expanded: false,
        id: rowData.id,
        parentId: (rowData.parentid ? rowData.parentid.toString() : "0"),
        displayCode: rowData.display_code,
        dimensionCode: rowData.dimension_code,
        allowSelection: (rowData.selection_allow == "YES" ? true : false),
        fill_field: rowData.fill_field,
        fill_value: rowData.fill_value
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

function bindSelectedNode() {
    let isSelected = false;
    let valArr = parent.GetFieldValue(parent.$treeTextField.attr("id")).split(",");
    $("#tree").fancytree("getRootNode").visit(function (node) {

        if (valArr.indexOf(node.data.dimensionCode) > -1) {
            isSelected = true;
            node.setSelected(true);
            //node.addClass("selected");
            var nodeArr = node.getParentList(true, true);
            nodeArr.forEach(function (item) {
                item.setExpanded(true);
            });
        }
    });

    if (!isSelected) {
        $.ui.fancytree.getTree("#tree").expandAll(true);
    }

    //let val = parent.$treeTextField.val();
    //let SelectedNodes = $.ui.fancytree.getTree("#tree").findAll(function (node) {

    //    if (val.indexOf(node.data.dimensionCode) > -1) {
    //        node.setSelected(true);
    //        node.setActive(true); //it will expand all the parents of selected node
    //        //node.addClass("selected");
    //        return true;
    //    }
    //});
    //SelectedNodes.length == 0 &&  $.ui.fancytree.getTree("#tree").expandAll(true);
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

function maximizeTree() {
    parent.$(".customPopupModal").find(".modal-dialog").toggleClass("modal-md modal-lg");
    $("#maximizeTree").hide();
    $("#minimizeTree").show();
}

function minimizeTree() {
    parent.$(".customPopupModal").find(".modal-dialog").toggleClass("modal-md modal-lg");
    $("#maximizeTree").show();
    $("#minimizeTree").hide();
}

var selectionArray = [];
function setAttributes() {
    let dimensionCode = "";
    selectionArray = [];
    $("#tree").fancytree("getRootNode").visit(function (node) {

        if (node.selected) {
            if (dimensionCode == "")
                dimensionCode = node.data.dimensionCode;
            else
                dimensionCode = dimensionCode + "," + node.data.dimensionCode;

            getParentAttributes(node);

        }
    });

    let fldId = parent.$treeTextField.attr("id");
    let fldTreePicker = fldId.replace(parent.GetFieldsName(fldId), parent.GetFieldsName(fldId) + "_treepicker");

    if (selectionArray.length == 0) {
        if (parent.$("#" + fldTreePicker).length) {
            parent.SetFieldValue(fldTreePicker, "F");
            parent.UpdateFieldArray(fldTreePicker, "0", "F");

        }
    }
    else {
        if (parent.$("#" + fldTreePicker).length) {
            parent.SetFieldValue(fldTreePicker, "T");
            parent.UpdateFieldArray(fldTreePicker, "0", "T");
        }
    }

    parent.treeSelectedAttributes = [];
    selectionArray = removedItemsArr.concat(selectionArray); 
    parent.treeSelectedAttributes = [...new Set(selectionArray)]
    parent.bindSelectedAttributes();


    //if (parent.FMoe[parent.GetFieldIndex(parent.GetFieldsName(fldId))] == "Select")
    //    parent.UpdateFieldArray(fldId, "0", dimensionCode, "parent", "AutoComplete");

    //if (dimensionCode == "")
    //    parent.AxOldValue = " ";
    //else
    //    parent.AxOldValue = dimensionCode;

    if (dimensionCode == "") {
        let axpTreeViewFld = "axpview_" + fldId;
        //parent.SetFieldValue(fldId, "");

        parent.$('ul[data-id="' + axpTreeViewFld + '"].TreeBreadcrumb').remove();

    }

    parent.$treeTextField.find('option').remove()
    parent.SetFieldValue(fldId, dimensionCode);
    parent.UpdateFieldArray(fldId, "0", dimensionCode);
    parent.MainBlur(parent.$treeTextField);

    parent.$("div[data-iframe-id='" + parent.$('iframe[id^="iFrame"]').attr("id") + "']").modal("hide");
}
