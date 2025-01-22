
$(function () {

  $("#div1").resizable();

  $("#div2").resizable();


  var sampleSource = [

    {
      title: "<p class='title1'>Quess Corporation(Company)</p>", "expanded": true, "children": [
        {
          title: "<i class='material-icons icone' >groups_2</i><p class='title'>Allsec Technologies(Entity)</p>", "children": [
            {
              title: "<i class='material-icons icone'>keyboard_double_arrow_down</i><p class='title'>Technology Vertical</p> ", "children": [
                { title: "<i class='material-icons icone' >lan</i><p class='title'>Software & Services(Department)</p>", },
                {
                  title: "<i class='material-icons icone'>lan</i><p class='title'>Infra & Hardware(Department)</p>", "children": [
                    { title: "<i class='material-icons icone'>meeting_room</i><p class='title'>Chennai</p>", }
                  ]
                },
              ]
            },
            {
              title: "<i class='material-icons icone'>keyboard_double_arrow_down</i><p class='title'>Services (Vertical)</p>", "children": [
                { title: "<i class='material-icons icone' >lan</i><p class='title'>Payroll</p>", },
                { title: "<i class='material-icons icone'>lan</i><p class='title'>HCM</p>", }
              ]
            },
            { title: "<i class='material-icons icone'>keyboard_double_arrow_down</i><p class='title'>Vertical 3</p>", }
          ]
        },

        { title: "<i class='material-icons icone'>groups_2</i><p class='title'>Entity 2</p>", },
        { title: "<i class='material-icons icone'>groups_2</i><p class='title'>Entity 3</p>", },
      ]
    },
  ];
  
  $("#tree").fancytree({
    extensions: ["dnd", "glyph", "filter"],

    //checkbox: true,
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
      mode: "hide"       // Grayout unmatched nodes (pass "hide" to remove unmatched node instead)
    },
    //  checkbox:"radio",
    // selectMode: 1,
    source: sampleSource,
    icon: false,




    edit: {
      triggerStart: ["clickActive", "dblclick", "f2", "mac+enter", "shift+click"],
      beforeEdit: function (event, data) {
      },
      edit: function (event, data) {
      },
      beforeClose: function (event, data) {
        console.log(event.type, event, data);
        if (data.originalEvent.type === "mousedown") {
        }
      },
    },
    dnd: {
      smartRevert: true,
      autoExpandMS: 400,
      focusOnClick: true,
      preventVoidMoves: true, // Prevent dropping nodes 'before self', etc.
      preventRecursiveMoves: true, // Prevent dropping nodes on own descendants
      dragStart: function (node, data) {
        return true;
      },
      dragEnter: function (node, data) {
        return true;
      },
      dragDrop: function (node, data) {
        data.otherNode.moveTo(node, data.hitMode);
      }
    },
    activate: function (event, data) {
    },
    click: function (event, data) {

    },


  });
});

/*---- Tree1 concepts ---*/

$("#tree1").fancytree({

  extensions: ["dnd", "glyph"],
  // checkbox: true,
  source: [


    { _class: 'Maslosoft.Koe.TreeItem', title: "<a class='btn btn-primary' data-bs-toggle='collapse' href='#collapseExample' role='button' aria-expanded='false' aria-controls='collapseExample'>Entity</a> &nbsp;&nbsp; <button class='btn btn-primary' type='button' data-bs-toggle='collapse' data-bs-target='#collapseExample1' aria-expanded='false' aria-controls='collapseExample1'>Vertical</button> &nbsp;&nbsp;<button class='btn btn-primary' type='button' data-bs-toggle='collapse' data-bs-target='#collapseExample1' aria-expanded='false' aria-controls='collapseExample1'>Department</button><div class='collapse' id='collapseExample'><div class='card card-body'><ul><li>test</li><li>test</li><li>test</li><li>test</li></ul></div></div><div class='collapse' id='collapseExample1'><div class='card card-body'><ul><li>test</li><li>test</li><li>test</li><li>test</li></ul></div>", },
  ],
  edit: {
    triggerStart: ["clickActive", "dblclick", "f2", "mac+enter", "shift+click"],
    beforeEdit: function (event, data) {
      // Return false to prevent edit mode
    },
    edit: function (event, data) {
      // Editor was opened (available as data.input)
    },
    beforeClose: function (event, data) {
      // Return false to prevent cancel/save (data.input is available)
      console.log(event.type, event, data);
      if (data.originalEvent.type === "mousedown") {

      }
    },
  },
  dnd: {
    smartRevert: true,
    autoExpandMS: 400,
    focusOnClick: true,
    preventVoidMoves: true, // Prevent dropping nodes 'before self', etc.
    preventRecursiveMoves: true, // Prevent dropping nodes on own descendants
    dragStart: function (node, data) {
      return true;
    },
    dragEnter: function (node, data) {
      return true;
    },
    dragDrop: function (node, data) {
      /** This function MUST be defined to enable dropping of items on
       *  the tree.
       */
      data.otherNode.moveTo(node, data.hitMode);
    }
  },
  activate: function (event, data) {
    //				alert("activate " + data.node);
  },
  click: function (event, data) {
    //  alert('Clicked');
  },
});

