<%@ Page Language="C#" AutoEventWireup="true" CodeFile="TreePicker.aspx.cs" Inherits="CustomPages_aspx_TreePicker" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta charset="utf-8" />
    <title></title>
  
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <!--begin::Fonts-->
    <link rel="stylesheet" href="../assets/css/all.min.css" />
    <!--end::Fonts-->
    <!--begin::Global Stylesheets Bundle(used by all pages)-->
    <link href="../assets/plugins/global/plugins.bundle.css" rel="stylesheet" type="text/css" />
    <link href="../assets/css/style.bundle.css" rel="stylesheet" type="text/css" />
    <!-- <link rel="stylesheet" href="../assets/css/ui.fancytree.min.css" />
    <link href="../../Css/thirdparty/bootstrap/3.3.6/bootstrap.min.css" rel="stylesheet" />-->
    <link rel="stylesheet" href="../assets/css/material-icons.css" /> 
    <link href="../../Css/thirdparty/font-awesome/4.6.3/css/font-awesome.min.css" rel="stylesheet" />
    <link href="../../ThirdParty/fancytree-master/src/skin-material/ui.fancytree.css" rel="stylesheet">
    <link rel="stylesheet" href="../assets/css/jquery-ui.css" />
    <link rel="stylesheet" href="../../newpopups/remodal/remodal.css" />
    <link rel="stylesheet" href="../../newpopups/axpertpopup.css" />
    <link rel="stylesheet" href="../css/TreePicker.css?v=1.2" type="text/css" />

</head>
<body>

    <form runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server">
        </asp:ScriptManager>
    </form>

    <div class="d-flex pe-6 my-1 ms-auto Dept-modal-Toolbar">
        <div class="d-flex ms-auto">
            <div data-popper-placement="bottom-end" data-kt-menu="true" class="w-250px menu menu-sub menu-sub-dropdown menu-column   show searchBoxChildContainer treeSearch initialized d-none">
                <div class="icon">
                    <span id="idsearch" class="material-icons material-icons-style position-absolute       ms-3">
                        search
                    </span>
                    <input type="search" id="treeSearch" placeholder="Search records..." class="form-control form-control-flush ps-13" onblur="$('.searchBoxChildContainer.treeSearch').addClass('d-none');">
                </div>
            </div>
        </div>
        <a href="#" data-bs-toggle="tooltip" data-bs-placement="bottom" data-bs-dismiss="click" data-bs-original-title="Search" onclick="$('.searchBoxChildContainer.treeSearch').removeClass('d-none');$('#treeSearch').focus();"
           class="btn btn-round btn-icon btn-white btn-color-gray-500 btn-active-primary shadow-sm me-2 tb-btn btn-sm ">

            <i class="fa fa-search btn-text-primary"></i>
            <!--end::Svg Icon-->

        </a>

        <a href="#" id="expandTree" data-bs-toggle="tooltip" data-bs-placement="bottom" data-bs-dismiss="click" data-bs-original-title="Expand All" onclick="expandTree();"
           class="btn btn-round btn-icon btn-white btn-color-gray-500 btn-active-primary shadow-sm me-2 tb-btn btn-sm ">

            <i class="fa fa-sort-amount-down btn-text-primary"></i>
            <!--end::Svg Icon-->

        </a>

        <a href="#" id="collapseTree" data-bs-toggle="tooltip" data-bs-placement="bottom" data-bs-dismiss="click" data-bs-original-title="Collapse All" onclick="collapseTree();" style="display: none;"
           class="btn btn-round btn-icon btn-white btn-color-gray-500 btn-active-primary shadow-sm me-2 tb-btn btn-sm  ">

            <i class="fa fa-sort-amount-up btn-text-primary"></i>
            <!--end::Svg Icon-->

        </a>

        <button data-bs-toggle="tooltip" data-bs-placement="bottom" data-bs-dismiss="click" data-bs-original-title="Maximize Tree" class="btn btn-round btn-icon btn-white btn-color-gray-500 btn-active-primary shadow-sm me-2 tb-btn btn-sm" id="maximizeTree" onclick="maximizeTree();">
            <i class="fa fa-expand-arrows-alt  btn-text-primary "></i>
        </button>

        <button data-bs-toggle="tooltip" data-bs-placement="bottom" data-bs-dismiss="click" data-bs-original-title="Minimize Tree" class="btn btn-round btn-icon btn-white btn-color-gray-500 btn-active-primary shadow-sm me-2 tb-btn btn-sm" style="display: none;" id="minimizeTree" onclick="minimizeTree();">
            <i class="fa fa-compress-arrows-alt  btn-text-primary "></i>
        </button>
        <!--end::Search-->
    </div>



    <div id="tree" class="Dept-modal-Tree"></div>
    <div id="footer" class="">
        <a href="#" class="btn btn-primary btn-sm me-2" onclick="setAttributes();" id="btnSave">Apply</a>

    </div>

    <script src="../assets/plugins/global/plugins.bundle.js"></script>
    <script src="../assets/js/scripts.bundle.js"></script>
    <!--end::Global Javascript Bundle-->
    <!--begin::Page Custom Javascript(used by this page)-->
    <!-- <script src="../assets/js/jquery-3.6.0.min.js"></script> -->
    <script src="../assets/js/jquery-ui.min.js"></script>
    <!-- <script src="../assets/js/fancytree-all.min.js"></script> 
    <script src="../assets/js/fancytree.glyph.js"></script>
    <script src="../assets/js/fancytree.filter.js"></script>-->

    <script src="../../ThirdParty/fancytree-master/src/jquery.fancytree.js"></script>
    <script src="../../ThirdParty/fancytree-master/src/jquery.fancytree.dnd5.js"></script>
    <script src="../../ThirdParty/fancytree-master/src/jquery.fancytree.glyph.js"></script>
    <script src="../../ThirdParty/fancytree-master/src/jquery.fancytree.filter.js"></script>
    <!-- <script src="../ThirdParty/fancytree-master/src/jquery.fancytree.edit.js"></script>
     -->

    <script src="../../js/common.js"></script>
    <script src="../js/TreePicker.js?v=4"></script>

</body>
</html>
