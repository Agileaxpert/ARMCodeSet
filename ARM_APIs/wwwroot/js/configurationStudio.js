$(()=>{
    $(".adminMainLeft .menu-link[onclick]:first").click();
})
function LoadIframeac(src) {
    // ShowDimmer(true);
    //splitfullDwb();
    isTstructSplited = false;
    try {
        AxOnLoadIframe();
    }
    catch (ex) { }
    if (src.indexOf("iviewInteractive") !== 1)
        src = src.replace("iviewInteractive", "iview");

    if (window.globalChange) {l̥
        if (confirm(callParent('lcm[31]'))) {
            SetFormDirty(false);
        } else {
            return;
        }
    } else if ($("#axpiframeac")[0].contentWindow.designChanged != undefined && $("#axpiframeac")[0].contentWindow.designChanged == true) {

        if (!confirm(callParent('lcm[31]')))
            return;
    }
    var el = "";
    let el2 = "";
    try {
        el2 = AxOnLoadMiddleIframe(src);
        if (el2 != undefined || el2 != "") {
            // el2.src = "";
            el2.src = '../../aspx/' + src;
        }
    }
    catch (ex) { }
    if (el2 === undefined || el2 === "") {
        el = document.getElementById('axpiframeac');
        el.src = src;
    }

    isTstructPopup = false;
    return false;
}

function SetFormDirty(status) {
    IsFormDirty = status;
    window.globalChange = status;
    if (typeof $("#axpiframeac") != "undefined" && $("#axpiframeac").attr("src").indexOf("tstruct.aspx") > -1 && typeof tstAxpFileFlds != "undefined" && tstAxpFileFlds == true) {
        tstAxpFileFlds = false;
        ASB.WebService.RemoveUnwantedAxpFiles();
    }
}
function loadFrameAc() {
    if (typeof $('#axpiframeac')[0].contentWindow.ShowDimmer != "undefined") { $('#axpiframeac')[0].contentWindow.ShowDimmer(false) };
    $("body").addClass($.axpertUI.options.loader.parent.substr(1));
}

function closeFrameAc() {
    $("body").removeClass($.axpertUI.options.loader.parent.substr(1));
}