$("input[name=search]").keyup(function (e) {
  var n,
    tree = $.ui.fancytree.getTree(),
    args = "autoApply autoExpand fuzzy hideExpanders highlight leavesOnly nodata".split(" "),
    opts = {},
    filterFunc = $("#branchMode").is(":checked") ? tree.filterBranches : tree.filterNodes,
    match = $(this).val();

  $.each(args, function (i, o) {
    opts[o] = $("#" + o).is(":checked");
  });
  opts.mode = $("#hideMode").is(":checked") ? "hide" : "dimm";

  if (e && e.which === $.ui.keyCode.ESCAPE || $.trim(match) === "") {
    $("button#btnResetSearch").click();
    return;
  }
  if ($("#regex").is(":checked")) {
    // Pass function to perform match
    n = filterFunc.call(tree, function (node) {
      return new RegExp(match, "i").test(node.title);
    }, opts);
  } else {
    // Pass a string to perform case insensitive matching
    n = filterFunc.call(tree, match, opts);
  }
  $("button#btnResetSearch").attr("disabled", false);
  $("span#matches").text("(" + n + " matches)");
  $('.title').css({
    'color': 'red',
    'font-weight': 'bold',
  });
}).focus();

$("button#btnResetSearch").click(function (e) {
  $("input[name=search]").val("");
  $("span#matches").text("");
  tree.clearFilter();
}).attr("disabled", true);

$("fieldset input:checkbox").change(function (e) {
  var id = $(this).attr("id"),
    flag = $(this).is(":checked");

  // Some options can only be set with general filter options (not method args):
  switch (id) {
    case "counter":
    case "hideExpandedCounter":
      tree.options.filter[id] = flag;
      break;
  }
  tree.clearFilter();
  $("input[name=search]").keyup();
});



/*-- End of Tree1 Concepts --*/
function Deletenode() {

  var tree = $("#tree").fancytree("getTree"),
    node = tree.getActiveNode();

  searchIDs = tree.getSelectedNodes();

  searchIDs.forEach(function (node) {
    $children = node.children;
    if ($children !== null) node.parent.addChildren($children, node.getNextSibling());
    node.remove();
  });

}

$("#button1").click(function (event) {

  var tree = $("#tree").fancytree("getTree"),
    node = tree.getActiveNode();

  searchIDs = tree.getSelectedNodes();

  searchIDs.forEach(function (node) {
    $children = node.children;
    if ($children !== null) node.parent.addChildren($children, node.getNextSibling());
    node.remove();
  });
});

function Disabled() {

  $("#tree").fancytree("disable");
  $("#btnDisable").hide();
  $("#btnEnable").show();

}

function Enabled() {

  $("#tree").fancytree("enable");
  $("#btnEnable").hide();
  $("#btnDisable").show();

}

function addChildren() {

  var rootNode = $("#tree").fancytree("getRootNode");
  var childNode = rootNode.addChildren({
    title: "Programatically addded nodes",
    tooltip: "This folder and all child nodes were added programmatically.",
    folder: true
  });

}

function addChildnode() {

  var tree = $("#tree").fancytree("getTree"),
    node = tree.getActiveNode(),
    newData = { title: "New Entity" },
    newSibling = node.appendSibling(newData);

}
function Expand() {

  $("#tree").fancytree("getRootNode").visit(function (node) {
    node.setExpanded(true);
    $('#collapse').show();
    $('#expand').hide();
  });

}

function Collapse() {

  $("#tree").fancytree("getRootNode").visit(function (node) {
    node.setExpanded(false);
    $('#collapse').hide();
    $('#expand').show();
  });

}
function Entitydetails() {
  $('#office1').hide();
  $('#office').hide();
  $('#department').hide();
  $('#entity').show();
  $('#vertical').hide();

}
function Verticaldetails() {

  $('#office1').hide();
  $('#office').hide();
  $('#department').hide();
  $('#entity').hide();
  $('#vertical').show();
}
function Department() {

  $('#office1').hide();
  $('#office').hide();
  $('#department').show();
  $('#entity').hide();
  $('#vertical').hide();

}
function Office() {

  $('#office1').hide();
  $('#office').show();
  $('#department').hide();
  $('#entity').hide();
  $('#vertical').hide();

}


function Expandscreen() {

  $("#div2").hide();
  $(".col-sm-6").switchClass("col-sm-6", "col-sm-12");
  $("#tree").fancytree("getRootNode").visit(function (node) {
    node.setExpanded(true);
  });
  $('#collapse').show();
  $('#expand').hide();
  $("#expandall").hide();
  $("#collapseall").show();

  $(".icone").switchClass("icone", "icone1");

  $('#div1').css({
    'text-align': 'center',
  });

  $('.title').css({
    'margin-top': '-26px',
    'margin-left': '60px',

  });

}

function Collapsescreen() {

  $("#div2").show();
  $(".col-sm-12").switchClass("col-sm-12", "col-sm-6");
  $("#tree").fancytree("getRootNode").visit(function (node) {
    node.setExpanded(false);
  });
  $('#collapse').hide();
  $('#expand').show();
  $("#expandall").show();
  $("#collapseall").hide();
  $(".icone1").switchClass("icone1", "icone");
  $('#div1').css({
    'text-align': 'left',
  });

  $('.title').css({
    'margin-top': '-25px',
    'margin-left': '40px',

  });

}



