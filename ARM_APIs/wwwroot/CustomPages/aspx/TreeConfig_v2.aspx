<%@ Page Language="C#" AutoEventWireup="true" CodeFile="TreeConfig_v2.aspx.cs" Inherits="TreeConfig_v2" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <title></title>
    <meta name="description"
        content="Rider admin dashboard live demo. Check out all the features of the admin panel. A large number of settings, additional services and widgets." />
    <meta name="keywords"
        content="Rider, bootstrap, bootstrap 5, dmin themes, free admin themes, bootstrap admin, bootstrap dashboard" />
    <link rel="canonical" href="Https://preview.keenthemes.com/rider-free" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <link rel="shortcut icon" href="../assets/media/logos/favicon.ico" />
    <!--begin::Fonts-->
    <!--<link rel="stylesheet" href="https://fonts.googleapis.com/css?family=Roboto:300,400,500,600,700" />-->
    <link rel="stylesheet" href="../assets/css/all.min.css" />
    <!--end::Fonts-->
    <!--begin::Global Stylesheets Bundle(used by all pages)-->
    <link href="../assets/plugins/global/plugins.bundle.css" rel="stylesheet" type="text/css" />
    <link href="../assets/css/style.bundle.css" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" href="../assets/css/ui.fancytree.min.css" />
    <link rel="stylesheet" href="../assets/css/jquery.contextMenu.css" />
    <link rel="stylesheet" href="../assets/css/bootstrap.min.css" />
    <link rel="stylesheet" href="../assets/css/material-icons.css" />
    <link rel="stylesheet" href="../assets/css/jquery-ui.css" />
    <link rel="stylesheet" href="../../newpopups/remodal/remodal.css" />
    <link rel="stylesheet" href="../../newpopups/axpertpopup.css" />
    <link href="../css/style.bundle.css" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" href="../css/TreeConfig_v2.css?v=5" type="text/css" />
    <link rel="stylesheet" href="../css/fonts.css" type="text/css" />
