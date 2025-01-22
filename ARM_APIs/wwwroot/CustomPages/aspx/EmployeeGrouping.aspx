<%@ Page Language="C#" AutoEventWireup="true" CodeFile="EmployeeGrouping.aspx.cs" Inherits="EmployeeGrouping" %>

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
    <link rel="stylesheet" href="../css/EmployeeGrouping.css?v=5" type="text/css" />
    <link rel="stylesheet" href="../css/fonts.css" type="text/css" />
</head>
<body id="Employee_grouping" class="header-fixed header-tablet-and-mobile-fixed aside-fixed">

    <form runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server">
        </asp:ScriptManager>
    </form>
    <!--begin::Content-->
    <div class="content d-flex flex-column flex-column-fluid " id="">
    
        <!--begin::Container-->
        <div class="" id="kt_content_container">
            <!--begin::Row-->
            <div class="row g-xl-8" id="overalldiv">
                <div class="toolbar Emp-Grouping-Title">
                    <div class=" d-flex flex-stack flex-wrap flex-sm-nowrap p-0">
                        <div class="col-xl-12  col-md-12">
                            <h1 class="text-dark fw-bolder my-1 fs-4 page-caption" id="attributeCaption">Employee Grouping</h1>


                        </div>

                        <!--begin::Menu wrapper-->
                        <div class="Select-attributes">
                            <!--begin::Toggle-->
                            <button type="button" id="Select_Emp_attribute" class="btn btn-white rotate" data-kt-menu-trigger="click"
                                data-kt-menu-placement="bottom-start" data-kt-menu-offset="30px, 30px">
                                <span class="material-icons material-icons-style material-icons-1">groups</span>
                                Select Employee Attributes
                                <span class="svg-icon svg-icon-3 rotate-180 ms-3 me-0">
                                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                                        <path d="M11.4343 12.7344L7.25 8.55005C6.83579 8.13583 6.16421 8.13584 5.75 8.55005C5.33579 8.96426 5.33579 9.63583 5.75 10.05L11.2929 15.5929C11.6834 15.9835 12.3166 15.9835 12.7071 15.5929L18.25 10.05C18.6642 9.63584 18.6642 8.96426 18.25 8.55005C17.8358 8.13584 17.1642 8.13584 16.75 8.55005L12.5657 12.7344C12.2533 13.0468 11.7467 13.0468 11.4343 12.7344Z" fill="currentColor"></path>
                                    </svg></span>
                            </button>
                            <!--end::Toggle-->

                            <!--begin::Menu-->
                            <div class="menu menu-sub menu-sub-dropdown menu-column menu-rounded menu-gray-800 menu-state-bg-light-primary fw-semibold w-800px"
                                data-kt-menu="true">

                                <div class="card">



                                    <!--begin::Card body-->
                                    <div class="card-body ">
                                        <!--begin::Scroll-->
                                        <div class="mh-300px scroll-y me-n5 pe-5">
                                            <!--begin::Row-->
                                            <div class="row g-2" id="appendGroup">
                                                <!--begin::Col-->

                                                <!-- <div class="col-3">
                                                    <div class="form-check">
                                                        <input class="form-check-input" type="checkbox" value=""
                                                            id="flexCheckDefault1">
                                                        <label class="form-check-label"
                                                            for="flexCheckDefault1">Department</label>
                                                    </div>
                                                </div>-->

                                            </div>
                                        </div>

                                    </div>

                                    <div class="card-footer  py-5">
                                        <a href="#" class="btn btn-primary btn-sm px-4" onclick="groupingSubmit()">Apply</a>
                                    </div>


                                </div>


                            </div>
                            <!--end::Menu-->
                        </div>
                        <!--end::Dropdown wrapper-->



                    </div>
                </div>

                <!--begin:::Col-->
                <div class="col-xl-4 col-md-4 d-flex flex-column flex-column-fluid vh-100 min-vh-100"
                    id="groupviewDiv">

                    <div class="toolbar m-0 p-0 py-1">
                        <div class=" d-flex flex-stack flex-wrap flex-sm-nowrap p-0">
                            <h5 id="groupCode"></h5>  <h5 id="groupName"></h5>
                        </div>
                        <!-- <div class=" d-flex flex-stack flex-wrap flex-sm-nowrap p-0">
                          
                        </div> -->
                    </div>

                    <div class="card card-xl-stretch shadow-sm flex-root h-1px  ">

                        <!--begin::Body-->
                        <div class="card-body h-300px ">

                            <div id="tree" class="accordion"></div>

                        </div>
                        <!--end::Body-->
                    </div>
                    <!--end::Table Widget 1-->



                </div>
                <!--end:::Col-->


                <!--begin:::Col-->
                <div class="col-xl-8  col-md-8 d-flex flex-column flex-column-fluid vh-100 min-vh-100"
                    id="attributediv">

                    <div class="modal" id="attributeSelectorModal" tabindex="-1">
                        <div class="modal-dialog">
                            <div class="modal-content">
                                <div class="modal-header">
                                    <h5 class="modal-title">Select Attributes</h5>
                                    <button type="button" class="btn-close" data-bs-dismiss="modal"
                                        aria-label="Close">
                                    </button>
                                </div>
                                <div class="modal-body">
                                    <%--<select class="attributeSelector" multiple="multiple"></select>--%>
                                </div>
                            </div>
                        </div>
                    </div>
                    <!-- <div class="toolbar m-0 p-0 py-1">
                        <div class=" d-flex flex-stack flex-wrap flex-sm-nowrap p-0">
                            <div class="col-xl-12  col-md-12">
                                <h1 class="text-dark fw-bolder my-1 fs-4 page-caption" id="attributeCaption" onclick="closeAttribute()">Attributes</h1>
                            </div>
                            <div class="col-xl-6  col-md-6">
                                <select class="attributeSelector" id="attributeSelector" multiple="multiple" style="display:none"></select>
                            </div>
                            <div class="col-xl-1  col-md-1">
                                <span onclick="showAttributes();" class="material-icons" style="color:#000000">filter_alt</span>
                            </div>
                        </div>
                    </div>-->

                    <!--begin::List Widget 3-->
                    <div class="card card-xl-stretch shadow-sm flex-root h-1px" id="attributeCard">

                        <!--begin::Body-->
                        <div class="card-body h-300px">
                            <!--begin::Tabs-->
                            <!---->
                            <ul class="nav nav-tabs nav-line-tabs mb-8 fs-6" id="attributeTabNav">
                            </ul>

                            <a href="#" class="btn btn-primary btn-sm me-2 AdvanceGrouping" onclick="openAdvancedGrouping()">Advanced Grouping</a>
                            <!--end::Tabs-->
                            <!--begin:Tab content-->
                            <div class="tab-content" id="attributeTabContent">
                                <!--begin::Tab pane-->

                                <!--end::Tab pane-->
                            </div>
                            <!--end:Tab content-->

                        </div>

                        <!--end::Body-->

                    </div>
                    <div class="tree-footer mt-2">
                        <a href="#" class="btn btn-primary btn-sm me-2" onclick="saveGroupingData()">Submit</a>
                        <!-- <a href="#" class="btn btn-primary btn-sm me-2" onclick="openAdvancedGrouping()">Advanced Grouping...</a> -->
                    </div>


                    <!--end::List Widget 3-->
                    <iframe class="d-none attributeIframe"
                        style="height: calc(100% - 0px); width: calc(100% - 0px);" id="rightIframe"></iframe>
                    <%--<div class="modal fade" id="attributeModal" tabindex="-1"
                    aria-labelledby="attributeModalLabel" aria-hidden="true">
                    <div class="modal-dialog">
                        <div class="modal-content">
                            <div class="modal-header">
                                <button type="button" class="btn-close" data-bs-dismiss="modal"
                                    aria-label="Close"></button>
                            </div>
                            <div class="modal-body">
                                <div class="container">
                                    <iframe width="400" height="400" id="attributeFrame" style="border: 1px;"
                                        allowfullscreen=""></iframe>
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
    <script src="../js/EmployeeGrouping.js?v=12"></script>

    <!--end::Page Custom Javascript-->
    <!--end::Javascript-->
</body>
</html>
