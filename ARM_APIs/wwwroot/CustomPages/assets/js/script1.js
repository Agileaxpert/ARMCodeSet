$(function(){
  $("#tree3").fancytree({
  rootVisible: false,
  checkbox: false,
  selectMode: 1,
  clickFolderMode: 4, 
  debugLevel: 0, 
    
  extensions: ["dnd"],
  dnd: {
    autoExpandMS: 400,
    focusOnClick: false,
    preventVoidMoves: true, // Prevent dropping nodes 'before self', etc.
    preventRecursiveMoves: true, // Prevent dropping nodes on own descendants
    dragStart: function(node, data) {
      return true;
    },
    dragEnter: function(node, data) {
       return true;
    },
    dragDrop: function(node, data) {
              if( !data.otherNode ){
                  // It's a non-tree draggable
                  var title = $('.drag').attr('data-title');
                  node.addNode({title: title}, data.hitMode);
                  return;
              }
              data.otherNode.moveTo(node, data.hitMode);
          }
  }
  });
      
  $( ".drag").draggable({
      revert: true, //"invalid",
        cursorAt: { top: -5, left: -5 },
        connectToFancytree: true,
  });
  
  $('#addnode').click(function(){
      AddNode();
  });
  
  $('.drag').blur(function(){
      AddNode();
  });
  
  function AddNode(){
        var rootNode = $("#tree").fancytree("getRootNode");
        var childNode = rootNode.addChildren({
          title: "Programatically addded nodes",
          tooltip: "This folder and all child nodes were added programmatically.",
          folder: true
        });
        childNode.addChildren({
          title: "Document using a custom icon",
          icon: "customdoc1.gif"
        });
  }
  
});