</head>
<body id="kt_body" class="header-fixed header-tablet-and-mobile-fixed aside-fixed">

    <form runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server">
        </asp:ScriptManager>
    </form>
    <!--begin::Content-->
    <div class="content d-flex flex-column flex-column-fluid fs-6" id="Corporate_Tree">
        <!--begin::Container-->
        <div class="" id="kt_content_container">
            <!--begin::Row-->
            <div class="row g-xl-8" id="overalldiv">

                <!--begin:::Col-->
                <div class="col-xl-6 col-md-6 d-flex flex-column flex-column-fluid vh-100 min-vh-100" id="corporatediv">

                    <div class="toolbar m-0 p-0 py-1">
                        <div class=" d-flex flex-stack flex-wrap flex-sm-nowrap p-0">

                            <h1 class="text-dark fw-bolder my-1 fs-4 page-caption" id="treeCaption"></h1>

                            <div class="d-flex pe-6 my-1">
                                <div class="d-flex">
                                    <div data-popper-placement="bottom-end" data-kt-menu="true" class="w-250px menu menu-sub menu-sub-dropdown menu-column   show searchBoxChildContainer treeSearch initialized d-none">
                                        <div class="icon">
                                            <span id="idsearch" class="material-icons material-icons-style position-absolute       ms-3">search
                                            </span>
                                            <input type="search" id="treeSearch" placeholder="Search records..." class="form-control form-control-flush ps-13" onblur="$('.searchBoxChildContainer.treeSearch').addClass('d-none');" />
                                        </div>
                                    </div>
                                </div>
                                <a href="#" data-bs-toggle="tooltip" data-bs-placement="bottom" data-bs-dismiss="click" data-bs-original-title="Search" onclick="$('.searchBoxChildContainer.treeSearch').removeClass('d-none');$('#treeSearch').focus();"
                                    class="btn btn-round btn-icon btn-white btn-color-gray-500 btn-active-primary shadow-sm me-2 tb-btn btn-sm ">
                                    <i class="fa fa-search btn-text-primary"></i>
                                </a>

                                <a href="#" id="expandTree" data-bs-toggle="tooltip" data-bs-placement="bottom" data-bs-dismiss="click" data-bs-original-title="Expand All" onclick="expandTree();"
                                    class="btn btn-round btn-icon btn-white btn-color-gray-500 btn-active-primary shadow-sm me-2 tb-btn btn-sm ">
                                    <i class="fa fa-sort-amount-down btn-text-primary"></i>
                                </a>

                                <a href="#" id="collapseTree" data-bs-toggle="tooltip" data-bs-placement="bottom" data-bs-dismiss="click" data-bs-original-title="Collapse All" onclick="collapseTree();" style="display: none;"
                                    class="btn btn-round btn-icon btn-white btn-color-gray-500 btn-active-primary shadow-sm me-2 tb-btn btn-sm  ">
                                    <i class="fa fa-sort-amount-up btn-text-primary"></i>
                                </a>
                                <!--end::Search-->

                                <button data-bs-toggle="tooltip" data-bs-placement="bottom" data-bs-dismiss="click" data-bs-original-title="Maximize Tree" class="btn btn-round btn-icon btn-white btn-color-gray-500 btn-active-primary shadow-sm me-2 tb-btn btn-sm" id="maximizeTree" onclick="maximizeTree();">
                                    <i class="fa fa-expand-arrows-alt  btn-text-primary "></i>
                                </button>

                                <button data-bs-toggle="tooltip" data-bs-placement="bottom" data-bs-dismiss="click" data-bs-original-title="Minimize Tree" class="btn btn-round btn-icon btn-white btn-color-gray-500 btn-active-primary shadow-sm me-2 tb-btn btn-sm" style="display: none;" id="minimizeTree" onclick="minimizeTree();">
                                    <i class="fa fa-compress-arrows-alt  btn-text-primary "></i>
                                </button>
                            </div>
                        </div>
                    </div>

                    <div class="card card-xl-stretch shadow-sm flex-root h-1px  ">

                        <!--begin::Body-->
                        <div class="card-body h-300px ">

                            <div id="tree"></div>

                        </div>
                        <!--end::Body-->
                    </div>
                    <div class="tree-footer mt-2">
                        <a href="#" class="btn btn-primary btn-sm me-2" onclick="saveTreeData();" id="btnSave">Save</a>
                        <a href="#" class="btn btn-primary btn-sm me-2" onclick="publishTreeData();" id="btnPublish">Publish</a>
                        <a href="#"
                            class="btn btn-white btn-color-gray-700 btn-active-primary align-items-center shadow-sm me-2 btn-sm" onclick="discardTreeData();" id="btnDiscard">Discard</a>
                    </div>
                    <!--end::Table Widget 1-->



                </div>
                <!--end:::Col-->


                <!--begin:::Col-->
                <div class="col-xl-6  col-md-6 d-flex flex-column flex-column-fluid vh-100 min-vh-100" id="dimesiondiv">

                    <div class="modal" id="attributeSelectorModal" tabindex="-1">
                        <div class="modal-dialog">
                            <div class="modal-content">
                                <div class="modal-header">
                                    <h5 class="modal-title">Select Attributes</h5>
                                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                                </div>
                                <div class="modal-body">
                                    <%--<select class="attributeSelector" multiple="multiple"></select>--%>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="toolbar m-0 p-0 py-1">
                        <div class=" d-flex flex-stack flex-wrap flex-sm-nowrap p-0">
                            <div class="col-xl-12  col-md-12">
                                <h1 class="text-dark fw-bolder my-1 fs-4 page-caption" id="dimensionCaption" onclick="closeDimension()">Attributes</h1>
                            </div>
                            <!--<div class="col-xl-6  col-md-6">
                                <select class="attributeSelector" id="attributeSelector" multiple="multiple" style="display:none"></select>
                            </div>
                            <div class="col-xl-1  col-md-1">
                                <span onclick="showAttributes();" class="material-icons" style="color:#000000">filter_alt</span>
                            </div>-->
                        </div>
                    </div>

                    <!--begin::List Widget 3-->
                    <div class="card card-xl-stretch shadow-sm flex-root h-1px" id="dimensionCard">

                        <!--begin::Body-->
                        <div class="card-body h-300px">
                            <!--begin::Tabs-->


                            <ul class="nav nav-tabs nav-line-tabs mb-8 fs-6" id="dimensionTabNav">
                            </ul>
                            <!--end::Tabs-->
                            <!--begin:Tab content-->
                            <div class="tab-content" id="dimensionTabContent">
                                <!--begin::Tab pane-->

                                <!--end::Tab pane-->
                            </div>
                            <!--end:Tab content-->
                        </div>
                        <!--end::Body-->
                    </div>
                    <!--end::List Widget 3-->
                    <iframe class="d-none dimensionIframe" style="height: calc(100% - 0px); width: calc(100% - 0px);" id="rightIframe"></iframe>
                    <%--<div class="modal fade" id="dimensionModal" tabindex="-1" aria-labelledby="dimensionModalLabel" aria-hidden="true">
                        <div class="modal-dialog">
                            <div class="modal-content">
                                <div class="modal-header">
                                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                                </div>
                                <div class="modal-body">
                                    <div class="container">
                                        <iframe width="400" height="400" id="dimensionFrame" style="border: 1px;" allowfullscreen=""></iframe>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>--%>
                </div>
                <!--end:::Col-->

            </div>
            <!--end::Row-->

        </div>
        <!--end::Container-->
    </div>
    <!--end::Content-->

    <div id="waitDiv" class="page-loader rounded-2 bg-radial-gradient">
        <div class="loader-box-wrapper d-flex bg-white p-20 shadow rounded">
            <span class="loader"></span>
        </div>
    </div>


    <!--begin::Javascript-->
    <!--begin::Global Javascript Bundle(used by all pages)-->
    <!-- <script src="../assets/js/jquery-3.6.0.min.js"></script> -->
    <script src="../assets/plugins/global/plugins.bundle.js"></script>
    <script src="../assets/js/scripts.bundle.js"></script>
    <!--end::Global Javascript Bundle-->
    <!--begin::Page Custom Javascript(used by this page)-->
    <script src="../assets/js/custom/widgets.js"></script>
    <!-- <script src="../assets/js/jquery-3.6.0.min.js"></script> -->
    <script src="../assets/js/jquery-ui.min.js"></script>
    <%--<script src="../assets/js/jquery-ui.js"></script>--%>
    <script src="../assets/js/jquery.contextMenu.min.js"></script>
    <%--    <script src="../assets/js/bootstrap.bundle.min.js"></script>--%>
    <script src="../assets/js/fancytree-all.min.js"></script>
    <script src="../assets/js/fancytree.glyph.js"></script>
    <script src="../assets/js/fancytree.filter.js"></script>
    <script src="../assets/js/fancytree.filter.js"></script>

    <script src="../../js/common.js"></script>
    <script src="../../newpopups/remodal/remodal.js"></script>
    <script src="../../newpopups/axpertpopup.js"></script>
    <script src="../js/TreeConfig_v2.js?v=12"></script>

    <!--end::Page Custom Javascript-->
    <!--end::Javascript-->
</body>
</html>
