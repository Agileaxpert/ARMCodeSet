
$(document).ready(function () {
    var axHtmlObj = new AxHTML();
    let input = parent.$("#input_text000F1").val();
    let jsonData = {};
    axHtmlObj.parse({ name: "Preview", input: input, data: jsonData, container: "#configParamsContainer" });

});